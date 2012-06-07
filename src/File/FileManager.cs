using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.IO;
using System.Text;

namespace tyfu.JsonStorage.File
{
    public class FileManager : IFileManager
    {
        private IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
        private string BlocksFile { get; set; }
        private ISerializeJson Json;

        public string FileName { get; set; }
        
        public int BlockSize { get { return 256; } }
        public IDictionary<int, bool> Blocks { get; set; }



        public FileManager(string filename, ISerializeJson json)
        {
            Initalize(filename, json);
        }

        public FileManager(string filename)
        {
            Initalize(filename, new JsonNewtonsoft());
        }


        public void Initalize(string filename, ISerializeJson json)
        {
            Json = json;
            FileName = filename + ".docs";
            BlocksFile = filename + ".blocks";

            if (isf.FileExists(FileName)) 
                LoadBlocks();
            else
            {
                Blocks = new Dictionary<int, bool>();
                ResizeFile();
                StoreBlocks();
            }
        }



        public int DocumentSizeToBlocks(int documentSize)
        {
            int blocks = documentSize / BlockSize;
            if (documentSize % BlockSize > 0) blocks++;

            return blocks;
        }


        public int AllocateBlocks(int documentSize)
        {
            int blocks = DocumentSizeToBlocks(documentSize);
            int startBlock = GetStartingBlock(blocks);

            if (startBlock == -1)
            {
                startBlock = Blocks.Count;
                ResizeFile();
            }

            for (int i = startBlock; i < (startBlock + blocks); i++)
            {
                Blocks[i] = false;
            }
            
            StoreBlocks();

            return startBlock * BlockSize;
        }


        public int RecalculateFilePosition(int currentPosition, int oldDocumentSize, int newDocumentSize)
        {
            int blocksTaken = this.DocumentSizeToBlocks(oldDocumentSize);
            int blocksNeeded = this.DocumentSizeToBlocks(newDocumentSize);
            int position = -1;

            if (blocksNeeded == blocksTaken)
            {
                return currentPosition;
            }
            else if (blocksNeeded > blocksTaken)
            {
                //todo: check the loops on both of these calls
                if (this.CanExpandBlocksTaken(currentPosition, oldDocumentSize, newDocumentSize))
                {
                    this.ExpandBlocksTaken(currentPosition, oldDocumentSize, newDocumentSize);
                    return currentPosition;
                }
                else
                {
                    // we need to move the document to a new position.
                    this.ReleaseBlocks(currentPosition, oldDocumentSize, false);
                    return this.AllocateBlocks(newDocumentSize);
                }
            }
            else
            {
                this.ShrinkBlocksTaken(currentPosition, oldDocumentSize, newDocumentSize);
                return currentPosition;
            }
        }


        private int GetStartingBlock(int blocksRequired)
        {
            int start = -1;
            int length = 0;

            foreach (var block in Blocks)
            {
                // we have found a taken space, so reset everything and look for another space
                if (!block.Value)
                {
                    start = -1;
                    length = 0;
                }
                else
                {
                    // set as starting block
                    if (block.Value && start == -1) start = block.Key;

                    // increment the length as we have found another free block
                    if (block.Value && start > -1) length++;

                    // we have found a space big enough, so return its starting block position
                    if (blocksRequired == length) return start;
                }
            }

            // we didn't find an available space
            return -1;
        }


        public void ReleaseBlocks(int documentPosition, int documentSize, bool save)
        {
            int startBlock = (int)(documentPosition / BlockSize);
            int blocksTaken = DocumentSizeToBlocks(documentSize);

            for (int i = startBlock; i < (startBlock + blocksTaken); i++)
            {
                Blocks[i] = true;
            }

            if (save) StoreBlocks();
        }


        private bool CanExpandBlocksTaken(int documentPosition, int oldSize, int newSize)
        {
            int startBlock = (int)(documentPosition / BlockSize);
            int blocksTaken = DocumentSizeToBlocks(oldSize);
            int blocksNeeded = DocumentSizeToBlocks(newSize);

            if ((startBlock + blocksNeeded) > (this.Blocks.Count - 1))
            {   // object is at the end of the file & would overflow, just expand the file
                this.ResizeFile();
                return true;
            }

            int start = startBlock + blocksTaken;
            for (int i = start; i < (start + (blocksNeeded - blocksTaken)); i++)
            {
                if (this.Blocks[i] == false)
                    return false;
            }

            return true;
        }

        private void ExpandBlocksTaken(int documentPosition, int oldSize, int newSize)
        {
            int startBlock = (int)(documentPosition / BlockSize);
            int blocksTaken = DocumentSizeToBlocks(oldSize);
            int blocksNeeded = DocumentSizeToBlocks(newSize);

            int start = startBlock + blocksTaken;
            for (int i = start; i < (start + (blocksNeeded - blocksTaken)); i++)
            {
                this.Blocks[i] = false;
            }

            StoreBlocks();
        }
        
        public void ShrinkBlocksTaken(int documentPosition, int oldSize, int newSize)
        {
            int startBlock = (int)(documentPosition / BlockSize);
            int blocksTaken = DocumentSizeToBlocks(oldSize);
            int blocksNeeded = DocumentSizeToBlocks(newSize);

            int start = (startBlock + blocksNeeded);
            for (int i = start; i > (start + (blocksTaken - blocksNeeded)); i++)
            {
                this.Blocks[i] = true;
            }

            StoreBlocks();
        }



        public void ResizeFile()
        {
            const int blocks = 10000;
            int blockcount = Blocks.Count;
            for (int i = 0; i < blocks; i++)
            {
                Blocks.Add(blockcount + i, true);
            }

            using (var fs = new IsolatedStorageFileStream(FileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, isf))
            {
                fs.SetLength(fs.Length + (blocks * BlockSize));
                fs.Flush();
            }
        }




        public void Write(byte[] doc, int position)
        {
            using (var writeIso = new IsolatedStorageFileStream(FileName, FileMode.Open, FileAccess.Write, FileShare.None, isf))
            {
                using (var writer = new StreamWriter(writeIso))
                {
                    Write(doc, position, writer);
                    
                    writer.Flush();
                    writer.Close();
                }
            }
        }

        public void Write(IDictionary<int, byte[]> documents)
        {
            using (var writeIso = new IsolatedStorageFileStream(FileName, FileMode.Open, FileAccess.Write, FileShare.None, isf))
            {
                using (var writer = new StreamWriter(writeIso))
                {
                    foreach (var doc in documents)
                    {
                        Write(doc.Value, doc.Key, writer);
                        writer.Flush();
                    }

                    writer.Close();
                }
            }
        }

        private void Write(byte[] doc, int position, StreamWriter writer)
        {
            writer.BaseStream.Seek(position, SeekOrigin.Begin);
            writer.BaseStream.Write(doc, 0, doc.Length);
            //writer.BaseStream.Write(doc, 0, doc.Length);
        }




        public string Read(int position, int length)
        {
            using (var iso = new IsolatedStorageFileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read, isf))
            {
                using (var reader = new StreamReader(iso))
                {
                    return Read(position, length, reader);
                }
            }
        }

        public IEnumerable<string> Read(IDictionary<int, int> documents)
        {
            using (var iso = new IsolatedStorageFileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read, isf))
            {
                using (var reader = new StreamReader(iso))
                {
                    foreach(var doc in documents)
                        yield return Read(doc.Key, doc.Value, reader);
                }
            }
        }

        public string Read(int position, int length, StreamReader reader)
        {
            byte[] result = new byte[length];
            reader.BaseStream.Seek(position, SeekOrigin.Begin);
            reader.BaseStream.Read(result, 0, length);

            return Encoding.UTF8.GetString(result, 0, length);
        }



        private void StoreBlocks()
        {
            FileMode fileMode;

            if (isf.FileExists(BlocksFile)) fileMode = FileMode.Truncate;
            else fileMode = FileMode.Create;

            using (var write = new StreamWriter(new IsolatedStorageFileStream(BlocksFile, fileMode, FileAccess.Write, isf)))
            {
                write.WriteLine(Json.Serialize<IDictionary<int, bool>>(Blocks));
                write.Close();
            }
        }


        private void LoadBlocks()
        {
            if (isf.FileExists(BlocksFile))
            {
                using (var read = new StreamReader(new IsolatedStorageFileStream(BlocksFile, FileMode.Open, FileAccess.Read, isf)))
                {
                    Blocks = Json.Deserialize<IDictionary<int, bool>>(read.ReadLine());
                    read.Close();
                }
            }
        }
    }
}

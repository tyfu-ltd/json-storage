using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tyfu.JsonStorage.File
{
    public interface IFileManager
    {
        string FileName { get; set; }
        int BlockSize { get; }
        IDictionary<int, bool> Blocks { get; set; }

        void Initalize(string filename, ISerializeJson json);

	    int AllocateBlocks(int documentSize);
        int RecalculateFilePosition(int currentPosition, int oldDocumentSize, int newDocumentSize);
        void ReleaseBlocks(int documentPosition, int documentSize, bool save);

	    void ResizeFile();

        void Write(byte[] document, int position);
        void Write(IDictionary<int, byte[]> documents);

        string Read(int position, int length);
        IEnumerable<string> Read(IDictionary<int, int> documents);
    }
}

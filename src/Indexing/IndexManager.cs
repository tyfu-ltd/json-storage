using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Linq;
using tyfu.JsonStorage;

namespace tyfu.JsonStorage.Indexing
{
    public class IndexManager<T>
    {
        public List<Index> Indexes { get; set; }

        private IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
        private string objectName = "";
        private string filename = "";
        private ISerializeJson Json;
        
        public IndexManager()
        {
            Initalize(new JsonNewtonsoft());
        }

        public IndexManager(ISerializeJson json)
        {
            Initalize(json);
        }


        private void Initalize(ISerializeJson json)
        {
            Json = json;

            objectName = typeof(T).Name;
            filename = objectName + ".index";

            if (isoStore.FileExists(filename)) 
                LoadIndex();
            else 
                Indexes = new List<Index>();
        }



        public List<Index> Where(Func<Index, bool> predicate)
        {
            return Indexes.Where(predicate).ToList();
        }


        private void RebuildIndex()
        {
            // todo: figure out a way to automatically rebuild the index if the file is updated, or check the 
            // datetime of the index an only rebuild if different
            LoadIndex();
        }


        public void UpdateIndex(T graph)
        {
            string id = PropertyResolver<T>.GetIdValue(graph);
            Dictionary<string, object> fields = new IndexProperties<T>().GetIndexProperties(graph);
            
            Index indexItem = GetById(id);

            if (indexItem != null)
            {
                indexItem.Fields = fields;
                StoreIndex();
            }
        }

        public void UpdateIndex(string id, int position, int length)
        {
            Index index = GetById(id);
            index.Position = position;
            index.Length = length;

            StoreIndex();
        }

        public void UpdateIndex(T document, int position, int length)
        {
            string id = PropertyResolver<T>.GetIdValue(document);
            Dictionary<string, object> fields = new IndexProperties<T>().GetIndexProperties(document);

            Index index = GetById(id);

            if (index == null) Indexes.Add(new Index(id, fields, position, length));
            else 
            {
                index.Fields = fields;
                index.Position = position;
                index.Length = length;
            }
            
            StoreIndex();
        }


        public void DeleteFromIndex(string id)
        {
            int idx = 0;

            for (int i = 0; i < Indexes.Count(); i++)
            {
                if (Indexes[i].Id == id)
                {
                    idx = i;
                    break;
                }
            }
            
            Indexes.RemoveAt(idx);
            StoreIndex();
        }




        public Index GetById(string id)
        {
            try
            {
                this.RebuildIndex();

                Index i = (from io in Indexes
                          where io.Id == id
                         select io).Single();

                return i;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool Exists(string id)
        {
            if (GetById(id) == null) return false;

            return true;
        }




        internal void StoreIndex()
        {
            FileMode fileMode;

            if (isoStore.FileExists(filename)) fileMode = FileMode.Truncate;
            else fileMode = FileMode.Create;

            StreamWriter writeFile = new StreamWriter(new IsolatedStorageFileStream(filename, fileMode, FileAccess.Write, isoStore));

            writeFile.WriteLine(Json.Serialize<IList<Index>>(Indexes));
            writeFile.Close();
        }


        private void LoadIndex()
        {
            Indexes = new List<Index>();
            IndexProperties<T> indexProp = new IndexProperties<T>();

            if (isoStore.FileExists(filename))
            {
                StreamReader readFile 
                    = new StreamReader(new IsolatedStorageFileStream(filename, FileMode.Open, FileAccess.Read, isoStore));

                Indexes = Json.Deserialize<List<Index>>(readFile.ReadLine());
                readFile.Close();
            }
        }
    }
}

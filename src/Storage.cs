using System;
using System.Net;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.IO;
using System.Threading;
using System.Text;
using System.ComponentModel;


using tyfu.JsonStorage.Indexing;
using tyfu.JsonStorage.File;


namespace tyfu.JsonStorage
{
    public class Storage<T>
    {
        private IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
        private string filename;
        private ISerializeJson Json;

        internal IndexManager<T> IndexManager { get; set; }
        internal IFileManager FileManager { get; set; } 


        public Storage()
        {
            Initalize(new JsonNewtonsoft());
        }


        private void Initalize(ISerializeJson json)
        {
            filename = typeof(T).Name + ".json";
            Json = new JsonNewtonsoft();
            //Json = new JsonServiceStack();

            IndexManager = new IndexManager<T>(Json);
            FileManager = new FileManager(typeof(T).Name, Json);
        }


        internal int Count()
        {
            return IndexManager.Indexes.Count();
        }


        internal T LoadById(string id)
        {
            Index index = IndexManager.GetById(id);

            if (index == null) return default(T);
            else return Load(index);
        }



        internal IList<T> Load(IEnumerable<Index> indexes)
        {
            IList<T> results = new List<T>(indexes.Count());
            IDictionary<int, int> positions = new Dictionary<int, int>();

            foreach (var i in indexes)
            {
                positions.Add(i.Position, i.Length);
            }

            foreach (var doc in FileManager.Read(positions))
            {
                results.Add(Json.Deserialize<T>(doc));
            }

            return results;
        }


        internal IEnumerable<T> LoadEnumerable(IEnumerable<Index> indexes)
        {
            IDictionary<int, int> positions = new Dictionary<int, int>();

            foreach (var i in indexes)
            {
                positions.Add(i.Position, i.Length);
            }

            foreach (var doc in FileManager.Read(positions))
            {
                yield return Json.Deserialize<T>(doc);
            }
        }

        internal IEnumerable<projection> LoadEnumerable<projection>(IEnumerable<Index> indexes)
        {
            IDictionary<int, int> positions = new Dictionary<int, int>();

            foreach (var i in indexes)
            {
                positions.Add(i.Position, i.Length);
            }

            foreach (var doc in FileManager.Read(positions))
            {
                yield return Json.Deserialize<projection>(doc);
            }
        }

        
        private T Load(Index index)
        {
            var doc = FileManager.Read(index.Position, index.Length);
            return Json.Deserialize<T>(doc);
        }



        

        
        internal void Store(T document)
        {
            Index index;

            if (IndexManager.Exists(PropertyResolver<T>.GetIdValue(document)))
            {
                index = IndexManager.GetById(PropertyResolver<T>.GetIdValue(document));
                Update(document, index);
            }
            else Save(document);
        }

        internal void StoreAsync(T document)
        {
            Thread thread = new Thread((object d) =>
            {
                Index index;
                T doc = (T)d;

                if (IndexManager.Exists(PropertyResolver<T>.GetIdValue(doc)))
                {
                    index = IndexManager.GetById(PropertyResolver<T>.GetIdValue(doc));
                    Update(doc, index);
                }
                else Save(doc);
            });

            thread.Start(document);
        }


        private void Save(T document)
        {
            byte[] json = Encoding.UTF8.GetBytes(Json.Serialize<T>(document).ToCharArray());
            int position = FileManager.AllocateBlocks(json.Count());

            FileManager.Write(json, position);
            IndexManager.UpdateIndex(document, position, json.Count());
        }

        private void Update(T document, Index index)
        {
            byte[] json = Encoding.UTF8.GetBytes(Json.Serialize<T>(document).ToCharArray());

            int position = FileManager.RecalculateFilePosition(index.Position, index.Length, json.Count());

            FileManager.Write(json, position);
            IndexManager.UpdateIndex(document, position, json.Count());
        }



        internal void Delete(string id)
        {
            Index index = IndexManager.GetById(id);
            
            IndexManager.DeleteFromIndex(id);
            FileManager.ReleaseBlocks(index.Position, index.Length, true);
        }
    }
}
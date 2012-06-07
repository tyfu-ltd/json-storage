using System;
using System.Linq;
using System.Collections.Generic;
using tyfu.JsonStorage.Indexing;


namespace tyfu.JsonStorage
{
    public class StorageInstance<T>
    {
        internal Storage<T> Instance { get; set; }
        internal object Padlock { get; set; }

        private Type _type = null;
        internal Type Type
        { 
            get 
            { 
                if (_type == null)
                    _type = typeof(T);

                return _type;
            }
        }

        internal StorageInstance(Storage<T> instance)
        {
            Padlock = new object();
            Instance = instance;
        }
    }

    public class StorageManager
    {
        private readonly Dictionary<Type, object> storageInstances 
            = new Dictionary<Type, object>();


        public static readonly StorageManager Instance = new StorageManager();


        private StorageManager()
        {
        }
        

        private StorageInstance<TDocument> GetDocumentStroageInstance<TDocument>()
        {
            var type = typeof(TDocument);
            
            if (!storageInstances.ContainsKey(type))
                this.RegisterDocument<TDocument>();

            return (StorageInstance<TDocument>)storageInstances[type];
        }

        private Storage<TDocument> GetDocumentStroage<TDocument>()
        {
            return GetDocumentStroageInstance<TDocument>().Instance;
        }




        public void RegisterDocument<TDocument>()
        {
            var type = typeof(TDocument);

            if (storageInstances.ContainsKey(type))
                return;

            storageInstances[type] = new StorageInstance<TDocument>(new Storage<TDocument>());
        }


        public void Store<TDocument>(TDocument document)
        {
            var storage = this.GetDocumentStroage<TDocument>();
            storage.Store(document);
        }

        public void Delete<TDocument>(string id)
        {
            var storage = this.GetDocumentStroage<TDocument>();
            storage.Delete(id);
        }



        public int Count<TDocument>()
        {
            var storage = this.GetDocumentStroage<TDocument>();
            return storage.Count();
        }

        public TDocument GetById<TDocument>(string id)
        {
            var storage = this.GetDocumentStroage<TDocument>();
            return storage.LoadById(id);
        }


        public JsonStorageQuery<TDocument> Query<TDocument>()
        {
            var storage = this.GetDocumentStroage<TDocument>();
            return new JsonStorageQuery<TDocument>(storage);
        }
    }
}
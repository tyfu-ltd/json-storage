using System;
using System.Linq;
using System.Collections.Generic;
using tyfu.JsonStorage.Indexing;

namespace tyfu.JsonStorage
{
    public class JsonQueryStatistics
    {
        public int TotalDocuments { get; set; }
        public int QueryResult { get; set; }
    }


    public class JsonStorageQuery<T>
    {
        private IList<Index> indexes;
        private Storage<T> storage;
        
        private int skip = 0;
        private int take = 0;

        private JsonQueryStatistics statistics = new JsonQueryStatistics();


        public JsonStorageQuery(Storage<T> storage)
        {
            this.storage = storage;
            this.indexes = storage.IndexManager.Indexes;
        }


        public JsonStorageQuery<T> Where(Func<Index, bool> predicate)
        {
            this.indexes = indexes.Where(predicate).ToList();
            return this;
        }

        public JsonStorageQuery<T> OrderBy<TOrderby>(Func<Index, TOrderby> orderby)
        {
            this.indexes = indexes.OrderBy(orderby).ToList();
            return this;
        }

        public JsonStorageQuery<T> OrderByDecending<TOrderby>(Func<Index, TOrderby> orderby)
        {
            this.indexes = indexes.OrderByDescending(orderby).ToList();
            return this;
        }

        public JsonStorageQuery<T> Skip(int skip)
        {
            this.skip = skip;
            return this;
        }

        public JsonStorageQuery<T> Take(int take)
        {
            this.take = take;
            return this;
        }

        public JsonStorageQuery<T> Statistics(out JsonQueryStatistics statistics)
        {
            statistics = this.statistics;
            return this;
        }



        public IEnumerable<TProjection> Execute<TProjection>()
        {
            this.statistics.QueryResult = indexes.Count();
            this.statistics.TotalDocuments = storage.Count();

            if (take > 0)
                indexes = indexes.Skip(skip).Take(take).ToList();

            return storage.LoadEnumerable<TProjection>(this.indexes);
        }

        public IEnumerable<T> Execute()
        {
            this.statistics.QueryResult = indexes.Count();
            this.statistics.TotalDocuments = storage.Count();

            if (take > 0)
                indexes = indexes.Skip(skip).Take(take).ToList();
            
            return storage.LoadEnumerable(this.indexes);
        }
    }
}
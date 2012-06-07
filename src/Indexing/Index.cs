using System;
using System.Collections.Generic;

namespace tyfu.JsonStorage.Indexing
{
    public class Index
    {
        public string Id { get; set; }
        public Dictionary<string, object> Fields { get; set; }

        public int Position { get; set; }
        public int Length { get; set; }


        public Index() 
        {
            Fields = new Dictionary<string, object>();
        }

        public Index(string id, Dictionary<string, object> fields, int position, int length)
        {
            this.Id = id;
            this.Fields = fields;
            this.Position = position;
            this.Length = length;
        }
    }
}

using System;


namespace tyfu.JsonStorage.Indexing
{
    public class IndexedProperty
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public Type ValueType { get; set; }

        public IndexedProperty() { }

        public IndexedProperty(string name, object value, Type type)
        {
            this.Name = name;
            this.Value = value;
            this.ValueType = type;
        }

        public ValueType Typed()
        {
            return (ValueType)Value;
        }
    }
}

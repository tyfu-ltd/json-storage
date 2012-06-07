using System;

namespace tyfu.JsonStorage.Attributes
{
    [AttributeUsage(AttributeTargets.All)]
    public class IndexAttribute : Attribute
    {
        public string type { get; set; }

        public IndexAttribute(string type)
        {
            this.type = type;
        }
    }
}
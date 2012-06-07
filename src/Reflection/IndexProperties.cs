using System;

using System.Reflection;
using tyfu.JsonStorage.Attributes;
using System.Collections.Generic;

namespace tyfu.JsonStorage.Indexing
{
    public class IndexProperties<t>
    {
        public IndexProperties() { }

        public Dictionary<string, object> GetIndexProperties(t graph)
        {
            Type type = graph.GetType();
	        PropertyInfo[] pi = type.GetProperties();

            Dictionary<string, object> properties = new Dictionary<string, object>();
	        
            foreach (PropertyInfo p in pi)
	        {
		        // getting custom attribute information
		        object[] attr = p.GetCustomAttributes(true);
		        
                foreach (var o in attr)
		        {
			        if (o.GetType() == typeof(IndexAttribute))
			        {
                        properties.Add(p.Name, p.GetValue(graph, null));
        			}
		        }
	        }

            return properties;
        }


        public Dictionary<string, string> GetIndexFieldTypes(Type t)
        {
            PropertyInfo[] pi = t.GetProperties();

            Dictionary<string, string> fields = new Dictionary<string, string>();

            foreach (PropertyInfo p in pi)
            {
                // getting custom attribute information
                object[] attr = p.GetCustomAttributes(true);

                foreach (var o in attr)
                {
                    if (o.GetType() == typeof(IndexAttribute))
                    {
                        IndexAttribute ia = o as IndexAttribute;
                        fields.Add(p.Name, ia.type);
                    }
                }
            }

            return fields;
        }
    }
}

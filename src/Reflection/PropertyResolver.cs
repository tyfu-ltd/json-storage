using System;
using System.Reflection;

namespace tyfu.JsonStorage
{
    public static class PropertyResolver<t>
    {
        public static string GetIdValue(t obj)
        {
            return GetStringPropertyInfo(obj, "Id");
        }

        public static string GetNameValue(t obj)
        {
            return GetStringPropertyInfo(obj, "name");
        }

        public static DateTime? GetDateValue(t obj)
        {
            return GetDateTimePropertyInfo(obj, "date");
        }


        public static string GetStringPropertyInfo(t obj, string propertyName)
        {
            PropertyInfo pi = obj.GetType().GetProperty(propertyName);
            
            if (pi == null) return "";
            else return (string)pi.GetValue(obj, null);
        }

        public static DateTime? GetDateTimePropertyInfo(t obj, string propertyName)
        {
            PropertyInfo pi = obj.GetType().GetProperty(propertyName);
            
            if (pi == null) return null;
            else return (DateTime?)pi.GetValue(obj, null);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;

namespace just4net.reflect
{
    public class ReflectUtil
    {
        public static T GetAttr<T>(MethodInfo method) where T : Attribute
        {
            if (method == null)
                return null;

            var attrs = method.GetCustomAttributes(typeof(T), false);
            if (attrs.Length != 0)
            {
                var attr = attrs[0] as T;
                if (attr != null)
                    return attr;
            }
            return null;
        }

        public static List<T> GetAttrs<T>(MethodInfo method) where T : Attribute
        {
            if (method == null)
                return null;

            List<T> list = new List<T>();
            var attrs = method.GetCustomAttributes(typeof(T), false);
            if (attrs.Length != 0)
            {
                
                foreach(object attr in attrs)
                {
                    var attribute = attr as T;
                    if (attribute != null)
                        list.Add(attribute);
                }
            }
            return list;
        }
    }
}

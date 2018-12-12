using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace just4net.reflect
{
    /// <summary>
    /// Utils for assembly.
    /// </summary>
    public static class AssemblyUtil
    {
        /// <summary>
        /// Create the instance from type name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T CreateInstance<T>(string type)
        {
            return CreateInstance<T>(type, new object[0]);
        }

        /// <summary>
        /// Create the intance from type name and parameters.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static T CreateInstance<T>(string type, object[] parameters)
        {
            Type instanceType = null;
            var result = default(T);

            instanceType = Type.GetType(type, true);

            if (instanceType == null)
                throw new Exception($"the type {type} was not found!");

            object instance = Activator.CreateInstance(instanceType, parameters);
            result = (T)instance;
            return result;
        }

        /// <summary>
        /// Get the type by full name.
        /// </summary>
        /// <param name="fullTypeName"></param>
        /// <param name="throwOnError"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        public static Type GetType(string fullTypeName, bool throwOnError, bool ignoreCase)
        {
            var targetType = Type.GetType(fullTypeName, false, ignoreCase);

            if (targetType != null)
                return targetType;

            var names = fullTypeName.Split(',');
            var assemblyName = names[1].Trim();

            try
            {
                var assembly = Assembly.Load(assemblyName);

                var typeNamePrefix = names[0].Trim() + "`";
                var matchedTypes = assembly.GetExportedTypes().Where(t => t.IsGenericType
                        && t.FullName.StartsWith(typeNamePrefix, ignoreCase, CultureInfo.InvariantCulture)).ToArray();

                if (matchedTypes.Length != 1)
                    return null;

                return matchedTypes[0];
            }
            catch(Exception ex)
            {
                if (throwOnError)
                    throw ex;

                return null;
            }
        }

        /// <summary>
        /// Get the types from assembly.
        /// </summary>
        /// <typeparam name="TBaseType"></typeparam>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetImplementTypes<TBaseType>(this Assembly assembly)
        {
            return assembly.GetExportedTypes().Where(t => t.IsSubclassOf(typeof(TBaseType)) && t.IsClass && !t.IsAbstract);
        }

        /// <summary>
        /// Get all instances of classes which implement specific interface.
        /// </summary>
        /// <typeparam name="TBaseInterface"></typeparam>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IEnumerable<TBaseInterface> GetImplementedObjectsByInterface<TBaseInterface>(this Assembly assembly)
            where TBaseInterface : class
        {
            return GetImplementedObjectsByInterface<TBaseInterface>(assembly, typeof(TBaseInterface));
        }

        /// <summary>
        /// Get all instances of classes which implement specific interface.
        /// </summary>
        /// <typeparam name="TBaseInterface"></typeparam>
        /// <param name="assembly"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<TBaseInterface> GetImplementedObjectsByInterface<TBaseInterface>(this Assembly assembly, Type type)
        {
            Type[] arrType = assembly.GetExportedTypes();

            var result = new List<TBaseInterface>();
            for(int i = 0; i < arrType.Length; i++)
            {
                var currentImplementType = arrType[i];
                if (currentImplementType.IsAbstract)
                    continue;
                if (!type.IsAssignableFrom(currentImplementType))
                    continue;

                result.Add((TBaseInterface)Activator.CreateInstance(currentImplementType));
            }

            return result;
        }

        /// <summary>
        /// Copy the properties from one object to another.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static T CopyPropertiesTo<T>(this T source, T target)
        {
            return source.CopyPropertiesTo(p => true, target);
        }

        /// <summary>
        /// Copy the properties from one object to another.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="predict"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static T CopyPropertiesTo<T>(this T source, Predicate<PropertyInfo> predict, T target)
        {
            PropertyInfo[] properties = source.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);

            Dictionary<string, PropertyInfo> sourcePropertiesDict = properties.ToDictionary(p => p.Name);

            PropertyInfo[] targetProperties = target.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty)
                .Where(p => predict(p)).ToArray();

            for (int i = 0; i < targetProperties.Length; i++)
            {
                var p = targetProperties[i];
                PropertyInfo sourceProperty;

                if (sourcePropertiesDict.TryGetValue(p.Name, out sourceProperty))
                {
                    if (sourceProperty.PropertyType != p.PropertyType)
                        continue;

                    if (!sourceProperty.PropertyType.IsSerializable)
                        continue;

                    p.SetValue(target, sourceProperty.GetValue(source, null), null);
                }
            }

            return target;
        }

        /// <summary>
        /// Get assembly from a string def of assembly.
        /// </summary>
        /// <param name="assemblyDef"></param>
        /// <returns></returns>
        public static IEnumerable<Assembly> GetAssembliesFromString(string assemblyDef)
        {
            return GetAssembliesFromStrings(assemblyDef.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries));
        }

        /// <summary>
        /// Get assembly from strings.
        /// </summary>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IEnumerable<Assembly> GetAssembliesFromStrings(string[] assemblies)
        {
            List<Assembly> result = new List<Assembly>(assemblies.Length);

            foreach(var a in assemblies)
            {
                result.Add(Assembly.Load(a));
            }

            return result;
        }
    }
}

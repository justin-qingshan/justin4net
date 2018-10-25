using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace just4net.util
{
    public static class EnumUtil
    {
        private const string ENUM_VALUE_FIELD = "value__";


        /// <summary>
        /// Get description of enum value.
        /// </summary>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static string GetEnumMember(this Enum @enum)
        {
            Type type = @enum.GetType();
            FieldInfo field = type.GetField(@enum.ToString());
            DescriptionAttribute enumMember = field.GetCustomAttribute<DescriptionAttribute>();
            return enumMember == null ? @enum.ToString()
                : string.IsNullOrEmpty(enumMember.Description) ? @enum.ToString() : @enumMember.Description;
        }


        /// <summary>
        /// Get members of enum type.
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static IDictionary<string, string> GetEnumMembers(this Type enumType)
        {
            if (!enumType.IsSubclassOf(typeof(Enum)))
                throw new ArgumentOutOfRangeException($"类型{enumType.Name}不是枚举类型！", enumType.Name);

            FieldInfo[] fields = enumType.GetFields();
            IDictionary<string, string> dictionaries = new Dictionary<string, string>();
            foreach(FieldInfo field in fields.Where(x => x.Name != ENUM_VALUE_FIELD))
            {
                DescriptionAttribute enumMember = field.GetCustomAttribute<DescriptionAttribute>();
                dictionaries.Add(field.Name, enumMember == null ? field.Name 
                    : string.IsNullOrEmpty(enumMember.Description) ? field.Name : enumMember.Description);
            }
            return dictionaries;
        }


        /// <summary>
        /// Get all infomation of enum.
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static IEnumerable<Tuple<int, string, string>> GetEnumInfos(this Type enumType)
        {
            if (!enumType.IsSubclassOf(typeof(Enum)))
                throw new ArgumentOutOfRangeException($"类型{enumType.Name}不是枚举类型", enumType.Name);

            FieldInfo[] fields = enumType.GetFields();
            ICollection<Tuple<int, string, string>> enumInfos = new HashSet<Tuple<int, string, string>>();
            foreach(FieldInfo field in fields.Where(x => x.Name != ENUM_VALUE_FIELD))
            {
                DescriptionAttribute enumMember = field.GetCustomAttribute<DescriptionAttribute>();
                int value = Convert.ToInt32(field.GetValue(Activator.CreateInstance(enumType)));
                enumInfos.Add(Tuple.Create(value, field.Name, enumMember == null ? field.Name
                    : string.IsNullOrEmpty(enumMember.Description) ? field.Name : enumMember.Description));
            }

            return enumInfos;
        }
    }
}

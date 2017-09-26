namespace just4net.util
{
    public static class StringUtil
    {

        /// <summary>
        /// Replace the chars of specific string with a new char.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="newChar"></param>
        /// <param name="oldChars"></param>
        /// <returns></returns>
        public static string Replaces(this string self, char newChar, params char[] oldChars)
        {
            foreach (char oldChar in oldChars)
            {
                self = self.Replace(oldChar, newChar);
            }
            return self;
        }


        /// <summary>
        /// Replace the string values in specific string with a new string value.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="newValue"></param>
        /// <param name="oldValues"></param>
        /// <returns></returns>
        public static string Replaces(this string self, string newValue, params string[] oldValues)
        {
            foreach (string oldValue in oldValues)
            {
                self = self.Replace(oldValue, newValue);
            }
            return self;
        }


        /// <summary>
        /// Remove the string values in specific string.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="toRemove"></param>
        /// <returns></returns>
        public static string Remove(this string self, params string[] toRemove)
        {
            return self.Replaces("", toRemove);
        }





    }
}

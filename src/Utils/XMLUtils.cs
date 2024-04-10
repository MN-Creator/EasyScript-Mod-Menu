using System;
using System.Xml.Linq;

namespace EasyScript.UserScripting
{
    internal class XMLUtils
    {
        /// <summary>
        /// Get enum value from attribute. If attribute is null or value is not valid, return default value.
        /// </summary>
        public static T GetEnum<T>(XAttribute attribute, T defaultValue) where T : struct, Enum
        {
            if (attribute != null && Enum.TryParse(attribute.Value, out T value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Get enum value from element. If element is null or value is not valid, return default value.
        /// </summary>
        public static T GetEnum<T>(XElement element, T defaultValue) where T : struct, Enum
        {
            if (element != null && Enum.TryParse(element.Value, out T value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Get bool value from attribute. If attribute is null or value is not valid, return default value.
        /// </summary>
        public static bool GetBool(XAttribute attribute, bool defaultValue)
        {
            if (attribute != null && bool.TryParse(attribute.Value, out bool value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Get bool value from element. If element is null or value is not valid, return default value.
        /// </summary>
        public static bool GetBool(XElement element, bool defaultValue)
        {
            if (element != null && bool.TryParse(element.Value, out bool value))
            {
                return value;
            }
            return defaultValue;
        }

        public static float GetFloat(XAttribute attribute, float defaultValue)
        {
            if (attribute != null && float.TryParse(attribute.Value, out float value))
            {
                return value;
            }
            return defaultValue;
        }

        public static string GetText(XAttribute attribute, string defaultValue)
        {
            return attribute?.Value ?? defaultValue;
        }
    }
}

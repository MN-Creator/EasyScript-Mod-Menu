using GTA;
using System;
using System.Linq;
using System.Text;

namespace Utils
{
    internal class GeneralUtils
    {
        public static T[] ConvertToArray<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        }

        public static T ConvertTextToEnum<T>(string text) where T : Enum
        {
            return (T)Enum.Parse(typeof(T), text);
        }

        public static string AddSpaceForEachCapitalLetter(string input)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (char.IsUpper(c) && builder.Length > 0)
                {
                    builder.Append(' ');
                }
                builder.Append(c);
            }
            return builder.ToString();
        }

        public static string Percentage(float value, float max, string format = "0")
        {
            float percentage = (value / max) * 100;
            percentage = Math.Min(percentage, 100f);
            percentage = Math.Max(percentage, 0f);
            return percentage.ToString(format) + "%";
        }

        public static RaycastResult RaycastFromCamera(float maxDistance, IntersectFlags intersect, Entity ignore = null)
        {
            return World.Raycast(GameplayCamera.Position, GameplayCamera.Direction, maxDistance, intersect, ignore);
        }
    }
}

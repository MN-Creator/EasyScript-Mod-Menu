using GTA;
using LemonUI.Menus;
using System.Drawing;
using Utils;

namespace EasyScript
{
    internal static class MenuColors
    {
        public static Color FranklinColor = Color.LightGreen;
        public static Color FranklinHoveredColor = Color.Green;
        public static Color MichaelColor = Color.FromArgb(100, 179, 211);
        public static Color MichaelHoveredColor = Color.FromArgb(40, 110, 140);
        public static Color TrevorColor = Color.FromArgb(239, 146, 71);
        public static Color TrevorHoveredColor = Color.DarkOrange;

        public static ColorSet Default = new ColorSet()
        {
            BackgroundNormal = Color.FromArgb(0, 0, 0, 0),
            BackgroundHovered = Color.White,
            TitleNormal = Color.White,
            TitleHovered = Color.Black,
            AltTitleNormal = Color.White,
            AltTitleHovered = Color.Black,
        };
        public static ColorSet GreenTitle = new ColorSet()
        {
            TitleNormal = Color.LightGreen,
            TitleHovered = Color.Green,
            AltTitleNormal = Color.LightGreen,
            AltTitleHovered = Color.Green,
        };
        public static ColorSet RedTitle = new ColorSet()
        {
            TitleNormal = Color.Red,
            TitleHovered = Color.Red,
            AltTitleNormal = Color.Red,
            AltTitleHovered = Color.Red,
        };
        public static ColorSet RedAltTitle = new ColorSet()
        {
            AltTitleNormal = Color.Red,
            AltTitleHovered = Color.Red,
        };
        public static ColorSet Background = new ColorSet()
        {
            BackgroundNormal = Color.FromArgb(255, 255, 0, 0),
            BackgroundHovered = Color.FromArgb(255, 255, 0, 0),
        };
        public static ColorSet PurpleBackground = new ColorSet()
        {
            BackgroundNormal = Color.FromArgb(0, 0, 0, 0),
            BackgroundHovered = Color.Purple,
            TitleHovered = Color.White,
            AltTitleHovered = Color.White,
        };

        public static ColorSet YellowBackground = new ColorSet()
        {
            BackgroundNormal = Color.FromArgb(0, 0, 0, 0),
            BackgroundHovered = Color.FromArgb(255, 200, 200, 0),
            TitleHovered = Color.Black,
            AltTitleHovered = Color.Black,
        };

        public static ColorSet FranklinTitle = new ColorSet()
        {
            BackgroundNormal = Default.BackgroundNormal,
            BackgroundHovered = Default.BackgroundHovered,
            TitleNormal = FranklinColor,
            TitleHovered = FranklinHoveredColor,
            AltTitleNormal = FranklinColor,
            AltTitleHovered = FranklinHoveredColor,
        };

        public static ColorSet MichaelTitle = new ColorSet()
        {
            BackgroundNormal = Default.BackgroundNormal,
            BackgroundHovered = Default.BackgroundHovered,
            TitleNormal = MichaelColor,
            TitleHovered = MichaelHoveredColor,
            AltTitleNormal = MichaelColor,
            AltTitleHovered = MichaelHoveredColor,
        };

        public static ColorSet TrevorTitle = new ColorSet()
        {
            BackgroundNormal = Default.BackgroundNormal,
            BackgroundHovered = Default.BackgroundHovered,
            TitleNormal = TrevorColor,
            TitleHovered = TrevorHoveredColor,
            AltTitleNormal = TrevorColor,
            AltTitleHovered = TrevorHoveredColor,
        };

        public static ColorSet GetColorSetForCurrentCharacter()
        {
            if (PlayerUtils.IsTrevor())
            {
                return TrevorTitle;
            }
            if (PlayerUtils.IsMichael())
            {
                return MichaelTitle;
            }
            return FranklinTitle;
        }

        public static ColorSet GetColorSetForCharacter(PedHash ped)
        {
            if (ped == PedHash.Trevor)
            {
                return TrevorTitle;
            }
            if (ped == PedHash.Michael)
            {
                return MichaelTitle;
            }
            return FranklinTitle;
        }

        public static Color GetColorForCurrentCharacter()
        {
            if (PlayerUtils.IsTrevor())
            {
                return TrevorColor;
            }
            if (PlayerUtils.IsMichael())
            {
                return MichaelColor;
            }
            return FranklinColor;
        }
    }
}

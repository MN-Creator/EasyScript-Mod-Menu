using System;
using System.Drawing;

namespace Utils
{
    internal class UIUtils
    {

        // Take in values between 0 and 1 and scale them to the screen size.
        public static PointF ScalePositionToScreen(float x, float y)
        {
            x = Clamp(x, 1f);
            y = Clamp(y, 1f);
            float width = GTA.UI.Screen.Width;
            float height = GTA.UI.Screen.Height;
            return new PointF(x * width, y * height);
        }

        private static float Clamp(float value, float max)
        {
            value = Math.Abs(value);
            value = Math.Min(max, value);
            return value;
        }

        public static bool IsAreaNameVisible()
        {
            return GTA.UI.Hud.IsComponentActive(GTA.UI.HudComponent.AreaName);
        }

        public static bool IsStreetNameVisible()
        {
            return GTA.UI.Hud.IsComponentActive(GTA.UI.HudComponent.StreetName);
        }

        public static void IsSavingIconVisible()
        {
            GTA.UI.Hud.IsComponentActive(GTA.UI.HudComponent.Saving);
        }

        public static void IsWantedStarsVisible()
        {
            GTA.UI.Hud.IsComponentActive(GTA.UI.HudComponent.WantedStars);
        }

        public static void IsVehicleNameVisible()
        {
            GTA.UI.Hud.IsComponentActive(GTA.UI.HudComponent.VehicleName);
        }

        public static void ShowMinimap()
        {
            GTA.UI.Hud.IsRadarVisible = true;
        }

        public static void HideMinimap()
        {
            GTA.UI.Hud.IsRadarVisible = false;
        }

        public static void ToggleMinimap()
        {
            GTA.UI.Hud.IsRadarVisible = !GTA.UI.Hud.IsRadarVisible;
        }
    }
}

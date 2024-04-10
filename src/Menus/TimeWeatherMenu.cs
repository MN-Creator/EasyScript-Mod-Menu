using GTA;
using LemonUI.Menus;
using System;
using Utils;

namespace EasyScript
{
    class TimeWeatherMenu : Submenu
    {
        private readonly NativeListItem<Weather> _weatherList;
        private readonly PropertyPanel _panel;

        public TimeWeatherMenu(MenuPool pool, NativeMenu parent, string title) : base(pool, parent, title)
        {
            CreateItem("Morning", (a, o) => SetTime(9));
            CreateItem("Noon", (a, o) => SetTime(12));
            CreateItem("Evening", (a, o) => SetTime(18));
            CreateItem("Night", (a, o) => SetTime(23));
            _weatherList = CreateList("Weather", WeatherChanged, GeneralUtils.ConvertToArray<Weather>());
            CreateCheckbox("Pause Time", ToggleTimePaused);

            _panel = new PropertyPanel();
            _panel.Add("Time", () => $"{World.CurrentTimeOfDay.Hours:00}:{World.CurrentTimeOfDay.Minutes:00}");
            _panel.Add("Weather", () => World.Weather.ToString());
            SubmenuItem.Panel = _panel;

            foreach (var item in Menu.Items)
            {
                item.Panel = _panel;
            }
        }

        protected override void OnMainMenuOpen(object sender, EventArgs e)
        {
            base.OnMainMenuOpen(sender, e);
            _weatherList.SelectedItem = World.Weather;
            _panel.Recalculate();
        }

        private void SetTime(int hours)
        {
            World.CurrentTimeOfDay = new TimeSpan(hours, 0, 0);
        }

        private void WeatherChanged(object sender, ItemChangedEventArgs<Weather> e)
        {
            World.Weather = e.Object;
        }

        private void ToggleTimePaused(object sender, EventArgs e)
        {
            var timeCheckbox = (NativeCheckboxItem)sender;
            World.IsClockPaused = timeCheckbox.Checked;
        }
    }
}

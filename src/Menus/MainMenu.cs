using EasyScript.Extensions;
using LemonUI.Menus;
using System.Windows.Forms;
using Utils;

namespace EasyScript.Menus
{
    internal class MainMenu : NativeMenu
    {
        private readonly MenuPool _menuPool;
        private NativeListItem<Keys> _openMenuKeyList;
        private Keys _openMenuKey = Keys.F10;

        public MainMenu(string title, MenuPool pool) : base(title)
        {
            BannerText.Text = Main.DisplayName;
            UseMouse = false;
            pool.Add(this);
            _menuPool = pool;
            _menuPool.StartMenu = this;
            _openMenuKey = SettingsManager.GetValue("Settings", "open_menu", Keys.F10);
            CreateMenus();
            _menuPool.KeyUp += (a, o) => this.SelectItemWithNumKeys(o);
        }

        private void CreateMenus()
        {
            new PlayerMenu(_menuPool, this, "Player");
            new WeaponMenu(_menuPool, this, "Weapons");
            new VehicleMenu(_menuPool, this, "Vehicle");
            new BodyguardMenu(_menuPool, this, "Bodyguard");
            new TeleportMenu(_menuPool, this, "Teleport");
            new TimeWeatherMenu(_menuPool, this, "Time & Weather");
            new ScriptingMenu(_menuPool, this, "Scripting");
            CreateSettingsMenu();
        }

        private void CreateSettingsMenu()
        {
            var _settingsMenu = new Submenu(_menuPool, this, "Settings");
            var allKeys = GeneralUtils.ConvertToArray<Keys>();
            _openMenuKeyList = _settingsMenu.CreateList("open_menu", OnOpenMenuKeyChanged, allKeys);
            _openMenuKeyList.SelectedItem = _menuPool.ToggleMenuKey;
            _settingsMenu.CreateItem("Reset Open Menu Key", (a, o) => _openMenuKeyList.SelectedItem = Keys.F10);
            var loggingCheckbox = _settingsMenu.CreateCheckbox("Logging", (o) => Logger.IsLogging = o);
            loggingCheckbox.Checked = Logger.IsLogging;
        }

        private void OnOpenMenuKeyChanged(object sender, ItemChangedEventArgs<Keys> e)
        {
            _openMenuKey = e.Object;
            _menuPool.ToggleMenuKey = _openMenuKey;
            SettingsManager.SetValue("Settings", "open_menu", _openMenuKey);
        }
    }
}

using EasyScript.Extensions;
using GTA;
using LemonUI.Menus;
using System;
using System.Linq;
using System.Windows.Forms;

namespace EasyScript
{
    public class Submenu
    {
        public string Title;
        public readonly MenuPool Pool;
        public NativeMenu Menu => _menu;
        public NativeSubmenuItem SubmenuItem => _submenuItem;
        public static Keys[] AllKeys = GetAllKeys();
        private readonly NativeMenu _menu;
        private readonly NativeSubmenuItem _submenuItem;
        public static Ped PlayerPed => Game.Player.Character;
        private static readonly string _altTitle = "→";

        public Submenu(MenuPool pool, NativeMenu parent, string title)
        {
            Title = title;
            _menu = new NativeMenu(Main.DisplayName, title);
            _menu.UseMouse = false;
            _submenuItem = parent.AddSubMenu(_menu);
            _submenuItem.AltTitle = _altTitle;
            Pool = pool;
            pool.Add(this);
            pool.MenuOpened += OnMainMenuOpen;
            pool.KeyUp += OnKeyUp;
        }

        protected virtual void OnMainMenuOpen(object sender, EventArgs e) { }

        public virtual void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (!_menu.Visible) return;
            _menu.SelectItemWithNumKeys(e);
        }

        public NativeItem CreateItem(string title, EventHandler onActivated = null)
        {
            var item = new NativeItem(title);
            if (onActivated != null)
            {
                item.Activated += onActivated;
            }
            _menu.Add(item);
            return item;
        }

        public NativeItem CreateItem(string title, Action onActivated)
        {
            var item = new NativeItem(title);
            if (onActivated != null)
            {
                item.Activated += (a, o) => onActivated?.Invoke();
            }
            _menu.Add(item);
            return item;
        }

        public NativeSeparatorItem CreateSeparator(string title = "")
        {
            var item = new NativeSeparatorItem(title);
            _menu.Add(item);
            return item;
        }

        public NativeCheckboxItem CreateCheckbox(string title, EventHandler onChanged)
        {
            var item = new NativeCheckboxItem(title);
            if (onChanged != null)
            {
                item.CheckboxChanged += onChanged;
            }
            _menu.Add(item);
            return item;
        }

        public NativeCheckboxItem CreateCheckbox(string title, Action<bool> onChanged = null)
        {
            var item = new NativeCheckboxItem(title);
            if (onChanged != null)
            {
                item.CheckboxChanged += (a, o) => onChanged?.Invoke(item.Checked);
            }
            _menu.Add(item);
            return item;
        }

        public NativeSliderItem CreateSlider(string title, int max, int min, EventHandler onChanged = null)
        {
            var item = new NativeSliderItem(title, max, min);
            if (onChanged != null)
            {
                item.ValueChanged += onChanged;
            }
            _menu.Add(item);
            return item;
        }

        public NativeListItem<int> CreateList(string title, ItemChangedEventHandler<int> onChanged = null, params int[] items)
        {
            var item = new NativeListItem<int>(title, items);
            if (onChanged != null)
            {
                item.ItemChanged += onChanged;
            }
            _menu.Add(item);
            return item;
        }

        public NativeListItem<string> CreateList(string title, ItemChangedEventHandler<string> onChanged = null, params string[] items)
        {
            var item = new NativeListItem<string>(title, items);
            if (onChanged != null)
            {
                item.ItemChanged += onChanged;
            }
            _menu.Add(item);
            return item;
        }

        public NativeListItem<T> CreateList<T>(string title, ItemChangedEventHandler<T> onChanged = null, params T[] items)
        {
            var item = new NativeListItem<T>(title, items);
            if (onChanged != null)
            {
                item.ItemChanged += onChanged;
            }
            _menu.Add(item);
            return item;
        }

        public NativeListItem<T> CreateList<T>(string title, ItemChangedEventHandler<T> onChanged, T defaultValue, params T[] items)
        {
            var item = new NativeListItem<T>(title, items);
            item.SelectedItem = defaultValue;
            if (onChanged != null)
            {
                item.ItemChanged += onChanged;
            }
            _menu.Add(item);
            return item;
        }

        public void AddItem(NativeItem item)
        {
            _menu.Add(item);
        }

        public NativeSubmenuItem AddSubMenu(Submenu menu)
        {
            return _menu.AddSubMenu(menu._menu);
        }

        /// <summary>
        /// Returns letters, numbers, numpad and function keys.
        /// </summary>
        private static Keys[] GetAllKeys()
        {
            Array array = Enum.GetValues(typeof(Keys));
            return array.Cast<Keys>().Where(k => ((int)k >= 48 && (int)k <= 90) ||
                                                 ((int)k >= 96 && (int)k <= 123)).ToArray();
        }
    }
}

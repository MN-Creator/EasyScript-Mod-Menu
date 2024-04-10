using GTA;
using LemonUI;
using LemonUI.Menus;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EasyScript
{
    public class MenuPool
    {
        public ObjectPool Pool = new ObjectPool();
        public GameEventTracker GameEvents = new GameEventTracker();
        public List<Submenu> Submenus = new List<Submenu>();
        public Keys ToggleMenuKey = Keys.F10;
        public GTA.Control ToggleMenuButton = GTA.Control.VehicleHandbrake;
        public GTA.Control SecondaryButton = GTA.Control.PhoneDown;
        public NativeMenu StartMenu;
        public bool IsTickingMenus = true;
        public ColorSet MenuColors = new ColorSet();
        public bool IsMenuVisible => _isMenuVisible;

        public event EventHandler MenuOpened;
        public event EventHandler MenuClosed;
        public event KeyEventHandler KeyUp;
        public event KeyEventHandler KeyDown;
        public event EventHandler Aborted;
        private bool _isMenuVisible;
        private bool _isOpenMenuButtonsPressed;

        public void Add(NativeMenu menu)
        {
            Pool.Add(menu);
        }

        public void Add(Submenu submenu)
        {
            Submenus.Add(submenu);
            Pool.Add(submenu.Menu);
        }

        public void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == ToggleMenuKey)
            {
                ToggleMenuVisible();
                return;
            }
            KeyUp?.Invoke(sender, e);
        }

        public void ToggleMenuGamepad()
        {
            if (Game.LastInputMethod == InputMethod.GamePad)
            {
                if (Game.IsControlPressed(SecondaryButton))
                {
                    Game.DisableControlThisFrame(ToggleMenuButton);
                }

                if (!_isOpenMenuButtonsPressed && Game.IsControlJustReleased(SecondaryButton) && Game.IsControlPressed(ToggleMenuButton))
                {
                    ToggleMenuVisible();
                    _isOpenMenuButtonsPressed = true;
                }
                else if (!_isOpenMenuButtonsPressed && Game.IsControlJustReleased(ToggleMenuButton) && Game.IsControlPressed(SecondaryButton))
                {
                    ToggleMenuVisible();
                    _isOpenMenuButtonsPressed = true;
                }
                else if (_isOpenMenuButtonsPressed && !Game.IsControlPressed(SecondaryButton) && !Game.IsControlPressed(ToggleMenuButton))
                {
                    _isOpenMenuButtonsPressed = false;
                }
            }
        }

        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            KeyDown?.Invoke(sender, e);
        }

        public void ToggleMenuVisible()
        {
            if (!Pool.AreAnyVisible)
            {
                StartMenu.Visible = true;
                _isMenuVisible = true;
                MenuOpened?.Invoke(this, EventArgs.Empty);
                return;
            }
            Pool.HideAll();
            MenuClosed?.Invoke(this, EventArgs.Empty);
            _isMenuVisible = false;
        }

        private void CheckBackspaceClosedMenu()
        {
            if (!_isMenuVisible) return;
            if (Game.IsControlJustReleased(GTA.Control.PhoneCancel))
            {
                MenuClosed?.Invoke(this, EventArgs.Empty);
                _isMenuVisible = false;
            }
        }

        public void Abort()
        {
            Aborted?.Invoke(this, EventArgs.Empty);
        }

        public void OnTick(object sender, EventArgs e)
        {
            ToggleMenuGamepad();
            CheckBackspaceClosedMenu();

            Pool.Process();
            GameEvents.Update();
            if (!IsTickingMenus) return;
            for (int i = 0; i < Submenus.Count; i++)
            {
                Submenu submenu = Submenus[i];
                if (submenu is IUpdate menu)
                {
                    menu.Update();
                }
            }
        }
    }
}

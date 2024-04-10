using EasyScript.Menus;
using GTA;
using System;
using System.Diagnostics;

namespace EasyScript
{
    public class Main : Script
    {
        public static readonly string DisplayName = "EasyScript";
        private static readonly MenuPool _menuPool = new MenuPool();
        private static readonly MainMenu _mainMenu = new MainMenu(DisplayName, _menuPool);
        private readonly bool _displayWelcomeMessage;
        private readonly Timer _welcomeMessageTimer;

        public Main()
        {
            StartLogging();
            var loadingWatch = new Stopwatch();
            loadingWatch.Start();
            loadingWatch.Stop();
            Logger.Log($"Menu loaded in {loadingWatch.ElapsedMilliseconds} ms");
            Logger.Log("Memory Usage: " + GC.GetTotalMemory(true) / 1024 + " KB");
            _displayWelcomeMessage = SettingsManager.GetValue("Settings", "display_welcome_message", true);
            _welcomeMessageTimer = new Timer(1, DisplayWelcomeMessage);

            Aborted += (s, e) => _menuPool.Abort();
            Tick += _menuPool.OnTick;
            Tick += (s, e) => _welcomeMessageTimer.Update();
            KeyUp += _menuPool.OnKeyUp;
            KeyDown += _menuPool.OnKeyDown;
            _menuPool.MenuClosed += OnMenuClosed;
        }

        private static void StartLogging()
        {
            Logger.IsLogging = SettingsManager.GetValue("Settings", "Logging", true);
            Logger.Clear();
            Logger.Log("Menu Started");
        }

        private void OnMenuClosed(object sender, EventArgs e)
        {
            SettingsManager.Save();
        }

        private void DisplayWelcomeMessage(object sender, EventArgs e)
        {
            if (!_displayWelcomeMessage) return;
            string openKey = _menuPool.ToggleMenuKey.ToString();
            string message = $"Welcome to the ~b~EasyScript~w~ Menu. " +
                             $"Open with ~y~{openKey}~w~ or use ~y~RT+Down~w~ on controller.";
            GTA.UI.Notification.Show(message);
            SettingsManager.SetValue("Settings", "display_welcome_message", false);
            SettingsManager.Save();
        }

        public static void ToggleMenu()
        {
            if (_menuPool.IsMenuVisible)
            {
                CloseMenu();
                return;
            }
            OpenMenu();
        }

        public static void OpenMenu()
        {
            _mainMenu.Visible = true;
        }

        public static void CloseMenu()
        {
            _mainMenu.Visible = false;
        }
    }
}

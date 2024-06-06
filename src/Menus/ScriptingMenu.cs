using EasyScript.UserScripting;
using GTA;
using LemonUI.Menus;
using LemonUI.Scaleform;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;

namespace EasyScript
{
    internal class ScriptingMenu : Submenu, IUpdate
    {
        private readonly List<NativeSubmenuItem> _items = new List<NativeSubmenuItem>();
        private readonly List<UserScript> _scripts = new List<UserScript>();
        private static readonly string _scriptFilePath = "Scripts/EasyScript/scripts.xml";
        private static readonly string _scriptingVersion = "1.0";
        private static readonly XAttribute _scriptingVersionAttribute = new XAttribute("version", _scriptingVersion);
        // Used to determine were the script items start, which is after the variable.
        private readonly int _startItemCount;
        private readonly GTA.Control _addScriptButton = GTA.Control.MeleeAttackHeavy;
        private readonly GTA.Control _removeScriptButton = GTA.Control.NextCamera;
        private bool _wasMinimapVisible;

        public ScriptingMenu(MenuPool pool, NativeMenu parent, string title) : base(pool, parent, title)
        {
            Menu.MaxItems = 17;
            var create = CreateItem("Create Script", CreateScript);
            var revertItem = CreateItem("Revert All Actions", UserActions.ResetEffects);
            revertItem.Description = "Revert all actions on the player and world.";
            CreateSeparator("Scripts");
            _startItemCount = Menu.Items.Count;
            LoadScriptsXML();
            CreateButtons();
            Menu.Opening += OnOpen;
            Menu.Closing += OnClose;
            Menu.Closed += (sender, e) => SaveScriptsXML();
        }

        private void OnOpen(object sender, CancelEventArgs e)
        {
            if (e.Cancel) return;
            _wasMinimapVisible = GTA.UI.Hud.IsRadarVisible;
            GTA.UI.Hud.IsRadarVisible = false;
        }

        private void OnClose(object sender, CancelEventArgs e)
        {
            GTA.UI.Hud.IsRadarVisible = _wasMinimapVisible;
        }

        private void CreateButtons()
        {
            var addButton = new InstructionalButton("Add", _addScriptButton);
            var removeButton = new InstructionalButton("Remove", _removeScriptButton);
            Menu.Buttons.Add(removeButton);
            Menu.Buttons.Add(addButton);
            Menu.Buttons.Add(new InstructionalButton("Rename", GTA.Control.Reload));
        }

        public override void OnKeyUp(object sender, KeyEventArgs e)
        {
            base.OnKeyUp(sender, e);
            if (Menu.Visible && e.KeyCode == Keys.R && Menu.SelectedIndex >= _startItemCount)
            {
                if (Menu.SelectedItem.Title.Length == 0) return;
                string name = Game.GetUserInput(WindowTitle.OutfitName, Menu.SelectedItem.Title, 40);
                if (name.Length > 0)
                {
                    Menu.SelectedItem.Title = name;
                    _scripts[Menu.SelectedIndex - _startItemCount].Title = name;
                }
            }
        }

        private void LoadScriptsXML()
        {
            if (!File.Exists(_scriptFilePath)) return;
            Logger.Log("Started Loading Scripts");
            var doc = XDocument.Load(_scriptFilePath);
            var root = doc.Root;
            foreach (XElement scriptNode in root.Elements())
            {
                var userScript = new UserScript(Pool, Menu, "");
                userScript.FromXMLNode(scriptNode);
                _scripts.Add(userScript);
                _items.Add(userScript.SubmenuItem);
            }
            Logger.Log("Finished Loading Scripts");
        }

        private void SaveScriptsXML()
        {
            if (_scripts.Count == 0) return;
            var xmlDoc = new XDocument();
            var root = new XElement("Scripts");
            root.Add(_scriptingVersionAttribute);
            xmlDoc.Add(root);
            for (int i = 0; i < _scripts.Count; i++)
            {
                UserScript script = _scripts[i];
                if (script.ActionList.Count == 0) continue;
                try
                {
                    var scriptNode = script.ToXMLNode();
                    root.Add(scriptNode);
                }
                catch (Exception e)
                {
                    Logger.Log($"Error: {e.Message}");
                    GTA.UI.Notification.Show($"EasyScript: Error saving scripts");
                }
            }
            if (!Directory.Exists("Scripts/EasyScript"))
            {
                Directory.CreateDirectory("Scripts/EasyScript");
            }
            xmlDoc.Save(_scriptFilePath);
        }

        private void RemoveScript(object sender, EventArgs e)
        {
            if (_scripts.Count > 0)
            {
                _scripts.RemoveAt(_scripts.Count - 1);
                Menu.Remove(_items[_items.Count - 1]);
                _items.RemoveAt(_items.Count - 1);
                SettingsManager.Save();
            }
        }

        private void CreateScript(object sender, EventArgs e)
        {
            var script = new UserScript(Pool, Menu, $"Script {_scripts.Count + 1}");
            script.LoadAction(UserActions.AllActions[0].Name);
            _scripts.Add(script);
            _items.Add(script.SubmenuItem);
        }

        public void Update()
        {
            if (!Menu.Visible) return;
            if (Game.IsControlJustReleased(_addScriptButton))
            {
                CreateScript(null, null);
            }
            if (Game.IsControlJustReleased(_removeScriptButton))
            {
                if (_scripts.Count == 0 || Menu.SelectedIndex <= _startItemCount) return;
                int currentIndex = Menu.SelectedIndex;
                int index = currentIndex - _startItemCount;
                Menu.Remove(_items[index]);
                _scripts.RemoveAt(index);
                _items.RemoveAt(index);
                Menu.SelectedIndex = currentIndex - 1;
            }
        }
    }
}

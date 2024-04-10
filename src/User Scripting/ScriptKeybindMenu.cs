using GTA;
using LemonUI.Menus;
using LemonUI.Scaleform;
using System;
using System.Windows.Forms;
using System.Xml.Linq;
using Utils;

namespace EasyScript.UserScripting
{
    internal class ScriptKeybindMenu : Submenu
    {
        public bool IsGamepadButtonEnabled => GamepadControlCheckbox.Checked;
        public bool IsKeyboardKeyEnabled => _enableKeybindCheckbox.Checked;
        public Keys ModifierKey = Keys.None;
        public Keys RunKey = Keys.O;
        public InstructionalButton RunScriptButton;
        public NativeCheckboxItem GamepadControlCheckbox;
        private readonly NativeCheckboxItem _enableKeybindCheckbox;
        private readonly NativeListItem<Keys> _keyShortcutList;
        private readonly NativeMenu _userScriptMenu;

        public ScriptKeybindMenu(MenuPool pool, NativeMenu parent, string title) : base(pool, parent, title)
        {
            _userScriptMenu = parent;
            Keys[] modifierKeys = { Keys.None, Keys.Control, Keys.Alt, Keys.Shift };
            NativeListItem<Keys> modifierList = CreateList("Modifier Key", ChangeModifierKey, modifierKeys);
            _keyShortcutList = CreateList("Key", ChangeRunScriptKey, AllKeys);
            _keyShortcutList.SelectedItem = RunKey;
            _keyShortcutList.Activated += SetKeyFromGameInput;
            _enableKeybindCheckbox = CreateCheckbox("Enable Key");
            _enableKeybindCheckbox.Checked = IsKeyboardKeyEnabled;
            CreateSeparator("Gamepad");
            var controlList = CreateList("Gamepad Control", OnGamepadControlChanged,
                GeneralUtils.ConvertToArray<GTA.Control>());
            controlList.SelectedItem = GTA.Control.Reload;
            GamepadControlCheckbox = CreateCheckbox("Enable Gamepad Shortcut", OnGamepadControlEnabled);
        }

        private void OnGamepadControlEnabled(object sender, EventArgs e)
        {
            if (GamepadControlCheckbox.Checked)
            {
                _userScriptMenu.Buttons.Add(RunScriptButton);
                return;
            }
            _userScriptMenu.Buttons.Remove(RunScriptButton);
        }

        private void OnGamepadControlChanged(object sender, ItemChangedEventArgs<GTA.Control> e)
        {
            RunScriptButton.Control = e.Object;
        }

        private void SetKeyFromGameInput(object sender, EventArgs e)
        {
            string input = Game.GetUserInput();
            input = input.Replace(" ", "").ToUpper();
            if (Enum.TryParse(input, out Keys key))
            {
                _keyShortcutList.SelectedItem = key;
            }
        }

        private void ChangeModifierKey(object sender, ItemChangedEventArgs<Keys> e)
        {
            ModifierKey = e.Object;
        }

        private void ChangeRunScriptKey(object sender, ItemChangedEventArgs<Keys> e)
        {
            RunKey = e.Object;
        }

        public bool IsKeybindReleased(KeyEventArgs e)
        {
            if (!IsKeyboardKeyEnabled) return false;
            bool isModifierPressed = (ModifierKey == Keys.None || e.Modifiers.HasFlag(ModifierKey));
            return isModifierPressed && RunKey == e.KeyCode;
        }

        public bool IsGamepadButtonReleased()
        {
            if (!GamepadControlCheckbox.Checked) return false;
            return Game.IsControlJustReleased(RunScriptButton.Control);
        }

        public void SetKeyboardKeyEnabled(bool value)
        {
            _enableKeybindCheckbox.Checked = value;
        }

        public void ToXMLNode(XElement scriptNode)
        {
            XElement keybindNode = new XElement("Keybinds");
            keybindNode.Add(new XElement("ModifierKey", ModifierKey));
            keybindNode.Add(new XElement("RunKey", RunKey));
            keybindNode.Add(new XElement("KeyboardKeyEnabled", IsKeyboardKeyEnabled));
            keybindNode.Add(new XElement("RunButton", RunScriptButton.Control));
            keybindNode.Add(new XElement("GamepadButtonEnabled", GamepadControlCheckbox.Checked));

            scriptNode.Add(keybindNode);
        }

        public void FromXMLNode(XElement scriptNode)
        {
            XElement keybindNode = scriptNode.Element("Keybinds");
            if (keybindNode == null) return;

            ModifierKey = XMLUtils.GetEnum(keybindNode.Element("ModifierKey"), ModifierKey);
            RunKey = XMLUtils.GetEnum(keybindNode.Element("RunKey"), RunKey);
            SetKeyboardKeyEnabled(XMLUtils.GetBool(keybindNode.Element("KeyboardKeyEnabled"), false));
            RunScriptButton.Control = XMLUtils.GetEnum(keybindNode.Element("RunButton"), RunScriptButton.Control);
            GamepadControlCheckbox.Checked = XMLUtils.GetBool(keybindNode.Element("GamepadButtonEnabled"), false);
        }
    }
}

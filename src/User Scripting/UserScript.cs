using EasyScript.Extensions;
using GTA;
using LemonUI.Menus;
using LemonUI.Scaleform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace EasyScript.UserScripting
{
    public class UserScript : Submenu, IUpdate
    {
        public List<NativeListItem<string>> ActionList = new List<NativeListItem<string>>();
        public bool Enabled => _enabledCheckbox.Checked;
        private UserEventMenu _eventMenu;
        private readonly GTA.Control _addActionButton = GTA.Control.MeleeAttackHeavy;
        private readonly GTA.Control _removeActionButton = GTA.Control.NextCamera;
        private readonly ScriptKeybindMenu _keybinds;
        private readonly NativeCheckboxItem _enabledCheckbox;
        private readonly string _enableDesc = "Enable keybinds, events and running in the background.";
        private ScriptConditions _conditions;
        private BackgroundRunner _backgroundRunner;
        private NativeCheckboxItem _randomizerCheckbox;
        private readonly Random _random = new Random();

        public UserScript(MenuPool pool, NativeMenu parent, string title) : base(pool, parent, title)
        {
            Menu.MaxItems = 18;
            var runScriptItem = CreateItem("Run Script", ForceRun);
            runScriptItem.Colors = MenuColors.FranklinTitle;
            _enabledCheckbox = CreateCheckbox("Enabled", OnEnabledChanged);
            _enabledCheckbox.Checked = true;
            _enabledCheckbox.Description = _enableDesc;
            CreateMenuForEveryCategory();

            _keybinds = new ScriptKeybindMenu(Pool, Menu, "Keybinds");
            CreateSettingsMenu();
            var createAction = CreateItem("Create Action", CreateAction);
            CreateSeparator("Actions");
            CreateButtons();
            var menuPanel = CreateMenuPanel();
            SubmenuItem.Panel = menuPanel;
        }

        private PropertyPanel CreateMenuPanel()
        {
            var propertyPanel = new PropertyPanel();
            propertyPanel.Add("Enabled", () => _enabledCheckbox.Checked.AsYesNo());
            propertyPanel.Add("Randomizer Active", () => _randomizerCheckbox.Checked.AsYesNo());
            propertyPanel.Add("Keybinds Active", () => (_keybinds.IsKeyboardKeyEnabled || _keybinds.IsGamepadButtonEnabled).AsYesNo());
            propertyPanel.Add("Events Active", () => _eventMenu.IsAnyEventActive().AsYesNo());
            propertyPanel.Add("Conditions Active", () => _conditions.DoesAnyConditionHaveValue().AsYesNo());
            propertyPanel.Add("Running in Background", () => _backgroundRunner.IsActive.AsYesNo());
            return propertyPanel;
        }

        private void OnEnabledChanged(bool value)
        {
            if (value)
            {
                SubmenuItem.Colors = MenuColors.Default;
                return;
            }
            SubmenuItem.Colors = MenuColors.RedAltTitle;
        }

        private Submenu CreateSettingsMenu()
        {
            Submenu settingsMenu = new Submenu(Pool, Menu, "Settings");
            _randomizerCheckbox = settingsMenu.CreateCheckbox("Randomizer");
            _randomizerCheckbox.Description = "Picks a random action when running the script.";
            _eventMenu = new UserEventMenu(Pool, settingsMenu.Menu, "Events", this);
            _backgroundRunner = new BackgroundRunner(Pool, settingsMenu.Menu, "Run in Background", this);
            _conditions = new ScriptConditions(Pool, settingsMenu.Menu, "Conditions");
            return settingsMenu;
        }

        private void CreateButtons()
        {
            var addButton = new InstructionalButton("Add", _addActionButton);
            var removeButton = new InstructionalButton("Remove", _removeActionButton);
            Menu.Buttons.Add(removeButton);
            Menu.Buttons.Add(addButton);
        }

        private void OnListItemChanged(object sender, ItemChangedEventArgs<string> e)
        {
            string action = e.Object;
            if (UserActions.NameToAction.Keys.Contains(action))
            {
                NativeListItem<string> listItem = (NativeListItem<string>)sender;
                listItem.Description = UserActions.NameToAction[action].Description;
                if (UserActions.NameToAction[action] is IActionParameter parameterizedAction)
                {
                    if (parameterizedAction.Parameter.Length > 15)
                    {
                        listItem.Title = parameterizedAction.Parameter.Substring(0, 15);
                        return;
                    }
                    listItem.Title = parameterizedAction.Parameter;
                    return;
                }
                listItem.Title = "";
            }
        }

        private void RunSimpleAction(string action)
        {
            if (UserActions.NameToAction.Keys.Contains(action))
            {
                UserActions.NameToAction[action].Execute();
            }
        }

        private void CreateMenuForEveryCategory()
        {
            Submenu chooseActions = new Submenu(Pool, Menu, "Actions");
            var input = chooseActions.CreateItem("Enter Name");
            input.Activated += (a, o) => LoadActionFromGameInput();

            foreach (ActionCategory category in UserActions.CategoryToActions.Keys)
            {
                var menu = new Submenu(Pool, chooseActions.Menu, category.ToString());
                UserAction[] actions = UserActions.CategoryToActions[category];
                foreach (UserAction action in actions)
                {
                    NativeItem item = menu.CreateItem(action.Name);
                    item.Description = action.Description;
                    item.Activated += (a, o) => AddActionFromCategory(action.Name);
                    if (UserActions.NameToAction[action.Name] is IActionParameter parameterizedAction)
                    {
                        item.Colors = MenuColors.MichaelTitle;
                        item.AltTitle = "(custom value)";
                    }
                }
            }
        }

        private void AddActionFromCategory(string actionName)
        {
            LoadAction(actionName);
            GTA.UI.Screen.ShowSubtitle($"Added ~y~{actionName}");
        }

        private void LoadActionFromGameInput()
        {
            string input = Game.GetUserInput();
            if (!UserActions.NameToAction.Keys.Contains(input)) return;
            AddActionFromCategory(input);
        }

        /// <summary>
        /// Run the script if all conditions return true.
        /// </summary>
        public void Run()
        {
            if (!_enabledCheckbox.Checked || !_conditions.AreConditionsTrue()) return;
            if (_randomizerCheckbox.Checked)
            {
                int index = _random.Next(ActionList.Count);
                RunAction(index);
                return;
            }
            for (int i = 0; i < ActionList.Count; i++)
            {
                RunAction(i);
            }
        }

        /// <summary>
        /// Run the script without checking conditions.
        /// </summary>
        public void ForceRun()
        {
            if (_randomizerCheckbox.Checked)
            {
                int index = _random.Next(ActionList.Count);
                RunAction(index);
                return;
            }
            for (int i = 0; i < ActionList.Count; i++)
            {
                RunAction(i);
            }
        }

        private void RunAction(int index)
        {
            if (index >= ActionList.Count) return;
            UserAction userAction = UserActions.NameToAction[ActionList[index].SelectedItem];
            if (userAction is IActionParameter parameterizedAction)
            {
                parameterizedAction.Parameter = ActionList[index].Title;
            }
            userAction.Execute();
        }

        private void CreateAction()
        {
            string actionTitle = "";
            NativeListItem<string> listItem = CreateList(actionTitle, OnListItemChanged, UserActions.ScriptActionNames);
            listItem.Colors.AltTitleHovered = MenuColors.MichaelColor;
            listItem.Colors.AltTitleNormal = MenuColors.MichaelColor;
            listItem.UpdateColors();

            listItem.Activated += (a, o) => OnListItemActivated(listItem);
            ActionList.Add(listItem);
        }

        private void OnListItemActivated(NativeListItem<string> listItem)
        {
            if (listItem == null) return;
            if (UserActions.NameToAction[listItem.SelectedItem] is IActionParameter actionParameter)
            {
                string parameter = Game.GetUserInput();
                if (actionParameter.IsValid(parameter))
                {
                    actionParameter.Parameter = parameter;
                    listItem.Title = actionParameter.Parameter;
                }
            }
            else
            {
                RunSimpleAction(listItem.SelectedItem);
            }
        }

        public void LoadAction(string command, string parameter = "")
        {
            NativeListItem<string> listItem = CreateList(parameter, OnListItemChanged, UserActions.ScriptActionNames);
            listItem.Activated += (a, o) => OnListItemActivated(listItem);
            if (!UserActions.NameToAction.Keys.Contains(command))
            {
                listItem.SelectedItem = UserActions.AllActions[0].Name;
                ActionList.Add(listItem);
                GTA.UI.Screen.ShowSubtitle($"{Main.DisplayName}: Did not find action '{command}'");
                return;
            }
            listItem.SelectedItem = command;
            if (UserActions.NameToAction[command] is IActionParameter actionParameter)
            {
                listItem.Title = actionParameter.Parameter;
            }
            listItem.Description = UserActions.NameToAction[command].Description;
            ActionList.Add(listItem);
        }

        public override void OnKeyUp(object sender, KeyEventArgs e)
        {
            base.OnKeyUp(sender, e);
            if (_enabledCheckbox.Checked && _keybinds.IsKeybindReleased(e))
            {
                ForceRun();
            }
        }

        public XElement ToXMLNode()
        {
            var scriptNode = new XElement("Script");
            scriptNode.Add(new XAttribute("name", Title));
            scriptNode.Add(new XAttribute("enabled", Enabled));
            scriptNode.Add(new XAttribute("randomize", _randomizerCheckbox.Checked));

            var actionParentNode = new XElement("Actions");
            scriptNode.Add(actionParentNode);
            foreach (NativeListItem<string> action in ActionList)
            {
                if (!UserActions.NameToAction.ContainsKey(action.SelectedItem)) continue;
                var actionNode = new XElement("Action", action.SelectedItem);
                actionParentNode.Add(actionNode);
                if (UserActions.NameToAction[action.SelectedItem] is IActionParameter actionParameter)
                {
                    actionNode.Add(new XAttribute("parameter", actionParameter.Parameter));
                }
            }

            _keybinds.ToXMLNode(scriptNode);
            _eventMenu.ToXMLNode(scriptNode);
            _conditions.ToXMLNode(scriptNode);
            _backgroundRunner.ToXMLNode(scriptNode);

            return scriptNode;
        }

        public void FromXMLNode(XElement scriptNode)
        {
            Title = XMLUtils.GetText(scriptNode.Attribute("name"), "Script");
            SubmenuItem.Title = Title;
            _enabledCheckbox.Checked = XMLUtils.GetBool(scriptNode.Attribute("enabled"), true);
            _randomizerCheckbox.Checked = XMLUtils.GetBool(scriptNode.Attribute("randomize"), false);

            var actionNodes = scriptNode.Element("Actions");
            foreach (XElement actionNode in actionNodes.Elements())
            {
                if (!UserActions.NameToAction.ContainsKey(actionNode.Value)) continue;
                string parameterText = GetActionParameter(actionNode);
                LoadAction(actionNode.Value, parameterText);
            }

            _keybinds.FromXMLNode(scriptNode);
            _eventMenu.FromXMLNode(scriptNode);
            _conditions.FromXMLNode(scriptNode);
            _backgroundRunner.FromXMLNode(scriptNode);
        }

        private string GetActionParameter(XElement actionNode)
        {
            var parameterAttribute = actionNode.Attribute("parameter");
            if (parameterAttribute == null) return "";
            var actionParameter = UserActions.NameToAction[actionNode.Value] as IActionParameter;
            if (actionParameter != null && actionParameter.IsValid(parameterAttribute.Value))
            {
                return parameterAttribute.Value;
            }
            return "";
        }

        public void Update()
        {
            if (_enabledCheckbox.Checked && _keybinds.IsGamepadButtonReleased())
            {
                ForceRun();
            }
            if (!Menu.Visible) return;

            if (Game.LastInputMethod == InputMethod.MouseAndKeyboard && Game.IsControlJustReleased(_addActionButton))
            {
                CreateAction();
            }

            if (Game.LastInputMethod == InputMethod.MouseAndKeyboard && Game.IsControlJustReleased(_removeActionButton))
            {
                if (ActionList.Contains(Menu.SelectedItem))
                {
                    int index = Menu.SelectedIndex;
                    NativeItem selectedItem = Menu.SelectedItem;
                    ActionList.Remove((NativeListItem<string>)selectedItem);
                    Menu.Remove(selectedItem);
                    Menu.SelectedIndex = index - 1;
                }
            }
        }
    }
}
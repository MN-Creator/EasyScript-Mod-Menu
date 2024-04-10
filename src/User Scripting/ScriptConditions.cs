using GTA;
using LemonUI.Menus;
using System;
using System.Linq;
using System.Xml.Linq;
using Utils;

namespace EasyScript.UserScripting
{
    internal enum ConditionValue
    {
        None,
        Yes,
        No
    }

    internal class ScriptConditions : Submenu
    {
        private static bool InVehicle => PlayerPed.IsInVehicle();
        private static Vehicle PlayerVehicle => PlayerPed.CurrentVehicle;

        private readonly Condition[] _conditions =
        {
            new Condition("Holding Weapon", () => PlayerPed.Weapons.Current != WeaponHash.Unarmed),
            new Condition("Mission Active", () => Game.IsMissionActive),
            new Condition("Cutscene Active", () => Game.IsCutsceneActive),
            new Condition("Player Wanted", () => Game.Player.WantedLevel > 0),
            new Condition("Special Ability Active", () => Game.Player.IsSpecialAbilityActive),
            new Condition("Waypoint Set", () => Game.IsWaypointActive),
            new Condition("In Any Vehicle", PlayerPed.IsInVehicle),
            new Condition("Is Vehicle Stopped", () => InVehicle && PlayerVehicle.Speed == 0),
            new Condition("In Car", () => InVehicle && PlayerVehicle.IsAutomobile),
            new Condition("In Airplane", () => InVehicle && PlayerVehicle.IsPlane),
            new Condition("In Helicopter", () => InVehicle && PlayerVehicle.IsHelicopter),
            new Condition("In Boat", () => InVehicle && PlayerVehicle.IsBoat),
            new Condition("In Bike", () => InVehicle && PlayerVehicle.IsBike),
            new Condition("In Train", () => InVehicle && PlayerVehicle.IsTrain),
            new Condition("In Blimp", () => InVehicle && PlayerVehicle.IsBlimp),
        };

        private readonly EnumCondition<Weather> _weather;
        private readonly EnumCondition<WeaponHash> _weapon;

        public ScriptConditions(MenuPool pool, NativeMenu parent, string title) : base(pool, parent, title)
        {
            CreateMenuItems();
            _weather = new EnumCondition<Weather>(this, "Current Weather", () => World.Weather);
            _weapon = new EnumCondition<WeaponHash>(this, "Current Weapon", () => PlayerPed.Weapons.Current.Hash);
            SubmenuItem.Description = "Set conditions for running the script.";
        }

        private void CreateMenuItems()
        {
            foreach (Condition condition in _conditions)
            {
                Menu.Add(condition);
            }
        }

        public bool AreConditionsTrue()
        {
            for (int i = 0; i < _conditions.Length; i++)
            {
                if (!_conditions[i].IsTrue()) return false;
            }
            if (!_weather.IsTrue() || !_weapon.IsTrue())
            {
                return false;
            }
            return true;
        }

        public bool DoesAnyConditionHaveValue()
        {
            return _conditions.Any(condition => condition.HasValue()) || _weather.HasValue() || _weapon.HasValue();
        }

        public void ToXMLNode(XElement scriptNode)
        {
            BooleanToXMLNode(scriptNode);
            EnumToXMLNode(scriptNode);
        }

        public void FromXMLNode(XElement scriptNode)
        {
            BooleanFromXMLNode(scriptNode);
            EnumFromXMLNode(scriptNode);
        }

        private void BooleanToXMLNode(XElement scriptNode)
        {
            XElement conditionsNode = new XElement("Conditions");

            foreach (Condition condition in _conditions)
            {
                if (!condition.HasValue()) continue;
                string name = condition.Name.Replace(" ", "");
                XElement conditionNode = new XElement(name, condition.SelectedItem);
                conditionsNode.Add(conditionNode);
            }
            if (conditionsNode.IsEmpty) return;
            scriptNode.Add(conditionsNode);
        }

        private void BooleanFromXMLNode(XElement scriptNode)
        {
            XElement conditionsNode = scriptNode.Element("Conditions");
            if (conditionsNode == null) return;
            foreach (XElement conditionNode in conditionsNode.Elements())
            {
                string conditionName = conditionNode.Name.LocalName;
                conditionName = GeneralUtils.AddSpaceForEachCapitalLetter(conditionName);
                Condition condition = _conditions.FirstOrDefault(c => c.Name == conditionName);
                if (condition != null)
                {
                    condition.SelectedItem = (ConditionValue)Enum.Parse(typeof(ConditionValue), conditionNode.Value);
                }
            }
        }

        private void EnumToXMLNode(XElement scriptNode)
        {
            var enumConditions = new XElement("EnumConditions");
            _weather.ToXMLNode(enumConditions);
            _weapon.ToXMLNode(enumConditions);
            if (enumConditions.IsEmpty) return;
            scriptNode.Add(enumConditions);
        }

        private void EnumFromXMLNode(XElement scriptNode)
        {
            XElement enumConditions = scriptNode.Element("EnumConditions");
            if (enumConditions == null) return;
            _weather.FromXMLNode(enumConditions);
            _weapon.FromXMLNode(enumConditions);
        }
    }

    interface IBoolean
    {
        bool IsTrue();
    }

    internal class Condition : NativeListItem<ConditionValue>, IBoolean
    {
        public string Name { get; }
        public Func<bool> GetValue;

        public Condition(string name, Func<bool> func) : base(name, ConditionValue.None)
        {
            Name = name;
            GetValue = func;
            Items = GeneralUtils.ConvertToArray<ConditionValue>().ToList();
        }

        public bool IsTrue()
        {
            if (!HasValue()) return true;
            return GetValue() == ToBool();
        }

        public bool ToBool()
        {
            if (!HasValue()) return false;
            if (SelectedItem == ConditionValue.Yes) return true;
            return false;
        }

        public bool HasValue()
        {
            return SelectedItem != ConditionValue.None;
        }
    }

    internal class EnumCondition<T> : IBoolean where T : Enum
    {
        public string Name { get; }
        public Func<T> GetValue;
        // Don't return value if function returns true.
        private readonly Func<bool> _failFunc;
        private readonly NativeListItem<T> _itemList;
        private readonly NativeCheckboxItem _checkbox;

        public EnumCondition(Submenu menu, string name, Func<T> func, Func<bool> failCondition = null)
        {
            Name = name;
            GetValue = func;
            string checkboxTitle = "Check " + name;
            _checkbox = menu.CreateCheckbox(checkboxTitle);
            T[] array = GeneralUtils.ConvertToArray<T>();
            _itemList = menu.CreateList<T>(name, null, array);
            _itemList.Items = array.ToList();
            _itemList.SelectedItem = array[0];
            _failFunc = failCondition;
        }

        public bool IsTrue()
        {
            if (!_checkbox.Checked || _failFunc?.Invoke() == true) return true;
            return GetValue().Equals(_itemList.SelectedItem);
        }

        public bool HasValue() => _checkbox.Checked;

        public void ToXMLNode(XElement parentNode)
        {
            if (!HasValue()) return;
            string name = Name.Replace(" ", "");
            XElement conditionNode = new XElement(name, GetValue().ToString());
            parentNode.Add(conditionNode);
        }

        public void FromXMLNode(XElement parentNode)
        {
            string name = Name.Replace(" ", "");
            XElement conditionNode = parentNode.Element(name);
            if (conditionNode == null) return;
            _checkbox.Checked = true;
            _itemList.SelectedItem = (T)Enum.Parse(typeof(T), conditionNode.Value);
        }
    }
}


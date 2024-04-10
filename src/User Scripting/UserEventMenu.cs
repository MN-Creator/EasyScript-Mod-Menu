﻿using LemonUI.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Utils;

namespace EasyScript.UserScripting
{
    internal class UserEventMenu : Submenu
    {
        public readonly Dictionary<GameEvent, NativeCheckboxItem> EventCheckboxes =
            new Dictionary<GameEvent, NativeCheckboxItem>();
        private readonly UserScript _userScript;
        private static readonly List<GameEvent> _allEvents = GeneralUtils.ConvertToArray<GameEvent>().ToList();

        public UserEventMenu(MenuPool pool, NativeMenu parent, string title, UserScript userScript)
            : base(pool, parent, title)
        {
            _userScript = userScript;
            CreateCheckboxes();
            SubmenuItem.Description = "Select the events that will run the script.";
        }

        public void CreateCheckboxes()
        {
            foreach (GameEvent gameEvent in _allEvents)
            {
                NativeCheckboxItem checkbox = CreateCheckbox(gameEvent.ToString(), OnCheckboxChanged);
                EventCheckboxes.Add(gameEvent, checkbox);
            }
        }

        private void OnCheckboxChanged(object sender, EventArgs e)
        {
            var checkbox = (NativeCheckboxItem)sender;
            string eventName = checkbox.Title;
            if (Enum.TryParse(eventName, out GameEvent gameEvent))
            {
                if (_allEvents.Contains(gameEvent))
                {
                    if (checkbox.Checked)
                    {
                        Pool.GameEvents.Subscribe(gameEvent, OnEvent);
                        return;
                    }
                    Pool.GameEvents.Unsubscribe(gameEvent, OnEvent);
                }
            }
        }

        private void OnEvent(object sender, EventArgs e)
        {
            if (!_userScript.Enabled) return;
            _userScript.Run();
        }

        public void FromXMLNode(XElement scriptNode)
        {
            XElement eventNode = scriptNode.Element("Events");
            if (eventNode == null) return;

            foreach (XElement checkboxNode in eventNode.Elements())
            {
                string eventName = checkboxNode.Name.LocalName;
                if (Enum.TryParse(eventName, out GameEvent gameEvent))
                {
                    bool value = bool.Parse(checkboxNode.Value);
                    if (value)
                    {
                        EventCheckboxes[gameEvent].Checked = value;
                    }
                }
            }
        }

        public void ToXMLNode(XElement scriptNode)
        {
            var eventNode = new XElement("Events");
            foreach (KeyValuePair<GameEvent, NativeCheckboxItem> checkbox in EventCheckboxes)
            {
                if (!checkbox.Value.Checked) continue;
                eventNode.Add(new XElement(checkbox.Key.ToString(), checkbox.Value.Checked));
            }
            if (eventNode.IsEmpty) return;
            scriptNode.Add(eventNode);
        }
    }
}

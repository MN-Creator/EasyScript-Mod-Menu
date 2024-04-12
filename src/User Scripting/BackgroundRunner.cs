using GTA;
using LemonUI.Menus;
using System;
using System.Xml.Linq;

namespace EasyScript.UserScripting
{
    internal class BackgroundRunner : Submenu, IUpdate
    {
        public bool IsActive => _updateTimer.IsActive;
        private readonly UserScript _userScript;
        private readonly Timer _updateTimer;
        private static readonly float[] _intervals = { 0f, 0.5f, 1f, 2f, 5f, 10f, 30f, 60f };
        private readonly NativeCheckboxItem _enabledCheckbox;
        private readonly NativeCheckboxItem _ignoreConditionsCheckbox;
        private readonly NativeListItem<float> _intervalListItem;

        public BackgroundRunner(MenuPool pool, NativeMenu parent, string title, UserScript userScript) : base(pool, parent, title)
        {
            SubmenuItem.Description = "Run the script in the background at a set interval.";
            _enabledCheckbox = CreateCheckbox("Run in Background", (a, o) => _updateTimer.ToggleTimerActive());
            _ignoreConditionsCheckbox = CreateCheckbox("Ignore Conditions");
            _intervalListItem = CreateList("Interval (seconds)", ChangeUpdateInterval, 1f, _intervals);
            _userScript = userScript;
            _updateTimer = new Timer(1f, isLooping: true);
            _updateTimer.Stop();
            _updateTimer.OnTimeReached += OnUpdate;
        }

        private void ChangeUpdateInterval(object sender, ItemChangedEventArgs<float> e)
        {
            _updateTimer.Interval = e.Object;
            _updateTimer.Reset();
        }

        private void OnUpdate(object sender, EventArgs e)
        {
            _userScript.Run();
        }

        public void ToXMLNode(XElement parentNode)
        {
            if (!_updateTimer.IsActive) return;
            XElement backgroundRunnerNode = new XElement("BackgroundRunner");
            backgroundRunnerNode.Add(new XAttribute("UpdateInterval", _updateTimer.Interval));
            parentNode.Add(backgroundRunnerNode);
        }

        public void FromXMLNode(XElement parentNode)
        {
            XElement backgroundRunnerNode = parentNode.Element("BackgroundRunner");
            if (backgroundRunnerNode == null) return;
            float updateInterval = XMLUtils.GetFloat(backgroundRunnerNode.Attribute("UpdateInterval"), -1);
            if (updateInterval == -1) return;
            _intervalListItem.SelectedItem = updateInterval;
            _enabledCheckbox.Checked = true;
        }

        private void RunScript()
        {
            if (_ignoreConditionsCheckbox.Checked)
            {
                _userScript.ForceRun();
                return;
            }
            _userScript.Run();
        }

        public void Update()
        {
            if (!_userScript.Enabled || !_updateTimer.IsActive || !Game.Player.IsAlive) return;
            if (_updateTimer.Interval == 0f)
            {
                RunScript();
                return;
            }
            _updateTimer.Update();
        }
    }
}

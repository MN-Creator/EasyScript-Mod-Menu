using System;

namespace EasyScript
{
    internal class NumberProperty : Property
    {
        public string Unit
        {
            get => _unit;
            set
            {
                _unit = value;
                if (_unit != string.Empty)
                {
                    _showUnit = true;
                }
            }
        }

        private int _currentValue;
        private string _unit;
        private string UnitText => $"{_currentValue} {Unit}";

        private bool _showUnit = false;

        private readonly Func<int> _valueFunc;
        // Will not get value if failFunc returns true.
        private readonly Func<bool> _failCondition;

        public NumberProperty(string name, int value, string unit = null) : base(name, value.ToString())
        {
            NameText = name;
            string text = null;
            if (unit != null)
            {
                Unit = unit;
                text = UnitText;
            }
            _currentValue = value;
            ValueText = text ?? value.ToString();
        }

        public NumberProperty(string name, Func<int> valueFunc, Func<bool> failCondition = null, string unit = null) : base(name, string.Empty)
        {
            NameText = name;
            _valueFunc = valueFunc;
            _failCondition = failCondition;
            if (unit != null)
            {
                Unit = unit;
            }
        }

        private void UpdateText()
        {
            if (_showUnit)
            {
                ValueText = UnitText;
                return;
            }

            ValueText = _currentValue.ToString();
        }

        private void UpdateValue()
        {
            if (_valueFunc == null) return;
            if (_failCondition != null && _failCondition())
            {
                if (ValueText != string.Empty)
                {
                    ValueText = string.Empty;
                }
                return;
            }
            int value = _valueFunc();
            if (value == _currentValue) return;
            _currentValue = value;
            UpdateText();
        }

        public override void Draw()
        {
            UpdateValue();
            base.Draw();
        }
    }
}

using System;

namespace EasyScript
{
    internal class FloatProperty : Property
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
        private float _currentValue;
        private string _unit = string.Empty;
        private string _format = null;
        private string UnitText => $"{_currentValue} {Unit}";

        private bool _showUnit = false;

        private readonly Func<float> _valueFunc;
        // Will not get value if failFunc returns true.
        private readonly Func<bool> _failCondition;

        public FloatProperty(string name, float value, string unit = null, string format = null) : base(name, value.ToString())
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
            _format = format;
        }

        public FloatProperty(string name, Func<float> valueFunc, Func<bool> failCondition = null, string unit = null, string format = null)
            : base(name, string.Empty)
        {
            NameText = name;
            _valueFunc = valueFunc;
            _failCondition = failCondition;
            if (unit != null)
            {
                Unit = unit;
            }
            _format = format;
        }

        private void UpdateText()
        {
            if (_showUnit)
            {
                if (_format != null)
                {
                    ValueText = $"{_currentValue.ToString(_format)} {Unit}";
                    return;
                }
                ValueText = UnitText;
                return;
            }
            if (_format != null)
            {
                ValueText = _currentValue.ToString(_format);
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
            float value = _valueFunc();
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

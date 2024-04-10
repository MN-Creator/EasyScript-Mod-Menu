using System;

namespace EasyScript
{
    internal class TextProperty : Property
    {
        private readonly Func<string> _valueFunc;
        // Will not get value if failFunc returns true.
        private readonly Func<bool> _failCondition;

        public TextProperty(string name, Func<string> valueFunc, Func<bool> failCondition = null) : base(name, string.Empty)
        {
            NameText = name;
            _valueFunc = valueFunc;
            _failCondition = failCondition;
        }

        private void UpdateValue()
        {
            if (_valueFunc == null) return;
            if (_failCondition != null && _failCondition())
            {
                ValueText = "";
                Visible = false;
                return;
            }
            Visible = true;
            string value = _valueFunc() ?? "";
            if (value == ValueText) return;
            ValueText = value;
        }

        public override void Draw()
        {
            UpdateValue();
            base.Draw();
        }
    }
}

using LemonUI.Menus;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace EasyScript
{
    internal class PropertyPanel : NativePanel
    {
        private List<Property> _fields = new List<Property>();
        private PointF _lastPosition = PointF.Empty;
        // Display panel if function returns true.
        private Func<bool> _displayCondition;

        /// <param name="displayCondition">Draw panel if function returns true or if it's null.</param>
        public PropertyPanel(Func<bool> displayCondition = null)
        {
            _displayCondition = displayCondition;
            Background.Color = Color.FromArgb(180, 0, 0, 0);
        }


        public Property Add(string name, string value)
        {
            var data = new Property(name, value);
            _fields.Add(data);
            return data;
        }

        public void Add(string name, int value)
        {
            Add(name, value.ToString());
        }

        public TextProperty Add(string name, Func<string> valueFunc, Func<bool> failCondition = null)
        {
            var data = new TextProperty(name, valueFunc, failCondition);
            _fields.Add(data);
            return data;
        }

        public void Add(TextProperty data)
        {
            if (_fields.Contains(data)) return;
            _fields.Add(data);
        }

        public FloatProperty Add(string name, float value, string unit = null)
        {
            var field = new FloatProperty(name, value, unit);
            _fields.Add(field);
            return field;
        }

        public FloatProperty Add(string name, Func<float> valueFunc, Func<bool> failCondition = null, string unit = null, string format = null)
        {
            var field = new FloatProperty(name, valueFunc, failCondition, unit, format);
            _fields.Add(field);
            return field;
        }

        public NumberProperty Add(string name, int value, string unit = null)
        {
            var field = new NumberProperty(name, value, unit);
            _fields.Add(field);
            return field;
        }

        public NumberProperty Add(string name, Func<int> valueFunc, Func<bool> failCondition = null, string unit = null)
        {
            var field = new NumberProperty(name, valueFunc, failCondition, unit);
            _fields.Add(field);
            return field;
        }

        public void Remove(TextProperty data)
        {
            if (!_fields.Contains(data)) return;
            _fields.Remove(data);
            Recalculate();
        }

        public void Clear()
        {
            _fields.Clear();
            Recalculate();
        }

        public void Recalculate()
        {
            Recalculate(_lastPosition, 0f);
        }

        public override void Recalculate(PointF position, float width)
        {
            base.Recalculate(position, width);
            int activeFields = _fields.Count(x => x.Visible);
            if (activeFields == 0)
            {
                Background.Size = new SizeF(0f, 0f);
                return;
            }
            Background.Size = new SizeF(width, 7f + 38f * activeFields);
            for (int i = 0; i < _fields.Count; i++)
            {
                if (_fields[i].Visible == false) continue;
                PointF position2 = new PointF(position.X, position.Y + 7f + 38f * i);
                _fields[i].Recalculate(position2, width);
            }
        }

        public override void Process()
        {
            if (_displayCondition?.Invoke() == false) return;
            base.Process();
            foreach (var item in _fields)
            {
                if (item.Visible == false) continue;
                item.Draw();
            }
        }
    }
}

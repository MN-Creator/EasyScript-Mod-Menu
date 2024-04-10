using GTA.UI;
using LemonUI;
using LemonUI.Elements;
using System.Drawing;

namespace EasyScript
{
    internal class Property
    {
        private readonly ScaledText _nameText;
        private readonly ScaledText _valueText;
        protected static readonly Color DefaultNameColor = Color.FromArgb(255, 255, 255, 255);
        protected static readonly Color DefaultValueColor = Color.LightGreen;
        public bool Visible = true;

        public string NameText
        {
            get => _nameText.Text;
            set
            {
                _nameText.Text = value;
            }
        }

        protected string ValueText
        {
            get => _valueText.Text;
            set
            {
                _valueText.Text = value;
            }
        }

        public Color TitleColor
        {
            get => _nameText.Color;
            set => _nameText.Color = value;
        }

        public Color ValueColor
        {
            get => _valueText.Color;
            set => _valueText.Color = value;
        }

        public Property(string name, string value = "")
        {
            _nameText = new ScaledText(PointF.Empty, name, 0.3f);
            _valueText = new ScaledText(PointF.Empty, value, 0.3f);
            _valueText.Alignment = Alignment.Right;
            _nameText.Color = DefaultNameColor;
            _valueText.Color = DefaultValueColor;
        }

        public void Recalculate(PointF position, float width)
        {
            _nameText.Position = new PointF(position.X + 9f, position.Y);
            _valueText.Position = new PointF(position.X + width - 9f, position.Y);
        }

        public virtual void Draw()
        {
            _nameText.Draw();
            _valueText.Draw();
        }
    }
}

using GTA;
using GTA.UI;
using LemonUI.Menus;
using System;
using System.Collections.Generic;
using System.Drawing;
using Utils;

namespace EasyScript.Menus
{
    internal class SpeedometerMenu : Submenu, IUpdate
    {
        public enum Placement
        {
            LowerRight,
            LowerLeft,
            TopRight,
        }
        public enum Colors
        {
            White,
            Aqua,
            Blue,
            LightGreen,
            Green,
            Yellow,
            Orange,
            Red,
            MediumPurple,
            Purple,
            Pink,
            HotPink,
            Black,
        }

        private static readonly GTA.UI.Font[] _fonts =
        {
            GTA.UI.Font.ChaletComprimeCologne,
            GTA.UI.Font.ChaletLondon,
            GTA.UI.Font.HouseScript,
            GTA.UI.Font.Pricedown,
        };

        private static readonly Dictionary<GTA.UI.Font, float> _fontLeftOffset = new Dictionary<GTA.UI.Font, float>
        {
            { GTA.UI.Font.ChaletComprimeCologne, 0 },
            { GTA.UI.Font.ChaletLondon, 13f },
            { GTA.UI.Font.HouseScript, 5f },
            { GTA.UI.Font.Pricedown, 13f },
        };
        private static readonly Dictionary<GTA.UI.Font, float> _fontTopOffset = new Dictionary<GTA.UI.Font, float>
        {
            { GTA.UI.Font.ChaletComprimeCologne, 0 },
            { GTA.UI.Font.ChaletLondon, 0 },
            { GTA.UI.Font.HouseScript, 3 },
            { GTA.UI.Font.Pricedown, 0 },
        };
        private readonly NativeCheckboxItem _showSpeedometerCheckbox;
        private readonly NativeListItem<Placement> _placementList;
        private readonly NativeListItem<Colors> _colorList;
        private readonly NativeListItem<GTA.UI.Font> _fontList;
        private readonly NativeCheckboxItem _characterColorsCheckbox;
        readonly TextElement _speedometerText;
        readonly TextElement _zeroToSixtyText;
        private readonly bool _hideWhenStopped = true;
        private bool _isSpeedoHidden = false;
        private readonly Timer _hideSpeedoTimer = new Timer(3, isLooping: true);
        private readonly Timer _zeroToSixtytimer = new Timer();
        private readonly Timer _displayZeroToSixtyTextTimer = new Timer(3, isLooping: false);
        private bool _isZeroToSixtyEnabled;
        private float _lastSpeed;

        public Vehicle CurrentVehicle => Game.Player.Character.CurrentVehicle;

        public SpeedometerMenu(MenuPool pool, NativeMenu parent, string title) : base(pool, parent, title)
        {
            PointF position = UIUtils.ScalePositionToScreen(0.96f, 0.96f);
            GTA.UI.Font font = SettingsManager.GetValue("Speedometer", "font", _fonts[0]);
            Color color = Color.LightGreen;
            Alignment alignment = Alignment.Center;
            _speedometerText = new TextElement("0", position, 0.5f, color, font, alignment);
            _zeroToSixtyText = new TextElement("", position, 0.5f, Color.Yellow, font, alignment);

            _showSpeedometerCheckbox = CreateCheckbox("Show Speedometer", OnShowSpeedoChanged);
            _showSpeedometerCheckbox.Checked = SettingsManager.GetValue("Speedometer", "visible", true);
            _placementList = CreateList("Placement", OnPlacementChanged, GeneralUtils.ConvertToArray<Placement>());
            _placementList.SelectedItem = SettingsManager.GetValue("Speedometer", "placement", Placement.LowerRight);

            _colorList = CreateList("Color", OnColorChanged, GeneralUtils.ConvertToArray<Colors>());
            _colorList.SelectedItem = SettingsManager.GetValue("Speedometer", "color", Colors.White);
            _characterColorsCheckbox = CreateCheckbox("Use Character Colors", OnCharacterColorCheckboxChanged);
            _characterColorsCheckbox.Checked = SettingsManager.GetValue("Speedometer", "character_colors", false);
            _fontList = CreateList("Font", OnFontChanged, _fonts);
            _fontList.SelectedItem = SettingsManager.GetValue("Speedometer", "font", _fonts[0]);
            CreateCheckbox("Text Outline", OnTextOutlineChanged);

            _hideSpeedoTimer.OnTimeReached += (sender, e) =>
            {
                _isSpeedoHidden = true;
            };
            _displayZeroToSixtyTextTimer.Stop();
            SettingsManager.Save();
            Menu.Closed += (a, o) => SettingsManager.Save();
        }

        private void OnShowSpeedoChanged(object sender, EventArgs e)
        {
            SettingsManager.SetValue("Speedometer", "visible", _showSpeedometerCheckbox.Checked);
        }

        private void OnCharacterColorCheckboxChanged(object sender, EventArgs e)
        {
            if (_characterColorsCheckbox.Checked)
            {
                _colorList.Enabled = false;
                _zeroToSixtyText.Color = Color.Yellow;
                SettingsManager.SetValue("Speedometer", "character_colors", true);
                return;
            }
            _colorList.Enabled = true;
            _colorList.SelectedItem = _colorList.SelectedItem;
        }

        private void OnTextOutlineChanged(bool value)
        {
            _speedometerText.Outline = value;
            _zeroToSixtyText.Outline = value;
            SettingsManager.SetValue("Speedometer", "text_outline", value);
        }

        private void OnPlacementChanged(object sender, ItemChangedEventArgs<Placement> e)
        {
            SettingsManager.SetValue("Speedometer", "placement", e.Object);
        }

        private void OnFontChanged(object sender, ItemChangedEventArgs<GTA.UI.Font> e)
        {
            _speedometerText.Font = e.Object;
            _zeroToSixtyText.Font = e.Object;
            SettingsManager.SetValue("Speedometer", "font", e.Object);
        }

        private void OnColorChanged(object sender, ItemChangedEventArgs<Colors> e)
        {
            Color color = Color.FromName(e.Object.ToString());
            if (color == null) return;
            _speedometerText.Color = color;
            SettingsManager.SetValue("Speedometer", "color", e.Object);
            _zeroToSixtyText.Color = Color.Yellow;
            if (color == Color.Yellow || color == Color.LightYellow)
            {
                _zeroToSixtyText.Color = Color.LightGreen;
            }
        }

        public PointF GetPosition()
        {
            switch (_placementList.SelectedItem)
            {
                case Placement.LowerRight:
                    return GetLowerRightPosition();
                case Placement.LowerLeft:
                    return GetLowerLeftPosition();
                default:
                    return GetTopRightPosition();
            }
        }

        private PointF GetLowerRightPosition()
        {
            if ((UIUtils.IsAreaNameVisible() && UIUtils.IsStreetNameVisible()) || Pool.IsMenuVisible)
            {
                return UIUtils.ScalePositionToScreen(0.96f, 0.895f);
            }
            else if (UIUtils.IsStreetNameVisible())
            {
                return UIUtils.ScalePositionToScreen(0.96f, 0.93f);
            }
            return UIUtils.ScalePositionToScreen(0.96f, 0.96f);
        }

        private PointF GetLowerLeftPosition()
        {
            float offsetX = _fontLeftOffset[_speedometerText.Font];
            PointF pos;
            if (Hud.IsRadarVisible)
            {
                float offsetY = _fontTopOffset[_speedometerText.Font];
                pos = UIUtils.ScalePositionToScreen(0.18f, 0.955f);
                pos.X += offsetX;
                pos.Y += offsetY;
                return pos;
            }

            pos = UIUtils.ScalePositionToScreen(0.025f, 0.97f);
            pos.X += offsetX;
            return pos;
        }

        private PointF GetTopRightPosition()
        {
            return UIUtils.ScalePositionToScreen(0.96f, 0.05f);
        }

        private void UpdateSpeedometer()
        {
            string unit = GetUnit();
            float speed = ConvertMetersPerSecondToUnitSpeed(CurrentVehicle.Speed);
            PointF speedoPos = GetPosition();
            _speedometerText.Position = speedoPos;

            if (_displayZeroToSixtyTextTimer.IsActive)
            {
                _zeroToSixtyText.Position = new PointF(speedoPos.X, speedoPos.Y - 20f);
                _zeroToSixtyText.Draw();
            }

            string speedText = speed.ToString("0");
            _speedometerText.Caption = $"{speedText} {unit}";
            _speedometerText.Draw();
        }

        private string GetUnit()
        {
            if (Game.MeasurementSystem == MeasurementSystem.Metric)
            {
                return "km/h";
            }
            return "mph";
        }

        private float ConvertMetersPerSecondToUnitSpeed(float speed)
        {
            if (Game.MeasurementSystem == MeasurementSystem.Metric)
            {
                return speed * 3.6f;
            }
            return speed * 2.23694f;
        }

        private void UpdateZeroToSixty()
        {
            if (CurrentVehicle.IsAircraft) return;
            _displayZeroToSixtyTextTimer.Update();
            if (CurrentVehicle.Speed == 0)
            {
                _isZeroToSixtyEnabled = true;
                _lastSpeed = 0;
                _zeroToSixtytimer.Reset();
                _zeroToSixtytimer.Start();
            }
            else if (_isZeroToSixtyEnabled && CurrentVehicle.Speed > 0)
            {
                float speedChange = CurrentVehicle.Speed - _lastSpeed;
                _lastSpeed = CurrentVehicle.Speed;


                if (Game.IsControlPressed(Control.VehicleAccelerate) || speedChange > 0f)
                {
                    _zeroToSixtytimer.Update();
                    float sixtyMphInMs = 27.78f;
                    if (CurrentVehicle.Speed >= sixtyMphInMs)
                    {
                        _zeroToSixtyText.Caption = _zeroToSixtytimer.Elapsed.ToString("0.00") + " s";
                        _displayZeroToSixtyTextTimer.Reset();
                        _displayZeroToSixtyTextTimer.Start();
                        _zeroToSixtytimer.Reset();
                        _zeroToSixtytimer.Stop();
                        _isZeroToSixtyEnabled = false;
                    }
                }
                else
                {
                    _zeroToSixtytimer.Reset();
                    _zeroToSixtytimer.Stop();
                    _isZeroToSixtyEnabled = false;
                }
            }
        }

        private void UpdateHideWhenStopped()
        {
            if (_hideWhenStopped)
            {
                _hideSpeedoTimer.Update();
                if (CurrentVehicle.Speed > 0f)
                {
                    _hideSpeedoTimer.Reset();
                    _isSpeedoHidden = false;
                }
            }
        }

        private void UpdateColorForCharacter()
        {
            if (!_characterColorsCheckbox.Checked) return;
            Color color = MenuColors.GetColorForCurrentCharacter();
            if (_speedometerText.Color != color)
            {
                _speedometerText.Color = color;
            }
        }

        public void Update()
        {
            if (!PlayerPed.IsInVehicle() || Game.IsCutsceneActive) return;
            if (Screen.IsEffectActive(ScreenEffect.SwitchSceneNeutral)) return;
            UpdateZeroToSixty();
            UpdateHideWhenStopped();
            if (_isSpeedoHidden && !Menu.Visible) return;

            if (_showSpeedometerCheckbox.Checked)
            {
                UpdateColorForCharacter();
                UpdateSpeedometer();
            }
        }
    }
}

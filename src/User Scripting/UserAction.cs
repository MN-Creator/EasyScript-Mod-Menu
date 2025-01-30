using GTA;
using System;
using System.Linq;

namespace EasyScript.UserScripting
{
    public interface IActionParameter
    {
        string Parameter { get; set; }
        bool IsValid(string value);
    }

    public abstract class UserAction
    {
        public string Name { get; }
        public string Description { get; protected set; }

        public UserAction(string name)
        {
            Name = name;
            Description = "";
        }

        public UserAction(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public abstract void Execute();
    }

    public class SimpleAction : UserAction
    {
        private readonly Action _action;


        public SimpleAction(string name, Action action) : base(name)
        {
            _action = action;
        }

        public SimpleAction(string name, string description, Action action) : base(name, description)
        {
            _action = action;
        }

        public override void Execute()
        {
            _action?.Invoke();
        }
    }

    public class TextAction : UserAction, IActionParameter
    {
        public string Parameter
        {
            get => _parameter;
            set
            {
                if (value != _parameter)
                {
                    _parameter = value;
                }
            }
        }
        private string _parameter;
        private readonly Action<string> _action;

        public TextAction(string name, Action<string> action, string defaultText = "") : base(name)
        {
            _action = action;
            _parameter = defaultText;
            Description = "Text can be changed.";
        }

        public TextAction(string name, string description, Action<string> action, string defaultText = "") : base(name, description)
        {
            _action = action;
            _parameter = defaultText;
        }

        public override void Execute()
        {
            _action?.Invoke(_parameter);
        }

        public bool IsValid(string value)
        {
            return true;
        }
    }

    public class NumberAction : UserAction, IActionParameter
    {
        public string Parameter
        {
            get => _parameter;
            set
            {
                if (value != _parameter && IsValid(value))
                {
                    _parameter = value;
                    UpdateNumber();
                }
            }
        }
        private int _number;
        private string _parameter;
        private readonly Action<int> _action;
        private readonly int _min = 0;
        private readonly int _max = 100;

        public NumberAction(string name, Action<int> action, int min = 0, int max = 100) : base(name)
        {
            _action = action;
            _parameter = _number.ToString();
            _min = min;
            _max = max;
        }

        public NumberAction(string name, Action<int> action, int max) : base(name)
        {
            _action = action;
            _parameter = _number.ToString();
            _max = max;
        }

        public NumberAction(string name, string description, Action<int> action, int min = 0, int max = 100) : base(name, description)
        {
            _action = action;
            _parameter = _number.ToString();
            _min = min;
            _max = max;
        }

        private void UpdateNumber()
        {
            if (int.TryParse(_parameter, out int number))
            {
                if (number < _min || number > _max)
                {
                    _number = Math.Min(_max, Math.Max(_min, number));
                    _parameter = _number.ToString();
                    return;
                }
                _number = number;
            }
        }

        public override void Execute()
        {
            _action?.Invoke(_number);
        }

        public bool IsValid(string value)
        {
            if (int.TryParse(value, out int _))
            {
                return true;
            }
            return false;
        }
    }

    public class FloatAction : UserAction, IActionParameter
    {
        public string Parameter
        {
            get => _parameter;
            set
            {
                if (value != _parameter && IsValid(value))
                {
                    _parameter = value;
                    UpdateFloat();
                }
            }
        }
        private float _float;
        private string _parameter;
        private readonly Action<float> _action;
        private readonly float _min = 0f;
        private readonly float _max = 100f;

        public FloatAction(string name, Action<float> action, float min = 0f, float max = 100f) : base(name)
        {
            _action = action;
            _parameter = _float.ToString();
            _min = min;
            _max = max;
        }

        public FloatAction(string name, Action<float> action, float max) : base(name)
        {
            _action = action;
            _parameter = _float.ToString();
            _max = max;
        }

        public FloatAction(string name, string description, Action<float> action, float min = 0f, float max = 100f) : base(name, description)
        {
            _action = action;
            _parameter = _float.ToString();
            _min = min;
            _max = max;
        }

        private void UpdateFloat()
        {
            if (float.TryParse(_parameter, out float number))
            {
                if (number < _min || number > _max)
                {
                    _float = Math.Min(_max, Math.Max(_min, number));
                    _parameter = _float.ToString();
                    return;
                }
                _float = number;
            }
        }

        public override void Execute()
        {
            _action?.Invoke(_float);
        }

        public bool IsValid(string value)
        {
            if (float.TryParse(value, out float _))
            {
                return true;
            }
            return false;
        }
    }

    public class VehicleAction : UserAction, IActionParameter
    {
        public string Parameter
        {
            get => Enum.GetName(typeof(VehicleHash), _vehicle);
            set
            {
                if (value != _parameter)
                {
                    _parameter = value;
                    UpdateVehicle();
                }
            }
        }
        private VehicleHash _vehicle = VehicleHash.Adder;
        private string _parameter;
        private readonly Action<VehicleHash> _action;

        public VehicleAction(string name, Action<VehicleHash> action) : base(name)
        {
            _action = action;
            _parameter = _vehicle.ToString();
        }

        public VehicleAction(string name, string description, Action<VehicleHash> action) : base(name, description)
        {
            _action = action;
            _parameter = _vehicle.ToString();
        }

        private void UpdateVehicle()
        {
            if (Enum.TryParse(_parameter, out VehicleHash vehicleHash))
            {
                _vehicle = vehicleHash;
            }
        }

        public override void Execute()
        {
            _action?.Invoke(_vehicle);
        }

        public bool IsValid(string value)
        {
            if (Enum.TryParse(value, out VehicleHash _))
            {
                return true;
            }
            return false;
        }
    }

    public class EnumAction<T> : UserAction, IActionParameter where T : struct, Enum
    {
        public string Parameter
        {
            get
            {
                return Enum.GetName(typeof(T), _parameter);
            }
            set
            {
                if (value != _parameterText)
                {
                    _parameterText = value;
                    UpdateEnum(value);
                }
            }
        }
        private T _parameter;
        private string _parameterText;
        private readonly Action<T> _action;

        public EnumAction(string name, Action<T> action) : base(name)
        {
            _action = action;
            _parameter = Enum.GetValues(typeof(T)).Cast<T>().FirstOrDefault();
        }

        public EnumAction(string name, Action<T> action, T defaultValue) : base(name)
        {
            _action = action;
            _parameter = defaultValue;
        }

        public EnumAction(string name, string description, Action<T> action) : base(name, description)
        {
            _action = action;
            _parameter = default;
        }

        private void UpdateEnum(string value)
        {
            if (Enum.TryParse(value, out T result))
            {
                _parameter = result;
            }
        }

        public override void Execute()
        {
            _action?.Invoke(_parameter);
        }

        public bool IsValid(string value)
        {
            return Enum.TryParse(value, out T _);
        }
    }
}

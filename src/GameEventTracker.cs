using GTA;
using System;

namespace EasyScript
{
    public enum GameEvent
    {
        MissionStarted,
        MissionEnded,
        WantedLevelGained,
        WantedLevelLost,
        PlayerRespawned,
        EnteredVehicle,
        LeftVehicle
    }

    public class GameEventTracker : IUpdate
    {
        public PropertyStructTracker<int> WantedLevel = CreateValueTracker(() => Game.Player.WantedLevel);
        public PropertyClassTracker<Vehicle> Vehicle = CreateClassTracker(() => Game.Player.Character.CurrentVehicle);
        public PropertyClassTracker<Weapon> Weapon = CreateClassTracker(() => Game.Player.Character.Weapons.Current);
        public PropertyStructTracker<bool> Waypoint = CreateValueTracker(() => Game.IsWaypointActive);

        public event EventHandler MissionStarted;
        public event EventHandler MissionEnded;
        public event EventHandler WantedLevelGained;
        public event EventHandler WantedLevelLost;
        public event EventHandler PlayerDied;
        public event EventHandler PlayerRespawned;
        public event EventHandler EnteredVehicle;
        public event EventHandler LeftVehicle;
        public event EventHandler WaypointSet;
        public event EventHandler WaypointRemoved;

        private readonly Timer _respawnTimer;
        private bool _isPlayerDead;
        private readonly PropertyStructTracker<bool> _missionActive = CreateValueTracker(() => Game.IsMissionActive);

        public GameEventTracker()
        {
            _respawnTimer = new Timer(10);
            _respawnTimer.Stop();
            _missionActive.ValueChanged += OnMissionStateChanged;
            WantedLevel.ValueChanged += OnWantedLevelChanged;
            Vehicle.ValueChanged += OnVehicleStateChanged;
            Waypoint.ValueChanged += OnWaypointChanged;
        }

        public void Subscribe(GameEvent name, EventHandler func)
        {
            switch (name)
            {
                case GameEvent.MissionStarted:
                    MissionStarted += func;
                    break;
                case GameEvent.MissionEnded:
                    MissionEnded += func;
                    break;
                case GameEvent.WantedLevelGained:
                    WantedLevelGained += func;
                    break;
                case GameEvent.WantedLevelLost:
                    WantedLevelLost += func;
                    break;
                case GameEvent.PlayerRespawned:
                    PlayerRespawned += func;
                    break;
                case GameEvent.EnteredVehicle:
                    EnteredVehicle += func;
                    break;
                case GameEvent.LeftVehicle:
                    LeftVehicle += func;
                    break;
            }
        }

        public void Unsubscribe(GameEvent name, EventHandler func)
        {
            switch (name)
            {
                case GameEvent.MissionStarted:
                    MissionStarted -= func;
                    break;
                case GameEvent.MissionEnded:
                    MissionEnded -= func;
                    break;
                case GameEvent.WantedLevelGained:
                    WantedLevelGained -= func;
                    break;
                case GameEvent.WantedLevelLost:
                    WantedLevelLost -= func;
                    break;
                case GameEvent.PlayerRespawned:
                    PlayerRespawned -= func;
                    break;
                case GameEvent.EnteredVehicle:
                    EnteredVehicle -= func;
                    break;
                case GameEvent.LeftVehicle:
                    LeftVehicle -= func;
                    break;
            }
        }

        private void OnVehicleStateChanged(object sender, Vehicle e)
        {
            if (e == null)
            {
                LeftVehicle?.Invoke(this, EventArgs.Empty);
                return;
            }
            EnteredVehicle?.Invoke(this, EventArgs.Empty);
        }

        private void OnWantedLevelChanged(object sender, int value)
        {
            if (value == 0)
            {
                WantedLevelLost?.Invoke(this, EventArgs.Empty);
                return;
            }
            WantedLevelGained?.Invoke(this, EventArgs.Empty);
        }

        private void OnMissionStateChanged(object sender, bool value)
        {
            if (value)
            {
                MissionStarted?.Invoke(this, EventArgs.Empty);
                return;
            }
            MissionEnded?.Invoke(this, EventArgs.Empty);
        }

        private void OnWaypointChanged(object sender, bool value)
        {
            GTA.UI.Screen.ShowSubtitle("Waypoint changed");
            if (value)
            {
                WaypointSet?.Invoke(this, EventArgs.Empty);
                return;
            }
            WaypointRemoved?.Invoke(this, EventArgs.Empty);
        }

        public static PropertyClassTracker<T> CreateClassTracker<T>(Func<T> getValue) where T : class
        {
            return new PropertyClassTracker<T>(getValue);
        }

        public static PropertyStructTracker<T> CreateValueTracker<T>(Func<T> getValue) where T : struct
        {
            return new PropertyStructTracker<T>(getValue);
        }

        public void Update()
        {
            WantedLevel.Tick();
            Vehicle.Tick();
            Weapon.Tick();
            Waypoint.Tick();
            _respawnTimer.Update();
            _missionActive.Tick();
            if (Game.Player.Character.IsDead && !_isPlayerDead)
            {
                _isPlayerDead = true;
                _respawnTimer.Start();
                PlayerDied?.Invoke(this, EventArgs.Empty);
            }
            else if (_isPlayerDead && !_respawnTimer.IsActive && Game.Player.Character.IsAlive)
            {
                _isPlayerDead = false;
                PlayerRespawned?.Invoke(this, EventArgs.Empty);
                _respawnTimer.Reset();
                _respawnTimer.Stop();
            }
        }
    }

    public class PropertyClassTracker<T> where T : class
    {
        public T LastValue;
        public event EventHandler<T> ValueChanged;
        private readonly Func<T> _getValue;

        public PropertyClassTracker(Func<T> getValue)
        {
            _getValue = getValue;
        }

        public void Tick()
        {
            T value = _getValue();

            if (!IsEqual(value, LastValue))
            {
                ValueChanged?.Invoke(this, value);
                LastValue = value;
            }
        }

        private bool IsEqual(T obj1, T obj2)
        {
            return obj1?.Equals(obj2) ?? obj2 == null;
        }
    }

    public class PropertyStructTracker<T> where T : struct
    {
        public T LastValue;
        public event EventHandler<T> ValueChanged;
        private readonly Func<T> _getValue;

        public PropertyStructTracker(Func<T> getValue)
        {
            _getValue = getValue;
        }

        public void Tick()
        {
            T value = _getValue();
            if (!value.Equals(LastValue))
            {
                ValueChanged?.Invoke(this, value);
                LastValue = value;
            }
        }
    }
}

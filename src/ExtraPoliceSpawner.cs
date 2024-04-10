using GTA;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Utils;

namespace EasyScript
{
    internal class ExtraPoliceSpawner : IUpdate
    {
        public int MaxPoliceVehicles = 3;
        public float SpawnInterval = 25f;
        public float SpawnRange = 200f;
        public bool IsSpawning { get; private set; }
        public int SpawnedPoliceVehiclesCount => _spawnedPoliceVehicles.Count;
        public int SpawnedPolicePedsCount => _spawnedPolicePeds.Count;
        private readonly List<Vehicle> _spawnedPoliceVehicles = new List<Vehicle>();
        private readonly List<Ped> _spawnedPolicePeds = new List<Ped>();
        private readonly PlayerActivity _playerActivity = new PlayerActivity();
        private Vehicle _spawnedHelicoper;
        private readonly Timer _spawnHelicopterTimer;
        private readonly Timer _animatePoliceLights;
        private readonly Dictionary<int, SpawnLevel> _wantedLevelToSpawnLevel = new Dictionary<int, SpawnLevel>(5);
        private readonly SpawnLevel _level3 = new SpawnLevel()
        {
            GroundVehicle = VehicleHash.Police4,
            PolicePed = PedHash.Cop01SMY,
            SpawnInterval = 30f,
            MaxGroundVehiclePassengers = 1,
        };
        private readonly SpawnLevel _level4 = new SpawnLevel()
        {
            GroundVehicle = VehicleHash.FBI,
            ArmoredGroundVehicle = VehicleHash.Baller5,
            Helicopter = VehicleHash.Annihilator,
            SpawnInterval = 30f,
        };
        private readonly SpawnLevel _level5 = new SpawnLevel()
        {
            GroundVehicle = VehicleHash.Granger2,
            ArmoredGroundVehicle = VehicleHash.Insurgent,
            PolicePed = PedHash.Swat01SMY,
            Helicopter = VehicleHash.Annihilator2,
            SpawnInterval = 30f,
        };
        private SpawnLevel _currentLevel;
        private float _spawnPoliceTimer = 0f;
        private int _lastFrameWantedLevel = 0;
        private float _checkForDestroyedVehiclesTimer = 0f;
        private readonly float _checkForDestroyedVehiclesInterval = 5f;
        private readonly int _minSpawnLevel;

        public Ped PlayerPed => Game.Player.Character;

        public ExtraPoliceSpawner()
        {
            _lastFrameWantedLevel = Game.Player.WantedLevel;
            _wantedLevelToSpawnLevel.Add(3, _level3);
            _wantedLevelToSpawnLevel.Add(4, _level4);
            _wantedLevelToSpawnLevel.Add(5, _level5);
            _minSpawnLevel = _wantedLevelToSpawnLevel.Keys.Min();
            _animatePoliceLights = new Timer(0.15f, AnimatePoliceLights, isLooping: true);
            _spawnHelicopterTimer = new Timer(30f, OnHelicopterTimerIntervalChange, isLooping: true);
            _spawnHelicopterTimer.Stop();
        }

        public void StartSpawning()
        {
            IsSpawning = true;
            _spawnPoliceTimer = 0f;
            if (Game.Player.WantedLevel >= _minSpawnLevel)
            {
                _currentLevel = _wantedLevelToSpawnLevel[Game.Player.WantedLevel];
                _spawnHelicopterTimer.Start();
            }
        }

        public void StopSpawning()
        {
            IsSpawning = false;
            RemoveVehiclesFromList(_spawnedPoliceVehicles);
            RemovePedsFromList(_spawnedPolicePeds);
            _spawnHelicopterTimer.Stop();
        }

        private void OnHelicopterTimerIntervalChange(object sender, EventArgs e)
        {
            if (!IsSpawning || !_currentLevel.HasHelicoper) return;
            if (_spawnedHelicoper == null || !_spawnedHelicoper.Exists() || _spawnedHelicoper.IsConsideredDestroyed)
            {
                Vehicle helicopter = SpawnHelicopter(_currentLevel);
                _spawnedHelicoper = helicopter;
            }
        }

        private Vehicle SpawnHelicopter(SpawnLevel level)
        {
            Vector3 spawnPos = PlayerPed.Position - PlayerPed.ForwardVector * 400f;
            spawnPos.Z += 100f;
            Vehicle helicopter = World.CreateVehicle(new Model(level.Helicopter), spawnPos);
            helicopter.IsEngineRunning = true;
            // Rotate towards player.
            helicopter.Heading = PlayerPed.Heading - 180;

            int passengerCapacity = helicopter.PassengerCapacity;
            for (int i = 0; i < passengerCapacity; i++)
            {
                Ped ped = helicopter.CreatePedOnSeat((VehicleSeat)i, new Model(level.PolicePed));
                ped.Weapons.Give(WeaponHash.CarbineRifle, 500, true, true);
                ped.Task.VehicleShootAtPed(PlayerPed);
                _spawnedPolicePeds.Add(ped);
            }
            Ped driver = helicopter.CreatePedOnSeat(VehicleSeat.Driver, new Model(level.PolicePed));
            driver.Weapons.Give(WeaponHash.Pistol, 50, true, true);
            driver.Task.ChaseWithHelicopter(PlayerPed, new Vector3(75f, 75f, 100f));
            _spawnedPolicePeds.Add(driver);
            return helicopter;
        }

        private void AnimatePoliceLights(object sender, EventArgs e)
        {
            foreach (Vehicle vehicle in _spawnedPoliceVehicles)
            {
                if (!vehicle.Model.IsEmergencyVehicle && vehicle.IsEngineRunning && vehicle.Mods.HasNeonLights)
                {
                    if (!vehicle.Mods.IsNeonLightsOn(VehicleNeonLight.Front))
                    {
                        VehicleUtils.EnableAllNeonLights(vehicle);
                    }
                    if (vehicle.Mods.NeonLightsColor.R == 255)
                    {
                        vehicle.Mods.NeonLightsColor = Color.Blue;
                        continue;
                    }
                    vehicle.Mods.NeonLightsColor = Color.Red;
                }
            }
        }

        private void RemoveVehiclesFromList(List<Vehicle> vehicles)
        {
            foreach (var vehicle in vehicles)
            {
                if (vehicle.Exists())
                {
                    vehicle.Delete();
                }
            }
            vehicles.Clear();
        }

        private void RemovePedsFromList(List<Ped> peds)
        {
            foreach (var ped in peds)
            {
                if (ped.Exists())
                {
                    ped.Delete();
                }
            }
            peds.Clear();
        }

        private VehicleHash DecideVehicle()
        {
            if (_currentLevel.HasArmoredGroundVehicle && _playerActivity.HasUsedExplosiveWeaponLately)
            {
                return _currentLevel.ArmoredGroundVehicle;
            }
            else
            {
                return _currentLevel.GroundVehicle;
            }
        }

        private void SpawnPolice()
        {
            Vector3 forward = PlayerPed.ForwardVector;

            Vector3 spawnPos = SpawnPointAroundPlayer();

            if (PlayerPed.IsInVehicle())
            {
                spawnPos = PlayerPed.CurrentVehicle.Position + forward * SpawnRange;
            }
            spawnPos = World.GetNextPositionOnStreet(spawnPos, true);
            VehicleHash vehicleHash = DecideVehicle();
            Vehicle vehicle = World.CreateVehicle(new Model(vehicleHash), spawnPos);
            vehicle.IsEngineRunning = true;
            vehicle.Heading = PlayerPed.Heading;
            UpgradeVehicle(vehicle);
            RotateTowardsPlayer(vehicle);

            SpawnPedsInVehicle(vehicle, _currentLevel.PolicePed);
            _spawnedPoliceVehicles.Add(vehicle);
        }

        private void SpawnPedsInVehicle(Vehicle vehicle, PedHash pedHash)
        {
            int passengerCount = vehicle.PassengerCapacity;
            if (_currentLevel.MaxGroundVehiclePassengers != -1)
            {
                passengerCount = _currentLevel.MaxGroundVehiclePassengers;
            }
            int worldMaxPeds = World.PedCapacity;
            // -1 is driver.
            for (int i = -1; i < passengerCount; i++)
            {
                if (World.PedCount == worldMaxPeds) return;
                Ped ped = vehicle.CreatePedOnSeat((VehicleSeat)i, new Model(pedHash));
                if (_currentLevel.HasSecondaryWeapon)
                {
                    ped.Weapons.Give(_currentLevel.SecondaryWeapon, 50, true, true);
                }
                if (_currentLevel.HasPrimaryWeapon)
                {
                    Weapon weapon = ped.Weapons.Give(_currentLevel.PrimaryWeapon, 150, true, true);
                    weapon.Components.GetScopeComponent(weapon.Components.ScopeVariationsCount - 1).Active = true;
                }
                _spawnedPolicePeds.Add(ped);
            }
        }

        private Vector3 SpawnPointAroundPlayer()
        {
            Random random = new Random();
            int num = random.Next(0, 4);
            Vector3 dir = PlayerPed.ForwardVector;
            // Spawn in front, behind, left, or right of player
            if (num == 1)
            {
                dir = PlayerPed.RightVector;
            }
            else if (num == 2)
            {
                dir = -PlayerPed.ForwardVector;
            }
            else if (num == 3)
            {
                dir = -PlayerPed.RightVector;
            }
            return PlayerPed.Position + dir * SpawnRange;
        }

        private void UpgradeVehicle(Vehicle vehicle)
        {
            vehicle.Mods.PrimaryColor = VehicleColor.MetallicBlack;
            vehicle.Mods.SecondaryColor = VehicleColor.MetallicBlack;
            vehicle.Mods.RimColor = VehicleColor.MetallicBlack;
            vehicle.Mods.PearlescentColor = VehicleColor.MetallicBlack;
            vehicle.Mods[VehicleModType.Engine].Index = 3;
            vehicle.Mods[VehicleModType.Transmission].Index = 3;
            vehicle.Mods[VehicleModType.Brakes].Index = 3;
            vehicle.Mods[VehicleModType.Suspension].Index = 3;
            vehicle.Mods[VehicleModType.Armor].Index = 4;
        }

        private void RotateTowardsPlayer(Vehicle vehicle)
        {
            Vector3 direction = PlayerPed.Position - vehicle.Position;
            direction.Normalize();
            Vector3 forward = vehicle.ForwardVector;
            forward.Normalize();
            float dot = Vector3.Dot(direction, forward);
            if (dot < 0.9f)
            {
                vehicle.Rotation = direction;
            }
        }

        private void RemoveDestroyedVehicles()
        {
            int count = _spawnedPoliceVehicles.Count;

            for (int i = count - 1; i >= 0; i--)
            {
                Vehicle vehicle = _spawnedPoliceVehicles[i];
                if (!vehicle.Exists() || vehicle.IsDead || vehicle.IsConsideredDestroyed)
                {
                    vehicle.MarkAsNoLongerNeeded();
                    _spawnedPoliceVehicles.RemoveAt(i);
                }
                else if (vehicle.Position.DistanceTo2D(PlayerPed.Position) > 2000f)
                {
                    if (vehicle.Passengers.Length > 0)
                    {
                        foreach (Ped ped in vehicle.Passengers)
                        {
                            _spawnedPolicePeds.Remove(ped);
                            ped.MarkAsNoLongerNeeded();
                            ped.Delete();
                        }
                    }
                    vehicle.MarkAsNoLongerNeeded();
                    vehicle.Delete();
                    _spawnedPoliceVehicles.RemoveAt(i);
                }
            }
        }

        private void OnWantedLevelChanged()
        {
            if (_wantedLevelToSpawnLevel.ContainsKey(Game.Player.WantedLevel))
            {
                _spawnPoliceTimer = 0f;
                _currentLevel = _wantedLevelToSpawnLevel[Game.Player.WantedLevel];
                SpawnInterval = _currentLevel.SpawnInterval;
                _spawnHelicopterTimer.Reset();
                if (_currentLevel.HasHelicoper)
                {
                    _spawnHelicopterTimer.Start();
                }
            }
            else if (Game.Player.WantedLevel == 0)
            {
                StopSpawning();
            }
        }

        public void Update()
        {
            if (IsSpawning && PlayerPed.IsDead)
            {
                StopSpawning();
            }

            _animatePoliceLights.Update();
            if (PlayerPed.Position.DistanceTo(Game.Player.WantedCenterPosition) > 100f) return;

            _spawnHelicopterTimer.Update();
            _playerActivity.Update();
            if (_lastFrameWantedLevel != Game.Player.WantedLevel)
            {
                OnWantedLevelChanged();
                _lastFrameWantedLevel = Game.Player.WantedLevel;
            }
            if (IsSpawning && Game.Player.WantedLevel >= _minSpawnLevel && _currentLevel != null && _spawnedPoliceVehicles.Count < MaxPoliceVehicles)
            {
                _spawnPoliceTimer += Game.LastFrameTime;
                if (_spawnPoliceTimer > SpawnInterval)
                {
                    SpawnPolice();
                    _spawnPoliceTimer = 0f;
                }
            }

            _checkForDestroyedVehiclesTimer += Game.LastFrameTime;
            if (IsSpawning && _checkForDestroyedVehiclesTimer > _checkForDestroyedVehiclesInterval)
            {
                RemoveDestroyedVehicles();
                _checkForDestroyedVehiclesTimer = 0f;
            }
        }
    }

    public class SpawnLevel
    {
        public VehicleHash GroundVehicle;
        public VehicleHash PursuitVehicle;
        public VehicleHash ArmoredGroundVehicle;
        public VehicleHash Helicopter;
        // Spawn limits, -1 means no limit.
        public int MaxGroundVehiclePassengers = -1;
        public int MaxGroundVehicles;
        public int MaxArmoredGroundVehicles;
        public int MaxHelicopters;
        public PedHash PolicePed = PedHash.Swat01SMY;
        public WeaponHash PrimaryWeapon = WeaponHash.CarbineRifle;
        public WeaponHash SecondaryWeapon = WeaponHash.Pistol;
        public bool HasPrimaryWeapon => PrimaryWeapon != 0;
        public bool HasSecondaryWeapon => SecondaryWeapon != 0;

        public float SpawnInterval;
        public bool HasHelicoper => Helicopter != 0;
        public bool HasArmoredGroundVehicle => ArmoredGroundVehicle != 0;
    }
}

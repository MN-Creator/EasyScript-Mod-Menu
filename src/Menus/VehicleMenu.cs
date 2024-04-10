using EasyScript.Extensions;
using EasyScript.Menus;
using GTA;
using LemonUI.Menus;
using System;
using System.Linq;
using Utils;


namespace EasyScript
{
    class VehicleMenu : Submenu, IUpdate
    {
        private NativeListItem<VehicleHash> _changeVehicleList;
        private readonly NativeCheckboxItem _vehicleInvincibleCheckBox;
        private Vehicle CurrentVehicle => Game.Player.Character.CurrentVehicle;
        private readonly VehicleHash[] _everyVehicle = Vehicle.GetAllModels();
        private readonly Random _random = new Random();
        private float _powerMultiplier = 1f;
        private float _torqueMultiplier = 1f;
        private NativeListItem<float> _lightsMultiplierList;
        private readonly NativeItem _warpInSightItem;
        private bool _isVehicleInSight;
        private Vehicle _vehicleInSight;

        public VehicleMenu(MenuPool pool, NativeMenu parent, string title) : base(pool, parent, title)
        {
            CreateItem("Repair & Clean", RepairAndCleanVehicle);
            CreateVehicleClassMenu();
            CreateRandomSpawnerMenu();
            new VehicleModMenu(pool, Menu, "Modify");
            CreateMultiplierMenu();
            _warpInSightItem = CreateItem("Warp Into Vehicle in Sight", WarpIntoVehicleInCameraDirection);
            CreateWarpPropertyPanel();
            _vehicleInvincibleCheckBox = CreateCheckbox("Vehicle Invincible", ToggleVehicleInvincible);
            new SpeedometerMenu(pool, Menu, "Speedometer");
            Pool.GameEvents.Vehicle.ValueChanged += OnPlayerVehicleChanged;
            CreateProperyPanel();
        }

        protected override void OnMainMenuOpen(object sender, EventArgs e)
        {
            base.OnMainMenuOpen(sender, e);
            if (PlayerPed.IsInVehicle())
            {
                _changeVehicleList.SelectedItem = CurrentVehicle.Model;
            }
        }

        private void CreateWarpPropertyPanel()
        {
            var warpPanel = new PropertyPanel(() => _isVehicleInSight);
            warpPanel.Add("Vehicle", () => _vehicleInSight.DisplayName);
            warpPanel.Add("Unlocked", () => (_vehicleInSight.LockStatus == VehicleLockStatus.Unlocked).AsYesNo());
            warpPanel.Add("Engine Running", () => _vehicleInSight.IsEngineRunning.AsYesNo());
            _warpInSightItem.Panel = warpPanel;
        }

        private void CreateProperyPanel()
        {
            var vehiclePanel = new PropertyPanel(() => CurrentVehicle != null && CurrentVehicle.Exists());
            vehiclePanel.Add("Name", () => $"{CurrentVehicle.DisplayName}");
            vehiclePanel.Add("Class", () => $"{CurrentVehicle.ClassLocalizedName}");
            vehiclePanel.Add("Health", () => (CurrentVehicle.HealthFloat / CurrentVehicle.MaxHealthFloat) * 100f, unit: "%", format: "0");
            vehiclePanel.Add("Engine Health", () => (CurrentVehicle.EngineHealth / 1000f) * 100f, unit: "%", format: "0");
            SubmenuItem.Panel = vehiclePanel;
        }

        private void WarpIntoVehicleInCameraDirection(object _, EventArgs e)
        {
            RaycastResult raycast = GeneralUtils.RaycastFromCamera(100f, IntersectFlags.Vehicles, PlayerPed.CurrentVehicle);
            if (raycast.DidHit && raycast.HitEntity != null && raycast.HitEntity is Vehicle vehicle)
            {
                vehicle.GetPedOnSeat(VehicleSeat.Driver)?.Delete();
                PlayerPed.SetIntoVehicle(vehicle, VehicleSeat.Driver);
            }
        }

        private void CreateMultiplierMenu()
        {
            var multiplierMenu = new Submenu(Pool, Menu, "Multipliers");
            int[] multiplierOptions = { 1, 10, 50, 100, 250, 500, 1000 };
            NativeListItem<int> powerList = multiplierMenu.CreateList("Power Multiplier", OnPowerMultiplierChanged, multiplierOptions);
            NativeListItem<int> torqueList = multiplierMenu.CreateList("Torque Multiplier", OnTorqueMultiplierChanged, multiplierOptions);
            float[] lightValues = { 1f, 2f, 5f, 10f, 20f, 50f, 100f };
            _lightsMultiplierList = multiplierMenu.CreateList("Lights Multiplier", OnLightsMultiplierChanged, lightValues);
            _powerMultiplier = powerList.SelectedItem;
            _torqueMultiplier = torqueList.SelectedItem;
        }

        private void OnLightsMultiplierChanged(object sender, ItemChangedEventArgs<float> e)
        {
            if (!CurrentVehicle.Exists()) return;
            CurrentVehicle.LightsMultiplier = e.Object;
        }

        private void CreateRandomSpawnerMenu()
        {
            Submenu randomMenu = new Submenu(Pool, Menu, "Random Vehicle");
            randomMenu.CreateItem("Spawn Random", SpawnRandomVehicle);
            randomMenu.CreateItem("Spawn Random Car", () => ReplacePlayersVehicleFromArray(VehicleUtils.Cars));
            var heli = randomMenu.CreateItem("Spawn Random Helicopter", () => SpawnRandomVehicleFromArray(VehicleUtils.Helicopters, 20f));
            var plane = randomMenu.CreateItem("Spawn Random Plane", () => SpawnRandomVehicleFromArray(VehicleUtils.Planes, 20f));
            randomMenu.CreateItem("Spawn Random Boat", () => SpawnRandomVehicleFromArray(VehicleUtils.Boats, 20f));
        }

        private void CreateVehicleClassMenu()
        {
            Submenu spawnMenu = new Submenu(Pool, Menu, "Spawn Vehicle");
            _changeVehicleList = spawnMenu.CreateList("Select Vehicle", SelectVehicleChanged, _everyVehicle);
            NativeItem enterName = spawnMenu.CreateItem("Enter Name");
            enterName.Activated += (a, o) => VehicleUtils.SpawnFromUserInput();

            spawnMenu.CreateItem("Spawn Last Vehicle", SpawnLastVehicle);

            foreach (VehicleClass vehicleClass in VehicleUtils.VehicleClassToHash.Keys)
            {
                string menuName = vehicleClass.ToString();
                Submenu classMenu = new Submenu(Pool, spawnMenu.Menu, menuName);
                foreach (VehicleHash vehicle in VehicleUtils.VehicleClassToHash[vehicleClass])
                {
                    string vehicleName = vehicle.ToString();
                    // Change to display name if only numbers.
                    if (vehicleName.All(char.IsDigit))
                    {
                        vehicleName = Vehicle.GetModelDisplayName(vehicle).ToLower();
                        // Capatilize the first letter.
                        vehicleName = char.ToUpper(vehicleName[0]) + vehicleName.Substring(1);
                    }
                    var item = classMenu.CreateItem(vehicleName, () => VehicleUtils.ReplacePlayersVehicle(vehicle));
                    item.AltTitle = Vehicle.GetModelMakeName(vehicle);
                }
            }
        }

        private void SpawnLastVehicle(object _, EventArgs e)
        {
            if (!Game.Player.LastVehicle.Exists()) return;
            VehicleUtils.SpawnInFrontOfPlayer(Game.Player.LastVehicle.Model, 5f);
        }

        private void OnPowerMultiplierChanged(object _, ItemChangedEventArgs<int> e)
        {
            if (PlayerPed.IsInVehicle())
            {
                Vehicle vehicle = PlayerPed.CurrentVehicle;
                vehicle.EnginePowerMultiplier = e.Object;
                _powerMultiplier = e.Object;
                SettingsManager.SetValue("Vehicle", "Power Multiplier", e.Object);
            }
        }

        private void OnTorqueMultiplierChanged(object _, ItemChangedEventArgs<int> e)
        {
            if (PlayerPed.IsInVehicle())
            {
                Vehicle vehicle = PlayerPed.CurrentVehicle;
                vehicle.EngineTorqueMultiplier = e.Object;
                _torqueMultiplier = e.Object;
                SettingsManager.SetValue("Vehicle", "Torque Multiplier", e.Object);
            }
        }

        private void PlaceCurrentVehicleOnRoad(object _, EventArgs e)
        {
            if (PlayerPed.IsInVehicle())
            {
                PlayerPed.CurrentVehicle.PlaceOnNextStreet();
            }
        }

        private Vehicle SpawnRandomVehicleFromArray(VehicleHash[] vehicles, float distanceFromPlayer)
        {
            int index = _random.Next(vehicles.Length);
            VehicleHash vehicle = vehicles[index];
            return VehicleUtils.SpawnInFrontOfPlayer(vehicle, distanceFromPlayer);
        }

        private Vehicle ReplacePlayersVehicleFromArray(VehicleHash[] vehicles)
        {
            int index = _random.Next(vehicles.Length);
            VehicleHash vehicle = vehicles[index];
            return VehicleUtils.ReplacePlayersVehicle(vehicle);
        }

        private void SelectVehicleChanged(object _, ItemChangedEventArgs<VehicleHash> e)
        {
            // Avoid respawning the players vehicle when the menu opens.
            if (PlayerPed.IsInVehicle() && e.Object == CurrentVehicle.Model) return;
            VehicleUtils.ReplacePlayersVehicle(e.Object);
        }

        private void ToggleVehicleInvincible(object _, EventArgs e)
        {
            if (PlayerPed.IsInVehicle())
            {
                CurrentVehicle.IsInvincible = _vehicleInvincibleCheckBox.Checked;
                CurrentVehicle.CanTiresBurst = !_vehicleInvincibleCheckBox.Checked;
                CurrentVehicle.CanBeVisiblyDamaged = !_vehicleInvincibleCheckBox.Checked;
                if (_vehicleInvincibleCheckBox.Checked)
                {
                    CurrentVehicle.Repair();
                }
            }
            else if (Game.Player.LastVehicle.Exists())
            {
                Game.Player.LastVehicle.IsInvincible = _vehicleInvincibleCheckBox.Checked;
                Game.Player.LastVehicle.CanTiresBurst = !_vehicleInvincibleCheckBox.Checked;
                Game.Player.LastVehicle.CanBeVisiblyDamaged = !_vehicleInvincibleCheckBox.Checked;
            }
        }

        private void SpawnRandomVehicle(object _, EventArgs e)
        {
            VehicleHash vehicleHash = _everyVehicle[_random.Next(_everyVehicle.Length)];
            VehicleUtils.SpawnInFrontOfPlayer(vehicleHash, 10f);
        }

        private void RepairAndCleanVehicle(object _, EventArgs e)
        {
            if (!PlayerPed.IsInVehicle()) return;
            PlayerPed.CurrentVehicle.Repair();
            PlayerPed.CurrentVehicle.DirtLevel = 0f;
        }

        private void CleanVehicle(object _, EventArgs e)
        {
            if (!PlayerPed.IsInVehicle()) return;
            PlayerPed.CurrentVehicle.DirtLevel = 0;
        }

        private void OnPlayerVehicleChanged(object _, Vehicle vehicle)
        {
            if (vehicle == null) return;
            if (_vehicleInvincibleCheckBox.Checked)
            {
                vehicle.IsInvincible = true;
                vehicle.CanBeVisiblyDamaged = false;
                vehicle.CanTiresBurst = false;
                vehicle.Repair();
            }
            vehicle.EnginePowerMultiplier = _powerMultiplier;
            vehicle.EngineTorqueMultiplier = _torqueMultiplier;
            vehicle.LightsMultiplier = _lightsMultiplierList.SelectedItem;
        }

        public void Update()
        {
            if (!Menu.Visible) return;
            if (Menu.SelectedItem != _warpInSightItem) return;
            RaycastResult raycast = GeneralUtils.RaycastFromCamera(200f, IntersectFlags.Vehicles, ignore: PlayerPed.CurrentVehicle);

            if (raycast.DidHit && raycast.HitEntity != null && raycast.HitEntity is Vehicle vehicle)
            {
                _vehicleInSight = vehicle;
                _isVehicleInSight = true;
                return;
            }
            _isVehicleInSight = false;
        }
    }
}

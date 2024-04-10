using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Utils
{
    public static class VehicleUtils
    {
        public static readonly VehicleHash[] Cars = Vehicle.GetAllModelsOfType(VehicleType.Automobile);
        public static readonly VehicleHash[] Motorcycles = Vehicle.GetAllModelsOfType(VehicleType.Motorcycle);
        public static readonly VehicleHash[] Helicopters = Vehicle.GetAllModelsOfType(VehicleType.Helicopter);
        public static readonly VehicleHash[] Planes = Vehicle.GetAllModelsOfType(VehicleType.Plane);
        public static readonly VehicleHash[] Boats = Vehicle.GetAllModelsOfType(VehicleType.Boat);
        public static readonly Dictionary<VehicleClass, VehicleHash[]> VehicleClassToHash =
            GeneralUtils.ConvertToArray<VehicleClass>().ToDictionary(c => c, c => Vehicle.GetAllModelsOfClass(c));

        public static bool CanWorldHandleVehicleSpawn => World.VehicleCount < World.VehicleCapacity;

        public static void RepairPlayerVehicle()
        {
            if (Game.Player.Character.IsInVehicle())
            {
                Game.Player.Character.CurrentVehicle.Repair();
            }
        }

        public static Vehicle GetPlayersCurrentOrLastVehicle()
        {
            if (Game.Player.Character.IsInVehicle())
            {
                return Game.Player.Character.CurrentVehicle;
            }
            return Game.Player.Character.LastVehicle;
        }

        public static VehicleHash GetVehicleFromName(string name)
        {
            if (Enum.TryParse(name, true, out VehicleHash vehicleHash))
            {
                return vehicleHash;
            }

            return 0;
        }

        public static void SpawnFromUserInput()
        {
            string input = Game.GetUserInput();

            if (Enum.TryParse(input, true, out VehicleHash vehicleHash))
            {
                ReplacePlayersVehicle(vehicleHash);
            }
        }

        /// <summary>
        /// Remove the player's vehicle and spawn a new one.
        /// </summary>
        /// <param name="vehicleHash">Vehicle to spawn.</param>
        /// <returns>Returns spawned vehicle or null if there are too many vehicles in the world.</returns>
        public static Vehicle ReplacePlayersVehicle(VehicleHash vehicleHash)
        {
            if (!CanWorldHandleVehicleSpawn)
            {
                if (Game.Player.Character.IsInVehicle())
                {
                    Game.Player.Character.CurrentVehicle.MarkAsNoLongerNeeded();
                    Game.Player.Character.CurrentVehicle.Delete();
                }
                else
                {
                    return null;
                }
            }

            float speed = 0.0f;
            float heading = Game.Player.Character.Heading;
            bool isEngineRunning = true;
            bool isRadioEnabled = true;
            Vehicle lastVehicle = null;
            RadioStation radioStation = RadioStation.RadioOff;

            if (Game.Player.Character.IsInVehicle())
            {
                Vehicle currentVehicle = Game.Player.Character.CurrentVehicle;
                speed = currentVehicle.Speed;
                heading = currentVehicle.Heading;
                isEngineRunning = currentVehicle.IsEngineRunning;
                isRadioEnabled = IsRadioEnabledInPlayerVehicle();
                radioStation = GetPlayerRadioStation();
                lastVehicle = currentVehicle;
            }
            Vehicle spawnedVehicle = World.CreateVehicle(new Model(vehicleHash), Game.Player.Character.Position, heading);
            if (spawnedVehicle == null) return null;
            spawnedVehicle.IsEngineRunning = isEngineRunning;
            spawnedVehicle.IsRadioEnabled = isRadioEnabled;
            spawnedVehicle.Speed = speed;
            TransferPedsToVehicle(lastVehicle, spawnedVehicle);
            Game.Player.Character.SetIntoVehicle(spawnedVehicle, VehicleSeat.Driver);
            spawnedVehicle.RadioStation = radioStation;
            if (lastVehicle != null && lastVehicle.Exists())
            {
                lastVehicle.MarkAsNoLongerNeeded();
                lastVehicle.Delete();
            }

            return spawnedVehicle;
        }

        public static void TransferPedsToVehicle(Vehicle lastVehicle, Vehicle newVehicle)
        {
            if (lastVehicle == null || !lastVehicle.Exists() || newVehicle == null || !newVehicle.Exists()) return;

            foreach (Ped passenger in lastVehicle.Passengers)
            {
                if (passenger == null || !passenger.Exists() || passenger.IsDead) continue;
                if (newVehicle.IsSeatFree(passenger.SeatIndex))
                {
                    passenger.SetIntoVehicle(newVehicle, passenger.SeatIndex);
                }
                else if (newVehicle.PassengerCount < newVehicle.PassengerCapacity)
                {
                    passenger.SetIntoVehicle(newVehicle, VehicleSeat.Any);
                }
            }
        }

        public static Vehicle SetMaxPerformanceUpgrades(Vehicle vehicle)
        {
            SetVehicleModMax(vehicle, VehicleModType.Engine);
            SetVehicleModMax(vehicle, VehicleModType.Brakes);
            SetVehicleModMax(vehicle, VehicleModType.Transmission);
            SetTurbo(vehicle, true);
            return vehicle;
        }

        public static void SetVehicleMod(Vehicle vehicle, VehicleModType type, int index)
        {
            if (!vehicle.Exists() || index > vehicle.Mods[type].Count) return;
            vehicle.Mods.InstallModKit();
            vehicle.Mods[type].Index = index;
            Function.Call(Hash.SET_VEHICLE_MOD, vehicle, type, index);
        }

        public static void SetVehicleMod(Vehicle vehicle, VehicleToggleModType type, bool value)
        {
            if (!vehicle.Exists()) return;
            Function.Call(Hash.TOGGLE_VEHICLE_MOD, vehicle, type, value);
        }

        public static void SetVehicleModMax(Vehicle vehicle, VehicleModType type)
        {
            SetVehicleMod(vehicle, type, vehicle.Mods[type].Count - 1);
        }

        public static int GetVehicleMod(Vehicle vehicle, VehicleModType type)
        {
            return Function.Call<int>(Hash.GET_VEHICLE_MOD, vehicle, type);
        }

        public static void SetTurbo(Vehicle vehicle, bool value)
        {
            SetVehicleMod(vehicle, VehicleToggleModType.Turbo, value);
        }

        public static void SetXenonHeadlights(Vehicle vehicle, bool value)
        {
            SetVehicleMod(vehicle, VehicleToggleModType.XenonHeadlights, value);
        }

        public static void SetAllNeonLights(Vehicle vehicle, bool value)
        {
            if (!vehicle.Exists() || !vehicle.Mods.HasNeonLights) return;
            vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Left, value);
            vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Right, value);
            vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Front, value);
            vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Back, value);
        }

        public static void EnableAllNeonLights(Vehicle vehicle)
        {
            SetAllNeonLights(vehicle, true);
        }

        public static void DisableAllNeonLights(Vehicle vehicle)
        {
            SetAllNeonLights(vehicle, false);
        }

        public static void SetPlayerVehiclePrimaryColor(VehicleColor color)
        {
            if (!Game.Player.Character.IsInVehicle()) return;
            Game.Player.Character.CurrentVehicle.Mods.PrimaryColor = color;
        }

        public static void SetPlayerVehicleSecondaryColor(VehicleColor color)
        {
            if (!Game.Player.Character.IsInVehicle()) return;
            Game.Player.Character.CurrentVehicle.Mods.SecondaryColor = color;
        }

        public static void UpgradePlayerVehicleModToMax(VehicleModType modType)
        {
            SetVehicleModMax(Game.Player.Character.CurrentVehicle, modType);
        }

        /// <summary>
        /// Spawn a vehicle in front of the player by a certain distance.
        /// </summary>
        public static Vehicle SpawnInFrontOfPlayer(VehicleHash vehicleHash, float distance)
        {
            if (!CanWorldHandleVehicleSpawn) return null;
            Vector3 spawnPos = Game.Player.Character.Position + Game.Player.Character.ForwardVector * distance;
            Vehicle vehicle = World.CreateVehicle(new Model(vehicleHash), spawnPos, Game.Player.Character.Heading);
            vehicle.IsEngineRunning = true;
            return vehicle;
        }

        public static Vehicle SpawnInFrontOfPlayerOnRoad(VehicleHash vehicleHash, float distance)
        {
            if (!CanWorldHandleVehicleSpawn) return null;
            Vector3 spawnPos = Game.Player.Character.Position + Game.Player.Character.ForwardVector * distance;
            spawnPos = World.GetNextPositionOnStreet(spawnPos, true);
            Vehicle vehicle = World.CreateVehicle(new Model(vehicleHash), spawnPos);
            vehicle.IsEngineRunning = true;
            return vehicle;
        }

        // Create passenger peds in the vehicle. No driver is created.
        public static void CreatePassengersInVehicle(Vehicle vehicle, PedHash pedHash, int passengerCount)
        {
            int count = Math.Min(passengerCount, vehicle.PassengerCapacity);
            for (int i = 0; i < count; i++)
            {
                if (vehicle.IsSeatFree((VehicleSeat)i))
                {
                    vehicle.CreatePedOnSeat((VehicleSeat)i, new Model(pedHash));
                }
            }
        }

        public static Ped CreateDriverInVehicle(Vehicle vehicle, PedHash pedHash)
        {
            return vehicle.CreatePedOnSeat(VehicleSeat.Driver, new Model(pedHash));
        }

        public static void CleanPlayerVehicle()
        {
            if (Game.Player.Character.IsInVehicle())
            {
                Game.Player.Character.CurrentVehicle.Wash();
            }
        }

        public static void CreatePedsOnHelicopterSides(Vehicle helicopter, PedHash pedHash)
        {
            VehicleSeat leftside = VehicleSeat.LeftRear;
            VehicleSeat rightside = VehicleSeat.RightRear;
            helicopter.CreatePedOnSeat(leftside, new Model(pedHash));
            helicopter.CreatePedOnSeat(rightside, new Model(pedHash));
        }

        public static bool IsRadioEnabledInPlayerVehicle()
        {
            return Function.Call<bool>(Hash.IS_PLAYER_VEH_RADIO_ENABLE);
        }

        public static RadioStation GetPlayerRadioStation()
        {
            return Function.Call<RadioStation>(Hash.GET_PLAYER_RADIO_STATION_INDEX);
        }

        public static void TogglePlayerVehicleRadio()
        {
            if (Game.Player.Character.IsInVehicle())
            {
                Vehicle vehicle = Game.Player.Character.CurrentVehicle;
                if (IsRadioEnabledInPlayerVehicle())
                {
                    vehicle.IsRadioEnabled = false;
                    return;
                }
                vehicle.IsRadioEnabled = true;
            }
        }

        public static void ToggleLeftIndicator(Vehicle vehicle)
        {
            if (vehicle == null || !vehicle.Exists()) return;
            vehicle.IsLeftIndicatorLightOn = !vehicle.IsLeftIndicatorLightOn;
        }

        public static void ToggleRightIndicator(Vehicle vehicle)
        {
            if (vehicle == null || !vehicle.Exists()) return;
            vehicle.IsRightIndicatorLightOn = !vehicle.IsRightIndicatorLightOn;
        }

        public static void TogglePlayerIndicatorBasedOnCameraDirection(bool inverse = false)
        {
            if (!Game.Player.Character.IsInVehicle()) return;
            Vehicle vehicle = Game.Player.Character.CurrentVehicle;
            if (vehicle.IsLeftIndicatorLightOn || vehicle.IsRightIndicatorLightOn)
            {
                vehicle.IsLeftIndicatorLightOn = false;
                vehicle.IsRightIndicatorLightOn = false;
                return;
            }

            Vector3 forward = Game.Player.Character.CurrentVehicle.RightVector;
            Vector3 cameraDir = GameplayCamera.Direction;

            // If Camera is pointing to the left of the vehicle enable left indicator.
            if (Vector3.Dot(cameraDir, forward) < 0)
            {
                vehicle.IsLeftIndicatorLightOn = true;
                if (inverse) SwitchActiveIndicatorSide(vehicle);
                return;
            }
            vehicle.IsRightIndicatorLightOn = true;
            if (inverse) SwitchActiveIndicatorSide(vehicle);
        }

        // Disable active indicator and enable other side.
        private static void SwitchActiveIndicatorSide(Vehicle vehicle)
        {
            if (vehicle.IsLeftIndicatorLightOn)
            {
                vehicle.IsLeftIndicatorLightOn = false;
                vehicle.IsRightIndicatorLightOn = true;
                return;
            }
            vehicle.IsLeftIndicatorLightOn = true;
            vehicle.IsRightIndicatorLightOn = false;
        }

        public static void ToggleHazardLights(Vehicle vehicle)
        {
            if (vehicle == null || !vehicle.Exists()) return;
            if (!vehicle.IsLeftIndicatorLightOn)
            {
                vehicle.IsLeftIndicatorLightOn = true;
                vehicle.IsRightIndicatorLightOn = true;
                return;
            }
            vehicle.IsLeftIndicatorLightOn = false;
            vehicle.IsRightIndicatorLightOn = false;
        }

        public static void ToggleEngine()
        {
            Vehicle vehicle = GetPlayersCurrentOrLastVehicle();
            if (vehicle == null || !vehicle.Exists()) return;
            vehicle.IsEngineRunning = !vehicle.IsEngineRunning;
        }

        public static void ToggleAllDoors()
        {
            Vehicle vehicle = GetPlayersCurrentOrLastVehicle();
            if (vehicle == null || !vehicle.Exists()) return;
            bool isAnyDoorOpen = false;
            foreach (VehicleDoor door in vehicle.Doors)
            {
                if (!door.IsBroken && door.IsOpen)
                {
                    isAnyDoorOpen = true;
                    door.Close();
                }
            }
            if (!isAnyDoorOpen)
            {
                foreach (VehicleDoor door in vehicle.Doors)
                {
                    if (!door.IsBroken)
                    {
                        door.Open();
                    }
                }
            }
        }

        public static void CloseAllDoors(Vehicle vehicle)
        {
            if (vehicle == null || !vehicle.Exists()) return;
            foreach (VehicleDoor door in vehicle.Doors)
            {
                if (!door.IsBroken && door.IsOpen)
                {
                    door.Close();
                }
            }
        }

        public static void ToggleDoor(Vehicle vehicle, VehicleDoorIndex door)
        {
            if (vehicle == null || !vehicle.Exists()) return;
            if (vehicle.Doors[door].IsOpen)
            {
                vehicle.Doors[door].Close();
                return;
            }
            vehicle.Doors[door].Open();
        }

        public static void OpenDoor(Vehicle vehicle, VehicleDoorIndex door)
        {
            if (vehicle == null || !vehicle.Exists() || vehicle.Doors[door].IsFullyOpen) return;
            vehicle.Doors[door].Open();
        }

        public static void CloseDoor(Vehicle vehicle, VehicleDoorIndex door)
        {
            if (vehicle == null || !vehicle.Exists() || !vehicle.Doors[door].IsOpen) return;
            vehicle.Doors[door].Close();
        }

        public static void ToggleTrunk()
        {
            Vehicle vehicle = GetPlayersCurrentOrLastVehicle();
            if (vehicle == null || !vehicle.Exists()) return;
            ToggleDoor(vehicle, VehicleDoorIndex.Trunk);
        }

        public static void ToggleHood()
        {
            Vehicle vehicle = GetPlayersCurrentOrLastVehicle();
            if (vehicle == null || !vehicle.Exists()) return;
            ToggleDoor(vehicle, VehicleDoorIndex.Hood);
        }

        public static void ToggleLights()
        {
            Vehicle vehicle = GetPlayersCurrentOrLastVehicle();
            if (vehicle == null || !vehicle.Exists()) return;
            if (vehicle.AreLightsOn)
            {
                vehicle.AreLightsOn = false;
                return;
            }
            vehicle.AreLightsOn = true;
        }
    }
}

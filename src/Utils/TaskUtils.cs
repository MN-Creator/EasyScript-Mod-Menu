using GTA;
using GTA.Math;
using System.Linq;

namespace EasyScript.Utils
{
    internal class TaskUtils
    {
        private static Ped PlayerPed => Game.Player.Character;

        public static void PlayerEnterClosestVehicle()
        {
            if (PlayerPed.IsInGroup)
            {
                PlayerEnterClosestVehicleForGroup();
                return;
            }
            Vehicle vehicle = World.GetClosestVehicle(PlayerPed.Position, 100);
            if (vehicle == null || !vehicle.Exists()) return;
            PlayerPed.Task.EnterVehicle(vehicle, VehicleSeat.Driver, -1, 2, flag: EnterVehicleFlags.ResumeIfInterupted);
        }

        public static void PlayerEnterClosestEmptyVehicle()
        {
            if (PlayerPed.IsInGroup)
            {
                Vehicle v = GetClosestEmptyVehicleForGroup();
                if (v == null || !v.Exists()) return;
                PlayerPed.Task.EnterVehicle(v, VehicleSeat.Driver, -1, 2, flag: EnterVehicleFlags.ResumeIfInterupted);
                return;
            }
            Vehicle vehicle = GetClosestEmptyVehicle();
            if (vehicle == null || !vehicle.Exists()) return;
            PlayerPed.Task.EnterVehicle(vehicle, VehicleSeat.Driver, -1, 2, flag: EnterVehicleFlags.ResumeIfInterupted);
        }

        public static Vehicle GetClosestEmptyVehicle()
        {
            Vehicle[] vehicles = World.GetNearbyVehicles(PlayerPed.Position, 200);
            if (vehicles.Length == 0) return null;
            Vehicle vehicle = vehicles.FirstOrDefault(v => v.Occupants.Count() == 0);
            return vehicle;
        }

        public static Vehicle GetClosestVehicleWithFourSeats()
        {
            Vehicle[] vehicles = World.GetNearbyVehicles(PlayerPed.Position, 200);
            if (vehicles.Length == 0) return null;
            Vehicle vehicle = null;
            float distance = 200;
            foreach (Vehicle v in vehicles)
            {
                if (v.PassengerCapacity >= 2 && v.Position.DistanceTo(PlayerPed.Position) < distance)
                {
                    vehicle = v;
                    distance = v.Position.DistanceTo(PlayerPed.Position);
                }
            }
            return vehicle;
        }

        public static Vehicle GetClosestEmptyVehicleForGroup()
        {
            Vehicle[] vehicles = World.GetNearbyVehicles(PlayerPed.Position, 200);
            if (vehicles.Length == 0) return null;
            Vehicle vehicle = null;
            float distance = 200;
            foreach (Vehicle v in vehicles)
            {
                if (v.Occupants.Length == 0 && CanVehicleSeatGroup(v) && v.Position.DistanceTo(PlayerPed.Position) < distance)
                {
                    vehicle = v;
                    distance = v.Position.DistanceTo(PlayerPed.Position);
                }
            }
            return vehicle;
        }

        private static bool CanVehicleSeatGroup(Vehicle vehicle)
        {
            return vehicle.PassengerCapacity >= PlayerPed.PedGroup.MemberCount;
        }

        public static void PlayerEnterClosestVehicleForGroup()
        {
            Vehicle vehicle = World.GetClosestVehicle(PlayerPed.Position, 200);
            if (PlayerPed.IsInGroup && PlayerPed.PedGroup.MemberCount >= vehicle.PassengerCapacity)
            {
                vehicle = GetClosestVehicleWithFourSeats();
                if (vehicle == null || !vehicle.Exists()) return;
            }

            if (vehicle == null || !vehicle.Exists()) return;
            PlayerPed.Task.EnterVehicle(vehicle, VehicleSeat.Driver, -1, 2, flag: EnterVehicleFlags.ResumeIfInterupted);
        }

        public static void PlayerEnterLastVehicle()
        {
            if (PlayerPed.LastVehicle == null || !PlayerPed.LastVehicle.Exists()) return;
            PlayerPed.Task.EnterVehicle(PlayerPed.LastVehicle, VehicleSeat.Driver, -1, 2);
        }

        public static void PlayerWarpIntoLastVehicle()
        {
            if (PlayerPed.LastVehicle == null || !PlayerPed.LastVehicle.Exists()) return;
            PlayerPed.SetIntoVehicle(PlayerPed.LastVehicle, VehicleSeat.Driver);
        }

        public static void TestSequence()
        {
            PlayerPed.Task.ClearAll();
            TaskSequence sequence = new TaskSequence(PlayerPed.Handle);
            Vector3 position = PlayerPed.GetOffsetPosition(new Vector3(0, 5, 0));
            position = World.GetNextPositionOnStreet(position);
            sequence.AddTask.GoStraightTo(position, 5);
            sequence.Close(false);
            //sequence.AddTask.LookAt(World.GetClosestPed(PlayerPed.Position, 100), 5);
            PlayerPed.AlwaysKeepTask = true;
            PlayerPed.Task.PerformSequence(sequence);
        }

        public static void DriveCarSequence()
        {
            PlayerPed.Task.ClearAll();
            TaskSequence sequence = new TaskSequence();
            Vehicle vehicle = PlayerPed.CurrentVehicle;
            if (!PlayerPed.IsInVehicle())
            {
                vehicle = World.GetClosestVehicle(PlayerPed.Position, 100);
                if (vehicle == null || !vehicle.Exists()) return;
                sequence.AddTask.GoTo(vehicle);
                sequence.AddTask.EnterVehicle(vehicle, VehicleSeat.Driver, flag: EnterVehicleFlags.JackAnyone);
            }
            sequence.AddTask.CruiseWithVehicle(vehicle, -1, DrivingStyle.Normal);
            PlayerPed.Task.PerformSequence(sequence);
        }

        public static void FightAgainstClosestPed()
        {
            Ped target = World.GetClosestPed(PlayerPed.Position, 100);
            if (target == null || !target.Exists()) return;
            PlayerPed.Task.FightAgainst(target);
        }

        public static void FleeFromClosestPed()
        {
            Ped target = World.GetClosestPed(PlayerPed.Position, 100);
            if (target == null || !target.Exists()) return;
            PlayerPed.Task.FleeFrom(target);
        }

        public static void PlayerLeaveVehicle()
        {
            PlayerPed.Task.LeaveVehicle();
        }

        public static void PlayerWanderAround()
        {
            PlayerPed.Task.WanderAround();
        }

        public static void PlayerStopTasks()
        {
            PlayerPed.Task.ClearAll();
        }

        public static void PlayerDriveToWaypoint()
        {
            if (!Game.IsWaypointActive) return;
            DriveToLocation(World.WaypointPosition);
        }

        public static void PlayerDriveToMissionStart()
        {
            Vector3 position = LocationUtils.GetMissionStartPosition();
            if (position == Vector3.Zero) return;
            DriveToLocation(position);
        }

        public static void PlayerDriveToMission()
        {
            Vector3 position = LocationUtils.GetYellowMissionPosition();
            if (position == Vector3.Zero) return;
            DriveToLocation(position);
        }

        public static void PlayerDriveToBlueMarker()
        {
            Vector3 position = LocationUtils.GetBlueMarkerPosition();
            if (position == Vector3.Zero) return;
            DriveToLocation(position);
        }

        public static void DriveToLocation(Vector3 position)
        {
            if (!PlayerPed.IsInVehicle()) return;
            PlayerPed.Task.ClearAll();
            float distance = PlayerPed.CurrentVehicle.Position.DistanceTo(position);
            PlayerPed.Task.DriveTo(PlayerPed.CurrentVehicle, position, distance, 200, DrivingStyle.Rushed);
        }

        public static void GetToLocation(Vector3 position)
        {
            TaskSequence sequence = new TaskSequence();
            Vehicle vehicle = PlayerPed.CurrentVehicle;

            if (!PlayerPed.IsInVehicle())
            {
                if (PlayerPed.IsInGroup)
                {
                    vehicle = GetClosestEmptyVehicleForGroup();
                    if (vehicle == null || !vehicle.Exists())
                    {
                        sequence.AddTask.EnterVehicle(vehicle, VehicleSeat.Driver, -1, speed: 2, flag: EnterVehicleFlags.ResumeIfInterupted);
                    }
                }
                else
                {
                    vehicle = GetClosestEmptyVehicle();
                    if (vehicle == null || !vehicle.Exists())
                    {
                        sequence.AddTask.EnterVehicle(vehicle, VehicleSeat.Driver, -1, speed: 2, flag: EnterVehicleFlags.ResumeIfInterupted);
                    }
                }
            }

            if (vehicle == null || !vehicle.Exists())
            {
                sequence.AddTask.GoTo(position);
                PlayerPed.Task.PerformSequence(sequence);
                GTA.UI.Notification.Show("No vehicle found to drive to the location.");
                return;
            }
            float distance = vehicle.Position.DistanceTo(position);
            sequence.AddTask.DriveTo(vehicle, position, distance, 30, DrivingStyle.Normal);
            PlayerPed.Task.ClearAll();
            PlayerPed.Task.PerformSequence(sequence);
            GTA.UI.Notification.Show($"Driving to waypoint.");
        }

        public static void PlayerDriveAround()
        {
            if (!PlayerPed.IsInVehicle()) return;
            PlayerPed.Task.CruiseWithVehicle(PlayerPed.CurrentVehicle, -1, DrivingStyle.Normal);
        }
    }
}

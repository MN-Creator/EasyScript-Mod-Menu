using EasyScript.Utils;
using GTA;
using GTA.Math;

namespace Utils
{
    internal class TeleportUtils
    {
        public static void TeleportPlayerToClosestRoad()
        {
            Vector3 roadPosition = World.GetNextPositionOnStreet(Game.Player.Character.Position, true);
            TeleportPlayerToPosition(roadPosition);
        }

        public static void TeleportPlayerToClosestSidewalk()
        {
            Vector3 streetPosition = World.GetNextPositionOnSidewalk(Game.Player.Character.Position);
            TeleportPlayerToPosition(streetPosition);
        }

        public static void TeleportToWaypoint()
        {
            if (!Game.IsWaypointActive) return;
            TeleportPlayerToPosition(World.WaypointPosition);
        }

        public static void TeleportToMission()
        {
            Vector3 missionPosition = LocationUtils.GetYellowMissionPosition();
            if (missionPosition == Vector3.Zero) return;
            TeleportPlayerToPosition(missionPosition);
        }

        public static void TeleportToBlueMarker()
        {
            Vector3 missionPosition = LocationUtils.GetBlueMarkerPosition(2f);
            if (missionPosition == Vector3.Zero) return;
            TeleportPlayerToPosition(missionPosition);
        }

        public static void TeleportToMissionStart()
        {
            Vector3 missionPosition = LocationUtils.GetMissionStartPosition();
            if (missionPosition == Vector3.Zero) return;
            TeleportPlayerToRoadByPosition(missionPosition);
        }

        public static void TeleportToBlip(BlipSprite blipSprite)
        {
            Vector3 position = LocationUtils.GetBlipPosition(blipSprite);
            if (position == Vector3.Zero) return;
            TeleportPlayerToRoadByPosition(position);
        }

        public static void TeleportPlayerToRoadByPosition(Vector3 position)
        {
            Vector3 roadPosition = World.GetNextPositionOnStreet(position, true);
            Vehicle currentVehicle = Game.Player.Character.CurrentVehicle;
            if (Game.Player.Character.IsInVehicle() && (currentVehicle.IsSubmarine || currentVehicle.IsBoat))
            {
                TeleportPlayerToPosition(position);
                return;
            }
            TeleportPlayerToPosition(roadPosition);
        }

        /// <summary>
        /// Teleport player or player's vehicle to position.
        /// </summary>
        public static void TeleportPlayerToPosition(Vector3 position)
        {
            if (Game.Player.Character.IsInVehicle())
            {
                Vehicle vehicle = Game.Player.Character.CurrentVehicle;
                if (vehicle.IsAircraft)
                {
                    position.Z += 20f;
                }

                vehicle.Position = position;
                return;
            }
            Game.Player.Character.Position = position;
        }
    }
}
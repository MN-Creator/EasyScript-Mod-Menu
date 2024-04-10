using EasyScript.Utils;
using GTA;
using GTA.Math;
using LemonUI.Menus;
using System;
using System.Collections.Generic;
using Utils;

namespace EasyScript
{
    internal class TeleportMenu : Submenu
    {
        private readonly Submenu _mapBlipMenu;

        public TeleportMenu(MenuPool pool, NativeMenu parent, string title) : base(pool, parent, title)
        {
            CreateTeleportToWaypoint();
            CreateItem("Teleport to Mission Start", TeleportUtils.TeleportToMissionStart);
            CreateTeleportToMissionEnd();
            CreateItem("Teleport to Blue Marker", TeleportUtils.TeleportToBlueMarker);
            CreateItem("Teleport to Personal Vehicle", () => TeleportUtils.TeleportToBlip(BlipSprite.PersonalVehicleCar));
            CreateItem("Teleport to Closest Road", TeleportUtils.TeleportPlayerToClosestRoad);
            CreateItem("Teleport to Closest Sidewalk", TeleportUtils.TeleportPlayerToClosestSidewalk);
            _mapBlipMenu = new Submenu(Pool, Menu, "Map Blips");
            _mapBlipMenu.Menu.Shown += CreateBlipMenuItems;

            CreateSeparator("Locations");
            CreateItemForEachLocation();
        }


        private void CreateTeleportToWaypoint()
        {
            var waypoint = CreateItem("Teleport to Waypoint", TeleportUtils.TeleportToWaypoint);
            waypoint.UseCustomBackground = true;
            waypoint.Colors = MenuColors.PurpleBackground;
            var waypointPanel = new PropertyPanel(() => Game.IsWaypointActive);
            waypointPanel.Add("Street", () => World.GetStreetName(World.WaypointPosition));
            waypointPanel.Add("Distance", () => $"{PlayerPed.Position.DistanceTo(World.WaypointPosition):0} m");
            waypoint.Panel = waypointPanel;
        }

        private void CreateTeleportToMissionEnd()
        {
            var missionEnd = CreateItem("Teleport to Mission End", TeleportUtils.TeleportToMission);
            missionEnd.UseCustomBackground = true;
            missionEnd.Colors = MenuColors.YellowBackground;
        }

        private void CreateBlipMenuItems(object sender, EventArgs e)
        {
            _mapBlipMenu.Menu.Clear();
            var blips = World.GetAllBlips();
            foreach (Blip blip in blips)
            {
                if (blip.Exists())
                {
                    var item = _mapBlipMenu.CreateItem(blip.Sprite.ToString(), () => TeleportUtils.TeleportPlayerToRoadByPosition(blip.Position));
                    item.AltTitle = World.GetStreetName(blip.Position);
                }
            }
        }

        private void CreateItemForEachLocation()
        {
            foreach (KeyValuePair<string, Vector3> location in LocationUtils.Locations)
            {
                CreateItem(location.Key, () => TeleportUtils.TeleportPlayerToPosition(location.Value));
            }
        }
    }
}

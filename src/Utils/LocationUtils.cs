using GTA;
using GTA.Math;
using System;
using System.Collections.Generic;

namespace EasyScript.Utils
{
    internal class LocationUtils
    {
        public static readonly List<BlipSprite> PlayerColoredBlipsThatAreNotMissions = new List<BlipSprite>
        {
            BlipSprite.PersonalVehicleCar,
            BlipSprite.PersonalVehicleBike,
            BlipSprite.Business,
            BlipSprite.StrangersAndFreaks,
            BlipSprite.Hangar,
            BlipSprite.Garage,
            BlipSprite.Garage2,
            BlipSprite.Helipad,
            BlipSprite.Airport,
            BlipSprite.Helicopter,
        };

        public static Dictionary<string, Vector3> Locations = new Dictionary<string, Vector3>
        {
            { "Michaels House", new Vector3(-852.4f, 160.0f, 65.6f) },
            { "Franklins House", new Vector3(7.9f, 548.1f, 175.5f) },
            { "Trevors House", new Vector3(1985.7f, 3820.2f, 32.2f) },
            { "Lesters House", new Vector3(1273.9f, -1719.305f, 54.77141f) },
            { "Los Santos Airport", new Vector3(-1034.6f, -2733.6f, 13.8f) },
            { "Sandy Shores Airfield", new Vector3(1741.5f, 3273.7f, 41.1f) },
            { "Mount Chiliad", new Vector3(423.2199f, 5613.901f, 766.7894f) },
            { "Fort Zancudo", new Vector3(-2047.4f, 3132.1f, 32.8f) },
            { "Paleto Bay", new Vector3(-360.8f, 6069.9f, 31.5f) },
            { "Maze Bank Arena", new Vector3(-245.7481f, -2033.534f, 29.94605f) },
            { "Del Perro Pier", new Vector3(-1853.8f, -1225.3f, 13.0f) },
            { "Pacific Standard Bank", new Vector3(235.046f, 216.434f, 106.2871f) },
            { "Eclipse Towers", new Vector3(-781.5705f, 336.7562f, 160.0017f) },
            { "Maze Bank Tower", new Vector3(-75.0f, -818.0f, 326.0f) },
            { "IAA Building", new Vector3(114.2659f, -618.1183f, 206.0466f) },
            { "FIB Building", new Vector3(135.6759f, -749.1976f, 258.1522f) },
            { "Weazel Plaza", new Vector3(-909.0f, -417.0f, 38.5f) },
        };

        public static Vector3 GetYellowMissionPosition()
        {
            if (!Game.IsMissionActive) return Vector3.Zero;
            bool predicate(Blip blip) => blip.Color == BlipColor.Yellow || blip.Color == BlipColor.Yellow2;
            Vector3 missionPosition = GetBlipPosition(predicate);
            return missionPosition;
        }

        public static Vector3 GetMissionStartPosition()
        {
            Model model = Game.Player.Character.Model;
            bool isFranklin = model == PedHash.Franklin;
            bool isMichael = model == PedHash.Michael;
            bool isTrevor = model == PedHash.Trevor;

            Predicate<Blip> predicate = blip =>
            (!PlayerColoredBlipsThatAreNotMissions.Contains(blip.Sprite)) &&
            ((isFranklin && blip.Color == BlipColor.Franklin) ||
            (isMichael && blip.Color == BlipColor.Michael) ||
            (isTrevor && blip.Color == BlipColor.Trevor));

            return GetClosestBlipPosition(predicate);
        }

        public static Vector3 GetBlueMarkerPosition(float offset = 0f)
        {
            Vector3 missionPosition = Vector3.Zero;
            Blip missionBlip = null;
            Blip backupBlip = null;
            foreach (Blip blip in World.GetAllBlips())
            {
                if (blip.Color == BlipColor.Red2)
                {
                    missionPosition = blip.Position;
                    missionBlip = blip;
                    break;
                }
                if (blip.Color == BlipColor.Blue)
                {
                    backupBlip = blip;
                }
                else if (blip.Color == BlipColor.Red)
                {
                    backupBlip = blip;
                }
            }
            if (missionPosition == Vector3.Zero)
            {
                if (backupBlip != null)
                {
                    missionPosition = backupBlip.Position;
                    missionBlip = backupBlip;
                }
                else
                {
                    return Vector3.Zero;
                }
            }
            // Check if it's a vehicle or a ped.
            if (missionBlip != null && missionBlip.Entity != null && missionBlip.Sprite != BlipSprite.StrangersAndFreaks)
            {
                Entity missionEntity = missionBlip.Entity;
                // Spawn behind the missionBlip.
                missionPosition -= missionEntity.ForwardVector * offset;
            }
            return missionPosition;
        }

        /// <summary>
        /// Get position of the first blip that matches the predicate, returns Vector3.Zero if not found.
        /// </summary>
        public static Vector3 GetBlipPosition(Predicate<Blip> predicate)
        {
            Blip[] blips = World.GetAllBlips();
            for (int i = 0; i < blips.Length; i++)
            {
                Blip blip = blips[i];
                if (blip.Exists() && predicate(blip))
                {
                    return blip.Position;
                }
            }
            return Vector3.Zero;
        }

        public static Vector3 GetBlipPosition(BlipSprite sprite)
        {
            return GetBlipPosition(blip => blip.Sprite == sprite);
        }

        /// <summary>
        /// Get position of the closest blip that matches the predicate, returns Vector3.Zero if not found.
        /// </summary>
        public static Vector3 GetClosestBlipPosition(Predicate<Blip> predicate)
        {
            float closestDistance = float.MaxValue;
            Vector3 closestPosition = Vector3.Zero;
            Blip[] blips = World.GetAllBlips();
            for (int i = 0; i < blips.Length; i++)
            {
                Blip blip = blips[i];
                if (blip.Exists() && predicate(blip))
                {
                    Vector3 position = blip.Position;
                    float distance = Game.Player.Character.Position.DistanceTo(position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestPosition = position;
                    }
                }
            }
            return closestPosition;
        }

        public static Vector3 GetClosestBlipPosition(BlipSprite sprite)
        {
            return GetClosestBlipPosition(blip => blip.Sprite == sprite);
        }
    }
}

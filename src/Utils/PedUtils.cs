using EasyScript.Extensions;
using GTA;
using System;

namespace EasyScript.Utils
{
    internal static class PedUtils
    {
        /// <summary>
        /// Run an action on each ped in the world except the player.
        /// </summary>
        public static void ForEachPed(Action<Ped> action)
        {
            World.GetAllPeds().ForEach(action, ped => ped != Game.Player.Character);
        }

        public static void SpawnRandomPedWithPlayer()
        {
            var ped = World.CreateRandomPed(Game.Player.Character.Position.Around(5));
            Vehicle vehicle = Game.Player.Character.CurrentVehicle;
            if (Game.Player.Character.IsInVehicle() && vehicle.PassengerCapacity < vehicle.PassengerCount)
            {
                ped.SetIntoVehicle(Game.Player.Character.CurrentVehicle, VehicleSeat.Any);
            }
        }

        public static void AllPedsAttackPlayer()
        {
            ForEachPed(ped => ped.Task.ShootAt(Game.Player.Character));
        }

        public static void GiveAllPedsWeapon(WeaponHash weapon, int ammo = 9999)
        {
            ForEachPed(ped => ped.Weapons.Give(weapon, ammo, true, true));
        }

        public static void ResurrectAllPeds()
        {
            ForEachPed(ped => ped.Resurrect());
        }

        public static void KillAllPeds()
        {
            ForEachPed(ped => ped.Kill());
        }
    }
}

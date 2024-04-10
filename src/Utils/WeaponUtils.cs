using GTA;
using System;

namespace EasyScript.Utils
{
    internal class WeaponUtils
    {
        /// <summary>
        /// Get all weapon hashes.
        /// </summary>
        public static readonly WeaponHash[] Weapons = (WeaponHash[])Enum.GetValues(typeof(WeaponHash));
        private static readonly Random _random = new Random();

        public static void GivePlayerWeapon(WeaponHash weapon, int ammoAmount = 9999)
        {
            Game.Player.Character.Weapons.Give(weapon, ammoAmount, true, true);
        }

        public static void GivePlayerAllWeapons()
        {
            foreach (var weapon in Weapons)
            {
                GivePlayerWeapon(weapon);
            }
        }

        public static void RemovePlayersWeapons()
        {
            Game.Player.Character.Weapons.RemoveAll();
        }

        public static void SetPlayerInfiniteAmmo(bool value)
        {
            Game.Player.Character.Weapons.Current.InfiniteAmmo = value;
        }

        public static void SetPlayerInfiniteClip(bool value)
        {
            Game.Player.Character.Weapons.Current.InfiniteAmmoClip = value;
        }

        /// <summary>
        /// Remove players weapons and replace with random weapons.
        /// </summary>
        /// <param name="numOfWeapons">Number of weapons to give player, -1 gives a random amount.</param>
        /// <param name="ammoAmount">Amount of ammo to give player, -1 gives a random amount.</param>
        public static void ReplaceWeaponsWithRandom(int numOfWeapons = -1, int ammoAmount = -1)
        {
            if (numOfWeapons == -1)
            {
                numOfWeapons = _random.Next(Weapons.Length);
            }
            RemovePlayersWeapons();
            int spawnedWeapons = 0;
            int attempts = 0;
            while (spawnedWeapons < numOfWeapons)
            {
                var weapon = Weapons[_random.Next(Weapons.Length)];
                if (Game.Player.Character.Weapons.HasWeapon(weapon))
                {
                    attempts++;
                    if (attempts > 1000) break;
                    continue;
                }
                if (ammoAmount == -1)
                {
                    ammoAmount = _random.Next(1, 9999);
                }
                Game.Player.Character.Weapons.Give(weapon, ammoAmount, true, true);
                spawnedWeapons++;
                attempts = 0;
            }
        }

        public static WeaponHash RandomWeapon()
        {
            return Weapons[_random.Next(Weapons.Length)];
        }

        internal static void PlayerMaxAmmoForAllWeapons()
        {
            var playerWeapons = Game.Player.Character.Weapons;
            foreach (var weapon in Weapons)
            {
                if (playerWeapons.HasWeapon(weapon))
                {
                    playerWeapons[weapon].Ammo = playerWeapons[weapon].MaxAmmo;
                }
            }
        }
    }
}

using GTA;
using System;

namespace Utils
{
    internal static class PlayerUtils
    {
        private static Ped PlayerPed => Game.Player.Character;

        public static void FullHealth()
        {
            PlayerPed.Health = PlayerPed.MaxHealth;
        }

        public static void FullHealthAndArmor()
        {
            PlayerPed.Health = PlayerPed.MaxHealth;
            PlayerPed.Armor = 400;
        }

        public static void ClearWantedLevel(object sender, EventArgs e)
        {
            Game.MaxWantedLevel = 0;
        }

        public static void SetNeverWanted()
        {
            Game.MaxWantedLevel = 0;
            Game.Player.WantedLevel = 0;
        }

        public static bool IsFranklin()
        {
            return Game.Player.Character.Model == PedHash.Franklin;
        }

        public static bool IsMichael()
        {
            return Game.Player.Character.Model == PedHash.Michael;
        }

        public static bool IsTrevor()
        {
            return Game.Player.Character.Model == PedHash.Trevor;
        }

        /// <summary>
        /// Check if the player is playing as Franklin, Michael or Trevor.
        /// </summary>
        public static bool IsPlayerCharacter()
        {
            return IsFranklin() || IsMichael() || IsTrevor();
        }

        /// <summary>
        /// Check if ped is Franklin, Michael or Trevor.
        /// </summary>
        public static bool IsPlayerCharacter(PedHash ped)
        {
            return ped == PedHash.Franklin || ped == PedHash.Michael || ped == PedHash.Trevor;
        }

        public static PedHash GetPlayerCharacter(string name)
        {
            switch (name.ToLower())
            {
                case "franklin":
                    return PedHash.Franklin;
                case "michael":
                    return PedHash.Michael;
                case "trevor":
                    return PedHash.Trevor;
                default:
                    return PedHash.Franklin;
            }
        }
    }
}

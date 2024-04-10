using GTA;
using GTA.Math;
using LemonUI.Menus;
using System;
using System.Collections.Generic;

namespace EasyScript
{
    internal class BodyguardMenu : Submenu
    {
        private readonly List<Ped> _spawnedBodyguards = new List<Ped>();
        private readonly NativeCheckboxItem _bodyguardsInvincibleCheckbox;
        private readonly NativeListItem<PedHash> _bodyguardPedList;
        private readonly NativeListItem<WeaponHash> _bodyguardPrimaryWeaponList;
        private readonly NativeListItem<WeaponHash> _bodyguardSecondaryWeaponList;

        private PedHash _currentBodyguardPedHash = PedHash.Blackops01SMY;
        private WeaponHash _primaryWeapon = WeaponHash.CarbineRifle;
        private WeaponHash _secondaryWeapon = WeaponHash.MachinePistol;
        private static readonly PedHash[] _allPedHashes = (PedHash[])Enum.GetValues(typeof(PedHash));

        public BodyguardMenu(MenuPool pool, NativeMenu parent, string title) : base(pool, parent, title)
        {
            CreateItem("Spawn Bodyguard", SpawnBodyguard);
            CreateItem("Dismiss Bodyguards", DismissBodyguards);
            _bodyguardsInvincibleCheckbox = CreateCheckbox("Invincible", ToggleBodyguardsInvincible);
            _bodyguardPedList = CreateList("Ped", OnBodyguardChanged, _allPedHashes);
            _bodyguardPedList.SelectedItem = _currentBodyguardPedHash;
            WeaponHash[] weapons = Weapon.GetAllWeaponHashesForHumanPeds();
            _bodyguardPrimaryWeaponList = CreateList("Primary Weapon", OnPrimaryWeaponChanged, weapons);
            _bodyguardPrimaryWeaponList.SelectedItem = _primaryWeapon;
            _bodyguardSecondaryWeaponList = CreateList("Secondary Weapon", OnSecondaryWeaponChanged, weapons);
            _bodyguardSecondaryWeaponList.SelectedItem = _secondaryWeapon;
        }

        private void OnSecondaryWeaponChanged(object sender, ItemChangedEventArgs<WeaponHash> e)
        {
            _secondaryWeapon = e.Object;
        }

        private void OnPrimaryWeaponChanged(object sender, ItemChangedEventArgs<WeaponHash> e)
        {
            _primaryWeapon = e.Object;
        }

        private void OnBodyguardChanged(object sender, ItemChangedEventArgs<PedHash> e)
        {
            _currentBodyguardPedHash = e.Object;
        }

        private void ToggleBodyguardsInvincible(object sender, EventArgs e)
        {
            bool invincible = _bodyguardsInvincibleCheckbox.Checked;
            foreach (Ped ped in _spawnedBodyguards)
            {
                ped.IsInvincible = invincible;
            }
        }

        private void DismissBodyguards(object sender, EventArgs e)
        {
            foreach (Ped ped in _spawnedBodyguards)
            {
                if (ped.Exists() && ped.IsAlive)
                {
                    ped.LeaveGroup();
                    ped.MarkAsNoLongerNeeded();
                    ped.Task.ClearAll();
                }
                else if (ped.Exists())
                {
                    ped.MarkAsNoLongerNeeded();
                }
            }
            _spawnedBodyguards.Clear();
        }

        private void SpawnBodyguard(object sender, EventArgs e)
        {
            Vector3 spawnPos = Game.Player.Character.Position.Around(5f);
            Ped ped = World.CreatePed(_currentBodyguardPedHash, spawnPos);
            // If too many peds are spawned.
            if (ped == null) return;
            GiveWeapons(ped);
            PlayerPed.PedGroup.Add(ped, false);
            _spawnedBodyguards.Add(ped);
            ped.RelationshipGroup = PlayerPed.RelationshipGroup;
            ped.IsInvincible = _bodyguardsInvincibleCheckbox.Checked;
        }

        private void GiveWeapons(Ped ped)
        {
            ped.Weapons.Give(_primaryWeapon, 1000, true, true);
            ped.Weapons.Give(_secondaryWeapon, 1000, false, true);
        }
    }
}

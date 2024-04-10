using EasyScript.Utils;
using GTA;
using LemonUI.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils;

namespace EasyScript
{
    class WeaponMenu : Submenu
    {
        private readonly NativeCheckboxItem _infiniteAmmoCheckbox;
        private readonly NativeCheckboxItem _infiniteClipCheckbox;
        private readonly NativeCheckboxItem _randomWeaponsOnRespawnCheckbox;
        private readonly NativeCheckboxItem _randomWeaponsOnMissionStartCheckbox;
        private readonly List<WeaponComponentItem> _weaponComponentItems = new List<WeaponComponentItem>();
        private readonly List<WeaponItem> _weaponItems = new List<WeaponItem>(110);

        public WeaponMenu(MenuPool pool, NativeMenu parent, string title) : base(pool, parent, title)
        {
            CreateItem("Get All Weapons", (o, e) => WeaponUtils.GivePlayerAllWeapons());
            CreateWeaponSelectMenu();
            CreateModifyWeaponMenu();
            CreateItem("Max Ammo", (o, e) => WeaponUtils.PlayerMaxAmmoForAllWeapons());
            CreateItem("Replace with Random Weapons", (o, e) => WeaponUtils.ReplaceWeaponsWithRandom(3, -1));
            CreateItem("Give Peds in Group Weapons", GivePedsInGroupWeapons);
            CreateItem("Remove All Weapons", RemoveAllWeapons);
            _infiniteAmmoCheckbox = CreateCheckbox("Infinite Ammo", InfiniteAmmo);
            _infiniteClipCheckbox = CreateCheckbox("Infinite Clip", InfiniteClip);
            _randomWeaponsOnRespawnCheckbox = CreateCheckbox("Random Weapons on Respawn");
            _randomWeaponsOnMissionStartCheckbox = CreateCheckbox("Random Weapons on Mission Start");
            Pool.GameEvents.PlayerRespawned += OnPlayerRespawned;
            Pool.GameEvents.MissionStarted += OnMissionStarted;
            Pool.GameEvents.Weapon.ValueChanged += OnWeaponChanged;
        }

        private void CreateWeaponSelectMenu()
        {
            var weaponSelectMenu = new Submenu(Pool, Menu, "Weapons");
            var weapons = GeneralUtils.ConvertToArray<WeaponHash>();
            foreach (var weapon in weapons)
            {
                if (weapon == WeaponHash.Unarmed) continue;
                var item = new WeaponItem(weapon);
                weaponSelectMenu.AddItem(item);
                _weaponItems.Add(item);
            }

            weaponSelectMenu.Menu.Shown += (o, e) =>
            {
                foreach (var item in _weaponItems)
                {
                    item.Update();
                }
            };
        }

        private void CreateModifyWeaponMenu()
        {
            var modifyWeaponMenu = new Submenu(Pool, Menu, "Modify");

            Weapon weapon = PlayerPed.Weapons.Current;
            var components = GeneralUtils.ConvertToArray<WeaponComponentHash>();
            foreach (var component in components)
            {
                if (component == WeaponComponentHash.Invalid) continue;
                _weaponComponentItems.Add(new WeaponComponentItem(component));
            }

            modifyWeaponMenu.Menu.Shown += (o, e) =>
            {
                Weapon currentWeapon = PlayerPed.Weapons.Current;
                if (currentWeapon == null) return;
                foreach (var item in _weaponComponentItems)
                {
                    if (item.HasComponent())
                    {
                        if (!modifyWeaponMenu.Menu.Contains(item))
                            modifyWeaponMenu.Menu.Add(item);
                        item.Update();
                    }
                    else
                    {
                        modifyWeaponMenu.Menu.Remove(item);
                    }
                }
            };
        }

        private void GivePedsInGroupWeapons(object sender, EventArgs e)
        {
            foreach (Ped ped in PlayerPed.PedGroup)
            {
                ped.Weapons.Give(WeaponHash.Pistol, 9999, false, true);
                ped.Weapons.Give(WeaponHash.CarbineRifle, 9999, true, true);
            }
        }

        private void OnMissionStarted(object sender, EventArgs e)
        {
            if (_randomWeaponsOnMissionStartCheckbox.Checked)
            {
                WeaponUtils.ReplaceWeaponsWithRandom(3, -1);
            }
        }

        private void OnPlayerRespawned(object sender, EventArgs e)
        {
            if (_randomWeaponsOnRespawnCheckbox.Checked)
            {
                WeaponUtils.ReplaceWeaponsWithRandom(3, -1);
            }
        }

        private void OnWeaponChanged(object sender, Weapon e)
        {
            if (e == null || e.Model == WeaponHash.Unarmed) return;
            InfiniteAmmo(_infiniteAmmoCheckbox.Checked);
            InfiniteClip(_infiniteClipCheckbox.Checked);
        }

        private void GiveAllWeapons(object sender, EventArgs e)
        {
            var weapons = Enum.GetValues(typeof(WeaponHash)).Cast<WeaponHash>();
            foreach (var weapon in weapons)
            {
                PlayerPed.Weapons.Give(weapon, 9999, true, true);
            }
        }

        private void RemoveAllWeapons(object sender, EventArgs e)
        {
            PlayerPed.Weapons.RemoveAll();
        }

        private void InfiniteAmmo(bool value)
        {
            PlayerPed.Weapons.Current.InfiniteAmmo = value;
        }

        private void InfiniteClip(bool value)
        {
            PlayerPed.Weapons.Current.InfiniteAmmoClip = value;
        }

        class WeaponItem : NativeItem
        {
            private Weapon WeaponObject => PlayerPed.Weapons[_weapon];
            private readonly WeaponHash _weapon;
            private int _lastAmmo = -1;

            public WeaponItem(WeaponHash weapon) : base("")
            {
                _weapon = weapon;
                Activated += OnActivated;
                Title = GeneralUtils.AddSpaceForEachCapitalLetter(weapon.ToString());
            }

            private void OnActivated(object sender, EventArgs e)
            {
                if (HasWeapon())
                {
                    PlayerPed.Weapons.Remove(_weapon);
                    AltTitle = "";
                    _lastAmmo = -1;
                    Update();
                    return;
                }
                PlayerPed.Weapons.Give(_weapon, 9999, true, true);
                Update();
            }

            public bool HasWeapon()
            {
                return PlayerPed.Weapons.HasWeapon(_weapon);
            }

            public void Update()
            {
                if (HasWeapon() && _lastAmmo != WeaponObject.Ammo)
                {
                    AltTitle = WeaponObject.Ammo.ToString();
                    _lastAmmo = WeaponObject.Ammo;
                    Colors = MenuColors.GreenTitle;
                    return;
                }
                else if (!HasWeapon())
                {
                    Colors = MenuColors.Default;
                }
            }
        }

        class WeaponComponentItem : NativeItem
        {
            private readonly WeaponComponentHash _componentHash;
            private WeaponComponent Component => PlayerPed.Weapons.Current.Components[_componentHash];

            public WeaponComponentItem(WeaponComponentHash component) : base(component.ToString())
            {
                _componentHash = component;
                Activated += OnActivated;
            }

            private void OnActivated(object sender, EventArgs e)
            {
                if (PlayerPed.Weapons.Current.Hash == WeaponHash.Unarmed || Component == null) return;
                Component.Active = !Component.Active;
                Colors = Component.Active ? MenuColors.GreenTitle : MenuColors.Default;
            }

            public bool HasComponent()
            {
                return PlayerPed.Weapons.Current.Components[_componentHash] != WeaponComponentHash.Invalid;
            }

            public void Update()
            {
                Colors = Component.Active ? MenuColors.GreenTitle : MenuColors.Default;
            }
        }
    }
}

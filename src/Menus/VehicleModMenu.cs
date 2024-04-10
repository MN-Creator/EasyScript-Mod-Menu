using GTA;
using LemonUI.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils;

namespace EasyScript
{
    internal class VehicleModMenu : Submenu
    {
        private Vehicle CurrentVehicle => PlayerPed.CurrentVehicle ?? Game.Player.LastVehicle;
        private bool IsVehicleValid => CurrentVehicle != null && CurrentVehicle.Exists();
        private static readonly string[] _openDoorsWords = new string[] { "DoorSpeakers", "Seats", "Dashboard" };
        private static readonly string[] _openHoodWords = new string[] { "EngineBlock", "AirFilter", "Struts" };
        private static readonly string[] _openTrunkWords = new string[] { "Trunk", "Hydraulics" };
        private readonly NativeListItem<VehicleColor> _primaryColorItem;
        private readonly NativeListItem<VehicleColor> _secondaryColorItem;
        private readonly NativeListItem<VehicleColor> _pearlescentColorItem;
        private readonly List<ModList> _modLists = new List<ModList>();

        public VehicleModMenu(MenuPool pool, NativeMenu parent, string title) : base(pool, parent, title)
        {
            _primaryColorItem = CreateList("Primary Color", PrimaryColorChanged, GeneralUtils.ConvertToArray<VehicleColor>());
            _secondaryColorItem = CreateList("Secondary Color", SecondaryColorChanged, GeneralUtils.ConvertToArray<VehicleColor>());
            _pearlescentColorItem = CreateList("Pearlescent Color", OnPearlescentColorChanged, GeneralUtils.ConvertToArray<VehicleColor>());
            CreateItem("Reset to Default", ResetVehicleMods);

            CreateModLists();

            Menu.Shown += OnMenuOpen;
            Menu.SelectedIndexChanged += OnIndexChanged;
            Pool.GameEvents.Vehicle.ValueChanged += OnVehicleChanged;
        }

        private void ResetVehicleMods(object sender, EventArgs e)
        {
            if (!IsVehicleValid) return;
            CurrentVehicle.Mods.ClearCustomPrimaryColor();
            CurrentVehicle.Mods.ClearCustomSecondaryColor();
            foreach (ModList modList in _modLists)
            {
                VehicleUtils.SetVehicleMod(CurrentVehicle, modList.ModType, -1);
                modList.SelectedIndex = 0;
            }
        }

        private void OnVehicleChanged(object sender, Vehicle e)
        {
            if (!Menu.Visible) return;
            if (e == null || !e.Exists() || !IsVehicleValid)
            {
                Menu.Back();
                return;
            }
        }

        private void CreateModLists()
        {
            var mods = GeneralUtils.ConvertToArray<VehicleModType>();
            int[] modsArray = Enumerable.Range(0, 100).ToArray();
            foreach (VehicleModType modType in mods)
            {
                string modTitle = $"{modType}";
                var modList = new ModList(modTitle, modType, modsArray);
                modList.ItemChanged += (a, o) => ModListChanged(modType, a, o);
                modList.Activated += (a, o) => modList.SelectedIndex++;
                _modLists.Add(modList);
            }
            _modLists.Sort((a, b) => a.Title.CompareTo(b.Title));
        }

        private void OnMenuOpen(object sender, EventArgs e)
        {
            if (!IsVehicleValid) return;
            CurrentVehicle.Mods.InstallModKit();
            Menu.SelectedIndex = 0;
            _primaryColorItem.SelectedItem = CurrentVehicle.Mods.PrimaryColor;
            _secondaryColorItem.SelectedItem = CurrentVehicle.Mods.SecondaryColor;
            _pearlescentColorItem.SelectedItem = CurrentVehicle.Mods.PearlescentColor;

            UpdateModItemsForVehicle();
        }

        private void UpdateModItemsForVehicle()
        {
            for (int i = 0; i < _modLists.Count; i++)
            {
                ModList modList = _modLists[i];
                modList.Enabled = CurrentVehicle.Mods[modList.ModType].Count > 0;
                if (modList.Enabled)
                {
                    modList.SelectedItem = CurrentVehicle.Mods[modList.ModType].Index + 1;
                    if (!Menu.Contains(modList))
                    {
                        Menu.Add(modList);
                    }
                    continue;
                }
                modList.SelectedIndex = 0;
                Menu.Remove(modList);
            }
        }

        private void OnIndexChanged(object sender, SelectedEventArgs e)
        {
            if (!IsVehicleValid) return;
            bool isMovingSlow = (CurrentVehicle.Speed * 3.6f) < 50;
            VehicleUtils.CloseAllDoors(CurrentVehicle);
            if (isMovingSlow && Menu.SelectedItem is NativeListItem<int> currentList && currentList.Enabled)
            {
                if (ContainsAnyWord(currentList.Title, _openHoodWords))
                {
                    CurrentVehicle.Doors[VehicleDoorIndex.Hood].Open(false, false);
                    VehicleUtils.OpenDoor(CurrentVehicle, VehicleDoorIndex.Hood);
                    return;
                }
                else if (ContainsAnyWord(currentList.Title, _openDoorsWords))
                {
                    VehicleUtils.OpenDoor(CurrentVehicle, VehicleDoorIndex.FrontLeftDoor);
                    VehicleUtils.OpenDoor(CurrentVehicle, VehicleDoorIndex.FrontRightDoor);
                    VehicleUtils.OpenDoor(CurrentVehicle, VehicleDoorIndex.BackLeftDoor);
                    VehicleUtils.OpenDoor(CurrentVehicle, VehicleDoorIndex.BackRightDoor);
                    return;
                }
                else if (ContainsAnyWord(currentList.Title, _openTrunkWords))
                {
                    VehicleUtils.OpenDoor(CurrentVehicle, VehicleDoorIndex.Trunk);
                    return;
                }
            }
        }

        private static bool ContainsAnyWord(string title, string[] words)
        {
            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];
                if (title.Contains(word)) return true;
            }
            return false;
        }

        private void ModListChanged(VehicleModType modType, object sender, ItemChangedEventArgs<int> e)
        {
            if (!IsVehicleValid) return;
            int index = e.Object - 1;
            int modCount = CurrentVehicle.Mods[modType].Count;
            var modList = (ModList)sender;

            if (index >= 98)
            {
                modList.SelectedIndex = modCount;
                return;
            }

            if (index >= modCount)
            {
                modList.SelectedItem = 0;
                return;
            }
            VehicleUtils.SetVehicleMod(CurrentVehicle, modType, index);
        }

        private void PrimaryColorChanged(object _, ItemChangedEventArgs<VehicleColor> e)
        {
            if (!IsVehicleValid) return;
            CurrentVehicle.Mods.PrimaryColor = e.Object;
        }

        private void SecondaryColorChanged(object _, ItemChangedEventArgs<VehicleColor> e)
        {
            if (!IsVehicleValid) return;
            CurrentVehicle.Mods.SecondaryColor = e.Object;
        }

        private void OnPearlescentColorChanged(object sender, ItemChangedEventArgs<VehicleColor> e)
        {
            if (!IsVehicleValid) return;
            CurrentVehicle.Mods.PearlescentColor = e.Object;
        }

        public class ModList : NativeListItem<int>
        {
            public VehicleModType ModType { get; private set; }

            public ModList(string title, VehicleModType modType, params int[] values) : base(title, values)
            {
                ModType = modType;
            }
        }
    }
}

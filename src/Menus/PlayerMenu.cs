using GTA;
using LemonUI.Menus;
using System;
using Utils;

namespace EasyScript
{
    class PlayerMenu : Submenu, IUpdate
    {
        private readonly NativeCheckboxItem _invincibleCheckbox;
        private readonly NativeCheckboxItem _neverWantedCheckbox;
        private readonly NativeCheckboxItem _spawnSpecialPoliceCheckbox;
        private readonly NativeListItem<int> _wantedLevelList;
        private readonly ExtraPoliceSpawner _specializedPoliceSpawner = new ExtraPoliceSpawner();
        private bool _playerDeadLastFrame;

        public PlayerMenu(MenuPool pool, NativeMenu parent, string title) : base(pool, parent, title)
        {
            CreateItem("Full Health & Armor", PlayerUtils.FullHealthAndArmor);
            CreateItem("Clear Wanted Level", ClearWantedLevel);
            _invincibleCheckbox = CreateCheckbox("Invincible", ToggleInvincible);
            _invincibleCheckbox.Checked = SettingsManager.GetValue("Player", "invincible", false);
            Game.Player.IsInvincible = _invincibleCheckbox.Checked;
            _neverWantedCheckbox = CreateCheckbox("Never Wanted", ToggleNeverWanted);
            _neverWantedCheckbox.Checked = SettingsManager.GetValue("Player", "never_wanted", false);
            if (_neverWantedCheckbox.Checked)
            {
                PlayerUtils.SetNeverWanted();
            }
            _wantedLevelList = CreateList("Wanted Level", WantedLevelChanged, 0, 1, 2, 3, 4, 5);
            CreateChangeModelMenu();
            _spawnSpecialPoliceCheckbox = CreateCheckbox("Spawn Extra Police", ToggleSpawnExtraPolice);
            CreateItem("Kill Player", PlayerPed.Kill);
            _wantedLevelList.SelectedIndex = Game.Player.WantedLevel;
            CreatePropertyPanel();
            Pool.Aborted += OnAbort;
        }

        protected override void OnMainMenuOpen(object sender, EventArgs e)
        {
            base.OnMainMenuOpen(sender, e);
            _wantedLevelList.SelectedIndex = Game.Player.WantedLevel;
        }

        private void OnAbort(object sender, EventArgs e)
        {
            Game.Player.IsInvincible = false;
            Game.MaxWantedLevel = 5;
            _specializedPoliceSpawner.StopSpawning();
        }

        private void CreateChangeModelMenu()
        {
            var modelMenu = new Submenu(Pool, Menu, "Change Model");
            var michael = modelMenu.CreateItem("Michael", (a, o) => Game.Player.ChangeModel(PedHash.Michael));
            var franklin = modelMenu.CreateItem("Franklin", (a, o) => Game.Player.ChangeModel(PedHash.Franklin));
            var trevor = modelMenu.CreateItem("Trevor", (a, o) => Game.Player.ChangeModel(PedHash.Trevor));
            michael.Colors = MenuColors.GetColorSetForCharacter(PedHash.Michael);
            franklin.Colors = MenuColors.GetColorSetForCharacter(PedHash.Franklin);
            trevor.Colors = MenuColors.GetColorSetForCharacter(PedHash.Trevor);
            michael.UseCustomBackground = true;
            franklin.UseCustomBackground = true;
            trevor.UseCustomBackground = true;

            var models = GeneralUtils.ConvertToArray<PedHash>();
            foreach (var model in models)
            {
                var item = modelMenu.CreateItem($"{model}", (a, o) => Game.Player.ChangeModel(model));
                if (PlayerUtils.IsPlayerCharacter(model))
                {
                    item.UseCustomBackground = true;
                    item.Colors = MenuColors.GetColorSetForCharacter(model);
                }
            }
        }

        private void CreatePropertyPanel()
        {
            var panel = new PropertyPanel();
            var character = panel.Add("Character", () => $"{(PedHash)PlayerPed.Model.Hash}");
            panel.Add("Health", () => PlayerPed.Health);
            panel.Add("Armor", () => PlayerPed.Armor);
            panel.Add("Money", () => Game.Player.Money);
            panel.Add("Street", () => World.GetStreetName(PlayerPed.Position));
            SubmenuItem.Panel = panel;
        }

        private void ClearWantedLevel(object sender, EventArgs e)
        {
            Game.Player.WantedLevel = 0;
            _wantedLevelList.SelectedIndex = 0;
        }

        private void WantedLevelChanged(object sender, ItemChangedEventArgs<int> e)
        {
            if (!_neverWantedCheckbox.Checked)
            {
                Game.Player.WantedLevel = e.Object;
                return;
            }
            var checkbox = (NativeListItem<int>)sender;
            checkbox.SelectedIndex = 0;
        }

        private void ToggleInvincible(object sender, EventArgs e)
        {
            Game.Player.IsInvincible = _invincibleCheckbox.Checked;
            SettingsManager.SetValue("Player", "Invincible", _invincibleCheckbox.Checked);
        }

        private void ToggleNeverWanted(object sender, EventArgs e)
        {
            SettingsManager.SetValue("Player", "Never Wanted", _neverWantedCheckbox.Checked);
            if (_neverWantedCheckbox.Checked)
            {
                Game.Player.WantedLevel = 0;
                Game.MaxWantedLevel = 0;
                return;
            }
            Game.MaxWantedLevel = 5;
        }

        private void ToggleSpawnExtraPolice(object sender, EventArgs e)
        {
            if (_spawnSpecialPoliceCheckbox.Checked)
            {
                _specializedPoliceSpawner.StartSpawning();
                return;
            }
            _specializedPoliceSpawner.StopSpawning();
        }

        // Certain peds can cause an infinite death loop.
        private void AvoidInfiniteDeathLoop()
        {
            if (!_playerDeadLastFrame && Game.Player.IsDead)
            {
                _playerDeadLastFrame = true;
            }
            else if (_playerDeadLastFrame && Game.Player.IsAlive)
            {
                if (!PlayerUtils.IsPlayerCharacter())
                {
                    Game.Player.ChangeModel(PedHash.Franklin);
                }
                _playerDeadLastFrame = false;
            }
        }

        public void Update()
        {
            _specializedPoliceSpawner.Update();
            if (_invincibleCheckbox.Checked)
            {
                Game.Player.IsInvincible = true;
            }
            if (_neverWantedCheckbox.Checked)
            {
                Game.Player.WantedLevel = 0;
            }
            AvoidInfiniteDeathLoop();
        }
    }
}

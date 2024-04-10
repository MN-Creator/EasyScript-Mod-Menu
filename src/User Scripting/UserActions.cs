using EasyScript.Extensions;
using EasyScript.Utils;
using GTA;
using GTA.Math;
using System.Collections.Generic;
using System.Linq;
using Utils;

namespace EasyScript.UserScripting
{
    public enum ActionCategory
    {
        Player,
        Tasks,
        Weapon,
        Vehicle,
        Teleport,
        Locations,
        Peds,
        Audio,
        Menu,
        Misc
    }

    internal static class UserActions
    {
        private static Ped PlayerPed => Game.Player.Character;
        private static Vehicle PlayerVehicle => PlayerPed.CurrentVehicle;
        private const string _toggleIndicatorsCameraDesc =
            "Enable left/right indicator based on the direction you're looking in.";
        private const string _pressButtonDesc =
            "Some buttons may only work when the menu is closed.";


        public static UserAction[] PlayerActions =
        {
            new SimpleAction("Full Health", PlayerUtils.FullHealth),
            new SimpleAction("Full Health and Armor", PlayerUtils.FullHealthAndArmor),
            new NumberAction("Set Health", (n) => PlayerPed.Health = n, 200),
            new NumberAction("Set Armor", (n) => PlayerPed.Armor = n, 200),
            new SimpleAction("Refill Special Ability", Game.Player.RefillSpecialAbility),
            new SimpleAction("Deplete Special Ability", Game.Player.DepleteSpecialAbility),
            new SimpleAction("Max Wanted Level", () => Game.MaxWantedLevel = 5),
            new SimpleAction("Increase Wanted Level", () => Game.Player.WantedLevel++),
            new SimpleAction("Decrease Wanted Level", () => Game.Player.WantedLevel--),
            new NumberAction("Set Wanted Level", (n) => Game.Player.WantedLevel = n, 5),
            new SimpleAction("Clear Wanted Level", () => Game.Player.WantedLevel = 0),
            new SimpleAction("Random Outfit", PlayerPed.Style.RandomizeOutfit),
            new SimpleAction("Random Outfit Props", PlayerPed.Style.RandomizeProps),
            new SimpleAction("Remove Outfit Props", PlayerPed.Style.ClearProps),
            new SimpleAction("Set Default Clothes", () => PlayerPed.Style.SetDefaultClothes()),
            new SimpleAction("Enable Ignored By Everyone", () => Game.Player.IgnoredByEveryone = true),
            new SimpleAction("Disable Ignored By Everyone", () => Game.Player.IgnoredByEveryone = false),
            new SimpleAction("Add 50.000$", () => Game.Player.Money += 50000),
            new SimpleAction("Remove 50.000$", () => Game.Player.Money -= 50000),
            new NumberAction("Set Money", (n) => Game.Player.Money = n, int.MaxValue),
            new SimpleAction("Kill Player", PlayerPed.Kill),
        };

        public static UserAction[] TaskActions =
        {
            new SimpleAction("Enter Closest Vehicle", TaskUtils.PlayerEnterClosestVehicle),
            new SimpleAction("Enter Closest Empty Vehicle", TaskUtils.PlayerEnterClosestEmptyVehicle),
            new SimpleAction("Enter Last Vehicle", TaskUtils.PlayerEnterLastVehicle),
            new SimpleAction("Warp Into Last Vehicle", TaskUtils.PlayerWarpIntoLastVehicle),
            new SimpleAction("Exit Vehicle", () => PlayerPed.Task.LeaveVehicle()),
            new SimpleAction("Drive To Waypoint", TaskUtils.PlayerDriveToWaypoint),
            new SimpleAction("Drive To Mission Start", TaskUtils.PlayerDriveToMissionStart),
            new SimpleAction("Drive To Mission", TaskUtils.PlayerDriveToMission),
            new SimpleAction("Drive To Blue Marker", TaskUtils.PlayerDriveToBlueMarker),
            new SimpleAction("Wander Around", PlayerPed.Task.WanderAround),
            new SimpleAction("Drive Around", TaskUtils.PlayerDriveAround),
            new SimpleAction("Stop Task", PlayerPed.Task.ClearAll),
            new SimpleAction("Cower", () => PlayerPed.Task.Cower(-1)),
        };

        public static UserAction[] WeaponActions =
        {
            new SimpleAction("Give All Weapons", WeaponUtils.GivePlayerAllWeapons),
            new SimpleAction("Remove All Weapons", WeaponUtils.RemovePlayersWeapons),
            new SimpleAction("Replace with Random Weapons", () => WeaponUtils.ReplaceWeaponsWithRandom(3, -1)),
            new NumberAction("Set Ammo", (n) => PlayerPed.Weapons.Current.Ammo = n, 9999),
            new EnumAction<WeaponHash>("Get Weapon",
                (hash) => PlayerPed.Weapons.Give(hash, 9999, true, true), WeaponHash.Pistol),
            new EnumAction<WeaponHash>("Remove Weapon", (hash) => PlayerPed.Weapons.Remove(hash), WeaponHash.Pistol),
        };

        public static UserAction[] VehicleActions =
        {
            new SimpleAction("Repair Vehicle", VehicleUtils.RepairPlayerVehicle),
            new SimpleAction("Clean Vehicle", VehicleUtils.CleanPlayerVehicle),
            new VehicleAction("Spawn Vehicle", (v) => VehicleUtils.ReplacePlayersVehicle(v)),
            new VehicleAction("Spawn Vehicle in Front", (v) => VehicleUtils.SpawnInFrontOfPlayer(v, 5f)),
            new SimpleAction("Bring to Halt", () => PlayerPed.CurrentVehicle?.BringToHalt(5f, 1)),
            new SimpleAction("Enable/Disable Radio", VehicleUtils.TogglePlayerVehicleRadio),
            new SimpleAction("Enable seatbelt", () => PlayerPed.CanFlyThroughWindscreen = false),
            new SimpleAction("Disable seatbelt", () => PlayerPed.CanFlyThroughWindscreen = true),
            new SimpleAction("Toggle Left Indicator", () => VehicleUtils.ToggleLeftIndicator(PlayerVehicle)),
            new SimpleAction("Toggle Right Indicator", () => VehicleUtils.ToggleRightIndicator(PlayerVehicle)),
            new SimpleAction("Toggle Indicators with Camera", _toggleIndicatorsCameraDesc,
                () => VehicleUtils.TogglePlayerIndicatorBasedOnCameraDirection()),
            new SimpleAction("Toggle Indicators with Camera (Inverse)", _toggleIndicatorsCameraDesc,
                () => VehicleUtils.TogglePlayerIndicatorBasedOnCameraDirection(inverse: true)),
            new SimpleAction("Enable/Disable Hazard Lights", () => VehicleUtils.ToggleHazardLights(PlayerVehicle)),
            new SimpleAction("Enable/Disable Engine", VehicleUtils.ToggleEngine),
            new SimpleAction("Enable/Disable Lights", VehicleUtils.ToggleLights),
            new SimpleAction("Open/Close All Doors", VehicleUtils.ToggleAllDoors),
            new SimpleAction("Open/Close Hood", VehicleUtils.ToggleHood),
            new SimpleAction("Open/Close Trunk", VehicleUtils.ToggleTrunk),
            new SimpleAction("Remove Traffic",
                () => World.GetAllVehicles().ForEach(v => v.Delete(), v => v.Driver != null && v != PlayerPed.CurrentVehicle)),
            new SimpleAction("Remove All Vehicles",
                () => World.GetAllVehicles().ForEach(v => v.Delete(), (v) => v != PlayerPed.CurrentVehicle)),
        };

        public static UserAction[] TeleportActions =
        {
            new SimpleAction("Teleport to Closest Road", TeleportUtils.TeleportPlayerToClosestRoad),
            new SimpleAction("Teleport to Closest Sidewalk", TeleportUtils.TeleportPlayerToClosestSidewalk),
            new SimpleAction("Teleport to Waypoint", TeleportUtils.TeleportToWaypoint),
            new SimpleAction("Teleport to Mission", TeleportUtils.TeleportToMission),
            new SimpleAction("Teleport to Mission Start", TeleportUtils.TeleportToMissionStart),
            new SimpleAction("Teleport to Blue Marker", TeleportUtils.TeleportToBlueMarker),
        };

        public static UserAction[] AudioActions =
        {
            new SimpleAction("Say Hi", () => PlayerPed.PlayAmbientSpeech("GENERIC_HI", SpeechModifier.Force)),
            new SimpleAction("Enable Player Pain Audio",() => PlayerPed.IsPainAudioEnabled = true),
            new SimpleAction("Disable Player Pain Audio", () => PlayerPed.IsPainAudioEnabled = false),
        };

        public static UserAction[] PedActions =
        {
            new SimpleAction("Spawn Random Ped", PedUtils.SpawnRandomPedWithPlayer),
            new SimpleAction("Peds Attack Player", PedUtils.AllPedsAttackPlayer),
            new EnumAction<WeaponHash>("Give All Peds Weapon", (w) => PedUtils.GiveAllPedsWeapon(w)),
            new SimpleAction("Give All Peds Random Weapon", () => PedUtils.GiveAllPedsWeapon(WeaponUtils.RandomWeapon())),
            new SimpleAction("Resurrect All Peds", PedUtils.ResurrectAllPeds),
            new SimpleAction("Kill All Peds", PedUtils.KillAllPeds),
        };

        public static UserAction[] MenuActions =
        {
            new SimpleAction("Toggle Menu", () => Main.ToggleMenu()),
            new SimpleAction("Open Menu", () => Main.OpenMenu()),
            new SimpleAction("Close Menu", () => Main.CloseMenu()),
        };

        public static UserAction[] MiscActions =
        {
            new EnumAction<Control>("Press Button", _pressButtonDesc, (c) => Game.SetControlValueNormalized(c, 1f)),
            new SimpleAction("Remove Waypoint", World.RemoveWaypoint),
            new SimpleAction("Enable All Lights", () => World.Blackout = false),
            new SimpleAction("Disable All Lights", () => World.Blackout = true),
            new TextAction("Show Subtitle", (e) => GTA.UI.Screen.ShowSubtitle(e), "Hello World"),
            new TextAction("Show Notification", (e) => GTA.UI.Notification.Show(e), "Hello World"),
            new SimpleAction("Show Save Menu", Game.ShowSaveMenu),
            new SimpleAction("Toggle Minimap", UIUtils.ToggleMinimap),
            new SimpleAction("Show Minimap", UIUtils.ShowMinimap),
            new SimpleAction("Hide Minimap", UIUtils.HideMinimap),
        };

        public static void ResetEffects()
        {
            Game.Player.IgnoredByEveryone = false;
            PlayerPed.Task.ClearAll();
            PlayerPed.Style.SetDefaultClothes();
            PlayerPed.IsPainAudioEnabled = true;
            World.Blackout = false;
        }

        private static SimpleAction[] CreateTeleportLocations()
        {
            List<SimpleAction> actions = new List<SimpleAction>(LocationUtils.Locations.Count);
            foreach (KeyValuePair<string, Vector3> location in LocationUtils.Locations)
            {
                string name = $"Teleport to {location.Key}";
                Vector3 pos = location.Value;
                var scriptAction = new SimpleAction(name, () => TeleportUtils.TeleportPlayerToPosition(pos));
                actions.Add(scriptAction);
            }
            return actions.ToArray();
        }

        private static SimpleAction[] TeleportLocations => CreateTeleportLocations();

        private static UserAction[] GetAllActions()
        {
            var allActions = new List<UserAction>(105);
            allActions.AddRange(PlayerActions);
            allActions.AddRange(TaskActions);
            allActions.AddRange(WeaponActions);
            allActions.AddRange(VehicleActions);
            allActions.AddRange(TeleportActions);
            allActions.AddRange(TeleportLocations);
            allActions.AddRange(PedActions);
            allActions.AddRange(AudioActions);
            allActions.AddRange(MenuActions);
            allActions.AddRange(MiscActions);
            Logger.Log($"All actions: {allActions.Count}");
            return allActions.ToArray();
        }

        public static readonly UserAction[] AllActions = GetAllActions();

        public static readonly string[] ScriptActionNames = AllActions.Select(c => c.Name).ToArray();
        // Create unique keys.
        public static readonly Dictionary<string, UserAction> NameToAction = AllActions
            .GroupBy(c => c.Name)
            .ToDictionary(group => group.Key, group => group.First());

        public static readonly Dictionary<ActionCategory, UserAction[]> CategoryToActions = new Dictionary<ActionCategory, UserAction[]>
        {
            {ActionCategory.Player, PlayerActions},
            {ActionCategory.Tasks, TaskActions},
            {ActionCategory.Weapon, WeaponActions},
            {ActionCategory.Vehicle, VehicleActions},
            {ActionCategory.Teleport, TeleportActions},
            {ActionCategory.Locations, TeleportLocations},
            {ActionCategory.Peds, PedActions},
            {ActionCategory.Audio, AudioActions},
            {ActionCategory.Menu, MenuActions},
            {ActionCategory.Misc, MiscActions},
        };
    }
}

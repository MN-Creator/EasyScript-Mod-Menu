using GTA;
using System.Linq;

namespace EasyScript
{
    public enum PlayerState
    {
        OnFoot,
        Swimming,
        Driving,
        Flying,
        Dead
    }

    internal class PlayerActivity : IUpdate
    {
        public float TrackingResetInterval = 30f;
        public float TrackingTime { get; private set; }
        public PlayerState CurrentState { get; private set; }
        public bool HasUsedExplosiveWeaponLately => _usedExplosiveWeaponLastInterval || _usedExplosiveWeaponThisInterval;
        private bool _usedExplosiveWeaponThisInterval;
        private bool _usedExplosiveWeaponLastInterval;
        public Vehicle CurrentVehicle => Game.Player.Character.CurrentVehicle;
        private Ped PlayerPed => Game.Player.Character;
        private readonly WeaponHash[] _explosiveWeapons =
        {
            WeaponHash.Grenade,
            WeaponHash.GrenadeLauncher,
            WeaponHash.HomingLauncher,
            WeaponHash.RPG,
            WeaponHash.StickyBomb,
            WeaponHash.ProximityMine,
            WeaponHash.PipeBomb,
            WeaponHash.Railgun,
        };

        public PlayerActivity()
        {
            CurrentState = PlayerState.OnFoot;
        }

        public PlayerState GetCurrentState()
        {
            if (Game.Player.Character.IsDead) return PlayerState.Dead;

            if (Game.Player.Character.IsInVehicle())
            {
                if (PlayerPed.IsInFlyingVehicle)
                {
                    return PlayerState.Flying;
                }
                return PlayerState.Driving;
            }
            else if (Game.Player.Character.IsTryingToEnterALockedVehicle || Game.Player.Character.IsGettingIntoVehicle)
            {
                return PlayerState.Driving;
            }
            else if (Game.Player.Character.IsSwimming)
            {
                return PlayerState.Swimming;
            }
            return PlayerState.OnFoot;
        }

        public void ResetTracking()
        {
            _usedExplosiveWeaponLastInterval = _usedExplosiveWeaponThisInterval;
            _usedExplosiveWeaponThisInterval = false;
        }

        public void Update()
        {
            TrackingTime += Game.LastFrameTime;
            if (TrackingTime >= TrackingResetInterval)
            {
                TrackingTime = 0f;
                ResetTracking();
            }
            if (PlayerPed.IsShooting && PlayerPed.Weapons.Current != null)
            {
                if (_explosiveWeapons.Contains(PlayerPed.Weapons.Current.Hash))
                {
                    _usedExplosiveWeaponThisInterval = true;
                }
            }
        }
    }
}

using Fusion;
using UnityEngine;

// orice button pui aici, va fi automat luat in calcul mai jos in GamePlayeInput -> NetworkButtons
//              (asta face intern INetworkStruct)
// In PlayerInput -> OnInput, se si pupuleaza valorile de Input
public enum EInputButtons {
    Fire = 0,
    AltFire,
    Jump,
    Reload,
    Test,
    Sprint,
    ZoomWeapon,
    ThrowGrenade,
    ThrowSmoke, 
    Interact,
    Slow
}

public struct GameplayInput : INetworkInput {
    public int WeaponSlot => WeaponButton - 1;

    public Vector2 MoveDirection;
    public Vector2 LookRotationDelta;
    public byte WeaponButton;
    public NetworkButtons Buttons;

    //public bool Sprint { get { return Buttons.IsSet(EInputButtons.Sprint); } set { Buttons.Set(EInputButtons.Sprint, value); } }
}

using Robust.Shared.GameStates; //#funkystation

namespace Content.Shared.Mutations;

[RegisterComponent, NetworkedComponent]
public sealed partial class MutationInjectComponent : Component   //this shit is for the gene injector
{
    public bool Twitch = false;
    public bool Light = false;
    public bool RedLight = false;
    public bool BlueLight = false;
    public bool RGBLight = false;
    public bool Vomit = false;
    public bool BloodVomit = false;
    public bool AcidVomit = false;
    public bool PlasmaFarter = false;
    public bool TritFarter = false;
    public bool BZFarter = false;
    public bool FireSkin = false;
    public bool TempImmune = false;
    public bool PressureImmune = false;
    public bool RadiationImmune = false;
    public bool BreathingImmune = false;
    public bool Clumsy = false;
    public bool OkayAccent = false;
    public bool Prickmode = false;

}

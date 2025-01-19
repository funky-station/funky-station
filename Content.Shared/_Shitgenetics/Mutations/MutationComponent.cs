using Robust.Shared.GameStates; //#funkystation

namespace Content.Shared.Mutations;

[RegisterComponent, NetworkedComponent]
public sealed partial class MutationComponent : Component
{

    public int MutationUpdateTimer = 0;

    public int MutationUpdateCooldown = 200;

    public bool Cancel = false;

    public int Amount = 0;

    #region Visual

    [DataField("twitch")]
    public bool Twitch = false;

    [DataField("light")]
    public bool Light = false;

    [DataField("redlight")]
    public bool RedLight = false;

    [DataField("bluelight")]
    public bool BlueLight = false;

    [DataField("rgblight")]
    public bool RGBLight = false;

    #endregion

    #region Emitting Stuff

    [DataField("vomit")]
    public bool Vomit = false;

    [DataField("vomitblood")]
    public bool BloodVomit = false;

    [DataField("vomitacid")]
    public bool AcidVomit = false;

    [DataField("plasmafarter")]
    public bool PlasmaFarter = false; //this is really funny to me

    [DataField("tritfarter")]
    public bool TritFarter = false;

    [DataField("BZfarter")]
    public bool BZFarter = false; //might be an instakill lmfao

    #endregion

    #region Body Stuff

    [DataField("fireskin")]
    public bool FireSkin = false; //lights em on fire constantly

    [DataField("tempimmune")]
    public bool TempImmune = false;

    [DataField("pressureimmune")]
    public bool PressureImmune = false;

    [DataField("radimmune")]
    public bool RadiationImmune = false;

    [DataField("clumsy")]
    public bool Clumsy = false; //aka the clown thing

    #endregion

    #region Accents

    [DataField("okayaccent")]
    public bool OkayAccent = false;

    [DataField("prickmode")]
    public bool Prickmode = false;

    #endregion
}

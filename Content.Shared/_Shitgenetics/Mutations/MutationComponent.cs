using Robust.Shared.GameStates; //#funkystation

namespace Content.Shared.Mutations;

[RegisterComponent, NetworkedComponent]
public sealed partial class MutationComponent : Component
{

    public int MutationUpdateTimer = 0;

    public int MutationUpdateCooldown = 200;

    public bool Cancel = false;

    [DataField("amount")]
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

    [DataField("EMPer")]
    public bool EMPer = false; //fuck you ipcs

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

    /// <summary>
    /// while john space doesn't employ reverse slip technique, his domain expansion, idle mutation gamble,
    /// grants him unlimited access to infinite doctors delight for as long as the mutation is active.
    /// Simply put, for as long as the mutation isnt stablized, john space is effectively, immortal.
    /// </summary>
    [DataField("selfheal")]
    public bool SelfHeal = false;

    [DataField("cancer")]
    public bool Cancer = false; //uber space cancer

    [DataField("leprosy")]
    public bool Leprosy = false; //uber space leprosy

    [DataField("slippy")]
    public bool Slippy = false; //makes em slip randomly

    [DataField("high")]
    public bool High = false; //gojo

    #endregion

    #region Accents

    [DataField("okayaccent")]
    public bool OkayAccent = false;

    [DataField("prickmode")]
    public bool Prickmode = false;

    [DataField("owoaccent")]
    public bool OWOAccent = false; //might kill myself after this

    [DataField("ohioaccent")]
    public bool OhioAccent = false; //I am 100% going to kill myself now

    [DataField("stutteraccent")]
    public bool StutterAccent = false;

    [DataField("scrambleaccent")]
    public bool ScrambleAccent = false;

    #endregion
}

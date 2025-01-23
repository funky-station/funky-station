using Robust.Shared.GameStates; //#funkystation

namespace Content.Shared.Genetics.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class InjectionPresetComponent : Component
{

    /// <summary>
    /// i literally just have this copying the mutation comp. theres probably a better way to do this but instead enjoy the copy and paste
    /// </summary>

    //https://www.youtube.com/watch?v=dQw4w9WgXcQ <-- important

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

    [DataField("smallsize")]
    public bool SmallSize = false;

    [DataField("bigsize")]
    public bool BigSize = false;

    [DataField("tinysize")]
    public bool TinySize = false;

    [DataField("vomit")]
    public bool Vomit = false;

    [DataField("vomitblood")]
    public bool BloodVomit = false;

    [DataField("vomitacid")]
    public bool AcidVomit = false;

    [DataField("vomitlube")]
    public bool LubeVomit = false;

    [DataField("plasmafarter")]
    public bool PlasmaFarter = false;

    [DataField("tritfarter")]
    public bool TritFarter = false;

    [DataField("BZfarter")]
    public bool BZFarter = false;

    [DataField("EMPer")]
    public bool EMPer = false;

    [DataField("explode")]
    public bool Explode = false;

    [DataField("fireskin")]
    public bool FireSkin = false;

    [DataField("tempimmune")]
    public bool TempImmune = false;

    [DataField("pressureimmune")]
    public bool PressureImmune = false;

    [DataField("radimmune")]
    public bool RadiationImmune = false;

    [DataField("clumsy")]
    public bool Clumsy = false;

    [DataField("selfheal")]
    public bool SelfHeal = false;

    [DataField("cancer")]
    public bool Cancer = false;

    [DataField("leprosy")]
    public bool Leprosy = false;

    [DataField("slippy")]
    public bool Slippy = false;

    [DataField("uberslippy")]
    public bool UberSlippy = false;

    [DataField("high")]
    public bool High = false;

    [DataField("item")]
    public bool Item = false;

    [DataField("anchorable")]
    public bool Anchorable = false;

    [DataField("blindness")]
    public bool Blindness = false;

    [DataField("eyedamage")]
    public bool EyeDamage = false;

    [DataField("okayaccent")]
    public bool OkayAccent = false;

    [DataField("prickmode")]
    public bool Prickmode = false;

    [DataField("owoaccent")]
    public bool OWOAccent = false;

    [DataField("ohioaccent")]
    public bool OhioAccent = false;

    [DataField("stutteraccent")]
    public bool StutterAccent = false;

    [DataField("scrambleaccent")]
    public bool ScrambleAccent = false;

    [DataField("backwardsaccent")]
    public bool BackwardsAccent = false;

    [DataField("mobsteraccent")]
    public bool MobsterAccent = false;
}

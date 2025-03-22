namespace Content.Server.GameTicking.Rules.Components;

[RegisterComponent, Access(typeof(SecretDirectorSystem))]
public sealed partial class SecretDirectorComponent : Component
{
    /// <summary>
    /// The gamerules that get added by secret.
    /// </summary>
    [DataField("additionalGameRules")]
    public HashSet<EntityUid> AdditionalGameRules = new();
}

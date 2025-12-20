namespace Content.Server.Fluids.Components
{
    [RegisterComponent]
    public sealed partial class PuddleFireLightComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite)]
        public TimeSpan ExtinguishTime;

        [ViewVariables]
        public EntityUid? LightEntity;
    }
}

namespace Content.Server.Atmos.Components
{
    [RegisterComponent]
    public sealed partial class GridFireLightsComponent : Component
    {
        public Dictionary<Vector2i, EntityUid> ActiveLights = new();
    }
}

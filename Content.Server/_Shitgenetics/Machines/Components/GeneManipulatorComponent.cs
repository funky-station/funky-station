using Content.Server.Machines.EntitySystems;

namespace Content.Server.Machines.Components
{
    /// <summary>
    ///     Component that represents a wall light. It has a light bulb that can be replaced when broken.
    /// </summary>
    [RegisterComponent, Access(typeof(GeneManipulatorSystem))]
    public sealed partial class GeneManipulatorComponent : Component
    {

        [DataField("CooldownNext")]
        public TimeSpan CooldownNext;

        [DataField("CooldownTime")]
        public TimeSpan CooldownTime = TimeSpan.FromSeconds(10);

        [DataField("Cooldown")]
        public bool Cooldown = false;
    }
}

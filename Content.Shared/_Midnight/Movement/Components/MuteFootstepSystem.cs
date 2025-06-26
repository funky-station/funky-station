using Content.Shared.Inventory;
using Content.Shared.Movement.Components;

public sealed class MuteFootstepSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    
    public bool ShouldMute(EntityUid uid, InputMoverComponent mover)
    {
        // Only mute when walking
        if (mover.Sprinting)
            return false;

        if (TryComp<MovementRelayTargetComponent>(uid, out var relayTarget))
        {
            uid = relayTarget.Source;
        }

        // Check player's shoes for mute component
        if (_inventory.TryGetSlotEntity(uid, "shoes", out var shoesEntity) && 
            HasComp<MuteWalkingFootstepComponent>(shoesEntity))
        {
            return true;
        }
        
        return false;
    }
}
using Content.Shared.Interaction;
using Content.Server.Chemistry.Components;
using Content.Server.Popups;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Labels.Components;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Map;
using Robust.Shared.Log;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server.Chemistry.EntitySystems
{
    public sealed partial class ClothPillConvertableSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entMan = default!;
        [Dependency] private readonly SharedSolutionContainerSystem _solutionSys = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly MetaDataSystem _metaData = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ClothPillConvertableComponent, InteractUsingEvent>(OnInteractUsing);
        }

        private void OnInteractUsing(EntityUid uid, ClothPillConvertableComponent component, InteractUsingEvent args)
        {
            var used = args.Used;

            var user = args.User;

            if (!TryGetClothType(used, out var newPillProto))
                return;

            // Get player position
            var transform = Transform(user);
            var coords = transform.Coordinates;

            // Spawn new pill at player position
            var newPill = _entMan.SpawnEntity(newPillProto, coords);
            Logger.Info($"Spawned {newPillProto} at player position");

            // Copy solution contents
            if (_entMan.HasComponent<SolutionContainerManagerComponent>(uid) &&
                _entMan.HasComponent<SolutionContainerManagerComponent>(newPill))
            {
                // Try get "food" solution from both
                if (_solutionSys.TryGetSolution(uid, "food", out _, out var oldSolution) &&
                    _solutionSys.TryGetSolution(newPill, "food", out _, out var newSolution))
                {
                    // Clear the new solution
                    newSolution.RemoveAllSolution();

                    // Copy all reagents
                    foreach (var reagent in oldSolution.Contents)
                    {
                        newSolution.AddReagent(reagent.Reagent, reagent.Quantity);
                    }
                }
                // Update entity name to include label
                if (_entMan.TryGetComponent(uid, out LabelComponent? oldLabel))
                {
                    if (!_entMan.HasComponent<LabelComponent>(newPill))
                        _entMan.AddComponent<LabelComponent>(newPill);

                    if (_entMan.TryGetComponent(newPill, out LabelComponent? newLabel))
                    {
                        newLabel.CurrentLabel = oldLabel.CurrentLabel;
                    }
                }

                _popupSystem.PopupEntity($"You finish wrapping the pill in the fabric.", newPill, Filter.Entities(user), false, PopupType.Medium);
                Timer.Spawn(1, () => _entMan.DeleteEntity(uid));
                Timer.Spawn(1, () =>
                {
                    if (_entMan.TryGetComponent(used, out StackComponent? stack) && stack.Count > 1)
                    {
                        stack.Count--;
                        stack.Dirty();
                    }
                    else
                    {
                        _entMan.DeleteEntity(used);
                    }
                });
            }
        }

        public bool TryGetClothType(EntityUid used, out string pillProto)
        {
            pillProto = string.Empty;

            if (!_entMan.TryGetComponent(used, out MetaDataComponent? meta))
                return false;

            var id = meta.EntityPrototype?.ID;

            if (id == "MaterialCloth" || id == "MaterialCloth1" || id == "MaterialCloth10")
            {
                pillProto = "CottonPill";
                var materialName = "cloth.";
                return true;
            }

            if (id == "MaterialWebSilk" || id == "MaterialWebSilk1" || id == "MaterialWebSilk25")
            {
                pillProto = "SilkPill";
                var materialName = "silk.";
                return true;
            }
            return false;
        }
    }
}

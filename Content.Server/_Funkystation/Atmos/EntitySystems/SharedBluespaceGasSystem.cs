using System.Linq;
using Content.Shared._Funkystation.CCVars;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Materials;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Content.Shared._Funkystation.Atmos.Components;
using Content.Server._Funkystation.Atmos;
using Content.Server._Funkystation.Atmos.Components;

using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;

namespace Content.Server._Funkystation.Atmos.Systems
{
    public sealed class SharedBluespaceGasSystem : EntitySystem
    {
        [Dependency] private readonly IConfigurationManager _cfg = default!;
        [Dependency] protected readonly SharedDeviceLinkSystem DeviceLink = default!;

        private bool _bluespaceGasEnabled;

        protected ProtoId<SourcePortPrototype> SourcePort = "BluespaceSender";
        protected ProtoId<SinkPortPrototype> SinkPort = "BluespaceGasUtilizer";

        public override void Initialize()
        {
            base.Initialize();

            _cfg.OnValueChanged(CCVars_Funky.BluespaceGasEnabled, enabled => _bluespaceGasEnabled = enabled, true);

            SubscribeLocalEvent<BluespaceSenderComponent, NewLinkEvent>(OnNewLink);
            SubscribeLocalEvent<BluespaceGasUtilizerComponent, PortDisconnectedEvent>(OnPortDisconnected);
        }

        private void OnPortDisconnected(Entity<BluespaceGasUtilizerComponent> ent, ref PortDisconnectedEvent args)
        {
            if (args.Port != SinkPort)
                return;

            if (!TryComp(ent, out BluespaceVendorComponent? vendor))
                return;

            vendor.BluespaceGasMixture = new();
            vendor.BluespaceSenderConnected = false;
            ent.Comp.BluespaceSender = null;
            Dirty(ent);
        }

        private void OnNewLink(Entity<BluespaceSenderComponent> ent, ref NewLinkEvent args)
        {
            if (args.SinkPort != SinkPort || args.SourcePort != SourcePort)
                return;

            if (!TryComp(args.Sink, out BluespaceGasUtilizerComponent? utilizer))
                return;

            if (utilizer.BluespaceSender != null)
                DeviceLink.RemoveSinkFromSource(utilizer.BluespaceSender.Value, args.Sink);

            if (!TryComp(args.Sink, out BluespaceVendorComponent? vendor))
                return;

            vendor.BluespaceGasMixture = ent.Comp.BluespaceGasMixture;
            vendor.BluespaceSenderConnected = true;
            utilizer.BluespaceSender = null;

            utilizer.BluespaceSender = ent;
            Dirty(args.Sink, utilizer);
        }
    }
}
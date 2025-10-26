// SPDX-FileCopyrightText: 2025 vectorassembly <vectorassembly@icloud.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared._Funkystation.Quirks;
using Content.Shared._Funkystation.Traits.Quirks;
using Content.Shared.GameTicking;
using Robust.Server.Player;

namespace Content.Server._Funkystation.Traits.Quirks;

/// <summary>
/// This handles...
/// </summary>
public sealed class NitrogenBreathingSystem : EntitySystem
{
    /// <inheritdoc/>
    [Dependency] private readonly InternalsSystem _internalsSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NitrogenBreathingSetup>(NitrogenBreathingSetup);
    }

    private void NitrogenBreathingSetup(NitrogenBreathingSetup ev)
    {
            if (TryComp<InternalsComponent>(ev.Uid, out var interalsComponent))
            {
                var gasTank = _internalsSystem.FindBestGasTank(ev.Uid);
                if (gasTank != null)
                    _internalsSystem.TryConnectTank((ev.Uid, interalsComponent), gasTank.Value.Owner);
            }
    }
}

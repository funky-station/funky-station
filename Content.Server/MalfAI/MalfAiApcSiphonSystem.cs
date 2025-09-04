// SPDX-FileCopyrightText: 2025 YourName
// SPDX-License-Identifier: MIT

using Content.Server.Store.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.MalfAI;
using Content.Shared.Popups;
using Content.Shared.Silicons.StationAi;
using Content.Server.Power.Components;
using Content.Shared.Alert;
using Content.Server.Administration.Logs;
using Content.Shared.Database;
using Robust.Shared.Localization;

namespace Content.Server.MalfAI;

/// <summary>
/// Handles the Malf AI's APC siphoning.
/// Pretty much their only method of getting CPU.
/// </summary>
public sealed class MalfAiApcSiphonSystem : EntitySystem
{
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;

    private const string CpuCurrency = "CPU";
    private const int CpuGainInt = 999;
    private static readonly FixedPoint2 SiphonAmount = FixedPoint2.New(CpuGainInt);

    public void OnApcStartSiphon(EntityUid uid, ApcComponent apc, ref ApcStartSiphonEvent args)
    {
        if (!TryComp<Content.Shared.Store.Components.StoreComponent>(args.SiphonedBy, out var store))
            return;

        // Grant CPU to the AI
        var dict = new System.Collections.Generic.Dictionary<string, FixedPoint2> { { CpuCurrency, SiphonAmount } };
        _store.TryAddCurrency(dict, args.SiphonedBy, store);

        // Log the APC siphoning for admin records
        _adminLogger.Add(LogType.Action, LogImpact.High, $"Malf AI {ToPrettyString(args.SiphonedBy)} siphoned APC {ToPrettyString(uid)} for {CpuGainInt} CPU");
    }
}

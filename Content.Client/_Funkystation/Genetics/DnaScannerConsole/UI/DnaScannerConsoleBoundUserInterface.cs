// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.UserInterface;
using Robust.Shared.GameObjects;
using Content.Shared._Funkystation.Genetics;
using Robust.Shared.Serialization;
using Content.Shared._Funkystation.Genetics.Components;
using Content.Shared._Funkystation.Genetics.Events;

namespace Content.Client._Funkystation.Genetics.DnaScannerConsole.UI;

public sealed class DnaScannerConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private DnaScannerConsoleWindow? _window;

    public DnaScannerConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<DnaScannerConsoleWindow>();

        if (EntMan.TryGetComponent(Owner, out MetaDataComponent? meta))
            _window.Title = meta.EntityName;

        _window.OnSequencerButtonPressed += (index, newBase, mutationId) =>
            SendMessage(new DnaScannerSequencerButtonPressedMessage(index, newBase, mutationId));

        _window.OnSaveMutationPressed += mutationId =>
            SendMessage(new DnaScannerSaveMutationToStorageMessage(mutationId));

        _window.OnDeleteMutationPressed += mutationId =>
            SendMessage(new DnaScannerDeleteMutationFromStorageMessage(mutationId));

        _window.OnPrintActivatorPressed += mutationId =>
            SendMessage(new DnaScannerPrintActivatorMessage(mutationId));

        _window.OnPrintMutatorPressed += mutationId =>
            SendMessage(new DnaScannerPrintMutatorMessage(mutationId));

        _window.OnScrambleDnaPressed += () =>
            SendMessage(new DnaScannerScrambleDnaMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not DnaScannerConsoleBoundUserInterfaceState scannerState)
            return;

        if (_window == null)
            return;

        _window.UpdateSubjectInfo(
            scannerState.SubjectName,
            scannerState.HealthStatus,
            scannerState.GeneticDamage,
            scannerState.SubjectGeneticInstability,
            scannerState.ScrambleCooldownEnd);

        if (scannerState.IsFullUpdate)
        {
            _window.UpdateGeneticsTab(scannerState.Mutations, scannerState.BaseMutationIds);
            _window.UpdateDiscoveredMutations(scannerState.DiscoveredMutationIds);
            _window.UpdateSavedMutations(scannerState.SavedMutations);
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _window?.Dispose();
        _window = null;
    }
}

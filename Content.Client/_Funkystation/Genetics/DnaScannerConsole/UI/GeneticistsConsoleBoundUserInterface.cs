// SPDX-FileCopyrightText: 2026 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.UserInterface;
using Robust.Shared.GameObjects;
using Content.Shared._Funkystation.Genetics;
using Content.Shared._Funkystation.Genetics.Components;
using Content.Shared._Funkystation.Genetics.Events;
using Robust.Shared.Serialization;

namespace Content.Client._Funkystation.Genetics.DnaScannerConsole.UI;

public sealed class GeneticistsConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private GeneticistsConsoleWindow? _mainWindow;

    public GeneticistsConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();

        _mainWindow = this.CreateWindow<GeneticistsConsoleWindow>();

        if (EntMan.TryGetComponent(Owner, out MetaDataComponent? meta))
            _mainWindow.Title = meta.EntityName;

        _mainWindow.OnSequencerButtonPressed += (index, newBase, mutationId) =>
            SendMessage(new DnaScannerSequencerButtonPressedMessage(index, newBase, mutationId));

        _mainWindow.OnSaveMutationPressed += mutationId =>
            SendMessage(new DnaScannerSaveMutationToStorageMessage(mutationId));

        _mainWindow.OnDeleteMutationPressed += mutationId =>
            SendMessage(new DnaScannerDeleteMutationFromStorageMessage(mutationId));

        _mainWindow.OnPrintActivatorPressed += mutationId =>
            SendMessage(new DnaScannerPrintActivatorMessage(mutationId));

        _mainWindow.OnPrintMutatorPressed += mutationId =>
            SendMessage(new DnaScannerPrintMutatorMessage(mutationId));

        _mainWindow.OnScrambleDnaPressed += () =>
            SendMessage(new DnaScannerScrambleDnaMessage());

        _mainWindow.OnToggleResearchPressed += mutationId =>
            SendMessage(new DnaScannerToggleResearchMessage(mutationId));

        _mainWindow.OnJokerUsed += () =>
            SendMessage(new DnaScannerUseJokerMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not GeneticistsConsoleBoundUserInterfaceState scannerState || _mainWindow == null)
            return;

        // Always update subject info
        _mainWindow.UpdateSubjectInfo(
            scannerState.SubjectName,
            scannerState.HealthStatus,
            scannerState.RadiationDamage,
            scannerState.SubjectGeneticInstability,
            scannerState.ScrambleCooldownEnd);

        // Always update research data
        _mainWindow.UpdateResearchData(
            scannerState.ResearchRemaining,
            scannerState.ResearchOriginal,
            scannerState.ActiveResearchMutationIds ?? new HashSet<string>());

        // Full update - refresh mutation lists
        if (scannerState.IsFullUpdate)
        {
            _mainWindow.UpdateGeneticsTab(scannerState.Mutations, scannerState.BaseMutationIds);
            _mainWindow.UpdateDiscoveredMutations(scannerState.DiscoveredMutationIds);
            _mainWindow.UpdateSavedMutations(scannerState.SavedMutations);
        }

        // Joker cooldown only affects enzymes view
        _mainWindow.UpdateResearchLabel();
        _mainWindow.UpdateJokerCooldown(scannerState.JokerCooldownEnd);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (_mainWindow != null)
        {
            _mainWindow.Dispose();
            _mainWindow = null;
        }
    }
}

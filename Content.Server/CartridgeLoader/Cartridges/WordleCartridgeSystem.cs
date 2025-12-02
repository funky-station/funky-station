// SPDX-FileCopyrightText: 2025 rex1431ify <r.l@live.se>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration.Logs;
using Content.Shared.CartridgeLoader;
using Content.Shared.CartridgeLoader.Cartridges;
using Content.Shared.Database;
using System.Linq;

namespace Content.Server.CartridgeLoader.Cartridges;

public sealed class WordleCartridgeSystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem? _cartridgeLoaderSystem = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WordleCartridgeComponent, CartridgeMessageEvent>(OnUiMessage);
        SubscribeLocalEvent<WordleCartridgeComponent, CartridgeUiReadyEvent>(OnUiReady);
    }

    /// <summary>
    /// This gets called when the ui fragment needs to be updated for the first time after activating
    /// </summary>
    private void OnUiReady(EntityUid uid, WordleCartridgeComponent component, CartridgeUiReadyEvent args)
    {
        // Initialize a new game if not already started
        if (component.SecretWord == "WORDLE")
        {
            component.SecretWord = WordleCartridgeComponent.GetRandomWord();
            component.CurrentGuess = "";
            component.PreviousGuesses = new();
            component.LetterStates = new();
            component.AttemptsRemaining = 6;
            component.GameWon = false;
            component.GameLost = false;
        }

        UpdateUiState(uid, args.Loader, component);
    }

    /// <summary>
    /// The ui messages received here get wrapped by a CartridgeMessageEvent and are relayed from the <see cref="CartridgeLoaderSystem"/>
    /// </summary>
    private void OnUiMessage(EntityUid uid, WordleCartridgeComponent component, CartridgeMessageEvent args)
    {
        if (args is not WordleUiMessageEvent message)
            return;

        // Don't process messages if game is over
        if (component.GameWon || component.GameLost)
        {
            if (message.Action == WordleUiAction.NewGame)
            {
                // Reset game
                component.SecretWord = WordleCartridgeComponent.GetRandomWord();
                component.CurrentGuess = "";
                component.PreviousGuesses = new();
                component.LetterStates = new();
                component.AttemptsRemaining = 6;
                component.GameWon = false;
                component.GameLost = false;
                _adminLogger.Add(LogType.PdaInteract, LogImpact.Low,
                    $"{ToPrettyString(args.Actor)} started a new Wordle game on: {ToPrettyString(uid)}");
            }
            UpdateUiState(uid, GetEntity(args.LoaderUid), component);
            return;
        }

        switch (message.Action)
        {
            case WordleUiAction.GuessLetter:
                if (component.CurrentGuess.Length < 5 && char.IsLetter(message.Letter))
                {
                    component.CurrentGuess += char.ToUpper(message.Letter);
                    _adminLogger.Add(LogType.PdaInteract, LogImpact.Low,
                        $"{ToPrettyString(args.Actor)} typed '{message.Letter}' in Wordle on: {ToPrettyString(uid)}");
                }
                break;

            case WordleUiAction.Backspace:
                if (component.CurrentGuess.Length > 0)
                {
                    component.CurrentGuess = component.CurrentGuess.Substring(0, component.CurrentGuess.Length - 1);
                }
                break;

            case WordleUiAction.SubmitGuess:
                if (component.CurrentGuess.Length == 5)
                {
                    ProcessGuess(uid, component, args.Actor);
                }
                break;

            case WordleUiAction.NewGame:
                component.SecretWord = WordleCartridgeComponent.GetRandomWord();
                component.CurrentGuess = "";
                component.PreviousGuesses = new();
                component.LetterStates = new();
                component.AttemptsRemaining = 6;
                component.GameWon = false;
                component.GameLost = false;
                _adminLogger.Add(LogType.PdaInteract, LogImpact.Low,
                    $"{ToPrettyString(args.Actor)} started a new Wordle game on: {ToPrettyString(uid)}");
                break;
        }

        UpdateUiState(uid, GetEntity(args.LoaderUid), component);
    }

    private void ProcessGuess(EntityUid uid, WordleCartridgeComponent component, EntityUid actor)
    {
        var guess = component.CurrentGuess.ToUpper();
        component.PreviousGuesses.Add(guess);

        // Calculate letter states
        var states = new List<int>();
        for (int i = 0; i < 5; i++)
        {
            if (guess[i] == component.SecretWord[i])
            {
                states.Add(2); // Correct position
            }
            else if (component.SecretWord.Contains(guess[i]))
            {
                states.Add(1); // Wrong position
            }
            else
            {
                states.Add(3); // Not in word
            }
        }

        component.LetterStates.Add(states);
        component.CurrentGuess = "";
        component.AttemptsRemaining--;

        // Check if won
        if (states.All(s => s == 2))
        {
            component.GameWon = true;
            _adminLogger.Add(LogType.PdaInteract, LogImpact.Low,
                $"{ToPrettyString(actor)} won a Wordle game with guess '{guess}' on: {ToPrettyString(uid)}");
        }
        // Check if lost
        else if (component.AttemptsRemaining <= 0)
        {
            component.GameLost = true;
            _adminLogger.Add(LogType.PdaInteract, LogImpact.Low,
                $"{ToPrettyString(actor)} lost a Wordle game on: {ToPrettyString(uid)}. Word was: {component.SecretWord}");
        }
    }

    private void UpdateUiState(EntityUid uid, EntityUid loaderUid, WordleCartridgeComponent? component)
    {
        if (!Resolve(uid, ref component))
            return;

        var state = new WordleUiState(
            component.CurrentGuess,
            component.PreviousGuesses,
            component.LetterStates,
            component.AttemptsRemaining,
            component.GameWon,
            component.GameLost,
            component.GameLost ? component.SecretWord : null // Only reveal word if lost
        );
        _cartridgeLoaderSystem?.UpdateCartridgeUiState(loaderUid, state);
    }
}

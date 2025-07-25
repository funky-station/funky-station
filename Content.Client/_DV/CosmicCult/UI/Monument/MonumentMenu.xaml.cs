// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Linq;
using System.Numerics;
using Content.Client.Stylesheets;
using Content.Client.UserInterface.Controls;
using Content.Shared._DV.CCVars;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared._DV.CosmicCult.Prototypes;
using Content.Shared._DV.CosmicCult;
using Robust.Client.AutoGenerated;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Client._DV.CosmicCult.UI.Monument;

[GenerateTypedNameReferences]
public sealed partial class MonumentMenu : FancyWindow
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IEntityManager _ent = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private readonly SpriteSystem _sprite;

    // All glyph prototypes
    private readonly IEnumerable<GlyphPrototype> _glyphPrototypes;
    // All influence prototypes
    private readonly IEnumerable<InfluencePrototype> _influencePrototypes;
    private readonly ButtonGroup _glyphButtonGroup;
    private ProtoId<GlyphPrototype> _selectedGlyphProtoId = string.Empty;
    private HashSet<ProtoId<GlyphPrototype>> _unlockedGlyphProtoIds = [];
    public Action<ProtoId<GlyphPrototype>>? OnSelectGlyphButtonPressed;
    public Action? OnRemoveGlyphButtonPressed;

    public Action<ProtoId<InfluencePrototype>>? OnGainButtonPressed;
    private int _entropyPerCultist = 0;

    public MonumentMenu()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);
        _sprite = _ent.System<SpriteSystem>();

        _glyphPrototypes = _proto.EnumeratePrototypes<GlyphPrototype>()
            .OrderBy(glyph => Loc.GetString(glyph.Name));
        _influencePrototypes = _proto.EnumeratePrototypes<InfluencePrototype>();

        _glyphButtonGroup = new ButtonGroup();

        RemoveGlyphButton.OnPressed += _ => OnRemoveGlyphButtonPressed?.Invoke();
        SelectGlyphButton.OnPressed += _ => OnSelectGlyphButtonPressed?.Invoke(_selectedGlyphProtoId);

        _cfg.OnValueChanged(DCCVars.CosmicCultistEntropyValue, entropy =>
        {
            _entropyPerCultist = entropy;
        },
        invokeImmediately: true);
    }

    public void UpdateState(MonumentBuiState state)
    {
        _selectedGlyphProtoId = state.SelectedGlyph;
        _unlockedGlyphProtoIds = state.UnlockedGlyphs;

        CultProgressBar.BackgroundStyleBoxOverride = new StyleBoxFlat { BackgroundColor = new Color(15, 17, 30) };
        CultProgressBar.ForegroundStyleBoxOverride = new StyleBoxFlat { BackgroundColor = new Color(91, 62, 124) };

        UpdateBar(state);
        UpdateEntropy(state);
        UpdateGlyphs();
        UpdateInfluences(state);
    }

    /// <summary>
    ///     Updates the progress bar
    /// </summary>
    private void UpdateBar(MonumentBuiState state)
    {
        var percentComplete = 100f * ((float)state.CurrentProgress / state.TargetProgress);

        percentComplete = Math.Min(percentComplete, 100f);

        CultProgressBar.Value = percentComplete;

        ProgressBarPercentage.Text = Loc.GetString("monument-interface-progress-bar", ("percentage", percentComplete.ToString("0")));
    }

    /// <summary>
    ///     Updates the entropy fields
    /// </summary>
    private void UpdateEntropy(MonumentBuiState state)
    {
        var availableEntropy = -1;
        if (_ent.TryGetComponent<CosmicCultComponent>(_player.LocalEntity, out var cultComp))
        {
            availableEntropy = cultComp.EntropyBudget;
        }

        var entropyToNextStage = Math.Max(state.TargetProgress - state.CurrentProgress, 0);
        var min = entropyToNextStage == 0 ? 0 : 1; //I have no idea what to call this. makes it so that it shows 0 crew for the final stage but at least one at all other times
        var crewToNextStage = (int)Math.Max(Math.Round((double)entropyToNextStage / _entropyPerCultist, MidpointRounding.ToPositiveInfinity), min); //force it to be at least one

        AvailableEntropy.Text = Loc.GetString("monument-interface-entropy-value", ("infused", availableEntropy));
        EntropyUntilNextStage.Text = Loc.GetString("monument-interface-entropy-value", ("infused", entropyToNextStage.ToString()));
        CrewToConvertUntilNextStage.Text = crewToNextStage.ToString();
    }

    // Update all the glyph buttons
    private void UpdateGlyphs()
    {
        GlyphContainer.RemoveAllChildren();
        foreach (var glyph in _glyphPrototypes)
        {
            var boxContainer = new BoxContainer();
            var unlocked = _unlockedGlyphProtoIds.Contains(glyph.ID);
            var button = new Button
            {
                HorizontalExpand = true,
                StyleClasses = { StyleBase.ButtonSquare },
                ToolTip = Loc.GetString(glyph.Tooltip),
                Group = _glyphButtonGroup,
                Pressed = glyph.ID == _selectedGlyphProtoId,
                Disabled = !unlocked,
                Modulate = !unlocked ? Color.Gray : Color.White,
            };
            button.OnPressed += _ => _selectedGlyphProtoId = glyph.ID;
            var glyphIcon = new TextureRect
            {
                Texture = _sprite.Frame0(glyph.Icon),
                TextureScale = new Vector2(2f, 2f),
                Stretch = TextureRect.StretchMode.KeepCentered,
            };
            button.AddChild(glyphIcon);
            boxContainer.AddChild(button);
            GlyphContainer.AddChild(boxContainer);
        }
    }

    // Update all the influence thingies
    private void UpdateInfluences(MonumentBuiState state)
    {
        InfluencesContainer.RemoveAllChildren();

        var influenceUIBoxes = new List<InfluenceUIBox>();
        foreach (var influence in _influencePrototypes)
        {
            var uiBoxState = GetUIBoxStateForInfluence(influence, state);
            var influenceBox = new InfluenceUIBox(influence, uiBoxState);
            influenceUIBoxes.Add(influenceBox);
            influenceBox.OnGainButtonPressed += () => OnGainButtonPressed?.Invoke(influence.ID);
        }

        //sort the list of UI boxes by state (locked -> owned -> not enough entropy -> enough entropy)
        //then sort alphabetically within those categories
        foreach (var box in influenceUIBoxes.OrderBy(box => box.State).ThenBy(box => box.Proto.ID))
        {
            InfluencesContainer.AddChild(box);
        }
    }

    private InfluenceUIBox.InfluenceUIBoxState GetUIBoxStateForInfluence(InfluencePrototype influence, MonumentBuiState state)
    {
        if (!_ent.TryGetComponent<CosmicCultComponent>(_player.LocalEntity, out var cultComp))
            return InfluenceUIBox.InfluenceUIBoxState.Locked; //early return with locked if there's somehow no cult comp

        var unlocked = cultComp.UnlockedInfluences.Contains(influence.ID);
        var owned = cultComp.OwnedInfluences.Contains(influence);

        //more verbose than it needs to be, but it reads nicer
        if (owned)
            return InfluenceUIBox.InfluenceUIBoxState.Owned;

        if (unlocked)
        {
            //if it's unlocked, do we have enough entropy to buy it?
            return influence.Cost > cultComp.EntropyBudget ? InfluenceUIBox.InfluenceUIBoxState.UnlockedAndNotEnoughEntropy : InfluenceUIBox.InfluenceUIBoxState.UnlockedAndEnoughEntropy;
        }

        return InfluenceUIBox.InfluenceUIBoxState.Locked;
    }
}

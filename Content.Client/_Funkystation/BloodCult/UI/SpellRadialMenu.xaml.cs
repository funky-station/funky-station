// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 kbarkevich <24629810+kbarkevich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later or MIT

using Content.Client.UserInterface.Controls;
using Content.Shared.BloodCult;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.IoC;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Log;
using System.Numerics;
using Content.Shared.BloodCult.Components;
using Content.Shared.BloodCult.Prototypes;

namespace Content.Client._Funkystation.BloodCult.UI;

public sealed partial class SpellRadialMenu : RadialMenu
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;
    private readonly SpriteSystem _spriteSystem;

	public event Action<ProtoId<CultAbilityPrototype>>? SendSpellsMessageAction;

    public EntityUid Entity { get; set; }
    public bool _shouldRefresh = true;

    public SpellRadialMenu()
    {
        var dependencies = IoCManager.Instance;
        if (dependencies != null)
            dependencies.InjectDependencies(this);
        RobustXamlLoader.Load(this);
        _spriteSystem = _entitySystem.GetEntitySystem<SpriteSystem>();
    }

    public void SetEntity(EntityUid uid)
    {
        Entity = uid;
        if (_shouldRefresh)
            RefreshUI();
    }

    private void RefreshUI()
    {
        var main = FindControl<RadialContainer>("Main");
        if (main == null)
            return;

        // Clear existing children before adding new ones
        main.Children.Clear();

        var player = _playerManager.LocalEntity;

        // Get known spells to filter them out
        // Note: We only filter spells that are fully learned (in KnownSpells).
        // Spells with DoAfters won't be in KnownSpells until the DoAfter completes,
        // so they will still show up in the UI until then.
        var knownSpells = new HashSet<ProtoId<CultAbilityPrototype>>();
        if (_entityManager.TryGetComponent<BloodCultistComponent>(Entity, out var cultistComp))
        {
            knownSpells.UnionWith(cultistComp.KnownSpells);
        }

        foreach (var cultAbility in CultistSpellComponent.ValidSpells)
		{
			// Skip spells that are already fully learned (not just being prepared)
			if (knownSpells.Contains(cultAbility))
				continue;

			_prototypeManager.TryIndex<CultAbilityPrototype>(cultAbility, out var cultAbilityPrototype);
			if (cultAbilityPrototype == null || cultAbilityPrototype.ActionPrototypes == null)
				continue;
			EntProtoId selectedProto = cultAbilityPrototype.ActionPrototypes[0];
			_prototypeManager.TryIndex<EntityPrototype>(selectedProto, out var spellPrototype);
			if (spellPrototype == null)
				continue;

			var button = new SpellsMenuButton
            {
                StyleClasses = { "RadialMenuButton" },
                SetSize = new Vector2(64, 64),
                ToolTip = spellPrototype.Name + ": " + spellPrototype.Description,//Loc.GetString(ritualPrototype.LocName),
                ProtoId = cultAbility//ritualPrototype.ID
            };
            var texture = new TextureRect
            {
                VerticalAlignment = VAlignment.Center,
                HorizontalAlignment = HAlignment.Center,
                Texture = _spriteSystem.GetPrototypeIcon(selectedProto).Default,
                TextureScale = new Vector2(1f, 1f)
            };

            button.AddChild(texture);
            main.AddChild(button);
        }

        AddSpellButtonOnClickAction(main);
    }

    private void AddSpellButtonOnClickAction(RadialContainer mainControl)
    {
        if (mainControl == null)
            return;

        foreach(var child in mainControl.Children)
        {
            var castChild = child as SpellsMenuButton;

            if (castChild == null)
                continue;

            castChild.OnPressed += _ =>
            {
                // Log.Info($"[SpellRadialMenu] Button clicked for spell {castChild.ProtoId}, invoking SendSpellsMessageAction");
                SendSpellsMessageAction?.Invoke(castChild.ProtoId);
                // Log.Info($"[SpellRadialMenu] SendSpellsMessageAction invoked, closing window");
                Close();
            };
        }
    }

    public sealed class SpellsMenuButton : RadialMenuButton
    {
		public required ProtoId<CultAbilityPrototype> ProtoId { get; set; }
    }
}

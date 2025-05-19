using Content.Client.UserInterface.Controls;
using Content.Shared.BloodCult;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Numerics;
using Content.Shared.BloodCult.Components;
using Content.Shared.BloodCult.Prototypes;

namespace Content.Client._Funkystation.BloodCult.UI;

public sealed partial class SpellRadialMenu : RadialMenu
{
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;
    private readonly SpriteSystem _spriteSystem;

	public event Action<ProtoId<CultAbilityPrototype>>? SendSpellsMessageAction;

    public EntityUid Entity { get; set; }

    public SpellRadialMenu()
    {
        IoCManager.InjectDependencies(this);
        RobustXamlLoader.Load(this);
        _spriteSystem = _entitySystem.GetEntitySystem<SpriteSystem>();
    }

    public void SetEntity(EntityUid uid)
    {
        Entity = uid;
        RefreshUI();
    }

    private void RefreshUI()
    {
        var main = FindControl<RadialContainer>("Main");
        if (main == null)
            return;

        var player = _playerManager.LocalEntity;

        foreach (var cultAbility in CultistSpellComponent.ValidSpells)
		{
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

            castChild.OnButtonUp += _ =>
            {
                SendSpellsMessageAction?.Invoke(castChild.ProtoId);
                Close();
            };
        }
    }

    public sealed class SpellsMenuButton : RadialMenuTextureButton
    {
		public required ProtoId<CultAbilityPrototype> ProtoId { get; set; }
    }
}

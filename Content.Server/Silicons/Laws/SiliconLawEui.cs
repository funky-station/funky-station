// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server._Impstation.Borgs.FreeformLaws;
using Content.Server.Administration.Managers;
using Content.Server.EUI;
using Content.Server.NPC.Queries.Considerations;
using Content.Server.Station.Components;
using Content.Shared.Administration;
using Content.Shared.Eui;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.MalfAI;
using Content.Shared.Mind.Components;

namespace Content.Server.Silicons.Laws;

public sealed class SiliconLawEui : BaseEui
{
    private readonly SiliconLawSystem _siliconLawSystem;
    private readonly EntityManager _entityManager;
    private readonly IAdminManager _adminManager;

    private List<SiliconLaw> _laws = new();
    private ISawmill _sawmill = default!;
    private EntityUid _target;

    public SiliconLawEui(SiliconLawSystem siliconLawSystem, EntityManager entityManager, IAdminManager manager, EntityUid? target = null) // imp - added target param
    {
        _siliconLawSystem = siliconLawSystem;
        _adminManager = manager;
        _entityManager = entityManager;
        _sawmill = Logger.GetSawmill("silicon-law-eui");
        if (target != null) // imp - added test for target, so that the eui can be fed a specific target
            _target = target.Value;
    }

    public override EuiStateBase GetNewState()
    {
        return new SiliconLawsEuiState(_laws, _entityManager.GetNetEntity(_target));
    }

    public void UpdateLaws(SiliconLawBoundComponent? lawBoundComponent, EntityUid player)
    {
        if (!IsAllowed())
            return;

        var laws = _siliconLawSystem.GetLaws(player, lawBoundComponent);
        _laws = laws.Laws;
        _target = player;
        StateDirty();
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        if (msg is not SiliconLawsSaveMessage message)
        {
            return;
        }

        if (!IsAllowed())
            return;

        var player = _entityManager.GetEntity(message.Target);
        if (_entityManager.TryGetComponent<SiliconLawProviderComponent>(player, out var playerProviderComp))
            _siliconLawSystem.SetLaws(message.Laws, player, playerProviderComp.LawUploadSound);
    }

    private bool IsAllowed()
    {
        var adminData = _adminManager.GetAdminData(Player);
        // imp - added check for FreeformLawEntryComponent so that players *can* access this EUI on freeform lawboards
        if (_entityManager.HasComponent<FreeformLawEntryComponent>(_target))
            return true;

        if (adminData == null || !adminData.HasFlag(AdminFlags.Moderator))
        {
            _sawmill.Warning("Player {0} tried to open / use silicon law UI without permission.", Player.UserId);
            return false;
        }

        return true;
    }
}

// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Voting;

namespace Content.Client.Voting;

public sealed class VotingSystem : EntitySystem
{
    public event Action<VotePlayerListResponseEvent>? VotePlayerListResponse; //Provides a list of players elligble for vote actions

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<VotePlayerListResponseEvent>(OnVotePlayerListResponseEvent);
    }

    private void OnVotePlayerListResponseEvent(VotePlayerListResponseEvent msg)
    {
        VotePlayerListResponse?.Invoke(msg);
    }

    public void RequestVotePlayerList()
    {
        RaiseNetworkEvent(new VotePlayerListRequestEvent());
    }
}

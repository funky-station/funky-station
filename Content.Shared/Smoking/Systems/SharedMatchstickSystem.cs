// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

// Shitmed Change Start
using Content.Shared.Smoking.Components;

namespace Content.Shared.Smoking.Systems;

public abstract class SharedMatchstickSystem : EntitySystem
{
    public virtual bool SetState(Entity<MatchstickComponent> ent, SmokableState state)
    {
        if (ent.Comp.CurrentState == state)
            return false;

        ent.Comp.CurrentState = state;
        Dirty(ent);
        return true;
    }
} 
// Shitmed Change End

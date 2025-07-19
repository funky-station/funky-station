// SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.StatusEffect;

namespace Content.Shared.Speech.EntitySystems;

public abstract class SharedRatvarianLanguageSystem : EntitySystem
{
    public virtual void DoRatvarian(EntityUid uid, TimeSpan time, bool refresh, StatusEffectsComponent? status = null)
    {
    }
}

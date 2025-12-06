// SPDX-FileCopyrightText: 2022 Pancake <Pangogie@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TrixxedHeart <46364955+TrixxedBit@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Speech.Components;

[RegisterComponent]
public sealed partial class RussianAccentComponent : Component
{
    /// <summary>
    /// The chance (0.0 to 1.0) that articles like "the", "a", "an" will be removed from sentences, default is 80%.
    /// </summary>
    [DataField("articleRemovalChance")]
    public float ArticleRemovalChance = 0.8f;

    /// <summary>
    /// The chance (0.0 to 1.0) that "tovarisch" will be replaced with "komrade" (comrade) instead, default is 20%.
    /// </summary>
    [DataField("komradeReplacementChance")]
    public float KomradeReplacementChance = 0.2f;
}

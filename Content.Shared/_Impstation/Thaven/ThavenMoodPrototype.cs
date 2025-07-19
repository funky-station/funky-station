// SPDX-FileCopyrightText: 2025 ATDoop <bug@bug.bug>
// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Linq;
using Content.Shared.Dataset;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;

namespace Content.Shared._Impstation.Thaven;

[Virtual, DataDefinition]
[Serializable, NetSerializable]
public partial class ThavenMood
{
    [DataField(readOnly: true), ViewVariables(VVAccess.ReadOnly)]
    public ProtoId<ThavenMoodPrototype> ProtoId = string.Empty;

    /// <summary>
    /// A locale string of the mood name.
    /// </summary>
    [DataField(required: true), ViewVariables(VVAccess.ReadWrite)]
    public string MoodName = string.Empty;

    /// <summary>
    /// A locale string of the mood description. Gets passed to
    /// <see cref="Loc.GetString"/> with <see cref="MoodVars"/>.
    /// </summary>
    [DataField(required: true), ViewVariables(VVAccess.ReadWrite)]
    public string MoodDesc = string.Empty;

    [DataField(serverOnly: true, customTypeSerializer: typeof(PrototypeIdHashSetSerializer<ThavenMoodPrototype>))]
    [ViewVariables(VVAccess.ReadWrite)]
    public HashSet<string> Conflicts = new();

    /// <summary>
    /// Additional localized words for the <see cref="MoodDesc"/>, for things like random
    /// verbs and nouns.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<string, string> MoodVars = new();

    // begin funky
    [DataField(serverOnly: true, customTypeSerializer: typeof(PrototypeIdHashSetSerializer<DepartmentPrototype>))]
    [ViewVariables(VVAccess.ReadWrite)]
    public HashSet<string> JobConflicts = new();
    // end funky

    public (string, object)[] GetLocArgs()
    {
        return MoodVars.Select(v => (v.Key, (object)v.Value)).ToArray();
    }

    public string GetLocName()
    {
        return Loc.GetString(MoodName, GetLocArgs());
    }

    public string GetLocDesc()
    {
        return Loc.GetString(MoodDesc, GetLocArgs());
    }
}

[Prototype("thavenMood")]
[Serializable, NetSerializable]
public sealed partial class ThavenMoodPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true), ViewVariables(VVAccess.ReadWrite)]
    public string MoodName = string.Empty;

    [DataField(required: true), ViewVariables(VVAccess.ReadWrite)]
    public string MoodDesc = string.Empty;

    /// <summary>
    /// A list of mood IDs that this mood will conflict with.
    /// </summary>
    [DataField("conflicts", customTypeSerializer: typeof(PrototypeIdHashSetSerializer<ThavenMoodPrototype>))]
    public HashSet<string> Conflicts = new();

    /// <summary>
    /// Extra mood variables that will be randomly chosen and provided
    /// to the <see cref="Loc.GetString"/> call on <see cref="ThavenMood.MoodDesc"/>.
    /// </summary>
    [DataField("moodVars", customTypeSerializer: typeof(PrototypeIdValueDictionarySerializer<string, DatasetPrototype>))]
    public Dictionary<string, string> MoodVarDatasets = new();

    /// <summary>
    /// If false, prevents the same variable from being rolled twice when rolling
    /// mood variables for this mood. Does not prevent the same mood variable
    /// from being present in other moods.
    /// </summary>
    [DataField("allowDuplicateMoodVars"), ViewVariables(VVAccess.ReadWrite)]
    public bool AllowDuplicateMoodVars = false;

    // begin funky
    /// <summary>
    /// A list of conflicting jobs with this mood, if applicable.
    /// </summary>
    [DataField("jobConflicts", customTypeSerializer: typeof(PrototypeIdHashSetSerializer<DepartmentPrototype>))]
    public HashSet<string> JobConflicts = new();
    // end funky
}

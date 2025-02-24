using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared.Silicons.Laws;

/// <summary>
/// Lawset data used internally.
/// </summary>
[DataDefinition, Serializable, NetSerializable]
public sealed partial class SiliconLawset
{
	/// <summary>
	/// Law Zero, the highest priority law (for emag loyalty, malf, onehuman).
	/// NEVER MODIFY THIS EXCEPT THROUGH THE SiliconLawSystem.cs
	/// </summary>
	[DataField]
	public SiliconLaw? Law0 = null;

	/// <summary>
	/// Hacked Laws (for emag directives).
	/// NEVER MODIFY THIS EXCEPT THROUGH THE SiliconLawSystem.cs
	/// </summary>
	[DataField]
	public List<SiliconLaw> HackedLaws = new();

	/// <summary>
	/// Ion Storm Laws.
	/// NEVER MODIFY THIS EXCEPT THROUGH THE SiliconLawSystem.cs
	/// </summary>
	[DataField]
	public List<SiliconLaw> IonLaws = new();

    /// <summary>
    /// List of laws in this lawset.
	/// NEVER MODIFY THIS EXCEPT THROUGH THE SiliconLawSystem.cs
    /// </summary>
    [DataField(required: true), ViewVariables(VVAccess.ReadWrite)]
    public List<SiliconLaw> LawsetLaws = new();

	/// <summary>
    /// Custom Laws, such as through a freeform law module.
	/// NEVER MODIFY THIS EXCEPT THROUGH THE SiliconLawSystem.cs
    /// </summary>
	[DataField]
	public List<SiliconLaw> CustomLaws = new();

    /// <summary>
    /// What entity the lawset considers as a figure of authority.
    /// </summary>
    [DataField(required: true), ViewVariables(VVAccess.ReadWrite)]
    public string ObeysTo = string.Empty;

	/// <summary>
    /// Gets all laws, properly ordered, from all law categories
	/// in one list.
    /// </summary>
	public List<SiliconLaw> Laws
	{
		get {
			var laws = new List<SiliconLaw>(CustomLaws.Count + LawsetLaws.Count + IonLaws.Count + HackedLaws.Count + (Law0 != null ? 1 : 0));
			if (Law0 != null)
				laws.Add(Law0.ShallowClone());
			foreach (var law in HackedLaws)
				laws.Add(law.ShallowClone());
			foreach (var law in IonLaws)
				laws.Add(law.ShallowClone());
			foreach (var law in LawsetLaws)
				laws.Add(law.ShallowClone());
			foreach (var law in CustomLaws)
				laws.Add(law.ShallowClone());
			return laws;
		}
		set {
			// Phase this out
		}
	}

    /// <summary>
    /// A single line used in logging laws.
    /// </summary>
    public string LoggingString()
    {
		var laws = new List<string>(Laws.Count);
		foreach (var law in Laws)
        {
            laws.Add($"{law.Order}: {Loc.GetString(law.LawString)}");
        }

        return string.Join(" / ", laws);
    }

    /// <summary>
    /// Do a clone of this lawset.
    /// It will have unique laws but their strings are still shared.
    /// </summary>
    public SiliconLawset Clone(bool deep = false)
    {
		SiliconLaw? law0 = null;
		var hackedLaws = new List<SiliconLaw>(HackedLaws.Count);
		var ionLaws = new List<SiliconLaw>(IonLaws.Count);
		var lawsetLaws = new List<SiliconLaw>(LawsetLaws.Count);
		var customLaws = new List<SiliconLaw>(CustomLaws.Count);

		if (Law0 != null)
			law0 = deep ? Law0.DeepClone() : Law0.ShallowClone();
		foreach (var law in HackedLaws)
			hackedLaws.Add(deep ? law.DeepClone() : law.ShallowClone());
		foreach (var law in IonLaws)
			ionLaws.Add(deep ? law.DeepClone() : law.ShallowClone());
		foreach (var law in LawsetLaws)
			lawsetLaws.Add(deep ? law.DeepClone() : law.ShallowClone());
		foreach (var law in CustomLaws)
			customLaws.Add(deep ? law.DeepClone() : law.ShallowClone());

        return new SiliconLawset()
        {
			Law0 = law0,
			HackedLaws = hackedLaws,
			IonLaws = ionLaws,
			LawsetLaws = lawsetLaws,
			CustomLaws = customLaws,
            ObeysTo = deep ? new string(ObeysTo) : ObeysTo
        };
    }
}

/// <summary>
/// This is a prototype for a <see cref="SiliconLawPrototype"/> list.
/// Cannot be used directly since it is a list of prototype ids rather than List<Siliconlaw>.
/// </summary>
[Prototype("siliconLawset"), Serializable, NetSerializable]
public sealed partial class SiliconLawsetPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// List of law prototype ids in this lawset.
    /// </summary>
    [DataField(required: true, customTypeSerializer: typeof(PrototypeIdListSerializer<SiliconLawPrototype>))]
    public List<string> Laws = new();

    /// <summary>
    /// What entity the lawset considers as a figure of authority.
    /// </summary>
    [DataField(required: true), ViewVariables(VVAccess.ReadWrite)]
    public string ObeysTo = string.Empty;
}

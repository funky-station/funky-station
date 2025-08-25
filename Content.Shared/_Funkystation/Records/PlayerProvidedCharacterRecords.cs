using System.Linq;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Records;

/// <summary>
/// Contains Cosmatic Drift records that can be changed in the character editor. This is stored on the character's profile.
/// </summary>
[DataDefinition]
[Serializable, NetSerializable]
public sealed partial class PlayerProvidedCharacterRecords
{
    public const int TextMedLen = 64;
    public const int TextVeryLargeLen = 4096;

    /* Basic info */

    // Additional data is fetched from the Profile

    // general information
    [DataField]
    public int Height { get; private set; }
    public const int MaxHeight = 800;

    [DataField]
    public int Weight { get; private set; }
    public const int MaxWeight = 300;

    [DataField]
    public string IdentifyingFeatures { get; private set; }

    [DataField]
    public bool HasWorkAuthorization { get; private set; }

    [DataField]
    public bool HasInsurance { get; private set; }

    [DataField]
    public int InsuranceProvider { get; private set; }

    [DataField]
    public int InsuranceType { get; private set; }

    // medical info

    /// <summary>
    /// A character's enabled medical info,
    /// includes: allergies, prescriptions, family history
    /// </summary>
    [DataField]
    private HashSet<ProtoId<MedicalInfoPrototype>> _medicalInfo = new();
    public HashSet<ProtoId<MedicalInfoPrototype>> MedicalInfo => _medicalInfo;

    [DataField]
    public int BloodType { get; private set; }

    [DataField]
    public string PostmortemInstructions { get; private set; }

    public PlayerProvidedCharacterRecords(
        bool hasWorkAuthorization,
        int height, int weight,
        string identifyingFeatures,
        bool hasInsurance,
        int insuranceProvider, int insuranceType,
        HashSet<ProtoId<MedicalInfoPrototype>> medicalInfo,
        int bloodType,
        string postmortemInstructions)
    {
        HasWorkAuthorization = hasWorkAuthorization;
        Height = height;
        Weight = weight;
        IdentifyingFeatures = identifyingFeatures;
        HasInsurance = hasInsurance;
        InsuranceProvider = insuranceProvider;
        InsuranceType = insuranceType;
        _medicalInfo = medicalInfo;
        BloodType = bloodType;
        PostmortemInstructions = postmortemInstructions;
    }

    public PlayerProvidedCharacterRecords(PlayerProvidedCharacterRecords other)
    {
        Height = other.Height;
        Weight = other.Weight;
        HasWorkAuthorization = other.HasWorkAuthorization;
        IdentifyingFeatures = other.IdentifyingFeatures;
        HasInsurance = other.HasInsurance;
        InsuranceProvider = other.InsuranceProvider;
        InsuranceType = other.InsuranceType;
        _medicalInfo = other.MedicalInfo;
        BloodType = other.BloodType;
        PostmortemInstructions = other.PostmortemInstructions;
    }

    public static PlayerProvidedCharacterRecords DefaultRecords()
    {
        return new PlayerProvidedCharacterRecords(
            hasWorkAuthorization: true,
            height: 170, weight: 70,
            identifyingFeatures: "",
            hasInsurance: true,
            insuranceProvider: 0,
            insuranceType: 0,
            medicalInfo: [],
            bloodType: 0,
            postmortemInstructions: "Return home"
        );
    }

    public bool MemberwiseEquals(PlayerProvidedCharacterRecords other)
    {
        // This is ugly but is only used for integration tests.
        var test = Height == other.Height
                   && Weight == other.Weight
                   && HasWorkAuthorization == other.HasWorkAuthorization
                   && IdentifyingFeatures == other.IdentifyingFeatures
                   && HasInsurance == other.HasInsurance
                   && InsuranceProvider == other.InsuranceProvider
                   && InsuranceType == other.InsuranceType
                   && _medicalInfo.SetEquals(other.MedicalInfo)
                   && BloodType == other.BloodType
                   && PostmortemInstructions == other.PostmortemInstructions;
        if (!test)
            return false;

        return true;
    }

    private static string ClampString(string str, int maxLen)
    {
        return str.Length > maxLen ? str[..maxLen] : str;
    }

    /// <summary>
    /// Clamp invalid entries to valid values
    /// </summary>
    public void EnsureValid()
    {
        var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
        var info = MedicalInfo
            .Where(prototypeManager.HasIndex)
            .ToList();

        Height = Math.Clamp(Height, 0, MaxHeight);
        Weight = Math.Clamp(Weight, 0, MaxWeight);
        IdentifyingFeatures = ClampString(IdentifyingFeatures, TextMedLen);
        PostmortemInstructions = ClampString(PostmortemInstructions, TextMedLen);
        _medicalInfo.UnionWith(GetValidInfo(info, prototypeManager));
    }
    public PlayerProvidedCharacterRecords WithHeight(int height)
    {
        return new(this) { Height = height };
    }
    public PlayerProvidedCharacterRecords WithWeight(int weight)
    {
        return new(this) { Weight = weight };
    }
    public PlayerProvidedCharacterRecords WithWorkAuth(bool auth)
    {
        return new(this) { HasWorkAuthorization = auth };
    }
    public PlayerProvidedCharacterRecords WithIdentifyingFeatures(string feat)
    {
        return new(this) { IdentifyingFeatures = feat};
    }

    public PlayerProvidedCharacterRecords WithInsurance(bool hasInsurance)
    {
        return new (this) { HasInsurance = hasInsurance };
    }

    public PlayerProvidedCharacterRecords WithInsuranceProvider(int insuranceProvider)
    {
        return new (this) { InsuranceProvider = insuranceProvider };
    }

    public PlayerProvidedCharacterRecords WithInsuranceType(int insuranceType)
    {
        return new (this) { InsuranceType = insuranceType };
    }

    public PlayerProvidedCharacterRecords WithMedicalInfo(ProtoId<MedicalInfoPrototype> medicalId, IPrototypeManager protoManager)
    {
        if (!protoManager.TryIndex(medicalId, out var medicalInfoProto))
            return new(this);

        var category = medicalInfoProto.Category;

        MedicalInfoCategoryPrototype? categoryProto = null;

        if (category != null && !protoManager.TryIndex(category, out categoryProto))
            return new(this);

        var list = new HashSet<ProtoId<MedicalInfoPrototype>>(_medicalInfo) { medicalId };

        if (categoryProto == null)
            return new(this) {_medicalInfo = list};

        foreach (var item in list)
        {
            if (!protoManager.TryIndex(item, out var otherProto)
                || otherProto.Category != categoryProto)
                continue;
        }

        return new(this) { _medicalInfo = list };
    }

    public PlayerProvidedCharacterRecords WithoutMedicalInfo(ProtoId<MedicalInfoPrototype> medicalId, IPrototypeManager protoManager)
    {
        var list = new HashSet<ProtoId<MedicalInfoPrototype>>(_medicalInfo);
        list.Remove(medicalId);

        return new(this) { _medicalInfo = list };
    }

    public PlayerProvidedCharacterRecords WithBloodType(int b)
    {
        return new (this) { BloodType = b };
    }

    public PlayerProvidedCharacterRecords WithPostmortemInstructions(string s)
    {
        return new(this) { PostmortemInstructions = s};
    }

    public HashSet<ProtoId<MedicalInfoPrototype>> GetValidInfo(IEnumerable<ProtoId<MedicalInfoPrototype>> info, IPrototypeManager protoManager)
    {
        var result = new HashSet<ProtoId<MedicalInfoPrototype>>();

        foreach (var item in info)
        {
            if (!protoManager.TryIndex(item, out var itemProto))
                continue;

            if (itemProto.Category == null)
                continue;

            // No category so dump it.
            if (!protoManager.TryIndex(itemProto.Category, out var category))
                continue;

            result.Add(item);
        }

        return result;
    }
}

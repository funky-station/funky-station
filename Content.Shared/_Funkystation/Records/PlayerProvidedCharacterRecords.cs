using System.Linq;
using System.Text.Json.Serialization;
using Robust.Shared.Serialization;

namespace Content.Shared._CD.Records;

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

    // TODO: implement reasons for prescriptions
    // sometime Soon id like to have immunosuppressed as a trait,
    // so keeping this for characters that would potentially need prescriptions across rounds
    [DataField]
    public Dictionary<string, bool> PrescriptionList { get; private set; }

    // TODO: implement allergies
    // keeping these for "tba mechanical allergies."
    // personally not a fan of keeping pure-flavor things that *should* be mechanics instead
    [DataField]
    public Dictionary<string, bool> AllergyList { get; private set; }

    [DataField]
    public Dictionary<string, bool> FamilyHistory { get; private set; }

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
        Dictionary<string, bool> prescriptionList,
        Dictionary<string, bool> allergyList,
        Dictionary<string, bool> familyHistory,
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
        PrescriptionList = prescriptionList;
        AllergyList = allergyList;
        FamilyHistory = familyHistory;
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
        PrescriptionList = other.PrescriptionList;
        AllergyList = other.AllergyList;
        FamilyHistory = other.FamilyHistory;
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
            prescriptionList: new Dictionary<string, bool>(), // need to fill this dict with
            allergyList: new Dictionary<string, bool>(), // the prototype list, then set to false
            familyHistory: new Dictionary<string, bool>(), // for every bool
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
                   && PrescriptionList == other.PrescriptionList
                   && AllergyList == other.AllergyList
                   && FamilyHistory == other.FamilyHistory
                   && BloodType == other.BloodType
                   && PostmortemInstructions == other.PostmortemInstructions;
        if (!test)
            return false;

        return true;
    }

    private static string ClampString(string str, int maxLen)
    {
        if (str.Length > maxLen)
        {
            return str[..maxLen];
        }
        return str;
    }


    /// <summary>
    /// Clamp invalid entries to valid values
    /// </summary>
    public void EnsureValid()
    {
        Height = Math.Clamp(Height, 0, MaxHeight);
        Weight = Math.Clamp(Weight, 0, MaxWeight);
        IdentifyingFeatures = ClampString(IdentifyingFeatures, TextMedLen);
        PostmortemInstructions = ClampString(PostmortemInstructions, TextMedLen);
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

    public PlayerProvidedCharacterRecords WithPrescriptions(string pres)
    {
        return new (this) { Prescriptions = pres };
    }
    public PlayerProvidedCharacterRecords WithAllergies(string s)
    {
        return new(this) { Allergies = s };
    }
    public PlayerProvidedCharacterRecords WithDrugAllergies(string s)
    {
        return new(this) { DrugAllergies = s };
    }
    public PlayerProvidedCharacterRecords WithBloodType(int b)
    {
        return new (this) { BloodType = b };
    }
    public PlayerProvidedCharacterRecords WithPostmortemInstructions(string s)
    {
        return new(this) { PostmortemInstructions = s};
    }
}

public enum CharacterRecordType : byte
{
    Employment, Medical, Security
}

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

    // physical information
    [DataField]
    public int Height { get; private set; }
    public const int MaxHeight = 800;

    [DataField]
    public int Weight { get; private set; }
    public const int MaxWeight = 300;

    [DataField]
    public string IdentifyingFeatures { get; private set; }

    [DataField]
    public bool HasInsurance { get; private set; }

    [DataField]
    public int InsuranceProvider { get; private set; }

    [DataField]
    public int InsuranceType { get; private set; }

    // medical info
    [DataField]
    public string PostmortemInstructions { get; private set; }

    // TODO: implement reasons for prescriptions
    // sometime Soon id like to have immunosuppressed as a trait,
    // so keeping this for characters that would potentially need prescriptions across rounds
    [DataField]
    public string Prescriptions { get; private set; }

    // TODO: implement allergies
    // keeping these for "tba mechanical allergies."
    // personally not a fan of keeping pure-flavor things that *should* be mechanics instead
    [DataField]
    public string Allergies { get; private set; }

    [DataField]
    public string DrugAllergies { get; private set; }

    // misc bureaucracy
    [DataField]
    public bool HasWorkAuthorization { get; private set; }


    // "incidents"
    [DataField, JsonIgnore]
    public List<RecordEntry> MedicalEntries { get; private set; }
    [DataField, JsonIgnore]
    public List<RecordEntry> SecurityEntries { get; private set; }
    [DataField, JsonIgnore]
    public List<RecordEntry> EmploymentEntries { get; private set; }


    [DataDefinition]
    [Serializable, NetSerializable]
    public sealed partial class RecordEntry
    {
        [DataField]
        public string Title { get; private set; }
        // players involved, can be left blank (or with a generic "CentCom" etc.) for backstory related issues
        [DataField]
        public string Involved { get; private set; }
        // Longer description of events.
        [DataField]
        public string Description { get; private set; }

        public RecordEntry(string title, string involved, string desc)
        {
            Title = title;
            Involved = involved;
            Description = desc;
        }

        public RecordEntry(RecordEntry other)
            : this(other.Title, other.Involved, other.Description)
        {
        }

        public bool MemberwiseEquals(RecordEntry other)
        {
            return Title == other.Title && Involved == other.Involved && Description == other.Description;
        }

        public void EnsureValid()
        {
            Title = ClampString(Title, TextMedLen);
            Involved = ClampString(Involved, TextMedLen);
            Description = ClampString(Description, TextVeryLargeLen);
        }
    }

    public PlayerProvidedCharacterRecords(
        bool hasWorkAuthorization,
        int height, int weight,
        string identifyingFeatures,
        bool hasInsurance,
        int insuranceProvider, int insuranceType,
        string prescriptions,
        string allergies, string drugAllergies,
        string postmortemInstructions,
        List<RecordEntry> medicalEntries, List<RecordEntry> securityEntries, List<RecordEntry> employmentEntries)
    {
        HasWorkAuthorization = hasWorkAuthorization;
        Height = height;
        Weight = weight;
        IdentifyingFeatures = identifyingFeatures;
        HasInsurance = hasInsurance;
        InsuranceProvider = insuranceProvider;
        InsuranceType = insuranceType;
        Prescriptions = prescriptions;
        Allergies = allergies;
        DrugAllergies = drugAllergies;
        PostmortemInstructions = postmortemInstructions;
        MedicalEntries = medicalEntries;
        SecurityEntries = securityEntries;
        EmploymentEntries = employmentEntries;
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
        Prescriptions = other.Prescriptions;
        Allergies = other.Allergies;
        DrugAllergies = other.DrugAllergies;
        PostmortemInstructions = other.PostmortemInstructions;
        MedicalEntries = other.MedicalEntries.Select(x => new RecordEntry(x)).ToList();
        SecurityEntries = other.SecurityEntries.Select(x => new RecordEntry(x)).ToList();
        EmploymentEntries = other.EmploymentEntries.Select(x => new RecordEntry(x)).ToList();
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
            prescriptions: "None",
            allergies: "None",
            drugAllergies: "None",
            postmortemInstructions: "Return home",
            medicalEntries: new List<RecordEntry>(),
            securityEntries: new List<RecordEntry>(),
            employmentEntries: new List<RecordEntry>()
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
                   && Prescriptions == other.Prescriptions
                   && Allergies == other.Allergies
                   && DrugAllergies == other.DrugAllergies
                   && PostmortemInstructions == other.PostmortemInstructions;
        if (!test)
            return false;
        if (MedicalEntries.Count != other.MedicalEntries.Count)
            return false;
        if (SecurityEntries.Count != other.SecurityEntries.Count)
            return false;
        if (EmploymentEntries.Count != other.EmploymentEntries.Count)
            return false;
        if (MedicalEntries.Where((t, i) => !t.MemberwiseEquals(other.MedicalEntries[i])).Any())
        {
            return false;
        }
        if (SecurityEntries.Where((t, i) => !t.MemberwiseEquals(other.SecurityEntries[i])).Any())
        {
            return false;
        }
        if (EmploymentEntries.Where((t, i) => !t.MemberwiseEquals(other.EmploymentEntries[i])).Any())
        {
            return false;
        }

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

    private static void EnsureValidEntries(List<RecordEntry> entries)
    {
        foreach (var entry in entries)
        {
            entry.EnsureValid();
        }
    }

    /// <summary>
    /// Clamp invalid entries to valid values
    /// </summary>
    public void EnsureValid()
    {
        Height = Math.Clamp(Height, 0, MaxHeight);
        Weight = Math.Clamp(Weight, 0, MaxWeight);
        IdentifyingFeatures = ClampString(IdentifyingFeatures, TextMedLen);
        Prescriptions = ClampString(Prescriptions, TextMedLen);
        Allergies = ClampString(Allergies, TextMedLen);
        DrugAllergies = ClampString(DrugAllergies, TextMedLen);
        PostmortemInstructions = ClampString(PostmortemInstructions, TextMedLen);

        EnsureValidEntries(EmploymentEntries);
        EnsureValidEntries(MedicalEntries);
        EnsureValidEntries(SecurityEntries);
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
    public PlayerProvidedCharacterRecords WithPostmortemInstructions(string s)
    {
        return new(this) { PostmortemInstructions = s};
    }
    public PlayerProvidedCharacterRecords WithEmploymentEntries(List<RecordEntry> entries)
    {
        return new(this) { EmploymentEntries = entries};
    }
    public PlayerProvidedCharacterRecords WithMedicalEntries(List<RecordEntry> entries)
    {
        return new(this) { MedicalEntries = entries};
    }
    public PlayerProvidedCharacterRecords WithSecurityEntries(List<RecordEntry> entries)
    {
        return new(this) { SecurityEntries = entries};
    }
}

public enum CharacterRecordType : byte
{
    Employment, Medical, Security
}

namespace Content.Shared._Funkystation.Records;

/// <summary>
/// For initializing the "insurance provider" dropdown in the "Records" tab of the character creator.
/// </summary>
public enum InsuranceProviders : byte
{
    None,
    NanoCare,
    UnitedCare,
    TrueHealth,
    InterstellarCorp,
}

/// <summary>
/// For initializing the "insurance type" dropdown in the "Records" tab of the character creator.
/// </summary>
public enum InsuranceTypes : byte
{
    None,
    Basic,
    Essentials,
    Premium,
    CentralCommand,
}

/// <summary>
/// For initializing the "blood type" dropdown in the "Records" tab of the character creator.
/// </summary>
public enum BloodTypes : byte
{
    OMinus,
    OPositive,
    AMinus,
    APositive,
    BMinus,
    BPositive,
    ABMinus,
    ABPositive,
}

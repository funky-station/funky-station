// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Atmos;

public readonly record struct GasMixtureStringRepresentation(float TotalMoles, float Temperature, float Pressure, Dictionary<string, float> MolesPerGas) : IFormattable
{
    public override string ToString()
    {
        return $"{Temperature}K {Pressure} kPa";
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return ToString();
    }

    public static implicit operator string(GasMixtureStringRepresentation rep) => rep.ToString();
}

// SPDX-FileCopyrightText: 2025 Lyndomen <49795619+Lyndomen@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Client._CD.Records.UI;

public static class UnitConversion
{
    public static string GetImperialDisplayLength(int lengthCm)
    {
        var heightIn = (int) Math.Round(lengthCm * 0.3937007874 /* cm to in*/);
        return $"({heightIn / 12}'{heightIn % 12}'')";
    }

    public static string GetImperialDisplayMass(int massKg)
    {
        var weightLbs = (int) Math.Round(massKg * 2.2046226218 /* kg to lbs */);
        return $"({weightLbs} lbs)";
    }
}

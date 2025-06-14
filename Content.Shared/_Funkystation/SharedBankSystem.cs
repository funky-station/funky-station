using System.Globalization;

namespace Content.Shared._Funkystation;

public sealed class SharedBankSystem
{
    private static readonly CultureInfo CultureInfo = new("en-US");

    public static string ToBalanceString(int balance)
    {
        return string.Format(CultureInfo, "{0:C0}", balance);
    }
}

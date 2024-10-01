namespace Darp.Utils.ResxSourceGenerator;

using System.Globalization;

internal static class CharExtensions
{
    private static bool IsLetterChar(this UnicodeCategory cat)
    {
        switch (cat)
        {
            case UnicodeCategory.UppercaseLetter:
            case UnicodeCategory.LowercaseLetter:
            case UnicodeCategory.TitlecaseLetter:
            case UnicodeCategory.ModifierLetter:
            case UnicodeCategory.OtherLetter:
            case UnicodeCategory.LetterNumber:
                return true;
            case UnicodeCategory.ClosePunctuation:
            case UnicodeCategory.ConnectorPunctuation:
            case UnicodeCategory.Control:
            case UnicodeCategory.CurrencySymbol:
            case UnicodeCategory.DashPunctuation:
            case UnicodeCategory.DecimalDigitNumber:
            case UnicodeCategory.EnclosingMark:
            case UnicodeCategory.FinalQuotePunctuation:
            case UnicodeCategory.Format:
            case UnicodeCategory.InitialQuotePunctuation:
            case UnicodeCategory.LineSeparator:
            case UnicodeCategory.MathSymbol:
            case UnicodeCategory.ModifierSymbol:
            case UnicodeCategory.NonSpacingMark:
            case UnicodeCategory.OpenPunctuation:
            case UnicodeCategory.OtherNotAssigned:
            case UnicodeCategory.OtherNumber:
            case UnicodeCategory.OtherPunctuation:
            case UnicodeCategory.OtherSymbol:
            case UnicodeCategory.ParagraphSeparator:
            case UnicodeCategory.PrivateUse:
            case UnicodeCategory.SpaceSeparator:
            case UnicodeCategory.SpacingCombiningMark:
            case UnicodeCategory.Surrogate:
            default:
                break;
        }

        return false;
    }

    public static bool IsIdentifierStartCharacter(this char ch) =>
        ch == '_' || CharUnicodeInfo.GetUnicodeCategory(ch).IsLetterChar();

    public static bool IsIdentifierPartCharacter(this char ch)
    {
        UnicodeCategory cat = CharUnicodeInfo.GetUnicodeCategory(ch);
        return cat.IsLetterChar()
            || cat == UnicodeCategory.DecimalDigitNumber
            || cat == UnicodeCategory.ConnectorPunctuation
            || cat == UnicodeCategory.Format
            || cat == UnicodeCategory.NonSpacingMark
            || cat == UnicodeCategory.SpacingCombiningMark;
    }
}

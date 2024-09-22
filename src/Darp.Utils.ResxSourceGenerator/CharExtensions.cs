namespace Darp.Utils.ResxSourceGenerator;

using System.Globalization;

internal static class CharExtensions
{
    public static bool IsLetterChar(this UnicodeCategory cat)
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
                break;
            case UnicodeCategory.ConnectorPunctuation:
                break;
            case UnicodeCategory.Control:
                break;
            case UnicodeCategory.CurrencySymbol:
                break;
            case UnicodeCategory.DashPunctuation:
                break;
            case UnicodeCategory.DecimalDigitNumber:
                break;
            case UnicodeCategory.EnclosingMark:
                break;
            case UnicodeCategory.FinalQuotePunctuation:
                break;
            case UnicodeCategory.Format:
                break;
            case UnicodeCategory.InitialQuotePunctuation:
                break;
            case UnicodeCategory.LineSeparator:
                break;
            case UnicodeCategory.MathSymbol:
                break;
            case UnicodeCategory.ModifierSymbol:
                break;
            case UnicodeCategory.NonSpacingMark:
                break;
            case UnicodeCategory.OpenPunctuation:
                break;
            case UnicodeCategory.OtherNotAssigned:
                break;
            case UnicodeCategory.OtherNumber:
                break;
            case UnicodeCategory.OtherPunctuation:
                break;
            case UnicodeCategory.OtherSymbol:
                break;
            case UnicodeCategory.ParagraphSeparator:
                break;
            case UnicodeCategory.PrivateUse:
                break;
            case UnicodeCategory.SpaceSeparator:
                break;
            case UnicodeCategory.SpacingCombiningMark:
                break;
            case UnicodeCategory.Surrogate:
                break;
            default:
                break;
        }

        return false;
    }

    public static bool IsIdentifierStartCharacter(this char ch)
        => ch == '_' || CharUnicodeInfo.GetUnicodeCategory(ch).IsLetterChar();

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

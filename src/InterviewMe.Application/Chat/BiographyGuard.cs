using System.Text.RegularExpressions;

namespace InterviewMe.Application.Chat;

/// <summary>
/// Last-line filter so spoken answers cannot emit banned product names,
/// language grades (CEFR / C1 / C2 / IELTS), or claim Silas worked with
/// Tim Berners-Lee on the calculator.
/// </summary>
public static class BiographyGuard
{
    private static readonly Regex LeaveHomeSafe = new(
        @"\bLeave[\s\-]?Home[\s\-]?Safe\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex StayHomeSafe = new(
        @"\bStay[\s\-]?Home[\s\-]?Safe\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex MikeAndTim = new(
        @"Mike Berners-Lee and Tim Berners-Lee",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex TimAndMike = new(
        @"Tim Berners-Lee and Mike Berners-Lee",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex HelpedTim = new(
        @"(?<!\b(?:not|never|didn't|did not|don't|do not)\s+)help(?:ed)? Tim(?: Berners-Lee)?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex WorkedWithTim = new(
        @"(?<!\b(?:not|never|didn't|did not|don't|do not)\s+)work(?:ed|ing)? with Tim(?: Berners-Lee)?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex TimWasBoss = new(
        @"Tim(?: Berners-Lee)? was my boss",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex Ielts = new(
        @"\bIELTS\b(?:\s*(?:band\s*)?\d+(?:\.\d+)?)?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex Toefl = new(
        @"\bTOEFL\b(?:\s*iBT)?(?:\s*\d+)?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex Pte = new(
        @"\bPTE\b(?:\s*Academic)?(?:\s*\d+)?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex Cefr = new(
        @"\bCEFR\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex LanguageBand = new(
        @"\bband\s+\d+(?:\.\d+)?\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex CefrLevel = new(
        @"\b(?:an?\s+)?(?:A1|A2|B1|B2|C1|C2)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex ExtraSpaces = new(@"[ \t]{2,}", RegexOptions.Compiled);

    public static string Sanitize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        var result = LeaveHomeSafe.Replace(text, "a government home-quarantine wristband project");
        result = StayHomeSafe.Replace(result, "a government home-quarantine wristband project");
        result = result.Replace("安心出行", "政府居家檢疫手帶項目", StringComparison.Ordinal);
        result = result.Replace("居安抗疫", "政府居家檢疫手帶項目", StringComparison.Ordinal);

        result = MikeAndTim.Replace(result, "Mike Berners-Lee");
        result = TimAndMike.Replace(result, "Mike Berners-Lee");
        result = HelpedTim.Replace(result, "helped Mike Berners-Lee");
        result = WorkedWithTim.Replace(result, "worked with Mike Berners-Lee");
        result = TimWasBoss.Replace(result, "Mike Berners-Lee was my boss");

        result = Ielts.Replace(result, "");
        result = Toefl.Replace(result, "");
        result = Pte.Replace(result, "");
        result = Cefr.Replace(result, "");
        result = LanguageBand.Replace(result, "");
        result = CefrLevel.Replace(result, "fluent");
        result = ExtraSpaces.Replace(result, " ").Trim();

        return result;
    }
}

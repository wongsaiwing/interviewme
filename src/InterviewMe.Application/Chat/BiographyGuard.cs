using System.Text.RegularExpressions;

namespace InterviewMe.Application.Chat;

/// <summary>
/// Last-line filter so spoken answers cannot emit banned product names
/// or claim Silas worked with Tim Berners-Lee on the calculator.
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

        return result;
    }
}

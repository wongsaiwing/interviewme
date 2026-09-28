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

    private static readonly Regex LanguageExamSentence = new(
        @"[^.!?]*\b(?:IELTS|TOEFL|PTE|CEFR|formal grading|grading|formal language certificate|language certificate|language certification|certificate to share|language exam|language test|band score|score to quote|(?:don't|do not|didn't|did not)\s+have\s+a\s+(?:formal\s+)?(?:language\s+)?(?:score|certificate))\b[^.!?]*[.!?]?",
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

    private static readonly Regex EmDash = new(@"\s*\u2014\s*", RegexOptions.Compiled);

    private static readonly Regex FullStack = new(@"\b(F|f)ull[\s\-]stack\b", RegexOptions.Compiled);

    private static readonly Regex ParenthesizedCjk = new(@"\s*[\(\uFF08][\u4e00-\u9fff\s]+[\)\uFF09]", RegexOptions.Compiled);

    private static readonly Regex FillerActually = new(@"(?:,\s*actually,|\s+actually)(?=[\s,.!?])", RegexOptions.Compiled);

    private static readonly Regex FillerGenuinely = new(@"\s+(?:genuinely|truly)(?=\s)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Not in the facts (model-invented detail); drop the whole sentence.
    private static readonly Regex PaperSpreadsheetSentence = new(
        @"(?<=^|[.!?]\s)[^.!?]*\b(?:paper or spreadsheets?|paper and spreadsheets?|spreadsheet steps|paper-based)\b[^.!?]*[.!?]\s*",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Negative openings on behavioural questions (rule A/F); prompt rules come first.
    private static readonly Regex NoStorySentence = new(
        @"(?<=^|[.!?]\s)I don'?t have an? (?:specific |particular )?[^.!?]*?\bstor(?:y|ies)\b[^.!?]*[.!?]\s*",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex KeepItGeneral = new(@",\s*so I'll keep it general(?=[.!?])", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex NotTheTechnicalDetail = new(@",\s*not the technical details?(?=[.!?,])", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex InPersonSentence = new(
        @"(?<=[.!?]\s)[^.!?]*\brather (?:answer|discuss|talk about|go through)[^.!?]*\bin person\b[^.!?]*[.!?]\s*",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // InterviewMe stack wording matches the CV: "a React front end and a .NET back end" (ASP.NET Core only as follow-up detail).
    private static readonly Regex InterviewMeStack = new(
        @"(?:\ban?\s+)?React front[- ]end and (?:an?\s+)?ASP\.NET(?: Core)? back[- ]end",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Expected salary: single figure, no range, no endorsement tail (approved "fair market rate" sentence is untouched).
    private static readonly Regex SalaryRange = new(
        @"HKD\s*30,?000\s*(?:to|-|\u2013)\s*(?:HKD\s*)?35,?000", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex SalaryTail = new(
        @",?\s*(?:which\s+)?(?:matching|matches|in line with)\s+(?:the\s+)?industry standard(?:\s+and\s+(?:my\s+)?years of experience)?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex SalaryEnoughSentence = new(
        @"\s*(?:That's|That is|That range) (?:enough|works)(?: for me)?\.", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Glasgow graduation is July 2022 (07/2022); rewrite the superseded June date in graduation answers.
    private static readonly Regex GlasgowJune = new(@"\b(?:23(?:rd)?\s+)?June,?\s+2022\b|\b06/2022\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex ExtraSpaces = new(@"[ \t]{2,}", RegexOptions.Compiled);

    public const string LanguageFallback =
        "Cantonese is my mother tongue. Mandarin is fluent. English is fluent.";

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

        result = LanguageExamSentence.Replace(result, "");
        result = Ielts.Replace(result, "");
        result = Toefl.Replace(result, "");
        result = Pte.Replace(result, "");
        result = Cefr.Replace(result, "");
        result = LanguageBand.Replace(result, "");
        result = CefrLevel.Replace(result, "fluent");
        // Spoken-style safeguards (prompt rules come first; this only catches slips).
        result = EmDash.Replace(result, ", ");
        result = FullStack.Replace(result, m => m.Groups[1].Value + "ullstack");
        result = InterviewMeStack.Replace(result, "a React front end and a .NET back end");
        if (result.Contains("HKD", StringComparison.OrdinalIgnoreCase))
        {
            result = SalaryRange.Replace(result, "HKD 35,000");
            result = SalaryTail.Replace(result, "");
            result = SalaryEnoughSentence.Replace(result, "");
        }
        if (result.Contains("Glasgow", StringComparison.OrdinalIgnoreCase) || result.Contains("graduat", StringComparison.OrdinalIgnoreCase))
        {
            result = GlasgowJune.Replace(result, m => m.Value.Contains('/') ? "07/2022" : "July 2022");
        }
        result = FillerActually.Replace(result, "");
        result = FillerGenuinely.Replace(result, "");
        result = PaperSpreadsheetSentence.Replace(result, "");
        result = NoStorySentence.Replace(result, "");
        result = KeepItGeneral.Replace(result, "");
        result = NotTheTechnicalDetail.Replace(result, "");
        result = InPersonSentence.Replace(result, "");
        if (!PromptBuilder.LooksMostlyChinese(result))
        {
            result = ParenthesizedCjk.Replace(result, "");
        }

        result = ExtraSpaces.Replace(result, " ").Trim();

        if (string.IsNullOrWhiteSpace(result))
        {
            return LanguageFallback;
        }

        return result;
    }
}

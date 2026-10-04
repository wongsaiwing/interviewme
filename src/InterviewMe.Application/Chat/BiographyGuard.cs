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

    // MCP: only connecting existing MCP servers to an agent. Never wrote / built an MCP server.
    private static readonly Regex McpServerBuiltSentence = new(
        @"[^.!?]*\b(?:wrote|written|write|writing|built|build|building|developed|develop|developing|created|create|creating|made|implemented|implement)\s+(?:an?\s+|my\s+own\s+|our\s+own\s+|own\s+|custom\s+|a\s+custom\s+|some\s+|several\s+|two\s+|the\s+)*MCP\s+servers?\b[^.!?]*[.!?]?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // IBM RAG and Agentic AI certificate is In progress; never obtained / completed.
    private static readonly Regex RagCertCompletedSentence = new(
        @"[^.!?]*\bRAG\s+(?:and|&)\s+Agentic\s+AI\b[^.!?]*[.!?]?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex CompletedWords = new(
        @"\b(?:completed|complete|obtained|earned|finished|achieved|awarded|passed|got|hold|holds|received)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // TradeLink stack is .NET Framework only (CV wording).
    private static readonly Regex TradeLinkReact = new(
        @"\.NET Framework(?:\s*(?:,|and|&|/|\+)\s*|\s+with\s+)React\b(?:\s+front[- ]?end)?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Never used Jira / Confluence / Azure DevOps; never did user training (Scyko 2026-10-05).
    private static readonly Regex NeverUsedSentence = new(
        @"[^.!?]*\b(?:Jira|Confluence|Azure\s+DevOps|user\s+training|train(?:ed|ing)?\s+(?:the\s+)?(?:end\s+)?users)\b[^.!?]*[.!?]?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex NegationWords = new(
        @"\b(?:haven'?t|have\s+not|hasn'?t|never|didn'?t|did\s+not|don'?t|do\s+not|not|no)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex NeverUsedTool = new(
        @"\b(?:Jira|Confluence|Azure\s+DevOps)\b",
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

    private static readonly Regex FullStack = new(@"\b(F|f)ull(?:\s+stack\b|stack\b)", RegexOptions.Compiled); // "fullstack" / "full stack" -> "full-stack" (CV spelling)

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

    // Shift Briefing metrics lock (2026-09-28): old figures are rewritten to the confirmed ones or dropped.
    private static readonly Regex OldSbMetricsClause = new(
        @"It took about (?:five|5) days to get it UAT-ready, against a past manager estimate of about 20 person-days, and the token cost was about US\$100",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Retired 2026-09-28 23:48: 60 man-days to 10 / about 83% / HK$48,000 become the 80% claim or are dropped.
    private static readonly Regex RetiredSbMetricsClause = new(
        @"(?:cut delivery|delivery went) from an estimated 60 man-days to 10, (?:about|saving about) 83% (?:less time and labour cost|in time and labour cost)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex OldSbMetricSentence = new(
        @"(?<=^|[.!?]\s)[^.!?]*(?:\b(?:five|5) days\b[^.!?]*UAT|20 person-days|US\$100|\b75%|\b4x\b|\b4\u00d7|four times faster|\b83%|\b60 man-?days?\b|\b60 man-day\b|48,000)[^.!?]*[.!?]\s*",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Method wording: AI-assisted -> AI-native SDLC (prompt is primary; this only catches slips).
    private static readonly Regex AiAssistedFullStackDev = new(@"\bAI-assisted full-stack development\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    // Read and Sign scope corrected 2026-09-29: old "up to 9 stakeholders across up to 3 departments" becomes "more than 10 stakeholders".
    private static readonly Regex OldRsStakeholders = new(@"up to (?:9|nine) stakeholders across up to (?:3|three) departments", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    // "reviewed every diff" retired 2026-09-29 00:08: strip the clause if it slips back in.
    private static readonly Regex ReviewedEveryDiff = new(@",?\s+and\s+(?:I\s+)?review(?:ed|ing)?\s+(?:every|each)\s+diffs?\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex AiAssistedDev = new(@"\bAI-assisted development\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

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

        result = McpServerBuiltSentence.Replace(result, m => m.Value.StartsWith(" ") ? " I've connected existing MCP servers to an agent." : "I've connected existing MCP servers to an agent.");
        result = RagCertCompletedSentence.Replace(result, m =>
            CompletedWords.IsMatch(m.Value) && !m.Value.Contains("in progress", StringComparison.OrdinalIgnoreCase)
                ? (m.Value.StartsWith(" ") ? " " : "") + "The IBM Professional Certificate in RAG and Agentic AI is in progress."
                : m.Value);
        if (result.Contains("TradeLink", StringComparison.OrdinalIgnoreCase))
        {
            result = TradeLinkReact.Replace(result, ".NET Framework");
        }
        result = NeverUsedSentence.Replace(result, m =>
        {
            if (NegationWords.IsMatch(m.Value)) return m.Value;
            var lead = m.Value.StartsWith(" ") ? " " : "";
            var tool = NeverUsedTool.Match(m.Value);
            if (!tool.Success) return lead + "I haven't done user training.";
            var name = tool.Value.ToLowerInvariant() switch { "jira" => "Jira", "confluence" => "Confluence", _ => "Azure DevOps" };
            return lead + $"I haven't used {name}.";
        });
        result = LanguageExamSentence.Replace(result, "");
        result = Ielts.Replace(result, "");
        result = Toefl.Replace(result, "");
        result = Pte.Replace(result, "");
        result = Cefr.Replace(result, "");
        result = LanguageBand.Replace(result, "");
        result = CefrLevel.Replace(result, "fluent");
        // Spoken-style safeguards (prompt rules come first; this only catches slips).
        result = EmDash.Replace(result, ", ");
        result = FullStack.Replace(result, m => m.Groups[1].Value + "ull-stack");
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
        result = OldSbMetricsClause.Replace(result, "It cut delivery time by 80% and man-hour cost by 80%");
        result = RetiredSbMetricsClause.Replace(result, "cut delivery time by 80% and man-hour cost by 80%");
        result = OldSbMetricSentence.Replace(result, "");
        result = AiAssistedFullStackDev.Replace(result, "full-stack development with an AI-native SDLC");
        result = AiAssistedDev.Replace(result, "an AI-native SDLC");
        result = ReviewedEveryDiff.Replace(result, "");
        result = OldRsStakeholders.Replace(result, "more than 10 stakeholders");
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

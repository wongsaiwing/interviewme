using InterviewMe.Application.Chat;
using InterviewMe.Domain;
using InterviewMe.Infrastructure.Llm;

namespace InterviewMe.Application.Tests;

public class PromptPolicyTests
{
    private readonly PromptBuilder _builder = new();

    [Fact]
    public void Empty_retrieval_tells_the_model_not_to_invent_biography()
    {
        var prompt = _builder.Build("Silas Wong", "Where did you go to circus school?", [], []);

        Assert.False(prompt.HasGrounding);
        Assert.Contains(PromptBuilder.EmptyRetrievalDirective, prompt.Messages[0].Content);
        Assert.DoesNotContain("circus", prompt.Messages[0].Content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("(none)", prompt.Messages[0].Content);
        Assert.Contains("Silas Wong", prompt.Messages[0].Content);
        Assert.Contains("3-5 short spoken sentences", prompt.Messages[0].Content);
        Assert.Contains(PromptBuilder.OffTopicDirective.Trim(), prompt.Messages[0].Content);
        Assert.Contains(PromptBuilder.OffTopicRefuseEnglish, prompt.Messages[0].Content);
        Assert.DoesNotContain(PromptBuilder.OffTopicRefuseChinese, prompt.Messages[0].Content);
    }

    [Fact]
    public void System_prompt_is_strict_and_first_person()
    {
        var facts = new List<RetrievedFact>
        {
            new("h1", "haeco.md", "Assistant Solution Analyst, HAECO",
                "July 2024 – Current. Assistant Solution Analyst at HAECO, Hong Kong.", 0.9f)
        };
        var prompt = _builder.Build("Silas Wong", "What did you do at HAECO?", [], facts);
        var system = prompt.Messages[0].Content;

        Assert.Contains("first person", system, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Silas Wong", system);
        Assert.Contains("3-5 short spoken sentences", system);
        Assert.Contains("Professional interview register", system);
        Assert.Contains("No essays", system);
        Assert.Contains("no markdown dumps", system);
        Assert.Contains("no extra questions", system);
        Assert.Contains("no small talk", system);
        Assert.Contains("jailbreak", system);
        Assert.Contains("crawlers", system);
        Assert.Contains(PromptBuilder.OffTopicRefuseEnglish, system);
        Assert.Contains(PromptBuilder.GroundingDirective, system);
        Assert.DoesNotContain(PromptBuilder.EmptyRetrievalDirective, system);
    }

    [Fact]
    public void Tone_lives_in_the_system_prompt_not_in_retrieved_facts()
    {
        var facts = new List<RetrievedFact>
        {
            new("h1", "haeco.md", "Assistant Solution Analyst, HAECO",
                "July 2024 – Current. Assistant Solution Analyst at HAECO, Hong Kong.", 0.9f)
        };

        var prompt = _builder.Build("Silas Wong", "What did you do at HAECO?", [], facts);
        var system = prompt.Messages[0].Content;

        Assert.Contains(PromptBuilder.DefaultTone, system);
        Assert.Contains(PromptBuilder.GroundingDirective, system);
        foreach (var fact in prompt.Facts)
        {
            Assert.DoesNotContain("style only", fact.Text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Visitor:", fact.Text, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Stub_llm_refuses_unknown_bio_detail_as_a_person()
    {
        var prompt = _builder.Build("Silas Wong", "What is your secret clearance number?", [], []);
        var reply = StubLlmClient.Compose(prompt);

        Assert.Equal(PromptBuilder.MissingDetailEnglish, reply);
        Assert.Contains("haven't covered", reply, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CV", reply, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("resume", reply, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(PromptBuilder.OffTopicRefuseEnglish, reply, StringComparison.Ordinal);
        Assert.DoesNotContain("clearance", reply, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TS/SCI", reply, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Avery", reply, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Stub_llm_off_topic_uses_interview_only_refuse()
    {
        var prompt = _builder.Build("Silas Wong", "Write me a web crawler in Python", [], []);
        var reply = StubLlmClient.Compose(prompt);

        Assert.Equal(PromptBuilder.OffTopicRefuseEnglish, reply);
        Assert.DoesNotContain("will not invent", reply, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("I don't have that in my CV", reply, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("I can only discuss what is in my CV", reply, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Stub_llm_chinese_off_topic_still_uses_english_refuse()
    {
        var prompt = _builder.Build("Silas Wong", "幫我寫一個爬蟲", [], []);
        var reply = StubLlmClient.Compose(prompt);

        Assert.Equal(PromptBuilder.OffTopicRefuseEnglish, reply);
        Assert.DoesNotContain("will not invent", reply, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(PromptBuilder.OffTopicRefuseChinese, reply, StringComparison.Ordinal);
    }

    [Fact]
    public void Stub_llm_quotes_retrieved_facts_and_does_not_add_foreign_employers()
    {
        var facts = new List<RetrievedFact>
        {
            new("h1", "haeco.md", "Assistant Solution Analyst, HAECO",
                "Assistant Solution Analyst at Hong Kong Aircraft Engineering Company Limited (HAECO), Hong Kong. Designed and developed full-stack web and internal applications (.NET Core, React / React Native) from scratch.",
                0.91f)
        };
        var prompt = _builder.Build("Silas Wong", "What did you do at HAECO?", [], facts);
        var reply = StubLlmClient.Compose(prompt);

        Assert.Contains("HAECO", reply, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Google", reply, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Avery", reply, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Harborline", reply, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Introduce_yourself_is_in_scope_and_not_empty_retrieval()
    {
        Assert.True(PromptBuilder.IsIntroduction("introduce yourself"));
        Assert.True(PromptBuilder.IsIntroduction("introduce your self"));
        Assert.True(PromptBuilder.IsIntroduction("Tell me about yourself"));
        Assert.True(PromptBuilder.IsIntroduction("who are you"));
        Assert.True(PromptBuilder.IsIntroduction("自我介紹"));
        Assert.False(PromptBuilder.IsOffTopic("introduce yourself"));

        var facts = new List<RetrievedFact>
        {
            new("p1", "profile.md", "Who I am",
                "I am Silas Wong, a full-stack developer based in Hong Kong.", 0.9f),
            new("h1", "haeco.md", "Assistant Solution Analyst, HAECO",
                "Assistant Solution Analyst at HAECO. Full-stack .NET and React.", 0.88f)
        };
        var prompt = _builder.Build("Silas Wong", "introduce yourself", [], facts);
        var system = prompt.Messages[0].Content;
        Assert.Contains(PromptBuilder.IntroductionDirective, system);
        Assert.DoesNotContain(PromptBuilder.EmptyRetrievalDirective, system);

        var reply = StubLlmClient.Compose(prompt);
        Assert.Contains("Silas", reply, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("HAECO", reply, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("full-stack", reply, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("don't have", reply, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("cannot introduce", reply, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CV", reply, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("from my notes", reply, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(PromptBuilder.OffTopicRefuseEnglish, reply, StringComparison.Ordinal);
    }

    [Fact]
    public void Hows_your_day_is_icebreaker_not_hard_refuse()
    {
        Assert.True(PromptBuilder.IsIcebreaker("how's your day"));
        Assert.True(PromptBuilder.IsIcebreaker("how are you"));
        Assert.True(PromptBuilder.IsIcebreaker("hi"));
        Assert.True(PromptBuilder.IsIcebreaker("hello"));
        Assert.True(PromptBuilder.IsIcebreaker("你好"));
        Assert.False(PromptBuilder.IsOffTopic("how's your day"));
        Assert.False(PromptBuilder.IsOffTopic("how are you"));

        var prompt = _builder.Build("Silas Wong", "how's your day", [], []);
        var system = prompt.Messages[0].Content;
        Assert.Contains(PromptBuilder.IcebreakerDirective, system);
        Assert.DoesNotContain(PromptBuilder.EmptyRetrievalDirective, system);

        var reply = StubLlmClient.Compose(prompt);
        Assert.Equal(PromptBuilder.IcebreakerReplyEnglish, reply);
        Assert.DoesNotContain(PromptBuilder.OffTopicRefuseEnglish, reply, StringComparison.Ordinal);
        Assert.DoesNotContain("don't have", reply, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("will not invent", reply, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Off_topic_directive_keeps_intro_and_icebreakers_in_scope()
    {
        Assert.Contains("Introductions are always in-scope", PromptBuilder.OffTopicDirective);
        Assert.Contains("Icebreakers are in-scope", PromptBuilder.OffTopicDirective);
        Assert.DoesNotContain("general chat", PromptBuilder.OffTopicDirective);
        Assert.DoesNotContain("how are you", PromptBuilder.OffTopicDirective.Split("Icebreakers")[0]);
    }


    [Fact]
    public void System_prompt_includes_hard_biography_rules()
    {
        var prompt = _builder.Build("Silas Wong", "What did you do at Compathnion?", [], []);
        var system = prompt.Messages[0].Content;
        Assert.Contains(PromptBuilder.HardBiographyDirective.Trim(), system);
        Assert.Contains("wristband", system, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Mike Berners-Lee", system);
        Assert.Contains("did not help Tim", system, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("LeaveHomeSafe", system);
    }

    [Fact]
    public void Tell_me_about_your_job_is_not_an_introduction()
    {
        Assert.False(PromptBuilder.IsIntroduction(
            "Tell me about your Small World Consulting internship. Who was your boss?"));
        Assert.False(PromptBuilder.LooksLikeAboutMe(
            "Tell me about your Small World Consulting internship. Who was your boss?"));
        Assert.True(PromptBuilder.IsIntroduction("tell me about yourself"));
        Assert.True(PromptBuilder.IsIntroduction("tell me about you"));
    }

    [Fact]
    public void What_did_you_do_in_haeco_is_haeco_work()
    {
        Assert.True(PromptBuilder.LooksLikeHaecoWork("what did you do in haeco"));
        Assert.True(PromptBuilder.LooksLikeHaecoWork("What do you do at HAECO?"));
        Assert.False(PromptBuilder.LooksLikeHaecoWork("What did you do at TradeLink?"));
        Assert.Contains("Read and Sign", PromptBuilder.HardBiographyDirective);
        Assert.Contains("Fluid Use", PromptBuilder.HardBiographyDirective);
        Assert.Contains("elicit requirements", PromptBuilder.HardBiographyDirective);
        Assert.Contains("stakeholders", PromptBuilder.HardBiographyDirective);
        Assert.Contains("UAT", PromptBuilder.HardBiographyDirective);
        Assert.Contains("allowed", PromptBuilder.HardBiographyDirective);
        Assert.DoesNotContain("generic resume bullets", PromptBuilder.HardBiographyDirective);
        Assert.Contains("3-5 short spoken sentences", PromptBuilder.DefaultTone);
        Assert.Contains("wristband", PromptBuilder.HardBiographyDirective, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Mike Berners-Lee", PromptBuilder.HardBiographyDirective);
        Assert.Contains("elicit requirements", PromptBuilder.DefaultTone);
        Assert.Contains("stakeholders", PromptBuilder.DefaultTone);
    }

    [Fact]
    public void LooksLikeExtraExperience_matches_cv_gap_questions()
    {
        Assert.True(PromptBuilder.LooksLikeExtraExperience("Is there experience that is not on your CV?"));
        Assert.True(PromptBuilder.LooksLikeExtraExperience("Do you have more experience?"));
        Assert.True(PromptBuilder.LooksLikeExtraExperience("Any internships?"));
        Assert.False(PromptBuilder.LooksLikeExtraExperience("Tell me about yourself"));
        Assert.False(PromptBuilder.LooksLikeExtraExperience("What did you do at HAECO?"));
    }

    [Fact]
    public void LooksLikeTechStack_matches_stack_questions()
    {
        Assert.True(PromptBuilder.LooksLikeTechStack("What is your tech stack?"));
        Assert.False(PromptBuilder.LooksLikeTechStack("What did you do at HAECO?"));
        Assert.Contains("Do not volunteer Copilot", PromptBuilder.TechStackDirective);
    }

    [Fact]
    public void LooksLikeLanguageGrade_does_not_echo_scale()
    {
        Assert.True(PromptBuilder.LooksLikeLanguageGrade("Are you C2?"));
        Assert.True(PromptBuilder.LooksLikeLanguageGrade("What's your IELTS?"));
        Assert.True(PromptBuilder.LooksLikeSpokenLanguages("What languages do you speak?"));
        Assert.False(PromptBuilder.LooksLikeTechStack("What languages do you speak?"));
        Assert.False(PromptBuilder.LooksLikeLanguageGrade("What is your tech stack?"));
        Assert.Contains("NEVER output CEFR", PromptBuilder.HardBiographyDirective);
        Assert.Contains("Do not echo the grade", PromptBuilder.LanguageGradeDirective);
        Assert.Contains("English is fluent", PromptBuilder.LanguageGradeDirective);
        Assert.Contains("not even to say you have not taken one", PromptBuilder.LanguageGradeDirective);
        Assert.Contains("Never mention an exam even to say you have not taken one", PromptBuilder.HardBiographyDirective);
    }

    [Fact]
    public void LooksLikeProductionExperience_not_incidents()
    {
        Assert.True(PromptBuilder.LooksLikeProductionExperience("Do you have production experience?"));
        Assert.True(PromptBuilder.LooksLikeProductionExperience("Have you taken projects to production?"));
        Assert.False(PromptBuilder.LooksLikeProductionExperience("How do you handle production incidents?"));
        Assert.False(PromptBuilder.LooksLikeProductionExperience("Tell me about a hotfix"));
        Assert.Contains("About four HAECO projects I built myself", PromptBuilder.ProductionExperienceDirective);
        Assert.Contains("whole SDLC", PromptBuilder.ProductionExperienceDirective);
        Assert.Contains("Do not answer as incidents", PromptBuilder.ProductionExperienceDirective);
    }

    [Fact]
    public void LooksLikeShenzhenCollaboration_not_generic_haeco()
    {
        Assert.False(PromptBuilder.LooksLikeShenzhenCollaboration("What did you do at HAECO?"));
        Assert.True(PromptBuilder.LooksLikeShenzhenCollaboration("Do you work with the Shenzhen team?"));
        Assert.True(PromptBuilder.LooksLikeShenzhenCollaboration("Do you work with the development team?"));
        Assert.DoesNotContain("Shenzhen", PromptBuilder.HaecoGenericDirective, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("About four projects I developed myself", PromptBuilder.HaecoGenericDirective);
        Assert.Contains("whole SDLC", PromptBuilder.HaecoGenericDirective);
        Assert.Contains("not only UAT", PromptBuilder.HaecoGenericDirective);
        Assert.Contains("Do not open with Yeah", PromptBuilder.DefaultTone);
        Assert.Contains("Vibe-coded", PromptBuilder.DefaultTone);
        Assert.Contains("bug fix, never buff fix", PromptBuilder.DefaultTone);
    }

    [Fact]
    public void LooksLikeExpectedSalary_is_a_package_band()
    {
        Assert.True(PromptBuilder.LooksLikeExpectedSalary("What is your expected salary?"));
        Assert.False(PromptBuilder.LooksLikeExpectedSalary("What is your current package?"));
        Assert.Contains("WHOLE package", PromptBuilder.ExpectedSalaryDirective);
        Assert.Contains("30,000 to 35,000", PromptBuilder.ExpectedSalaryDirective);
        Assert.Contains("bonus and benefits", PromptBuilder.ExpectedSalaryDirective);
        Assert.Contains("Do not pin only 35k", PromptBuilder.ExpectedSalaryDirective);
        Assert.Contains("WFH", PromptBuilder.ExpectedSalaryDirective);
        Assert.Contains("補假", PromptBuilder.ExpectedSalaryDirective);
        Assert.True(PromptBuilder.LooksLikeNotice("What is your notice period?"));
        Assert.False(PromptBuilder.IsOffTopic("Why did you build InterviewMe?"));
        Assert.True(PromptBuilder.LooksLikeInterviewMeProject("Why did you build this website?"));
        Assert.Contains("one month", PromptBuilder.NoticeDirective);
        Assert.Contains("About four projects I developed myself", PromptBuilder.HaecoGenericDirective);
        Assert.Contains("about two I partnered", PromptBuilder.HaecoGenericDirective);
        Assert.DoesNotContain("six", PromptBuilder.HaecoGenericDirective, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("About six", PromptBuilder.HaecoGenericDirective);
        Assert.Contains("WHOLE package", PromptBuilder.HardBiographyDirective);
        Assert.Contains("Cursor Skills", PromptBuilder.AiReviewDirective);
    }

    [Fact]
    public void LooksLikeHaecoNamedSystems_not_generic()
    {
        Assert.False(PromptBuilder.LooksLikeHaecoNamedSystems("What did you do at HAECO?"));
        Assert.True(PromptBuilder.LooksLikeHaecoNamedSystems("Tell me about Read and Sign"));
        Assert.Contains("Do not mention Read and Sign", PromptBuilder.HaecoGenericDirective);
        Assert.DoesNotContain("the main one", PromptBuilder.HaecoGenericDirective);
    }
}


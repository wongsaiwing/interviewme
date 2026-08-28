using InterviewMe.Application.Chat;

namespace InterviewMe.Application.Tests;

public class BiographyGuardTests
{
    [Fact]
    public void Strips_LeaveHomeSafe_and_does_not_invent_StayHomeSafe()
    {
        var raw = "I focused on test cases for a government project related to LeaveHomeSafe. I also used a dashboard.";
        var clean = BiographyGuard.Sanitize(raw);

        Assert.DoesNotContain("LeaveHomeSafe", clean, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("安心出行", clean, StringComparison.Ordinal);
        Assert.DoesNotContain("StayHomeSafe", clean, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("居安抗疫", clean, StringComparison.Ordinal);
        Assert.Contains("wristband", clean, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Does_not_claim_working_with_Tim_on_the_calculator()
    {
        var raw = "I helped Mike Berners-Lee and Tim Berners-Lee build a carbon emission calculator for tracking mass-production companies.";
        var clean = BiographyGuard.Sanitize(raw);

        Assert.Contains("Mike Berners-Lee", clean, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("and Tim Berners-Lee", clean, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("helped Tim", clean, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Keeps_did_not_work_with_Tim()
    {
        var raw = "My boss was Mike Berners-Lee. I did not work with Tim Berners-Lee personally.";
        var clean = BiographyGuard.Sanitize(raw);

        Assert.Contains("did not work with Tim", clean, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Mike Berners-Lee", clean, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Strips_language_grades_and_exams()
    {
        var raw = "Yes, my English is CEFR C2 and I have IELTS 8.0. I am a C1 in Mandarin.";
        var clean = BiographyGuard.Sanitize(raw);

        Assert.DoesNotContain("CEFR", clean, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C2", clean, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C1", clean, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IELTS", clean, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("8.0", clean, StringComparison.Ordinal);
        Assert.Contains("fluent", clean, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Drops_ielts_decline_sentence()
    {
        var raw = "I'd say my English is fluent. I haven't taken IELTS or any formal grading recently, so I don't have a score to quote.";
        var clean = BiographyGuard.Sanitize(raw);

        Assert.DoesNotContain("IELTS", clean, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("formal grading", clean, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("score", clean, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("fluent", clean, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Drops_certificate_decline_sentence()
    {
        var raw = "Cantonese is my mother tongue, Mandarin is fluent, and English is fluent. I don't have a formal language certificate to share.";
        var clean = BiographyGuard.Sanitize(raw);

        Assert.DoesNotContain("certificate", clean, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IELTS", clean, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("fluent", clean, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("mother tongue", clean, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Does_not_strip_csharp_as_a_cefr_level()
    {
        var raw = "My stack is C# and .NET Core.";
        var clean = BiographyGuard.Sanitize(raw);
        Assert.Contains("C#", clean, StringComparison.Ordinal);
    }
}

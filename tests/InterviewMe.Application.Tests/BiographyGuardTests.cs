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
}

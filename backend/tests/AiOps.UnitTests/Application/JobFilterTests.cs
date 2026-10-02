using AiOps.Application.Jobs;
using AiOps.UnitTests.TestDoubles;

namespace AiOps.UnitTests.Application;

public class JobFilterTests
{
    [Fact]
    public void Date_range_includes_from_and_excludes_to()
    {
        var filter = new JobFilter { From = Jobs.T0, To = Jobs.T0.AddDays(1) };

        Assert.True(filter.Matches(Jobs.New(createdAt: Jobs.T0)));
        Assert.False(filter.Matches(Jobs.New(createdAt: Jobs.T0.AddDays(1))));
        Assert.False(filter.Matches(Jobs.New(createdAt: Jobs.T0.AddTicks(-1))));
    }

    [Fact]
    public void Search_matches_document_name_case_insensitively()
    {
        var filter = new JobFilter { Search = "inv-42" };

        Assert.True(filter.Matches(Jobs.New(document: "INV-4211.pdf")));
        Assert.False(filter.Matches(Jobs.New(document: "receipt-1.jpg")));
    }
}

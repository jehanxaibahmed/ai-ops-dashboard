using AiOps.Domain.Common;
using AiOps.Domain.Evaluations;
using AiOps.UnitTests.TestDoubles;

namespace AiOps.UnitTests.Domain;

public class EvaluationResultTests
{
    private static EvaluationResult Create(string[] fields, string[] missed) =>
        new(Guid.NewGuid(), Guid.NewGuid(), "receipt-ocr", "gpt-mini", Jobs.T0, fields, missed);

    [Fact]
    public void Accuracy_is_share_of_correct_fields()
    {
        var result = Create(["a", "b", "c", "d"], ["b"]);

        Assert.Equal(3, result.FieldsCorrect);
        Assert.Equal(0.75, result.Accuracy);
        Assert.False(result.IsPerfect);
    }

    [Fact]
    public void Rejects_missed_fields_that_were_not_evaluated()
    {
        Assert.Throws<DomainException>(() => Create(["a"], ["z"]));
    }

    [Fact]
    public void Rejects_empty_field_list()
    {
        Assert.Throws<DomainException>(() => Create([], []));
    }
}

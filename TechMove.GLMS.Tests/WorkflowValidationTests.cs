using TechMove.GLMS.Models;
using TechMove.GLMS.Services;

namespace TechMove.GLMS.Tests;

public class WorkflowValidationTests
{
    private readonly WorkflowValidationService _sut = new();

    [Theory]
    [InlineData(ContractStatus.Expired)]
    [InlineData(ContractStatus.OnHold)]
    public void BlockedStatuses_CannotCreateServiceRequest_ReturnsFalse(ContractStatus status)
    {
        var result = _sut.CanCreateServiceRequest(status);

        Assert.False(result);
    }

    [Theory]
    [InlineData(ContractStatus.Active)]
    [InlineData(ContractStatus.Draft)]
    public void AllowedStatuses_CanCreateServiceRequest_ReturnsTrue(ContractStatus status)
    {
        var result = _sut.CanCreateServiceRequest(status);

        Assert.True(result);
    }
}

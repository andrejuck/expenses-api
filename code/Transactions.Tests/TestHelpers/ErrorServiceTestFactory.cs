using System.Net;
using Libs.Api.ErrorHandling;
using NSubstitute;

namespace Transactions.Tests.TestHelpers;

/// <summary>
/// Creates a stateful substitute for IErrorService: calling AddError flips HasErrors to true,
/// mimicking the behaviour of the real implementation used by the application layer.
/// </summary>
public static class ErrorServiceTestFactory
{
    public static IErrorService CreateStateful()
    {
        var errorService = Substitute.For<IErrorService>();
        var hasErrors = false;

        errorService
            .When(x => x.AddError(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<HttpStatusCode>()))
            .Do(_ => hasErrors = true);

        errorService.HasErrors.Returns(_ => hasErrors);

        return errorService;
    }
}

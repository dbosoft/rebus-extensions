using FluentAssertions;

namespace Dbosoft.Rebus.Operations.Tests;

public static class TestTraceEntryFluentExtensions
{
    public static void ShouldMatch(
        this TestTraceEntry trace,
        Type handlerType,
        string methodName,
        Type messageType)
    {
        trace.Handler.Should().Be(handlerType);
        trace.Method.Should().Be(methodName);
        trace.Message.Should().Be(messageType);
    }
}

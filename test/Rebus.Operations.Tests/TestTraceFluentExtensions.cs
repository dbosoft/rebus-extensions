using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace Dbosoft.Rebus.Operations.Tests;

public static class TestTraceFluentExtensions
{
    public static void ShouldMatch(
        this TestTrace trace,
        Type handlerType,
        string methodName,
        Type messageType)
    {
        trace.Handler.Should().Be(handlerType);
        trace.Method.Should().Be(methodName);
        trace.Message.Should().Be(messageType);
    }
}

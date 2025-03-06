using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dbosoft.Rebus.Operations.Tests;

public class TestTracer
{
    private readonly ConcurrentQueue<TestTrace> _traces = new ();

    public IList<TestTrace> Traces => _traces.ToList();

    public void Trace(object handler, string method, object message)
    {
        Trace(handler.GetType(), method, message.GetType());
    }

    public void Trace(Type type, string method, Type message)
    {
        _traces.Enqueue(new TestTrace(type, method, message));
    }
}

public record TestTrace(
    Type Handler,
    string Method,
    Type Message);

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dbosoft.Rebus.Operations.Tests;

public class TestTrace
{
    private readonly ConcurrentQueue<TestTraceEntry> _traces = new ();

    public IList<TestTraceEntry> Traces => _traces.ToList();

    public void Trace(object handler, string method, object message, object? data = null)
    {
        Trace(handler.GetType(), method, message.GetType(), data);
    }

    public void Trace(Type type, string method, Type message, object? data = null)
    {
        _traces.Enqueue(new TestTraceEntry(type, method, message, data));
    }
}

public record TestTraceEntry(
    Type Handler,
    string Method,
    Type Message,
    object? Data);

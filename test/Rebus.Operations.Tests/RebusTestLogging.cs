using Rebus.Logging;
using Xunit.Abstractions;

namespace Dbosoft.Rebus.Operations.Tests;

public class RebusTestLogging(ITestOutputHelper testOutputHelper)
    : AbstractRebusLoggerFactory
{
    protected override ILog GetLogger(Type type)
    {
        return new Log(testOutputHelper, this);
    }

    public class Log(
        ITestOutputHelper testOutputHelper,
        RebusTestLogging factory)
        : ILog
    {
        public void Debug(string message, params object[] objs)
        {
            testOutputHelper.WriteLine($"DBG: {factory.RenderString(message, objs)}");
        }

        public void Info(string message, params object[] objs)
        {
            testOutputHelper.WriteLine($"INF: {factory.RenderString(message, objs)}");
        }

        public void Warn(string message, params object[] objs)
        {
            testOutputHelper.WriteLine($"WRN: {factory.RenderString(message, objs)}");
        }

        public void Warn(Exception exception, string message, params object[] objs)
        {
            testOutputHelper.WriteLine($"WRN: {factory.RenderString(message, objs)}\n{exception}");
        }

        public void Error(string message, params object[] objs)
        {
            testOutputHelper.WriteLine($"ERR: {factory.RenderString(message, objs)}");
        }

        public void Error(Exception exception, string message, params object[] objs)
        {
            testOutputHelper.WriteLine($"ERR: {factory.RenderString(message, objs)}\n{exception}");
        }
    }
}

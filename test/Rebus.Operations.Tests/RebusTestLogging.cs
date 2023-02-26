using Rebus.Logging;
using Xunit.Abstractions;

namespace Dbosoft.Rebus.Operations.Tests;

public class RebusTestLogging : AbstractRebusLoggerFactory
{
    private readonly ITestOutputHelper _testOutputHelper;

    public RebusTestLogging(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    protected override ILog GetLogger(Type type)
    {
        return new Log(_testOutputHelper, this);
    }

    public class Log : ILog
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly RebusTestLogging _factory;

        public Log(ITestOutputHelper testOutputHelper, RebusTestLogging factory)
        {
            _testOutputHelper = testOutputHelper;
            _factory = factory;
        }

        public void Debug(string message, params object[] objs)
        {
            _testOutputHelper.WriteLine("DBG: "+ _factory.RenderString(message, objs));
        }

        public void Info(string message, params object[] objs)
        {
            _testOutputHelper.WriteLine("INF: "+ _factory.RenderString(message, objs));
        }

        public void Warn(string message, params object[] objs)
        {
            _testOutputHelper.WriteLine("WRN: "+ _factory.RenderString(message, objs));
        }

        public void Warn(Exception exception, string message, params object[] objs)
        {
            _testOutputHelper.WriteLine("WRN: "+ _factory.RenderString(message, objs) + exception);
        }

        public void Error(string message, params object[] objs)
        {
            _testOutputHelper.WriteLine("ERR: "+ _factory.RenderString(message, objs));
        }

        public void Error(Exception exception, string message, params object[] objs)
        {
            _testOutputHelper.WriteLine("ERR: "+ _factory.RenderString(message, objs)  + exception);
        }
        

    }
}
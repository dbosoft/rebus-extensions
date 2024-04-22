using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Rebus.Activation;
using Rebus.Config;
using Rebus.Injection;
using Rebus.Transport.InMem;
using Xunit;

namespace Dbosoft.Rebus.Configuration.Tests;

public abstract class SelectorTests
{
    protected (ILogger Logger, IConfiguration Configuration) SetupDeps(IDictionary<string, string> config)
    {
        var configRoot = new ConfigurationBuilder()
            .AddInMemoryCollection(config).Build();

        var nullLoggerFactory = new NullLoggerFactory();
        return (nullLoggerFactory.CreateLogger(""), configRoot);

    }

    protected RebusConfigurer CreateConfigurer(Action<RebusConfigurer> testSetup, bool noTransport)
    {
        var someContainerAdapter = new BuiltinHandlerActivator();

        var configure = Configure.With(someContainerAdapter);

        if(!noTransport)
            configure.Transport(cfg => cfg.UseInMemoryTransport(new InMemNetwork(), "null", false));

        testSetup(configure);

        return configure;
    }

    protected Injectionist ConfigureAndGetContainer(Action<RebusConfigurer> testSetup, bool noTransport = false)
    {
        var configure = CreateConfigurer(testSetup, noTransport);
        configure.Create();

        var injectionistField = configure.GetType().GetField("_injectionist", BindingFlags.Instance | BindingFlags.NonPublic);
        var injectionist = injectionistField?.GetValue(configure) as Injectionist;

        Assert.NotNull(injectionist);
        return injectionist!;

    }

}
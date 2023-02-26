using Dbosoft.Rebus.Operations;
using Dbosoft.Rebus.Operations.Tests;
using Rebus.Persistence.InMem;
using Rebus.Transport.InMem;
using SimpleInjector;
using Xunit.Abstractions;

namespace Dbosoft.Rebus.SimpleInjector.Tests;

public abstract class SimpleInjectorTestBase
{
    private readonly ITestOutputHelper _output;

    protected SimpleInjectorTestBase(ITestOutputHelper output)
    {
        _output = output;
    }

    public void SetupRebus(Container container)
    {
        var rebusNetwork = new InMemNetwork();
        
        container.Register<IRebusUnitOfWork, TestRebusUnitOfWork>(Lifestyle.Scoped);
        container.AddRebusOperationsHandlers<TestOperationManager, TestTaskManager>();
        container.ConfigureRebus(configurer =>
        {
            return configurer
                .Options(o => o.EnableSimpleInjectorUnitOfWork())
                .Transport(cfg => cfg.UseInMemoryTransport(rebusNetwork, "main"))
                .AddOperations("main")
                .Sagas(x => x.StoreInMemory())
                .Subscriptions(x=>x.StoreInMemory())
                .Logging(x=>x.Use(new RebusTestLogging(_output)))

                .Start();
        });

    }

}
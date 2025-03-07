using Dbosoft.Rebus.Operations;
using Dbosoft.Rebus.Operations.Tests;
using Dbosoft.Rebus.Operations.Tests.Data;
using Rebus.Persistence.InMem;
using Rebus.Routing.TypeBased;
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

    public void SetupRebus(
        Container container,
        WorkflowEventDispatchMode dispatchMode,
        bool useTypeBasedRouting)
    {
        var rebusNetwork = new InMemNetwork();
        
        container.Register<IRebusUnitOfWork, TestRebusUnitOfWork>(Lifestyle.Scoped);
        container.RegisterInstance(new WorkflowOptions
        {
            DispatchMode = dispatchMode,
            EventDestination = useTypeBasedRouting ? null : "main",
            OperationsDestination = useTypeBasedRouting ? null : "main",
        });
        container.AddRebusOperationsHandlers<TestOperationManager, TestTaskManager>();
        container.ConfigureRebus(configurer =>
        {
            return configurer
                .Options(o => o.EnableSimpleInjectorUnitOfWork())
                .Transport(cfg => cfg.UseInMemoryTransport(rebusNetwork, "main"))
                .Routing(r =>
                {
                    if (useTypeBasedRouting)
                    {
                        r.TypeBased().AddOperations("main");
                    }
                })
                .Sagas(x => x.StoreInMemory())
                .Logging(x=>x.Use(new RebusTestLogging(_output)))
                .Start();
        });
    }
}

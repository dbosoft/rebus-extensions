using Rebus.Injection;
using Xunit;

namespace Dbosoft.Rebus.Configuration.Tests;

public static class InjectionistExtensions
{
    public static Injectionist AssertConfigured<TRef>(this Injectionist container, string typeName)
    {
        var rebusAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.DefinedTypes.Contains(typeof(TRef)));
        var instance = container.Get<TRef>().Instance;
        Assert.IsType(rebusAssembly!.GetType(typeName, true)!, instance);

        return container;
    }
}
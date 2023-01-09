using Rebus.Config;

namespace Dbosoft.Rebus.Configuration;

public interface IGenericRebusConfigurer<T>
{
    void Configure(StandardConfigurer<T> configurer);
}
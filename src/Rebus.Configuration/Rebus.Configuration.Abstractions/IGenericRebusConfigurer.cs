using Rebus.Config;

namespace Dbosoft.Rebus.Configuration;

public interface IRebusConfigurer<T>
{
    void Configure(StandardConfigurer<T> configurer);
}
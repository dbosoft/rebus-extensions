using Microsoft.Extensions.Configuration;

namespace Dbosoft.Rebus.Configuration;

public abstract class RebusSelectorBase
{
    protected RebusSelectorBase(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public abstract string[] AcceptedConfigTypes { get; }
    public abstract string ConfigurationName { get; }

    public readonly IConfiguration Configuration;


}
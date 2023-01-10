using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rebus.Config;
using Rebus.Persistence.FileSystem;
using Rebus.Persistence.InMem;
using Rebus.Sagas;

namespace Dbosoft.Rebus.Configuration;

[PublicAPI]
public class DefaultSagaStoreSelector : GenericRebusSelectorBase<ISagaStorage>, IRebusSagaConfigurer
{
    

    public DefaultSagaStoreSelector(IConfiguration configuration, 
        ILogger log) : base(configuration, log)
    {
    }

    public override string[] AcceptedConfigTypes => new[] { "inmemory", "filesystem" };
    public override string ConfigurationName => "store";


    protected override void ConfigureByType(string busType, StandardConfigurer<ISagaStorage> configurer)
    {
        switch (busType)
        {
            case "inmemory":
                configurer.StoreInMemory();
                break;
            case "filesystem":
                var path = Configuration[$"{ConfigurationName}:path"];

                if(path == null)
                    throw new InvalidOperationException($"Missing configuration entry for {ConfigurationName}::path.");

                configurer.UseFilesystem(path);
                return;
        }
    }
}
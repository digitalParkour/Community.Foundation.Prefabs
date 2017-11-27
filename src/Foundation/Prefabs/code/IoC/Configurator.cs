using Community.Foundation.Prefabs.Abstractions.Services;
using Community.Foundation.Prefabs.Services;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;

namespace Community.Foundation.Prefabs.IoC
{
    public class Configurator : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IPrefabService, PrefabService>();
            serviceCollection.AddTransient<ISmartCopyService, SmartCopyService>();
        }
    }
}
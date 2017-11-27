using Community.Foundation.Prefabs.Sxa;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.XA.Foundation.IOC.Pipelines.IOC;
using Sitecore.XA.Foundation.Multisite;

namespace Community.Foundation.Prefabs.IoC
{
    public class GlobalMultisiteServices : Sitecore.XA.Foundation.Multisite.Pipelines.IoC.RegisterMultisiteServices
    {
        public override void Process(IocArgs args)
        {
            var sxa = new ServiceDescriptor(typeof(IMultisiteContext), typeof(MultisiteContext), ServiceLifetime.Singleton);
            args.ServiceCollection.Remove(sxa);
            args.ServiceCollection.AddSingleton<IMultisiteContext, MultisiteContextForGlobal>();
        }
    }
}
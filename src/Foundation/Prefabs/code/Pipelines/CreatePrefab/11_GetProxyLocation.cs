using Community.Foundation.Prefabs.Configuration;
using Sitecore.Diagnostics;

namespace Community.Foundation.Prefabs.Pipelines.CreatePrefab
{
    public class GetProxyLocation : CreatePrefabProcessor
    {
        /// <summary>
        /// Assume convention that rendering name matches prefab name
        /// Lookup item from defined global repository
        /// </summary>
        /// <param name="args"></param>
        public override void Process(CreatePrefabArgs args)
        {
            Assert.IsNotNull(args, "args");

            args.RenderingLocation = args.ContextItem.Database.GetItem(Config.Paths.Renderings);
        }
    }
}
using Community.Foundation.Prefabs.Configuration;
using Sitecore.Configuration;
using Sitecore.SecurityModel;

namespace Community.Foundation.Prefabs.Pipelines.PrefabsGetItem
{
    public class GetFromGlobalData : PrefabsGetItemProcessor
    {
        /// <summary>
        /// Assume convention that rendering name matches prefab name
        /// Lookup item from defined global repository
        /// </summary>
        /// <param name="args"></param>
        public override void Process(PrefabsGetItemArgs args)
        {
            if (args.Handled)
                return;

            using (new SecurityDisabler())
            {
                var globalFolder = args.PrefabProxy.Database.GetItem(Config.Paths.Prefabs);

                var result = globalFolder?.Axes.GetDescendant(args.PrefabProxy.Name);
                if (result == null)
                    return;

                args.Result = result;
                args.AbortPipeline();
            }
        }
    }
}
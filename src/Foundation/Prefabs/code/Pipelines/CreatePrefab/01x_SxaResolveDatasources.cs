using Sitecore.Diagnostics;
using Sitecore.SecurityModel;

namespace Community.Foundation.Prefabs.Pipelines.CreatePrefab
{
    /// <summary>
    /// Run this before datasource items are copied 
    /// </summary>
    public class SxaResolveDatasources : CreatePrefabProcessor
    {
        public override void Process(CreatePrefabArgs args)
        {
            Assert.IsNotNull(args, "args");
            Assert.IsNotNull(args.ContextItem, "args.Database");
            Assert.IsNotNull(args.Renderings, "args.Renderings");

            if (!args.ContextItem.HasChildren)
                return;

            var db = args.ContextItem.Database;
            var basePath = args.ContextItem.Paths.FullPath;

            foreach (var r in args.Renderings)
            {
                if (string.IsNullOrWhiteSpace(r.Datasource))
                    continue;

                if (!r.Datasource.StartsWith("local:"))
                    continue;

                using (new SecurityDisabler()) // Avoid permission issues
                {
                    // Resolve "local:" format to ID
                    var dataItem = db.GetItem($"{basePath}{r.Datasource.Remove(0, "local:".Length)}");
                    if (dataItem != null)
                        r.Datasource = dataItem.ID.ToString();
                }
            }
        }
                
    }
}
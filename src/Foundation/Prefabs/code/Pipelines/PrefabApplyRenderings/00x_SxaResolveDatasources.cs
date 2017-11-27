using Community.Foundation.Prefabs.Pipelines.PrefabGetRenderings;
using Sitecore.SecurityModel;

namespace Community.Foundation.Prefabs.Pipelines.PrefabApplyRenderings
{
    public class SxaResolveDatasources : PrefabApplyRenderingsProcessor
    {
        public override void Process(PrefabGetRenderingsArgs args)
        {
            if (!args.Handled)
                return;

            if (!args.Prefab.HasChildren)
                return;

            var basePath = args.PageItem.Paths.FullPath;
            foreach (var r in args.Result)
            {
                if (string.IsNullOrWhiteSpace(r.Datasource))
                    continue;

                if (!r.Datasource.StartsWith("local:"))
                    continue;

                using (new SecurityDisabler()) // Avoid permission issues
                {
                    // Resolve "local:" format to ID
                    var dataItem = args.Prefab.Database.GetItem($"{basePath}{r.Datasource.Remove(0, "local:".Length)}");
                    if (dataItem != null)
                        r.Datasource = dataItem.ID.ToString();
                }
            }
        }
                
    }
}
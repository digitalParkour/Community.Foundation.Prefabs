using Community.Foundation.Prefabs.Configuration;
using Community.Foundation.Prefabs.Pipelines.Shoehorn;
using Sitecore.Layouts;
using Sitecore.Web;
using System.Collections.Generic;
using System.Linq;

namespace Community.Foundation.Prefabs.Pipelines.PrefabApplyRenderings
{
    public class MapSxaDynamicPlaceholders : MapDynamicPlaceholders
    {
        protected ExposedInsertDynamicRendering SxaProcessor = new ExposedInsertDynamicRendering();

        public override bool ShouldRun()
        {
            // Run when we have placeholders configured and is SXA
            return _dynamicPlaceholders.Patterns.Any() && Config.Sxa.IsEnabled;
        }

        // REMOVED THIS since new configuration for placeholders is sufficient to check if rendering is dynamic
        //public override bool IsDynamic(RenderingDefinition rendering)
        //{
        //    return SxaProcessor.RenderingsWithDynamicPlaceholders.Contains(rendering.ItemID);
        //}


        public override int NextDynamicId(RenderingDefinition renderingDefinition, IEnumerable<RenderingDefinition> renderings)
        {
            int num = 0;
            
            if (renderings == null || !renderings.Any())
            {
                return 1;
            }
            foreach (RenderingDefinition renderingDefinition1 in renderings.Where((RenderingDefinition r) => {
                if (r == null || r.UniqueId == renderingDefinition.UniqueId)
                {
                    return false;
                }
                if (r.ItemID == renderingDefinition.ItemID)
                {
                    return true;
                }
                return SxaProcessor.AreCompatibleRenderings(renderingDefinition, r);
            }))
            {
                if (string.IsNullOrEmpty(renderingDefinition1.Parameters))
                {
                    continue;
                }
                int num1 = (int.TryParse(WebUtil.ParseUrlParameters(renderingDefinition1.Parameters)[DynamicPlaceholderIdParam], out num1) ? num1 : num);
                if (num1 <= num)
                {
                    continue;
                }
                num = num1;
            }
            return num + 1;
        }
    }
}
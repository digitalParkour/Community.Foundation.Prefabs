using Sitecore.Layouts;
using Sitecore.XA.Foundation.VersionSpecific.Pipelines.ExecutePageEditorAction;
using System.Collections.Generic;

namespace Community.Foundation.Prefabs.Pipelines.Shoehorn
{
    public class ExposedInsertDynamicRendering : InsertDynamicRendering
    {
        public new List<string> RenderingsWithDynamicPlaceholders { get { return InsertDynamicRendering.RenderingsWithDynamicPlaceholders; } }

        public new bool AreCompatibleRenderings(RenderingDefinition newRendering, RenderingDefinition existingRendering) {
            return base.AreCompatibleRenderings(newRendering, existingRendering);
        }
    }
}
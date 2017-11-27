using Community.Foundation.Prefabs.Configuration;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Diagnostics;
using Sitecore.Layouts;

namespace Community.Foundation.Prefabs.Pipelines.PrefabGetRenderings
{
    public class GetFromDefaultPlaceholder : PrefabGetRenderingsProcessor
    {
        /// <summary>
        /// Assume convention that rendering name matches prefab name
        /// Lookup item from defined global repository
        /// Pull from single placeholder, set by rendering defn item or by config default
        /// </summary>
        /// <param name="args"></param>
        public override void Process(PrefabGetRenderingsArgs args)
        {
            if (args.Handled)
                return;

            Assert.IsNotNull(args.Prefab, "args.Prefab");

            var prefab = args.Prefab;

            // Warn if xml exists in FinalLayoutField
            if (!string.IsNullOrWhiteSpace(prefab[FieldIDs.FinalLayoutField])) {
                Log.Warn($"Prefab item, {prefab.Name} - {prefab.ID}, has FinalLayout details that will not be used. Move to Shared Layout to apply.", this);
            }

            var prefabSharedLayout = LayoutDefinition.Parse(LayoutField.GetFieldValue(prefab.Fields[FieldIDs.LayoutField]));
            DeviceDefinition prefabDevice = prefabSharedLayout.GetDevice(args.DeviceLayout.ID); // match device

            // Get placeholder name
            var prefabPlaceholder = args.PrefabProxy.Placeholder;
            if(string.IsNullOrWhiteSpace(prefabPlaceholder))
            { 
                prefabPlaceholder = Helper.NormalizePath(Config.Prefab.Placeholder);
            }

            // Get all renderings under this placeholder
            foreach (RenderingDefinition rendering in prefabDevice.Renderings)
            {
                var match = Helper.NormalizePath( rendering.Placeholder );
                if (match.StartsWith(prefabPlaceholder))
                {
                    args.AddResult(rendering, prefabPlaceholder);
                }
            }

            if (args.Handled)
                args.AbortPipeline();
        }

    }
}
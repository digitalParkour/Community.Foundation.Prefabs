using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Layouts;
using Sitecore.Pipelines;
using System.Collections.Generic;

namespace Community.Foundation.Prefabs.Pipelines.PrefabGetRenderings
{
    public class PrefabGetRenderingsArgs : RequestPipelineArgs
    {
        public Item Prefab { get; set; }
        public RenderingItem PrefabProxy { get; set; }
        public DeviceDefinition DeviceLayout { get; set; }
        public string DestinationPlaceholderPath { get; set; }
        public Item PageItem { get; set; }
        public int Index { get; set; }

        public PrefabGetRenderingsArgs()
        {
            _results = new List<RenderingDefinition>();
        }
        private List<RenderingDefinition> _results { get; set; }

        /// <summary>
        /// Add Renderings qualified for this Prefab proxy
        /// </summary>
        /// <param name="renderingDefinition">Rendering to add</param>
        /// <param name="prefabRootPlaceholder">This is the placeholder path of the prefab item to be removed/replace for the target location on the page</param>
        public void AddResult(RenderingDefinition renderingDefinition, string prefabRootPlaceholder)
        {
            this.Handled = true;
            
            // Translate placeholder from prefab to page target
            var ph = Helper.NormalizePath(renderingDefinition.Placeholder);
            var prefabPlaceholder = Helper.NormalizePath(prefabRootPlaceholder);
            var destinationPlaceholder = Helper.NormalizePath(this.DestinationPlaceholderPath);
            renderingDefinition.Placeholder = ph.Equals(prefabPlaceholder) ? this.DestinationPlaceholderPath : ph.Replace(prefabPlaceholder, destinationPlaceholder).TrimEnd('/');

            // Renew UniqueId to allow Prefab to be applied more than once to a page
            renderingDefinition.UniqueId = ID.NewID.ToString();

            this._results.Add(renderingDefinition);
        }

        public List<RenderingDefinition> Result => _results;

        public void Reuse()
        {
            this.Resume();
        }
    }
}
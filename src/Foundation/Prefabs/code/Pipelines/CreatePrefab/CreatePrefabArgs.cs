using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Layouts;
using Sitecore.Pipelines;
using System.Collections.Generic;

namespace Community.Foundation.Prefabs.Pipelines.CreatePrefab
{
    public class CreatePrefabArgs : ResultPipelineArgs<Item>
    {
        #region Input
        public string Name { get; set; }
        public DeviceDefinition DeviceLayout { get; set; }
        public int Index { get; set; }
        public Item ContextItem { get; set; }
        #endregion

        #region Result
        // Result Item expected to be prefab item
        public Item ResultProxy { get; set; }
        #endregion

        #region Required to create prefab
        public List<RenderingDefinition> Renderings { get; set; }
        public string OldPlaceholderPath { get; set; }
        public string DeviceId { get; set; }
        public string LayoutId { get; set; }
        public string Placeholder { get; set; }
        public Item PrefabLocation { get; set; }
        #endregion

        #region Required to create proxy
        public Item RenderingLocation { get; set; }
        #endregion

    }
}
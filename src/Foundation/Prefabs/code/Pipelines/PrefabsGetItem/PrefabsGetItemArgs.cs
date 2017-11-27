using Sitecore.Data.Items;
using Sitecore.Layouts;
using Sitecore.Pipelines;

namespace Community.Foundation.Prefabs.Pipelines
{
    public class PrefabsGetItemArgs : ResultPipelineArgs<Item>
    {
        public RenderingItem PrefabProxy { get; set; }
        public DeviceDefinition DeviceLayout { get; set; }
        public string DestinationPlaceholderPath { get; set; }
        public Item PageItem { get; set; }
    }
}
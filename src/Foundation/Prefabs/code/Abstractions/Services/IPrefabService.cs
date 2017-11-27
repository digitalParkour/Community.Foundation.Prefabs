using Sitecore.Data.Items;
using Sitecore.Layouts;

namespace Community.Foundation.Prefabs.Abstractions.Services
{
    public interface IPrefabService
    {
        /// <summary>
        /// Validate if given rendering definition item qualifies as a Prefab proxy, to use as a lookup for a prefab definition item
        /// </summary>
        /// <param name="renderingDefinition"></param>
        /// <returns></returns>
        bool IsPrefab(Item renderingDefinition);

        Item CreatePrefab(string name, DeviceDefinition deviceLayout, int index, Item pageItem);

        /// <summary>
        /// Inserts Prefab into device rendering definitions
        /// This is the main manager for the insert operation
        /// Exposes extension points with three pipelines:
        ///     PrefabsGetItem - processors to get appropriate prefab definition item (site specific / global / branch template)
        ///     PrefabGetRenderings - given prefab definition item, get list of renderings to insert
        ///     PrefabApplyRenderings - insert renderings, and add child items
        /// </summary>
        /// <param name="prefabProxy">Sitecore Rendering Definition Item used as a lookup for the Prefab Definition item</param>
        /// <param name="deviceLayout">DeviceDefinition object which is parsed xml for rendering definitions - the place to add prefab</param>
        /// <param name="placeholderPath">target placeholder path to use</param>
        /// <param name="index">target index when inserting befor/after sibblings in same placeholder path</param>
        /// <param name="pageItem">context page item that is being affected (used if need to copy local page data)</param>
        void InjectPrefab(RenderingItem prefabProxy, DeviceDefinition deviceLayout, string placeholderPath, int index, Item pageItem);
        
    }
}

using Community.Foundation.Prefabs.Abstractions.Services;
using Sitecore.Data.Items;
using Sitecore.Pipelines;
using Sitecore.Layouts;
using Community.Foundation.Prefabs.Pipelines;
using Sitecore.Diagnostics;
using Community.Foundation.Prefabs.Pipelines.PrefabGetRenderings;
using Community.Foundation.Prefabs.Pipelines.CreatePrefab;

namespace Community.Foundation.Prefabs.Services
{
    /// <summary>
    /// This manages all the work to both create a prefab and inject the prefab
    /// </summary>
    public class PrefabService : IPrefabService
    {
        /// <summary>
        /// Validate if given rendering definition item qualifies as a Prefab proxy, to use as a lookup for a prefab definition item
        /// Here we are saying all Method Renderings with Robot icon qualify
        /// </summary>
        /// <param name="renderingDefinition"></param>
        /// <returns></returns>
        public bool IsPrefab(Item renderingDefinition)
        {
            return renderingDefinition != null 
                && renderingDefinition.TemplateName.Equals("Method Rendering") 
                && renderingDefinition.Appearance.Icon.EndsWith("pci_card.png");
        }

        public Item CreatePrefab(string name, DeviceDefinition deviceLayout, int index, Item pageItem)
        {
            var createPrefabArgs = new CreatePrefabArgs
            {
                Name = name,
                DeviceLayout = deviceLayout,
                Index = index,
                ContextItem = pageItem
            };
            CorePipeline.Run("createPrefab", createPrefabArgs);
            if (!createPrefabArgs.Handled || createPrefabArgs.Result == null)
            {
                Log.Warn($"{nameof(PrefabService)}::{nameof(CreatePrefab)} - Pipeline CreatePrefab result was not handled", this);
                return null;
            }

            return createPrefabArgs.Result;
        }

        public void InjectPrefab(RenderingItem prefabProxy, DeviceDefinition deviceLayout, string placeholderPath, int index, Item pageItem)
        {
            var getItemArgs = new PrefabsGetItemArgs {
                DeviceLayout = deviceLayout,
                DestinationPlaceholderPath = placeholderPath,
                PrefabProxy = prefabProxy,
                PageItem = pageItem
            };
            CorePipeline.Run("prefabsGetItem", getItemArgs);
            if (!getItemArgs.Handled || getItemArgs.Result == null)
            {
                throw new System.Exception($"Prefab not found");
            }

            var getRenderingsArgs = new PrefabGetRenderingsArgs
            {
                Prefab = getItemArgs.Result,
                DeviceLayout = deviceLayout,
                DestinationPlaceholderPath = placeholderPath,
                PrefabProxy = prefabProxy,
                PageItem = pageItem,
                Index = index
            };
            CorePipeline.Run("prefabGetRenderings", getRenderingsArgs);
            if (!getRenderingsArgs.Handled)
            {
                throw new System.Exception($"Prefab had no renderings to add");
            }

            getRenderingsArgs.Reuse();
            CorePipeline.Run("prefabApplyRenderings", getRenderingsArgs);
        }
    }
}
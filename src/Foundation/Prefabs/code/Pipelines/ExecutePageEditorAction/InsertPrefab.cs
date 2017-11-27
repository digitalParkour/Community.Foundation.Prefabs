using Community.Foundation.Prefabs.Abstractions.Services;
using Sitecore;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.Pipelines;
using Sitecore.Pipelines.ExecutePageEditorAction;
using System;

namespace Community.Foundation.Prefabs.Pipelines.ExecutePageEditorAction
{
    public class InsertPrefab
    {
        private IPrefabService _prefabService;
        public InsertPrefab(IPrefabService prefabService)
        {
            _prefabService = prefabService; // Sitecore.DependencyInjection.ServiceLocator.ServiceProvider.GetService<IPrefabService>();
        }

        public void Process(PipelineArgs args)
        {
            ExecuteInsertRenderingArgs insertRenderingArg = args as ExecuteInsertRenderingArgs; // IInsertRenderingArgs;
            if (insertRenderingArg == null)
            {
                return;
            }

            var renderingItem = insertRenderingArg.RenderingItem;
            if (!_prefabService.IsPrefab(renderingItem))
            {
                return;
            }

            var index = GetIndex(insertRenderingArg.Device, insertRenderingArg.Result.UniqueId);
            if (index != -1)
            {
                // SXA adds prefab proxy in previous processor... remove it
                insertRenderingArg.Device.Renderings.RemoveAt(index);
            }
            else {
                index = ConvertPosition(insertRenderingArg.Device, insertRenderingArg.PlaceholderKey, insertRenderingArg.Position);
            }
            
            // manipulate layoutDefinition via device reference
            _prefabService.InjectPrefab(
                renderingItem,
                insertRenderingArg.Device,
                insertRenderingArg.PlaceholderKey,
                index,
                insertRenderingArg.ContextItem
            );

            // Switch context from prefab proxy to first rendering of prefab
            var resultIndex = index >= 0 ? index : insertRenderingArg.Device.Renderings.Count - 1;
            insertRenderingArg.Result = new RenderingReference( 
                                            (RenderingDefinition)insertRenderingArg.Device.Renderings[resultIndex],
                                            insertRenderingArg.Language,
                                            insertRenderingArg.ContentDatabase
                                        );

            insertRenderingArg.PerformInlineRendering = true;
            insertRenderingArg.AbortPipeline();

        }


        protected virtual int GetIndex(DeviceDefinition device, string uniqueId)
        {
            for (var i = 0; i < device.Renderings.Count; i++)
            {
                RenderingDefinition rendering = device.Renderings[i] as RenderingDefinition;
                Assert.IsNotNull(rendering, "rendering");

                if (rendering.UniqueId == uniqueId)
                    return i;
            }

            return -1;
        }
        protected virtual int ConvertPosition(DeviceDefinition device, string targetPlaceholder, int insertPosition)
        {
            Assert.ArgumentNotNull(device, "device");
            if (insertPosition == 0 || device.Renderings == null)
            {
                return 0;
            }
            
            int num = 0;
            for (int i = 0; i < device.Renderings.Count; i++)
            {
                RenderingDefinition item = device.Renderings[i] as RenderingDefinition;
                Assert.IsNotNull(item, "rendering");
                string placeholder = item.Placeholder;
                if (AreEqualPlaceholders(placeholder, targetPlaceholder))
                {
                    num++;
                }
                if (num == insertPosition)
                {
                    return i+1;
                }
            }

            return device.Renderings.Count;
        }
        private static bool AreEqualPlaceholders(string lhsPlaceholderKey, string rhsPlaceholderKey)
        {
            if (lhsPlaceholderKey == null || rhsPlaceholderKey == null)
            {
                return string.Equals(lhsPlaceholderKey, rhsPlaceholderKey, StringComparison.InvariantCulture);
            }
            int num = lhsPlaceholderKey.LastIndexOf('/');
            int num1 = rhsPlaceholderKey.LastIndexOf('/');
            if (num >= 0 && num1 >= 0)
            {
                return lhsPlaceholderKey.Equals(rhsPlaceholderKey, StringComparison.InvariantCultureIgnoreCase);
            }
            string str = (num >= 0 ? StringUtil.Mid(lhsPlaceholderKey, num + 1) : lhsPlaceholderKey);
            return str.Equals((num1 >= 0 ? StringUtil.Mid(rhsPlaceholderKey, num1 + 1) : rhsPlaceholderKey), StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
using Community.Foundation.Prefabs.Constants;
using Sitecore;
using Sitecore.Data.Events;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.SecurityModel;
using System.Linq;

namespace Community.Foundation.Prefabs.Pipelines.CreatePrefab
{
    public class AddProxyItem : CreatePrefabProcessor
    {
        /// <summary>
        /// Assume convention that rendering name matches prefab name
        /// Lookup item from defined global repository
        /// </summary>
        /// <param name="args"></param>
        public override void Process(CreatePrefabArgs args)
        {
            Assert.IsNotNull(args, "args");
            Assert.IsNotNull(args.Name, "args.Name");
            Assert.IsNotNull(args.Result, "args.Result");
            Assert.IsNotNull(args.RenderingLocation, "args.RenderingLocation");

            using (new SecurityDisabler())
            {
                using (new EventDisabler()) // Must be silent as to not break content editor context item
                {
                    // See if we already have a proxy (improper delete)
                    var proxy = args.RenderingLocation.HasChildren ? args.RenderingLocation.Children.FirstOrDefault(x => x.Name.Equals(args.Result.Name)) : null;
                    if (proxy != null)
                    {
                        args.ResultProxy = proxy;
                        return;
                    }

                    // Create new item
                    proxy = args.RenderingLocation.Add(args.Result.Name, Templates.Proxy.BranchId);
                    
                    // For some reason branch won't add correctly in an Event Disabler or because it is a rendering item?... bug!
                    proxy.Editing.BeginEdit();
                    {
                        // Ugly, but force desired branch settings:
                        proxy.Appearance.Icon = "Office/32x32/pci_card.png";
                        proxy["Method"] = "ProxyItem";
                        proxy["Class"] = "Community.Foundation.Prefabs.Rendering.MethodRendering";
                        proxy["Assembly"] = "Community.Foundation.Prefabs";
                        
                        // Also check for display name case
                        if (args.Result.Name != args.Name) {
                                proxy[FieldIDs.DisplayName] = args.Name;
                        }
                    }
                    proxy.Editing.EndEdit(true, true);

                    // Manually clear the cache (because we are in silent mode)
                    proxy.Database.Caches.DataCache.RemoveItemInformation(proxy.ID);
                    proxy.Database.Caches.ItemCache.RemoveItem(proxy.ID);

                    args.ResultProxy = proxy;
                }
            }
        }
    }
}
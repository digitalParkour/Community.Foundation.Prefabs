using Community.Foundation.Prefabs.Configuration;
using Community.Foundation.Prefabs.Constants;
using Sitecore.Collections;
using Sitecore.Data;
using Sitecore.Data.Events;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.SecurityModel;
using Sitecore.Sites;
using Sitecore.Web;
using System.Linq;

namespace Community.Foundation.Prefabs.Pipelines.CreatePrefab
{
    public class SxaRegisterRendering : CreatePrefabProcessor
    {
        /// <summary>
        /// Assume convention that rendering name matches prefab name
        /// Lookup item from defined global repository
        /// </summary>
        /// <param name="args"></param>
        public override void Process(CreatePrefabArgs args)
        {
            if (!Config.Sxa.IsEnabled)
                return;

            Assert.IsNotNull(args, "args");
            Assert.IsNotNull(args.ContextItem, "args.Database");
            Assert.IsNotNull(args.ResultProxy, "args.ResultProxy");

            var db = args.ContextItem.Database;

            using (new SecurityDisabler())
            {
                using (new EventDisabler()) // Must be silent as to not break content editor context item
                {
                    foreach (var site in SiteContextFactory.Sites)
                    {
                        // skip system sites
                        if (Helper.SystemSites.Contains(site.Name))
                            continue;

                        if (site.RootPath.Length <= 1) // skip trivial checks
                            continue;

                        var availablePrefabsItem = GetOrAddAvailablePrefabsNode(site, db);
                        if (availablePrefabsItem == null)
                            continue;

                        availablePrefabsItem.Editing.BeginEdit();
                        {
                            var newId = args.ResultProxy.ID.ToString();
                            var value = availablePrefabsItem[Templates.Sxa.AvailableRenderings.Fields.Renderings];

                            availablePrefabsItem[Templates.Sxa.AvailableRenderings.Fields.Renderings] = string.IsNullOrWhiteSpace(value)
                                ? newId
                                : $"{value}|{newId}";
                        }
                        availablePrefabsItem.Editing.EndEdit(true, true);
                        
                        // Manually clear the cache (because we are in silent mode)
                        availablePrefabsItem.Database.Caches.DataCache.RemoveItemInformation(availablePrefabsItem.ID);
                        availablePrefabsItem.Database.Caches.ItemCache.RemoveItem(availablePrefabsItem.ID);

                    }
                    // foreach site
                    // GetOrAdd prefabs allowed renderings
                    // Add args.ResultProxy.ID to field
                }
            }
        }

        public Item GetOrAddAvailablePrefabsNode(SiteInfo site, Database db) {
            // Get Item
            var parentPath = $"{site.RootPath.TrimEnd('/')}/Presentation/Available Renderings";
            var parentItem = db.GetItem(parentPath);
            if (parentItem == null)
                return null; // Assume not SXA site

            var children = parentItem.HasChildren ? parentItem.GetChildren(ChildListOptions.SkipSorting) : null;
            var node = children?.FirstOrDefault(x => x.Name.Equals("Prefabs"));
            if (node != null)
                return node;

            if (!Config.Sxa.AutoAllow)
                return null;

            node = parentItem.Add("Prefabs", new TemplateID(Templates.Sxa.AvailableRenderings.Id));
            return node;
        }
    }
}
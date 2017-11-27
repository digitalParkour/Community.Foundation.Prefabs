using Community.Foundation.Prefabs.Pipelines.PrefabGetRenderings;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Events;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;
using System.Collections.Generic;
using System.Linq;

namespace Community.Foundation.Prefabs.Pipelines.PrefabApplyRenderings
{
    public class CopyPageData : PrefabApplyRenderingsProcessor
    {
        public override void Process(PrefabGetRenderingsArgs args)
        {
            if (!args.Handled)
                return;

            if (!args.Prefab.HasChildren)
                return;

            using (new SecurityDisabler()) // Avoid permission issues
            {
                using (new EventDisabler()) // Must run with EventDisabler... otherwise content editor's event listeners switch context item when any new item is created... which would break our dialog
                {

                    foreach (var r in args.Result)
                    {
                        if (string.IsNullOrWhiteSpace(r.Datasource))
                            continue;

                        ID dataId;
                        if (!ID.TryParse(r.Datasource, out dataId))
                            continue;

                        var datasourceItem = args.Prefab.Database.GetItem(dataId);
                        if (datasourceItem == null)
                            continue;

                        // Check if this item lives under prefab, ie. local page data
                        if (!args.Prefab.Axes.IsAncestorOf(datasourceItem))
                            continue;

                        var destinationDataFolder = GetOrAddDatafolder(datasourceItem.Parent, args.Prefab, args.PageItem);
                        if (destinationDataFolder == null)
                            continue;
                        var destItem = CopyDatasource(datasourceItem, destinationDataFolder);
                        if (destItem == null)
                            continue;

                        r.Datasource = destItem.ID.ToString();
                    }
                }
            }
        }


        /// <summary>
        /// Method exposed as extension point
        /// </summary>
        public virtual Item CopyDatasource(Item datasourceItem, Item destinationDatafolder)
        {
            var name = $"{datasourceItem.Name}";
            var prefixLength = 3;

            // Protect against long names
            var max = Settings.MaxItemNameLength;
            if (name.Length + prefixLength > max)
                name = name.Substring(0, max-prefixLength).Trim();
            
            // Get new name
            var num = 1;
            var newName = $"{num:00} {name}";
            var db = destinationDatafolder.Database;
            var path = $"{destinationDatafolder.Paths.FullPath}/";
            while (db.Items[$"{path}{newName}"] != null) {
                num++;
                newName = $"{num:00} {name}";
            }

            // Copy away!
            return datasourceItem.CopyTo(destinationDatafolder, newName);
        }


        public virtual Item GetOrAddDatafolder(Item datasourceParent, Item datasourcePage, Item destinationPage )
        {
            // list all items (data folders) between datasource item and page item
            var scaffolding = new List<Item>();
            while (datasourceParent != null && datasourceParent.ID != datasourcePage.ID)
            {
                scaffolding.Add(datasourceParent);
                datasourceParent = datasourceParent.Parent;
            }
            if (!scaffolding.Any())
                return destinationPage;

            // flip it top to bottom
            scaffolding.Reverse();

            // Ensure destination has identical scaffolding
            var destPointer = destinationPage;
            foreach (var source in scaffolding) {
                var existing = destPointer.Children?.FirstOrDefault(x => x.TemplateID == source.TemplateID && x.Name == source.Name);
                destPointer = existing ?? destPointer.Add(source.Name, new TemplateID(source.TemplateID));
            }

            return destPointer;
        }
        
    }
}
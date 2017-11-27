using Sitecore.Data;
using Sitecore.Data.Events;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;
using System.Collections.Generic;
using System.Linq;

namespace Community.Foundation.Prefabs.Pipelines.CreatePrefab
{
    public class CopyPageData : CreatePrefabProcessor
    {
        public override void Process(CreatePrefabArgs args)
        {
            if (!args.Handled)
                return;
            
            using (new SecurityDisabler()) // Avoid permission issues
            {
                using (new EventDisabler()) // Must run with EventDisabler... otherwise content editor's event listeners switch context item when any new item is created... which would break our dialog
                {

                    foreach (var r in args.Renderings)
                    {
                        if (string.IsNullOrWhiteSpace(r.Datasource))
                            continue;

                        ID dataId;
                        if (!ID.TryParse(r.Datasource, out dataId))
                            continue;

                        var datasourceItem = args.ContextItem.Database.GetItem(dataId);
                        if (datasourceItem == null)
                            continue;

                        // Check if this item lives under prefab, ie. local page data
                        if (!args.ContextItem.Axes.IsAncestorOf(datasourceItem))
                            continue;

                        var destinationDataFolder = GetOrAddDatafolder(datasourceItem.Parent, args.ContextItem, args.Result);
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
        /// <param name="datasourceItem"></param>
        /// <param name="destinationDatafolder"></param>
        /// <returns></returns>
        public virtual Item CopyDatasource(Item datasourceItem, Item destinationDatafolder)
        {
            // Copy away!
            return datasourceItem.CopyTo(destinationDatafolder, datasourceItem.Name);
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
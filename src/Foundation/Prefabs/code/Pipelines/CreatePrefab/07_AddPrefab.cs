using Community.Foundation.Prefabs.Constants;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Events;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.SecurityModel;

namespace Community.Foundation.Prefabs.Pipelines.CreatePrefab
{
    public class AddPrefab : CreatePrefabProcessor
    {
        /// <summary>
        /// Assume convention that rendering name matches prefab name
        /// Lookup item from defined global repository
        /// </summary>
        /// <param name="args"></param>
        public override void Process(CreatePrefabArgs args)
        {
            Assert.IsNotNull(args, "args");
            Assert.IsNotNullOrEmpty(args.Name, "args.Name");
            Assert.IsNotNull(args.PrefabLocation, "args.PrefabLocation");

            using (new SecurityDisabler())
            {
                using (new EventDisabler()) // Must be silent as to not break content editor context item
                {
                    // Get Safe Name
                    var name = ItemUtil.ProposeValidItemName(args.Name, Helper.GetSafeName(args.Name));

                    // Ensure Unique
                    var num = 1;
                    var origName = name;
                    var db = args.PrefabLocation.Database;
                    var path = $"{args.PrefabLocation.Paths.FullPath}/";
                    while (db.Items[$"{path}{name}"] != null)
                    {
                        if (num == 1 && origName.Length + 3 > Settings.MaxItemNameLength)
                            origName = origName.Substring(0, Settings.MaxItemNameLength - 3);

                        num++;
                        name = $"{origName} {num:00}";
                    }
                    if (num > 1)
                    {
                        args.Name = $"{args.Name} {num:00}"; // carry to display name too
                    }

                    // Add Item
                    var prefab = args.PrefabLocation.Add(name, new TemplateID(Templates.Prefab.Id));
                    
                    args.Result = prefab;
                }
            }
        }
    }
}
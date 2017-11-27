using Sitecore;
using Sitecore.Data.Events;
using Sitecore.Data.Fields;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.SecurityModel;
using System;

namespace Community.Foundation.Prefabs.Pipelines.CreatePrefab
{
    public class FillPrefab : CreatePrefabProcessor
    {
        /// <summary>
        /// Assume convention that rendering name matches prefab name
        /// Lookup item from defined global repository
        /// </summary>
        /// <param name="args"></param>
        public override void Process(CreatePrefabArgs args)
        {
            Assert.IsNotNull(args, "args");
            Assert.IsNotNull(args.Result, "args.Result");
            Assert.IsNotNullOrEmpty(args.Name, "args.Name");
            Assert.IsNotNull(args.DeviceId, "args.DeviceId");
            Assert.IsNotNull(args.LayoutId, "args.LayoutId");

            var prefab = args.Result;

            using (new SecurityDisabler())
            {
                using (new EventDisabler())
                {
                    // Fill Prefab
                    prefab.Editing.BeginEdit();
                    try
                    {
                        // Display Name (if input wasn't valid name)
                        if (prefab.Name != args.Name)
                        {
                            prefab[FieldIDs.DisplayName] = args.Name;
                        }

                        var prefabSharedLayoutField = prefab.Fields[FieldIDs.LayoutField];
                        var prefabSharedLayout = LayoutDefinition.Parse(LayoutField.GetFieldValue(prefabSharedLayoutField));
                        DeviceDefinition prefabDevice = prefabSharedLayout.GetDevice(args.DeviceId); // match device

                        // Set Layout
                        prefabDevice.Layout = args.LayoutId;

                        // Add Renderings
                        foreach (var r in args.Renderings)
                        {
                            prefabDevice.AddRendering(r);
                        }

                        LayoutField.SetFieldValue(prefabSharedLayoutField, prefabSharedLayout.ToXml());

                        prefab.Editing.EndEdit(true, true); // Must be silent as to not break content editor context item

                        // Manually clear the cache (because we are in silent mode)
                        prefab.Database.Caches.DataCache.RemoveItemInformation(prefab.ID); 
                        prefab.Database.Caches.ItemCache.RemoveItem(prefab.ID);
                    }
                    catch (Exception ex)
                    {
                        prefab.Editing.CancelEdit();
                        Log.Error($"{nameof(FillPrefab)}::{nameof(Process)} - Unable to fill prefab, {args.Result.ID}, {args.Name}", ex, this);
                    }
                }
            }
        }
    }
}
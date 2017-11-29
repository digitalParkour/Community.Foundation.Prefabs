using Community.Foundation.Prefabs.Abstractions.Services;
using Community.Foundation.Prefabs.Configuration;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Community.Foundation.Prefabs.Services
{
    public class SmartCopyService : ISmartCopyService
    {
        protected const string GuidPattern = "[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";
        protected const string SxaLocalPattern = "\"local:[^\"]*\"";

        #region MapIdFields

        public void MapIdFields(Item referenceItem, Item referenceCopy)
        {
            var db = referenceItem.Database;

            var commitEdit = false;
            referenceCopy.Editing.BeginEdit();
            try
            {
                // Get all fields...
                foreach (Field field in referenceItem.Fields)
                {
                    //  that have ID in value
                    var rawValue = field.Value;
                    if (string.IsNullOrWhiteSpace(rawValue) || !Regex.IsMatch(rawValue, GuidPattern))
                        continue;

                    // For each ID
                    var IDs = Regex.Matches(rawValue, GuidPattern);
                    foreach (Match match in IDs)
                    {
                        var id = new ID(match.Value);

                        // Verify ID is descendant of referenceItem
                        var linkedItem = db.GetItem(id);
                        if (linkedItem == null || !linkedItem.Axes.IsDescendantOf(referenceItem))
                            continue;

                        // Map ID
                        var mappedId = MapId(linkedItem, referenceItem, referenceCopy);
                        if (mappedId == (ID)null)
                            continue;

                        // Replace linking ID with mapped ID
                        commitEdit = true;
                        referenceCopy[field.ID] = referenceCopy[field.ID].Replace(linkedItem.ID.ToString().Trim(new[] { '{', '}' }), mappedId.ToString().Trim(new[] { '{', '}' }));

                    }

                    // Handle nested SXA Composite item layout field... "local:" datasources
                    if(Config.Sxa.IsEnabled && (field.ID.Equals(FieldIDs.LayoutField) || field.ID.Equals(FieldIDs.FinalLayoutField)))
                    { 
                        var locals = Regex.Matches(rawValue, SxaLocalPattern);
                        foreach (Match match in locals)
                        {
                            if (match.Value.Length <= "'local:'".Length)
                                continue;

                            var localPath = StringUtil.EnsurePrefix('/', match.Value.Trim('"').Substring("local:".Length)).TrimEnd('/');
                            var linkedPath = $"{referenceItem.Paths.Path}{localPath}";
                            var linkedItem = db.GetItem(linkedPath);
                            if (linkedItem == null)
                                continue;

                            // Map ID
                            var mappedPath = $"{referenceCopy.Paths.Path}{localPath}";
                            var mappedItem = db.GetItem(mappedPath);
                            if (mappedItem == null)
                                continue;
                            var mappedId = mappedItem.ID;

                            // Replace linking ID with mapped ID
                            commitEdit = true;
                            var replacement = $"\"{mappedId.ToString()}\"";
                            referenceCopy[field.ID] = referenceCopy[field.ID].Replace(match.Value, replacement);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(SmartCopyService)}::{nameof(MapIdFields)}", ex, this);
                commitEdit = false;
            }
            finally
            {
                if (commitEdit)
                { 
                    referenceCopy.Editing.EndEdit(true, true);

                    // Manually clear the cache (because we are in silent mode)
                    referenceCopy.Database.Caches.DataCache.RemoveItemInformation(referenceCopy.ID);
                    referenceCopy.Database.Caches.ItemCache.RemoveItem(referenceCopy.ID);
                }
                else
                    referenceCopy.Editing.CancelEdit();
            }
        }

        protected virtual ID MapId(Item linkedItem, Item referenceItem, Item referenceCopy)
        {
            // Get relative path
            var relativePath = linkedItem.Paths.Path.Substring(referenceItem.Paths.Path.Length);

            // For each relative segment dig via position to match destination item
            var segments = relativePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (!segments.Any())
            {
                Log.Error($"{nameof(SmartCopyService)}::{nameof(MapId)} - Something weird has happened, data item {linkedItem} matches reference item {referenceItem}", this);
                return null;
            }

            var refItem = referenceItem;
            var mappedItem = referenceCopy;
            foreach (var segment in segments)
            {
                var refPos = IndexOfChild(refItem, segment);
                var mappedChildren = mappedItem.HasChildren ? mappedItem.Children : null;
                if (refPos == -1 || (mappedChildren?.Count ?? 0) <= refPos)
                {
                    Log.Error($"{nameof(SmartCopyService)}::{nameof(MapId)} - Something unexpected has happened, copied structure {mappedItem.ID} does not match source {refItem.ID}, child {refPos}", this);
                    return null;
                }
                refItem = refItem.Children[refPos];
                mappedItem = mappedChildren[refPos];
            }

            return mappedItem.ID;
        }

        protected int IndexOfChild(Item parent, string childName)
        {
            var kids = parent.Children;
            var k = 0;
            foreach (Item kid in kids)
            {
                if (kid.Name.Equals(childName))
                    return k;
                k++;
            }
            return -1;
        }

        #endregion

        #region MapIdFieldsForDescendants

        /// <summary>
        /// Optional haultId to abort recursion early... defaults to itemCopy ID in case copied under sourceItem
        /// </summary>
        /// <param name="sourceItem"></param>
        /// <param name="itemCopy"></param>
        /// <param name="haultID"></param>
        public void MapIdFieldsForDescendants(Item sourceItem, Item itemCopy, ID haultID = (ID)null )
        {
            if (haultID == (ID)null)
                haultID = itemCopy.ID;

            ProcessChildren_R(sourceItem, itemCopy, haultID);
        }
        
        /// <summary>
        /// Crawl descendants and map ID fields
        /// Hault case... if source item was copied under itself, don't crawl copy!
        /// </summary>
        /// <param name="referenceItem"></param>
        /// <param name="destinationItem"></param>
        /// <param name="haultCase"></param>
        public virtual void ProcessChildren_R(Item referenceItem, Item referenceCopy, ID haultCase)
        {
            if (referenceItem == null
                 || !referenceItem.HasChildren
                 || !referenceCopy.HasChildren
                 || haultCase.Equals(referenceItem.ID)
               )
                return;

            var items = referenceItem.Children;
            var copies = referenceCopy.Children;

            // Crawl children in parallel
            var limit = Math.Min(items.Count, copies.Count); // should match, but just in case
            for (var i = 0; i < limit; i++)
            {
                var reff = items[i];
                var copy = copies[i];

                MapIdFields(reff, copy);
                ProcessChildren_R(reff, copy, haultCase);
            }
        }

        #endregion
        
        #region MapSiteScopedIds

        public void MapSiteScopedIds(List<RenderingDefinition> renderings, Item contextItem)
        {
            foreach (var r in renderings)
            {
                if (string.IsNullOrWhiteSpace(r.Parameters))
                    continue;

                if (!Regex.IsMatch(r.Parameters, GuidPattern))
                    continue;

                r.Parameters = MapSiteParameterValues(r.Parameters, contextItem);
            }
        }

        protected string MapSiteParameterValues(string rawValue, Item contextItem)
        {
            var db = contextItem.Database;

            if (contextItem == null)
                return rawValue;
            var contextPath = Helper.NormalizePath(contextItem.Paths.Path, false);

            var contextSite = Helper.GetSiteInfo(contextItem.Paths.FullPath);
            if (contextSite == null)
                return rawValue;
            var contextSitePath = Helper.NormalizePath(contextSite.RootPath, false);

            var IDs = Regex.Matches(rawValue, GuidPattern);
            foreach (Match match in IDs)
            {
                var id = new ID(match.Value);

                var linkedItem = db.GetItem(id);
                if (linkedItem == null)
                    continue;

                // CASE: linked item is already correct
                var linkedItemPath = linkedItem.Paths.Path;
                if (linkedItemPath.StartsWith(contextSitePath))
                    continue;

                var linkedItemSite = Helper.GetSiteInfo(linkedItem.Paths.FullPath);
                if (linkedItemSite == null)
                    continue;

                var linkedItemSitePath = Helper.NormalizePath(linkedItemSite.RootPath, false);

                var mappedId = MapId(linkedItem, linkedItemSitePath, contextSitePath);
                if (mappedId != (ID)null)
                {
                    rawValue = rawValue.Replace(linkedItem.ID.ToString().Trim(new[] { '{', '}' }), mappedId.ToString().Trim(new[] { '{', '}' }));
                }
            }
            return rawValue;
        }

        protected virtual ID MapId(Item linkedItem, string sourcePath, string destinationPath)
        {
            var db = linkedItem.Database;

            var linkedItemPath = Helper.NormalizePath(linkedItem.Paths.Path, false);
            var desiredItemPath = linkedItemPath.Replace(sourcePath, destinationPath);

            var desiredItem = db.GetItem(desiredItemPath);
            return desiredItem?.ID;
        }
        #endregion

        #region MapSiteScopedIdFields

        public void MapSiteScopedIdFields(Item contextItem) {

            var db = contextItem.Database;

            var commitEdit = false;
            contextItem.Editing.BeginEdit();
            try
            {
                // Get all fields...
                foreach (Field field in contextItem.Fields)
                {
                    //  that have ID in value
                    var rawValue = field.Value;
                    if (string.IsNullOrWhiteSpace(rawValue) || !Regex.IsMatch(rawValue, GuidPattern))
                        continue;

                    var mappedValue = MapSiteParameterValues(rawValue, contextItem);
                    if (rawValue.Equals(mappedValue))
                        continue;

                        // Replace linking ID with mapped ID
                    commitEdit = true;
                    field.Value = mappedValue;
                    
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(SmartCopyService)}::{nameof(MapSiteScopedIdFields)}", ex, this);
                commitEdit = false;
            }
            finally
            {
                if (commitEdit)
                { 
                    contextItem.Editing.EndEdit(true, true);

                    // Manually clear the cache (because we are in silent mode)
                    contextItem.Database.Caches.DataCache.RemoveItemInformation(contextItem.ID);
                    contextItem.Database.Caches.ItemCache.RemoveItem(contextItem.ID);
                }
                else
                    contextItem.Editing.CancelEdit();
            }
        }

        #endregion

        #region MapSiteScopedIdFieldsForDescendants

        public void MapSiteScopedIdFieldsForDescendants(Item contextItem)
        {
            if (contextItem == null || !contextItem.HasChildren)
                return;

            var children = contextItem.Children;
            
            foreach (Item child in children)
            {
                MapSiteScopedIdFields(child);
                MapSiteScopedIdFieldsForDescendants(child);
            }
        }
        #endregion
    }
}
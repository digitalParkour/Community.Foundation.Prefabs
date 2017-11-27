using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Layouts;
using System.Collections.Generic;

namespace Community.Foundation.Prefabs.Abstractions.Services
{
    public interface ISmartCopyService
    {
        /// <summary>
        /// For all field values with nested references on sourceItem, update field values on itemCopy to point to its appropriate descendants
        /// </summary>
        /// <param name="sourceItem"></param>
        /// <param name="referenceCopy"></param>
        void MapIdFields(Item sourceItem, Item itemCopy);

        /// <summary>
        /// Recurse over descendants and call MapIdFields
        /// </summary>
        /// <param name="sourceItem"></param>
        /// <param name="itemCopy"></param>
        /// <param name="haultID"></param>
        void MapIdFieldsForDescendants(Item sourceItem, Item itemCopy, ID haultID = (ID)null);
        
        /// <summary>
        /// This is a generic processor that checks if a linking id is outside the page item's site context
        /// If so this attempts to update the link id via the relative path for its site context
        /// With SXA the Styles Parameter uses style item that can be duplicated between sites.
        /// This processor matches the same name between sites and updates the linking ID
        /// SXA 1.5 introduced a way to share style item options between sites
        /// If unable to find the similar path, then ID is left as is, assuming it must then be a global ID
        /// </summary>
        void MapSiteScopedIds(List<RenderingDefinition> renderings, Item contextItem);

        void MapSiteScopedIdFields(Item contextItem);

        void MapSiteScopedIdFieldsForDescendants(Item contextItem);
    }
}

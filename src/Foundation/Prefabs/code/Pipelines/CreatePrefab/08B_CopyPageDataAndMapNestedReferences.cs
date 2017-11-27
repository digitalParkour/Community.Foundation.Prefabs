using Community.Foundation.Prefabs.Abstractions.Services;
using Sitecore.Data.Items;

namespace Community.Foundation.Prefabs.Pipelines.CreatePrefab
{
    public class CopyPageDataAndMapNestedReferences : CopyPageData
    {
        ISmartCopyService _smartCopyService;

        public CopyPageDataAndMapNestedReferences(ISmartCopyService smartCopyService) : base()
        {
            _smartCopyService = smartCopyService;
        }

        public override Item CopyDatasource(Item datasourceItem, Item destinationDatafolder)
        {
            // Copy
            var copy = base.CopyDatasource(datasourceItem, destinationDatafolder);

            // Then also fix nested references
            if (copy != null)
            {
                _smartCopyService.MapIdFieldsForDescendants(datasourceItem, copy, destinationDatafolder.ID);
                
                //if(!Helper.IsSameSite(datasourceItem, copy))
                //{ 
                //    _smartCopyService.MapSiteScopedIdFields(copy);
                //    _smartCopyService.MapSiteScopedIdFieldsForDescendants(copy);
                //}
            }

            return copy;
        }

    }
}
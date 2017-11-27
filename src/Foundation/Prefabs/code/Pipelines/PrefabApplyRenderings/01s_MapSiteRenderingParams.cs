using Community.Foundation.Prefabs.Abstractions.Services;
using Community.Foundation.Prefabs.Pipelines.PrefabGetRenderings;

namespace Community.Foundation.Prefabs.Pipelines.PrefabApplyRenderings
{
    /// <summary>
    /// This is a generic processor that checks if a linking id is outside the page item's site context
    /// If so this attempts to update the link id via the relative path for its site context
    /// With SXA the Styles Parameter uses style item that can be duplicated between sites.
    /// This processor matches the same name between sites and updates the linking ID
    /// SXA 1.5 introduced a way to share style item options between sites
    /// If unable to find the similar path, then ID is left as is, assuming it must then be a global ID
    /// </summary>
    public class MapSiteRenderingParams : PrefabApplyRenderingsProcessor
    {
        protected ISmartCopyService _smartCopyService;

        public MapSiteRenderingParams(ISmartCopyService smartCopyService) : base()
        {
            _smartCopyService = smartCopyService;
        }
        
        public override void Process(PrefabGetRenderingsArgs args)
        {
            if (!args.Handled)
                return;

            if (!Helper.IsSameSite(args.Prefab, args.PageItem))
            {
                _smartCopyService.MapSiteScopedIds(args.Result, args.PageItem);
            }
        }
        
    }
}
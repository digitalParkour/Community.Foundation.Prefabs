using Community.Foundation.Prefabs.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.GetPlaceholderRenderings;
using Sitecore.SecurityModel;
using System.Linq;

namespace Community.Foundation.Prefabs.Pipelines.GetPlaceholderRenderings
{
    public class GetAllPrefabs
    {       
        /// <summary>
        /// Add All Prefabs as options to all placeholders.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public void Process(GetPlaceholderRenderingsArgs args)
        {
            if (Config.Sxa.IsEnabled) // Don't need to do this for SXA, because SXA has it's own method of rendering registration
                return;

            Assert.ArgumentNotNull(args, "args");

            var root = Config.Paths.Renderings.TrimEnd('/');
            var query = $"{root}//*[@@templatename='Method Rendering' and @__Icon='Office/32x32/pci_card.png']";
            // Set root
            using (new SecurityDisabler())
            {
                var prefabs = args.ContentDatabase.SelectItems(query);
                if (prefabs == null || !prefabs.Any())
                    return;

                if(args.PlaceholderRenderings == null)
                    args.PlaceholderRenderings = prefabs.ToList();
                else
                    args.PlaceholderRenderings.AddRange(prefabs);
            }
            
        }
    }
}
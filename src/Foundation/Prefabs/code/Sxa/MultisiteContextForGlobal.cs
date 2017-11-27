using Community.Foundation.Prefabs.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Sites;
using Sitecore.XA.Foundation.Multisite;
using Sitecore.XA.Foundation.SitecoreExtensions.Extensions;
using System.Linq;

namespace Community.Foundation.Prefabs.Sxa
{
    public class MultisiteContextForGlobal : MultisiteContext
    {
        public override Item GetSiteItem(Item item)
        {
            return base.GetSiteItem(item) ?? GetGlobalSiteItem(item?.Database);
        }
        protected virtual Item GetGlobalSiteItem(Database db) {
            
            // See if config override is specified
            var siteName = Config.Sxa.GlobalSiteOverride;
            // Otherwise just grab first SXA site in the list
            var site = string.IsNullOrWhiteSpace(siteName)
                ? SiteContextFactory.Sites.FirstOrDefault(x => x.Properties.AllKeys.Contains("IsSxaSite"))
                    ?? SiteContextFactory.Sites.FirstOrDefault(x => !Helper.SystemSites.Contains(x.Name))
                : SiteContextFactory.GetSiteInfo(siteName);

            if (site == null)
                return null;

            // Get the ID
            return db?.GetItem($"{site.RootPath}{site.StartItem}")?.GetParentOfTemplate(Sitecore.XA.Foundation.Multisite.Templates.Site.ID);
        }

    }
}
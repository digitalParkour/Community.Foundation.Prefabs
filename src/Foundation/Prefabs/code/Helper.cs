using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Sites;
using Sitecore.Web;
using System.Collections.Generic;
using System.Text;

namespace Community.Foundation.Prefabs
{
    public static class Helper
    {
        /// <summary>
        /// Some placeholders look like this: "main"
        /// While others may look like this "/main/content"
        /// input of "main" should match both; transform input and always add slashes
        /// ie: "main" => "/main/"
        /// The trailing slash also avoids false positives
        /// ie: "head" should not match "header"
        ///     "/header/".StartsWith("/head/") == false
        ///      Avoiding
        ///      "header".StartsWith("head") == [false] positive
        /// Also convert case to avoid casing issues
        /// </summary>
        /// <param name="p">Placeholder path</param>
        /// <returns></returns>
        public static string NormalizePath(string p, bool convertCase = true)
        {
            var path = StringUtil.EnsurePrefix('/', StringUtil.EnsurePostfix('/', p));
            return convertCase? path.ToLower() : path;
        }

        public static bool IsSameSite(Item item1, Item item2)
        {
            var site1 = item1.GetSiteInfo()?.Name;
            var site2 = item2.GetSiteInfo()?.Name;
            return site1 == site2;
        }

        public static SiteInfo GetSiteInfo(this Item item) {
            return GetSiteInfo(item.Paths.FullPath);
        }
        public static SiteInfo GetSiteInfo(string itemPath)
        {
            // init itemPath
            if (string.IsNullOrEmpty(itemPath))
                return null;

            itemPath = NormalizePath(itemPath);

            foreach (var site in SiteContextFactory.Sites)
            {
                // skip system sites
                if (SystemSites.Contains(site.Name))
                    continue;

                if (site.RootPath.Length <= 1) // skip trivial checks
                    continue;

                var startpath = NormalizePath(site.RootPath);
                if (itemPath.StartsWith(startpath))
                {
                    return site;
                }
            }
            return null;
        }

        public static readonly List<string> SystemSites = new List<string> { "shell", "login", "admin", "service", "modules_shell", "modules_website", "scheduler", "system", "publisher", "coveo_website", "coveoanalytics", "coveorest", "exm" };


        public static string GetSafeName(string name)
        {
            var seoName = GetSeoName(name);
            var max = Settings.MaxItemNameLength;
            if (seoName.Length > max)
            {
                seoName = seoName.Substring(0, max).TrimEnd('-');
            }
            return seoName;
        }

        public static string GetSeoName(string title)
        {
            if (title == null) return "";

            const int maxlen = 200;
            int len = title.Length;
            bool prevdash = false;
            var sb = new StringBuilder(len);
            char c;

            for (int i = 0; i < len; i++)
            {
                c = title[i];
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    sb.Append(c);
                    prevdash = false;
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    // tricky way to convert to lowercase
                    sb.Append((char)(c | 32));
                    prevdash = false;
                }
                else if (c == ' ' || c == ',' || c == '.' || c == '/' ||
                    c == '\\' || c == '-' || c == '_' || c == '=')
                {
                    if (!prevdash && sb.Length > 0)
                    {
                        sb.Append('-');
                        prevdash = true;
                    }
                }
                else if ((int)c >= 128)
                {
                    int prevlen = sb.Length;
                    // sb.Append(RemapInternationalCharToAscii(c));
                    if (prevlen != sb.Length) prevdash = false;
                }
                if (i == maxlen) break;
            }

            if (prevdash)
                return sb.ToString().Substring(0, sb.Length - 1);
            else
                return sb.ToString();
        }
    }
}
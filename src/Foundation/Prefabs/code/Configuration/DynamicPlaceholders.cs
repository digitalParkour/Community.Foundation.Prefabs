using Sitecore.Xml;
using System.Collections.Generic;

namespace Community.Foundation.Prefabs.Configuration
{
    public class DynamicPlaceholders
    {
        public static DynamicPlaceholders Instance
        {
            get
            {
                return Sitecore.Configuration.Factory.CreateObject("foundation.prefabs/dynamicPlaceholders", false) as DynamicPlaceholders 
                    ?? new DynamicPlaceholders();
            }
        }

        /// <summary>
        /// Dictionary:
        ///     key => rendering ID as string
        ///     value => placeholder prefix
        /// </summary>
        public Dictionary<string, PlaceholderPattern> Patterns { get; private set; }

        public DynamicPlaceholders()
        {
            this.Patterns = new Dictionary<string, PlaceholderPattern>();
        }

        public void AddMapping(string key, System.Xml.XmlNode node)
        {
            AddMapping(node);
        }
        public void AddMapping(System.Xml.XmlNode node)
        {
            var renderingId = XmlUtil.GetAttribute("renderingId", node);
            var pattern = new PlaceholderPattern
            {
                Prefix = XmlUtil.HasAttribute("prefixPattern", node) ? XmlUtil.GetAttribute("prefixPattern", node) : string.Empty,
                Suffix = XmlUtil.HasAttribute("suffixPattern", node) ? XmlUtil.GetAttribute("suffixPattern", node) : null
            };
            this.Patterns.Add(renderingId, pattern);
        }
    }

    public class PlaceholderPattern
    {
        public string Prefix { get; set; }
        public string Suffix { get; set; }

    }
}
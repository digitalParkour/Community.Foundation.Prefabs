using Community.Foundation.Prefabs.Constants;
using Sitecore.Configuration;

namespace Community.Foundation.Prefabs.Configuration
{
    public class Config
    {
        public struct Sxa
        {
            public static bool IsEnabled => Settings.GetBoolSetting("Prefabs.Sxa.SupportEnabled", false);
            public static bool AutoAllow => Settings.GetBoolSetting("Prefabs.Sxa.AutoAllowPrefabs", true);
            public static string GlobalSiteOverride => Settings.GetSetting("Prefabs.Sxa.GlobalSiteNameOverride", null);
        }

        public struct DynamicPlaceholder
        {
            public static string RenderingParameter => Settings.GetSetting("Prefabs.DynamicPlaceholder.RenderingParameterName", "DynamicPlaceholderId");
        }

        public struct Paths
        { 
            public static string Prefabs => $"/{Settings.GetSetting("Prefabs.GlobalPath", "/sitecore/system/Modules/Prefabs").Trim('/')}";

            public static string Renderings => $"/{Settings.GetSetting("Prefabs.RenderingsPath", "/sitecore/layout/Renderings/Foundation/Prefabs").Trim('/')}";
        }

        public struct Prefab
        {
            public static string LayoutId
            {
                get
                {
                    var config =  Settings.GetSetting("Prefabs.LayoutId", string.Empty);
                    var isEmpty = string.IsNullOrWhiteSpace(config);

                    // If empty and using sxa, then use Sxa default layout instead
                    if (isEmpty && Sxa.IsEnabled)
                        return Layouts.SxaMvc.Id;
                    
                    return isEmpty ? Layouts.PrefabDefault.Id : config;
                }
            }

            // Decided to make device assume match between prefab and source(on create)/target(on inject)
            // public static string DeviceId => Settings.GetSetting("Prefabs.DeviceId", "{FE5D7FDF-89C0-4D99-9AA3-B5FBD009C9F3}");
            public static string Placeholder => Settings.GetSetting("Prefabs.Placeholder", "main");
        }
    }
}
using Sitecore.Data;

namespace Community.Foundation.Prefabs.Constants
{
    public class Templates
    {
        public struct Prefab {
            public static readonly ID Id = new ID("{E8DEC0BB-4DEC-474E-A812-EB5C49D0A665}");
        }
        public struct Proxy {
            public static readonly BranchId BranchId = new BranchId(new ID("{DEBB5C91-8AE8-465E-9CD1-BB71B1639C6D}"));
        }

        public struct Sxa
        {
            public struct AvailableRenderings
            {
                public static readonly ID Id = new ID("{76DA0A8D-FC7E-42B2-AF1E-205B49E43F98}");

                public struct Fields
                {
                    public static readonly ID Renderings = new ID("{715AE6C0-71C8-4744-AB4F-65362D20AD65}");
                }
            }
        }

    }
}
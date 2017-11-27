using Sitecore.Diagnostics;

namespace Community.Foundation.Prefabs.Pipelines.CreatePrefab
{
    public class MapPlaceholders : CreatePrefabProcessor
    {
        /// <summary>
        /// Assume convention that rendering name matches prefab name
        /// Lookup item from defined global repository
        /// </summary>
        /// <param name="args"></param>
        public override void Process(CreatePrefabArgs args)
        {
            Assert.IsNotNull(args, "args");
            Assert.IsNotNull(args.Renderings, "args.Renderings");
            Assert.IsNotNull(args.OldPlaceholderPath, "args.Placeholder");
            Assert.IsNotNull(args.Placeholder, "args.Placeholder");

            var removeLength = Sitecore.StringUtil.EnsurePrefix('/', args.OldPlaceholderPath ).Length;
            var newPlaceholder = Helper.NormalizePath(args.Placeholder, false).TrimEnd('/');

            foreach (var r in args.Renderings)
            {
                r.Placeholder = r.Placeholder.Length <= removeLength ? args.Placeholder : string.Concat(newPlaceholder, r.Placeholder.Substring(removeLength));
            }
        }
    }
}
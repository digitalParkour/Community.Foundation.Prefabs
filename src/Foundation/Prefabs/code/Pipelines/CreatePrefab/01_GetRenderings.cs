using Community.Foundation.Prefabs.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.Web;
using System.Linq;
using System.Text.RegularExpressions;

namespace Community.Foundation.Prefabs.Pipelines.CreatePrefab
{
    public class GetRenderings : CreatePrefabProcessor
    {
        protected virtual string DynamicPlaceholderIdParam => Config.DynamicPlaceholder.RenderingParameter;
        protected DynamicPlaceholders _dynamicPlaceholders;

        public GetRenderings()
        {
            _dynamicPlaceholders = DynamicPlaceholders.Instance;
        }

        /// <summary>
        /// Assume convention that rendering name matches prefab name
        /// Lookup item from defined global repository
        /// </summary>
        /// <param name="args"></param>
        public override void Process(CreatePrefabArgs args)
        {
            Assert.IsNotNull(args, "args");
            Assert.IsNotNull(args.DeviceLayout, "args.DeviceLayout");
            Assert.IsFalse(args.Index == -1, "index indicates no selection");
            Assert.IsTrue(args.DeviceLayout.Renderings.Count > args.Index, "index out of bounds");

            var selectedRendering = (RenderingDefinition)args.DeviceLayout.Renderings[args.Index];
            var selectedPlaceholder = Helper.NormalizePath(selectedRendering.Placeholder);

            // If we know this rendering exposes a dynamic placeholder path, then filter by it
            if (IsDynamic(selectedRendering))
            {
                var id = GetDynamicId(selectedRendering) ?? string.Empty;
                var pattern = _dynamicPlaceholders.Patterns[selectedRendering.ItemID];
                var selectedPatternFull = $@"^{selectedPlaceholder}{pattern.Prefix}{id}{pattern.Suffix}$";
                var selectedPatternPartial = $@"^{selectedPlaceholder}{pattern.Prefix}{id}{pattern.Suffix}/";

                args.Renderings = args.DeviceLayout.Renderings.Cast<RenderingDefinition>()
                    .Where(x => Regex.IsMatch(x.Placeholder, selectedPatternFull)
                             || Regex.IsMatch(x.Placeholder, selectedPatternPartial)
                    ).ToList();
            }
            else {
                args.Renderings = args.DeviceLayout.Renderings.Cast<RenderingDefinition>().Where(x => x.Placeholder.StartsWith(selectedPlaceholder)).ToList();
            }

            args.Renderings.Insert(0, selectedRendering);

            args.OldPlaceholderPath = selectedRendering.Placeholder;
        }

        public virtual string GetDynamicId(RenderingDefinition rendering)
        {
            var parameters = WebUtil.ParseUrlParameters(rendering.Parameters);
            return parameters[DynamicPlaceholderIdParam];
        }

        public virtual bool IsDynamic(RenderingDefinition rendering)
        {
            return _dynamicPlaceholders.Patterns.Any() && _dynamicPlaceholders.Patterns.ContainsKey(rendering.ItemID);
        }
    }
}

/*
 
        public DeviceDefinition DeviceLayout { get; set; }
        public int index { get; set; }
        public Item ContextItem { get; set; }
     */

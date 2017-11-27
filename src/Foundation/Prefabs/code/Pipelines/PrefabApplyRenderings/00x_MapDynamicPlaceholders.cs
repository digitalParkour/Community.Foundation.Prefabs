using Community.Foundation.Prefabs.Configuration;
using Community.Foundation.Prefabs.Pipelines.PrefabGetRenderings;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.Text;
using Sitecore.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Community.Foundation.Prefabs.Pipelines.PrefabApplyRenderings
{
    public class MapDynamicPlaceholders : PrefabApplyRenderingsProcessor
    {
        protected virtual string DynamicPlaceholderIdParam => Config.DynamicPlaceholder.RenderingParameter;

        protected int lastId = -1;
        protected DynamicPlaceholders _dynamicPlaceholders;

        public MapDynamicPlaceholders() {
            _dynamicPlaceholders = DynamicPlaceholders.Instance;
        }

        public virtual bool ShouldRun()
        {
            // Run when we have placeholders configured and is SXA, since we have a different processor for SXA
            return _dynamicPlaceholders.Patterns.Any() && !Config.Sxa.IsEnabled;
        }

        public override void Process(PrefabGetRenderingsArgs args)
        {
            if (!ShouldRun())
                return;

            foreach(var r in args.Result) {
                if(!IsDynamic(r))
                    continue;

                var oldId = GetDynamicId(r);
                if (string.IsNullOrWhiteSpace(oldId))
                    continue;
                var newId = NextDynamicId(r, args);
                if (oldId == newId)
                    continue;

                SetDynamicId(r, newId);
                MapPlaceholderPaths(args, r, oldId, newId);
            }
        }        

        public virtual bool IsDynamic(RenderingDefinition rendering)
        {
            return _dynamicPlaceholders.Patterns.ContainsKey(rendering.ItemID);
        }

        public virtual string GetDynamicId(RenderingDefinition rendering)
        {
            var parameters = WebUtil.ParseUrlParameters(rendering.Parameters);
            return parameters[DynamicPlaceholderIdParam];
        }

        public virtual void SetDynamicId(RenderingDefinition rendering, string id)
        {
            var parameters = WebUtil.ParseUrlParameters(rendering.Parameters);
            parameters[DynamicPlaceholderIdParam] = id;
            rendering.Parameters = new UrlString(parameters).GetUrl();
        }

        public virtual string NextDynamicId(RenderingDefinition rendering, PrefabGetRenderingsArgs args)
        {
            return System.Math.Max(
                NextDynamicId(rendering, args.DeviceLayout.Renderings.Cast<RenderingDefinition>()),
                NextDynamicId(rendering, args.Result)
                ).ToString();
        }
        

        public virtual void MapPlaceholderPaths(PrefabGetRenderingsArgs pargs, RenderingDefinition rendering, string oldId, string newId)
        {
            if (!_dynamicPlaceholders.Patterns.ContainsKey(rendering.ItemID))
            {
                Log.Error($"Foundation.Prefabs::{nameof(MapDynamicPlaceholders)}:: Rendering {rendering.ItemID} identified as having dynamic placeholder, but no config mapping exists under sitecore/foundation.prefabs/dynamicPlaceholders/mappings; add config mapping", this);
                return;
            }

            var placeholderPath = Helper.NormalizePath(rendering.Placeholder);
            var pattern = _dynamicPlaceholders.Patterns[rendering.ItemID];
            // Using Regex LookBehind and LookAhead to match replaceable value
            var placeholderPatternFull = $@"(?<=^{placeholderPath}{pattern.Prefix}){oldId}(?={pattern.Suffix}$)"; // full value match
            var placeholderPatternPartial = $@"(?<=^{placeholderPath}{pattern.Prefix}){oldId}(?={pattern.Suffix}/)"; // partial value match - need the trailing slash to avoid false positives

            foreach (RenderingDefinition r in pargs.Result)
            {
                r.Placeholder = Regex.Replace(r.Placeholder, placeholderPatternFull, newId);
                r.Placeholder = Regex.Replace(r.Placeholder, placeholderPatternPartial, newId);
            }
        }


        public virtual int NextDynamicId(RenderingDefinition renderingToUpdate, IEnumerable<RenderingDefinition> renderings)
        {
            int num = 0;
            
            if (renderings == null || !renderings.Any())
            {
                return 1;
            }
            foreach (RenderingDefinition rendering in renderings.Where((RenderingDefinition r) => r != null && r.UniqueId != renderingToUpdate.UniqueId))
            {
                if (string.IsNullOrEmpty(rendering.Parameters))
                {
                    continue;
                }
                int num1 = (int.TryParse(WebUtil.ParseUrlParameters(rendering.Parameters)[DynamicPlaceholderIdParam], out num1) ? num1 : num);
                if (num1 <= num)
                {
                    continue;
                }
                num = num1;
            }
            return num + 1;
        }
    }
}
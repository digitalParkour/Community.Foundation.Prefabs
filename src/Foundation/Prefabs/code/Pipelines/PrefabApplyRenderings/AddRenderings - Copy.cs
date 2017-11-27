using Community.Foundation.Prefabs.Pipelines.PrefabGetRenderings;

namespace Community.Foundation.Prefabs.Pipelines.PrefabApplyRenderings
{
    public class AddRenderings : PrefabApplyRenderingsProcessor
    {
        public override void Process(PrefabGetRenderingsArgs args)
        {
            if (!args.Handled)
                return;

            foreach(var r in args.Result) {
                if (args.Index >= 0)
                { 
                    args.DeviceLayout.Insert(args.Index++, r); // insert before (in place of)
                } else
                {
                    args.DeviceLayout.AddRendering(r); // when not specified, add to end
                }
            }
        }
        
    }
}
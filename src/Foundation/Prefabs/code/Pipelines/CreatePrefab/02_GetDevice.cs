using Sitecore.Diagnostics;

namespace Community.Foundation.Prefabs.Pipelines.CreatePrefab
{
    public class GetDevice : CreatePrefabProcessor
    {
        /// <summary>
        /// Assume convention that rendering name matches prefab name
        /// Lookup item from defined global repository
        /// </summary>
        /// <param name="args"></param>
        public override void Process(CreatePrefabArgs args)
        {
            Assert.IsNotNull(args, "args");
            Assert.IsNotNull(args.DeviceLayout, "args.DeviceLayout");

            args.DeviceId = args.DeviceLayout.ID;
        }
    }
}
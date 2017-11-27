using Community.Foundation.Prefabs.Abstractions.Services;
using Sitecore;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.Shell.Applications.WebEdit.Commands;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Data;
using Sitecore.Web;
using Community.Foundation.Prefabs.Configuration;

namespace Community.Foundation.Prefabs.Commands
{
    public class PrefabMake : WebEditCommand
    {
        protected IPrefabService _prefabService;
        public PrefabMake()
        {
            _prefabService = Sitecore.DependencyInjection.ServiceLocator.ServiceProvider.GetService<IPrefabService>();
        }

        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            WebEditCommand.ExplodeParameters(context);
            Sitecore.Context.ClientPage.Start(this, "Run", context.Parameters);
        }
        protected void Run(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            
            if (!args.IsPostBack)
            {
                SheerResponse.Input("Name: ", "New Prefab", "New Prefab");
                args.WaitForPostBack();
            }
            else if (args.HasResult)
            {
                var name = args.Result;

                var pageId = args.Parameters["id"];
                var uniqueId = args.Parameters["referenceId"];
                var renderingId = args.Parameters["renderingId"];

                var db = Client.ContentDatabase;

                LayoutDefinition layoutDefinition = GetCurrentLayoutDefinition();
                var deviceId = WebEditUtil.GetClientDeviceId();

                if (layoutDefinition == null || ID.IsNullOrEmpty(deviceId))
                {
                    SheerResponse.Alert("Action failed. Unable to find presentation details.");
                    return;
                }

                var deviceRenderings = layoutDefinition?.GetDevice(deviceId.ToString());
                if (deviceRenderings?.Renderings == null)
                {
                    SheerResponse.Alert("Action failed. Unable to find current device.");
                    return;
                }

                var index = GetIndex(deviceRenderings, uniqueId);
                if (index == -1)
                {
                    SheerResponse.Alert("Action failed. Unable to find selected rendering");
                    return;
                }
                
                var item = UIUtil.GetItemFromQueryString(db);

                // Create prefab
                var prefab = _prefabService.CreatePrefab(name, deviceRenderings, index, item);

                SheerResponse.Alert("Prefab Created");
                
                if(Config.Sxa.IsEnabled) // reload so toolbox shows new prefab
                    SheerResponse.Eval("window.parent.location.reload();");

                // TODO: Go Edit Prefab?
                return;
            }


        }
        protected virtual LayoutDefinition GetCurrentLayoutDefinition()
        {
            string formValue = WebUtil.GetFormValue("scLayout");
            if (string.IsNullOrEmpty(formValue))
            {
                return null;
            }
            string xML = WebEditUtil.ConvertJSONLayoutToXML(formValue);
            if (string.IsNullOrEmpty(xML))
            {
                return null;
            }
            return LayoutDefinition.Parse(xML);
        }

        protected virtual int GetIndex(DeviceDefinition device, string uniqueId)
        {
            for (var i = 0; i < device.Renderings.Count; i++)
            {
                RenderingDefinition rendering = device.Renderings[i] as RenderingDefinition;
                Assert.IsNotNull(rendering, "rendering");

                if (rendering.UniqueId == uniqueId)
                    return i;
            }

            return -1;
        }
    }
}
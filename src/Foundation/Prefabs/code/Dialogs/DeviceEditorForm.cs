using Community.Foundation.Prefabs.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;
using Sitecore;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.Reflection;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using System.Reflection;

namespace Community.Foundation.Prefabs.Dialogs
{
    public class DeviceEditorForm : Sitecore.Shell.Applications.Layouts.DeviceEditor.DeviceEditorForm
    {
        private IPrefabService _prefabService;
        public DeviceEditorForm() : base() {
            _prefabService = Sitecore.DependencyInjection.ServiceLocator.ServiceProvider.GetService<IPrefabService>();
        }

        /// <summary>
        /// Adds a prefab based on the selected rendering.
        /// </summary>
        /// <value>The Remove button.</value>
        protected Button btnPrefab
        {
            get;
            set;
        }

        // Enable/Disable button... This is as close as I could get without copy and pasting all the private methods... this does not get called on refresh and does not check all the conditions... but good enough I think
        protected new void OnRenderingClick(string index)
        {
            Assert.ArgumentNotNull(index, "index");

            var i = MainUtil.GetInt(index, -1);
            this.btnPrefab.Disabled = i < 0;

            base.OnRenderingClick(index);
        }

        [HandleMessage("prefab:create", true)]
        [UsedImplicitly]
        protected void PrefabCreate(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            int selectedIndex = this.SelectedIndex;
            if (selectedIndex < 0)
            {
                return;
            }
            LayoutDefinition layoutDefinition = GetLayoutDefinition_exposed();
            var deviceRenderings = layoutDefinition.GetDevice(this.DeviceID);
            if (deviceRenderings?.Renderings == null)
            {
                return;
            }
            if (selectedIndex < 0 || selectedIndex >= deviceRenderings.Renderings.Count)
            {
                return;
            }

            if (!args.IsPostBack)
            {
                SheerResponse.Input("Name: ", "New Prefab", "New Prefab");
                args.WaitForPostBack();
            }
            else if (args.HasResult)
            {
                var name = args.Result;
                
                var item = UIUtil.GetItemFromQueryString(Client.ContentDatabase);

                // manipulate layoutDefinition via device reference
                var prefab = _prefabService.CreatePrefab(name, deviceRenderings, this.SelectedIndex, item);

                SheerResponse.Alert("Prefab Created");

                // TODO: Go Edit Prefab?
                return;
            }
            
        }

        /// <summary>
        /// Extend Presentation Details dialog [Add] rendering process to insert prefab when proxy method rendering item is added
        /// </summary>
        /// <param name="args">
        /// The arguments.
        /// </param>
        [HandleMessage("device:add", true)]
        [UsedImplicitly]
        protected void PrefabInject(ClientPipelineArgs args)
        {
            var doFollowUp = false;
            var lastIndex = -1;

            Assert.ArgumentNotNull(args, "args");
            if (args.IsPostBack && args.HasResult)
            {
                string[] strArrays = args.Result.Split(new char[] { ',' });
                string renderingItemId = strArrays[0];
                string placeholderPath = strArrays[1].Replace("-c-", ",");

                var layoutDefinition = GetLayoutDefinition_exposed();
                var deviceRenderings = layoutDefinition.GetDevice(this.DeviceID);
                
                var renderingItem = Client.ContentDatabase.GetItem(renderingItemId);
                
                if(_prefabService.IsPrefab(renderingItem))
                {
                    var item = UIUtil.GetItemFromQueryString(Client.ContentDatabase);

                    doFollowUp = true;

                    // manipulate layoutDefinition via device reference
                    _prefabService.InjectPrefab(renderingItem, deviceRenderings, placeholderPath, this.SelectedIndex, item);

                    lastIndex = deviceRenderings.Renderings.Count;

                    SetDefinition_exposed(layoutDefinition);

                    // Never allow editor to open
                    if (args.Result.EndsWith("1"))
                        args.Result = string.Concat(args.Result.TrimEnd('1'), "0");
                }
            }

            // Call overridden method for all other logic and most importantly refresh() - NOTE: this adds our proxy item... so remove it next
            this.Add(args);

            // Remove robot (prefab proxy item) [this is because we can't call refresh() and exit above - it's private]
            if (doFollowUp) {
                this.SelectedIndex = lastIndex; // set index to delete (last item always)
                Remove(Message.Empty);  // unfortunately this always selects the last element
            }
        }

        /// <summary>
        /// Necessary to override Add method (skip the HandleMessageAttribute for it)
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMessage(Message message)
        {
            if(message.Name != "device:add")
            { 
                base.HandleMessage(message);
                return;
            }

            var methods = this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int i = 0; i < (int)methods.Length; i++)
            {
                MethodInfo methodInfo = methods[i];

                if (methodInfo.Name == "Add") continue; // Override Add method, skip it

                object[] customAttributes = methodInfo.GetCustomAttributes(typeof(HandleMessageAttribute), true);
                if (customAttributes.Length != 0)
                {
                    object[] objArray = customAttributes;
                    for (int j = 0; j < (int)objArray.Length; j++)
                    {
                        HandleMessageAttribute handleMessageAttribute = (HandleMessageAttribute)objArray[j];
                        if (handleMessageAttribute.Message == message.Name)
                        {
                            if (!handleMessageAttribute.Start)
                            {
                                ReflectionUtil.InvokeMethod(methodInfo, new object[] { message }, this);
                            }
                            else
                            {
                                Context.ClientPage.Start(this, methodInfo.Name, message.Arguments);
                            }
                            if (message.CancelBubble)
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }

        /*
         * EXPOSE PRIVATE METHODS
         * - These are methods we need to call but can't because the are marked private, so ported over the source code and renamed with "_exposed" suffix.
         */
        protected static LayoutDefinition GetLayoutDefinition_exposed()
        {
            string sessionString = WebUtil.GetSessionString(GetSessionHandle_exposed());
            Assert.IsNotNull(sessionString, "layout definition");
            return LayoutDefinition.Parse(sessionString);
        }

        protected static void SetDefinition_exposed(LayoutDefinition layout)
        {
            Assert.ArgumentNotNull(layout, "layout");
            string xml = layout.ToXml();
            WebUtil.SetSessionValue(GetSessionHandle_exposed(), xml);
        }

        protected static string GetSessionHandle_exposed()
        {
            return "SC_DEVICEEDITOR";
        }
        
    }
}
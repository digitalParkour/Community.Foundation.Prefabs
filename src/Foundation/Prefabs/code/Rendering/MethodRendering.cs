using Sitecore.Data.Managers;

namespace Community.Foundation.Prefabs.Rendering
{
    public class MethodRendering
    {
        /// <summary>
        /// The insert rendering processors should remove this rendering and add the prefab.
        /// But if something goes wrong and this did not get removed show a message so authors can delete this proxy rendering.
        /// </summary>
        /// <returns></returns>
        public static string ProxyItem() {
            
            // Gracefully ignore for live pages
            if (Sitecore.Context.PageMode.IsNormal)
                return string.Empty;

            // Alert authors
            var funIcon = ThemeManager.GetImage("Office/32x32/robot.png", 32, 32);
            return $"{funIcon} <span class=\"editor-hint\"> <strong>Whoops!</strong> Something went wrong with a prefab. You should delete this rendering. </span>";
        }
    }
}
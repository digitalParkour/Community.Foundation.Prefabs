using Community.Foundation.Prefabs.Pipelines.PrefabGetRenderings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Community.Foundation.Prefabs.Pipelines.PrefabApplyRenderings
{
    public abstract class PrefabApplyRenderingsProcessor
    {
        public abstract void Process(PrefabGetRenderingsArgs args);
    }
}
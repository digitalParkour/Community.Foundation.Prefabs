using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Community.Foundation.Prefabs.Pipelines.PrefabGetRenderings
{
    public abstract class PrefabGetRenderingsProcessor
    {
        public abstract void Process(PrefabGetRenderingsArgs args);
    }
}
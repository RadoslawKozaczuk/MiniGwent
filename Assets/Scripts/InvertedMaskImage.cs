using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class InvertedMaskImage : Image
    {
        public override Material materialForRendering
        {
            get
            {
                Material result = base.materialForRendering;
                result.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
                return result;
            }
        }
    }
}
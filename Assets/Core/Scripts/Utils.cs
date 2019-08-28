using UnityEngine;

namespace Assets.Core
{
    class Utils
    {
        public static int CountDigit(int n) => Mathf.FloorToInt(Mathf.Log10(n) + 1);
    }
}

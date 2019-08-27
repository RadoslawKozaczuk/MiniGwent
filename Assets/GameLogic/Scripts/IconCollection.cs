using UnityEngine;

namespace Assets.GameLogic
{
    [DisallowMultipleComponent]
    public sealed class IconCollection : MonoBehaviour
    {
        // custom indexers for convenience
        public Sprite this[int id] => _icons[id];

        [Header("Prefabs must match ids in DB.")]
        [SerializeField] Sprite[] _icons;
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [HideInInspector] public CardInfoUI CardInfoUI;
        public Image Image;
        public string Title;
        public string Description;
        public int Strength;

        [SerializeField] GameObject _front;
        [SerializeField] GameObject _back;

        [SerializeField] Outline _outline;
        bool _selected;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (_selected)
            {
                // multiplied by two to have more frequent pulses
                float value = (Mathf.Cos(Time.time * 2) + 2.5f) * 2; // from 4.5 to 7
                _outline.effectDistance = new Vector2(value, value);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _selected = true;
            CardInfoUI.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _selected = false;
            _outline.effectDistance = new Vector2(0, 0);
            CardInfoUI.gameObject.SetActive(false);
        }
    }
}

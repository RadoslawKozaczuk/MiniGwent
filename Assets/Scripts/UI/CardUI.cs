using Assets.Scripts.UI;
using Assets.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, 
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Image Image;
        public int Id;

        [SerializeField] GameObject _front;
        [SerializeField] GameObject _back;
        [SerializeField] OutlineController _outline;

        // predrag stuff
        Transform PreDragLocation;
        public LineUI ParentLineUI;

        public Canvas mainCanvas;
        public Canvas secondaryCanvas;

        // this corresponds both to the sibling index on the line as well as the index in the table in GameLogic
        public int NumberInLine;

        public void OnPointerEnter(PointerEventData eventData)
        {
            _outline.TurnPulsationOn();
            GameEngine.Instance.CardInfoPanel.gameObject.SetActive(true);
            GameEngine.Instance.CardInfoPanel.LoadDataForId(Id);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _outline.TurnPulsationOff();
            GameEngine.Instance.CardInfoPanel.gameObject.SetActive(false);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            PreDragLocation = transform.parent;
            
            // move to secondary canvas in order to be displayed always on top
            transform.SetParent(secondaryCanvas.transform, true);

            GameEngine.CardBeingDraged = this;
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            LineUI targetLine = eventData.pointerCurrentRaycast.gameObject.GetComponent<LineUI>();

            if (targetLine && targetLine != ParentLineUI) // dropped on a line
            {
                ParentLineUI.RemoveFromLine(NumberInLine);
                // inform the game logic about it
                GameEngine.GameLogic.MoveCard(ParentLineUI.LineIndicator, NumberInLine, targetLine.LineIndicator);

                targetLine.PutInLine(this);

                
            }
            else // dropped somewhere else or on the same line
            {
                transform.SetParent(PreDragLocation);
                transform.localPosition = Vector3.zero; // go back where you were
            }

            GameEngine.CardBeingDraged = null;
        }
    }
}

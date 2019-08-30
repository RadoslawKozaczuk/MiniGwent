using Assets.Core;
using Assets.Scripts.UI;
using TMPro;
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

        /// <summary>
        /// Indicates whether that card should be able to drag.
        /// </summary>
        public bool Draggable;

        [SerializeField] GameObject _front;
        [SerializeField] GameObject _back;

        bool _hidden;
        /// <summary>
        /// When set to true card back is shown.
        /// </summary>
        public bool Hidden
        {
            get => _hidden;
            set
            {
                _front.SetActive(!value);
                _back.SetActive(value);
                _hidden = value;
            }
        }

        [SerializeField] TextMeshProUGUI _stengthText;

        public OutlineController OutlineController;

        // predrag stuff
        Transform _preDragLocation;
        public LineUI ParentLineUI;

        public Canvas mainCanvas;
        public Canvas secondaryCanvas;

        // this corresponds both to the sibling index on the line as well as the index in the table in GameLogic
        public int NumberInLine;

        public int MaxStrength;
        public int CurrentStrength;

        public CardUI UpdateStrengthText()
        {
            if (CurrentStrength < MaxStrength)
                _stengthText.text = $"STR: <color=red>{CurrentStrength}</color>";
            else if (CurrentStrength == MaxStrength)
                _stengthText.text = $"STR: {CurrentStrength}";
            else 
                _stengthText.text = $"STR: <color=red>{CurrentStrength}</color>";

            return this; // return this to allow method chaining
        }

        #region Interface Implementation
        public void OnPointerEnter(PointerEventData eventData)
        {
            // if nothing is being dragged
            if(!GameEngine.CardBeingDraged && !_hidden)
            {
                OutlineController.TurnPulsationOn();
                GameEngine.Instance.CardInfoPanel.gameObject.SetActive(true);
                GameEngine.Instance.CardInfoPanel.LoadDataForId(Id);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OutlineController.TurnPulsationOff();
            GameEngine.Instance.CardInfoPanel.gameObject.SetActive(false);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!Draggable)
                return;

            _preDragLocation = transform.parent;
            
            // move to secondary canvas in order to be displayed always on top
            transform.SetParent(secondaryCanvas.transform, true);

            GameEngine.CardBeingDraged = this;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!Draggable)
                return;

            transform.position = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!Draggable)
                return;

            LineUI targetLine = eventData.pointerCurrentRaycast.gameObject.GetComponent<LineUI>();
            if (targetLine && targetLine != ParentLineUI) // dropped on a line
            {
                // its important to copy these values as they may get changed in the methods below
                Line parent = ParentLineUI.LineIndicator;
                int fromSlotNumber = NumberInLine;
                Line target = targetLine.LineIndicator;
                int targetSlotNumber = targetLine.TargetSlotPositionNumber;

                ParentLineUI.RemoveFromLine(fromSlotNumber, false);
                targetLine.InsertCard(this, targetSlotNumber);

                // every card once moved becomes non drag-able anymore
                Draggable = false;

                // inform logic abut it
                GameEngine.GameLogic.MoveCard(parent, fromSlotNumber, target, targetSlotNumber);

                targetLine.DestroyTargetSlotIndicator();
            }
            else // dropped somewhere else or on the same line
            {
                transform.SetParent(_preDragLocation);
                transform.localPosition = Vector3.zero; // go back where you were
            }

            GameEngine.CardBeingDraged = null;
        }
        #endregion
    }
}

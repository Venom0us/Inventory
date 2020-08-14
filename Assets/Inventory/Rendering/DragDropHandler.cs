using UnityEngine;
using UnityEngine.EventSystems;

namespace VenoLib.ItemManagement
{
    public class DragDropHandler : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        public InventoryRenderer Renderer { get; private set; }
        private Transform _previousParentTransform;
        private Vector3 _previousPosition;

        public void UpdateSlotData()
        {
            var oldSlotIndex = Renderer.GetSlotIndex(_previousParentTransform.parent);
            var newSlotIndex = Renderer.GetSlotIndex(transform.parent.parent);

            var oldSlot = Renderer.GetSlot(oldSlotIndex);
            var newSlot = Renderer.GetSlot(newSlotIndex);

            // Set correct amount of the slot
            var textComponent = oldSlot.GetComponentInChildren<TMPro.TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = Renderer.GetSlotAmount(oldSlotIndex);
            }

            textComponent = newSlot.GetComponentInChildren<TMPro.TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = Renderer.GetSlotAmount(newSlotIndex);
            }
        }

        public void Initialize(InventoryRenderer renderer)
        {
            Renderer = renderer;
            _rectTransform = GetComponent<RectTransform>();
            _canvas = renderer.Canvas;
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _previousPosition = transform.position;
            _previousParentTransform = transform.parent;
            _canvasGroup.alpha = 0.6f;
            _canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_previousParentTransform == transform.parent)
            {
                transform.position = _previousPosition;
            }
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
        }

        public void OnPointerDown(PointerEventData eventData)
        { }
    }
}

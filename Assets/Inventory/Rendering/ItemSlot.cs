using UnityEngine;
using UnityEngine.EventSystems;

namespace VenoLib.ItemManagement
{
    public class ItemSlot : MonoBehaviour, IDropHandler
    {
        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag != null)
            {
                var handler = eventData.pointerDrag.GetComponent<DragDropHandler>();
                var newSlotIndex = handler.Renderer.GetSlotIndex(transform.parent);
                var oldSlotIndex = handler.Renderer.GetSlotIndex(eventData.pointerDrag.transform.parent.parent);
                if (handler.Renderer.IsSlotOpen(newSlotIndex))
                {
                    // Move slot in renderer slots
                    handler.Renderer.MoveSlotIndex(oldSlotIndex, newSlotIndex);

                    eventData.pointerDrag.transform.position = transform.position;
                    eventData.pointerDrag.transform.parent = transform;
                    eventData.pointerDrag.transform.SetAsFirstSibling();

                    // Update the slot data
                    handler.UpdateSlotData();
                }
                else
                {
                    // Swap old slot
                    var oldGraphic = transform.GetChild(0);
                    var oldSlot = handler.Renderer.GetSlot(oldSlotIndex);
                    oldGraphic.transform.parent = oldSlot.transform;
                    oldGraphic.transform.position = oldSlot.transform.position;
                    oldGraphic.transform.SetAsFirstSibling();

                    // Swap new slot
                    eventData.pointerDrag.transform.position = transform.position;
                    eventData.pointerDrag.transform.parent = transform;
                    eventData.pointerDrag.transform.SetAsFirstSibling();

                    // Switch item slot
                    handler.Renderer.SwitchSlotIndex(oldSlotIndex, newSlotIndex);

                    // Update the slot data
                    handler.UpdateSlotData();
                }
            }
        }
    }
}

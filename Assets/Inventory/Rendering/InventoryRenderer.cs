using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace VenoLib.ItemManagement
{
    public class InventoryRenderer : MonoBehaviour
    {
        public Canvas Canvas;
        public Image[] Slots;

        private readonly Dictionary<int, Image> _slotIndexes = new Dictionary<int, Image>();

        public Func<int, string> GetSlotAmount;
        public Action<int, int> SwitchListIndex;

        public Image GetSlot(int slotIndex)
        {
            var slotHolder = Slots[slotIndex];
            return slotHolder.transform.GetChild(0).GetComponent<Image>();
        }

        public int GetSlotIndex(Transform slotTransform)
        {
            return Slots.Select(a => a.transform).ToList().IndexOf(slotTransform);
        }

        public void ClearSlot(int slotIndex)
        {
            if (slotIndex >= Slots.Length || slotIndex < 0)
                throw new Exception("[Inventory Rendering]: Slot was invalid?: " + slotIndex);

            if (_slotIndexes.TryGetValue(slotIndex, out Image slotImage))
            {
                Destroy(slotImage.gameObject);
            }

            var slot = GetSlot(slotIndex);

            // Empty out amount text
            var textComponent = slot.GetComponentInChildren<TMPro.TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = string.Empty;
            }
        }

        public bool IsSlotOpen(int slotIndex)
        {
            return !_slotIndexes.ContainsKey(slotIndex);
        }

        public void MoveSlotIndex(int oldSlotIndex, int newSlotIndex)
        {
            if (IsSlotOpen(newSlotIndex))
            {
                var old = _slotIndexes[oldSlotIndex];
                _slotIndexes.Remove(oldSlotIndex);
                _slotIndexes.Add(newSlotIndex, old);

                SwitchListIndex(oldSlotIndex, newSlotIndex);
            }
        }

        public void SwitchSlotIndex(int slotIndex1, int slotIndex2)
        {
            var temp = _slotIndexes[slotIndex1];
            var temp2 = _slotIndexes[slotIndex2];
            _slotIndexes[slotIndex1] = temp2;
            _slotIndexes[slotIndex2] = temp;

            SwitchListIndex(slotIndex1, slotIndex2);
        }

        public void UpdateSlot(int slotIndex, Texture2D texture, int amount)
        {
            if (slotIndex >= Slots.Length || slotIndex < 0)
                throw new Exception("[Inventory Rendering]: Slot was invalid?: " + slotIndex);
            if (texture == null)
                throw new Exception("[Inventory Rendering]: Texture is null: " + slotIndex);

            // Update graphic for the slot
            StartCoroutine(UpdateGraphic(slotIndex, texture, amount));
        }

        private bool isRunning = false;
        private IEnumerator UpdateGraphic(int slotIndex,  Texture2D texture, int amount)
        {
            // Make sure that the update graphic calls wait on eachother
            while (isRunning)
            {
                yield return null;
            }

            isRunning = true;
            if (_slotIndexes.TryGetValue(slotIndex, out Image slotImage))
            {
                slotImage.sprite = Sprite.Create(texture, slotImage.sprite.rect, slotImage.sprite.pivot);

                // Update amount text
                var textComponent = slotImage.GetComponentInChildren<TMPro.TMP_Text>();
                if (textComponent != null)
                {
                    textComponent.text = amount.ToString();
                }
            }
            else
            {
                // Create slot graphic object
                var newImage = new GameObject("SlotGraphic");

                // Add image component
                var imgComponent = newImage.AddComponent<Image>();

                yield return new WaitForEndOfFrame(); // Wait one frame for sizeDelta to update on the RectTransform

                // Add drag and drop handler
                var handler = newImage.AddComponent<DragDropHandler>();
                handler.Initialize(this); // Initialize drag and drop functionality

                // Copy rect size
                var rect = newImage.GetComponent<RectTransform>();
                var slot = GetSlot(slotIndex);
                rect.sizeDelta = slot.gameObject.GetComponent<RectTransform>().sizeDelta;

                // Set sprite to the texture
                imgComponent.color = new Color(slot.color.r, slot.color.g, slot.color.b, 1);
                imgComponent.sprite = Sprite.Create(texture, slot.sprite.rect, slot.sprite.pivot);
                newImage.transform.position = slot.transform.position;
                newImage.transform.localScale = slot.transform.localScale;

                // Correctly parent the object and set position in hierarchy
                newImage.transform.SetParent(slot.transform, false);
                newImage.transform.SetAsFirstSibling();

                // Add item slot for drop event if script is not there
                if (newImage.transform.parent.GetComponent<ItemSlot>() == null)
                    newImage.transform.parent.gameObject.AddComponent<ItemSlot>();

                // Update amount text
                var textComponent = slot.GetComponentInChildren<TMPro.TMP_Text>();
                if (textComponent != null)
                {
                    textComponent.text = amount.ToString();
                }

                _slotIndexes.Add(slotIndex, imgComponent);
            }
            isRunning = false;
        }
    }

    public class InventoryArgs : EventArgs
    {
        public IItem Item;
        public int OldAmount;
        public int NewAmount;

        public InventoryArgs(IItem item, int oldAmount, int newAmount)
        {
            Item = item;
            OldAmount = oldAmount;
            NewAmount = newAmount;
        }
    }
}
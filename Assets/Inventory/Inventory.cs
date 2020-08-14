using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VenoLib.ItemManagement
{
    /// <summary>
    /// Class to collect items derived from IItem interface.
    /// This class can be coupled with the InventoryRenderer class.
    /// </summary>
    /// <typeparam name="T">Custom item type</typeparam>
    public class Inventory<T> where T : IItem
    {
        private readonly InventoryItem[] _items;
        private readonly InventoryRenderer _renderer;
        private int _maxStackSize = 32;
        private int _maxItems = 30;

        public event EventHandler<InventoryArgs> ItemInserted;
        public event EventHandler<InventoryArgs> ItemRemoved;
        public event EventHandler<InventoryArgs> ItemUpdated;

        public int Count { get { return _items.Count(a => a != null); } }

        /// <summary>
        /// Use our own implementation, to not require unnecessary fields (like amount) on implementation side of IItem interface.
        /// </summary>
        class InventoryItem
        {
            public readonly T Item;
            public int Amount;

            public InventoryItem(T item, int amount = 1)
            {
                Item = item;
                Amount = amount;
            }
        }

        /// <summary>
        /// Base constructor for Inventory, pass along InventoryRenderer if you want items to be rendered.
        /// </summary>
        /// <param name="maxItems"></param>
        /// <param name="maxStackSize"></param>
        /// <param name="renderer"></param>
        public Inventory(int maxItems, int maxStackSize, InventoryRenderer renderer = null)
        {
            if (maxStackSize <= 0)
                throw new Exception("[Initialization]: Inventory max stack size cannot be zero or less.");
            if (maxItems <= 0)
                throw new Exception("[Initialization]: Max inventory size cannot be zero or less.");

            _maxItems = maxItems;
            _items = new InventoryItem[_maxItems];
            _maxStackSize = maxStackSize;
            _renderer = renderer;

            // Methods passed along to renderer
            _renderer.GetSlotAmount = (index) => 
            { 
                return _items[index] != null ? _items[index].Amount.ToString() : string.Empty; 
            };
            _renderer.SwitchListIndex = (index1, index2) =>
            {
                var temp = _items[index1];
                var temp2 = _items[index2];
                _items[index1] = temp2;
                _items[index2] = temp;
            };
        }

        /// <summary>
        /// Renders the changes to the connected InventoryRenderer.
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="item"></param>
        private void RenderChange(int slot, InventoryItem item)
        {
            if (_renderer == null) return;

            if (item.Amount == 0)
            {
                // Item removal, clear slot
                _renderer.ClearSlot(slot);
            }
            else
            {
                // Item updating, update texture and amount
                _renderer.UpdateSlot(slot, item.Item.Texture, item.Amount);
            }
        }

        /// <summary>
        /// Add's the given item, with the given amount to the pool.
        /// Amount will be batched automatically based on max stack size.
        /// </summary>
        /// <param name="newItem"></param>
        /// <param name="amount"></param>
        public bool Add(T newItem, int amount, out string message)
        {
            message = string.Empty;

            if (amount <= 0) 
                throw new Exception("[ItemInsert]: Item amount cannot be zero or less.");

            // Seperate into batches, if the amount is too much.
            if (amount > _maxStackSize)
            {
                var seperateAmount = Mathf.CeilToInt((float)amount / _maxStackSize);
                int totalAmount = amount;
                int leftOver = totalAmount;
                for (int i=0; i < seperateAmount; i++)
                {
                    if (leftOver >= _maxStackSize)
                    {
                        if (!Add(newItem, _maxStackSize, out message)) return false;
                    }
                    else if (leftOver > 0)
                    {
                        if (!Add(newItem, leftOver, out message)) return false;
                    }

                    leftOver -= _maxStackSize;
                    if (leftOver <= 0) break;
                }
                return true;
            }

            var validItems = _items
                .Where(a => a != null && a.Item.Id == newItem.Id && a.Amount < _maxStackSize)
                .OrderByDescending(a => a.Amount);

            // Add amount to existing items in the inventory already
            int totalValidAmount = amount;
            foreach (var item in validItems)
            {
                int differenceAmount = _maxStackSize - item.Amount;

                if (totalValidAmount >= differenceAmount)
                {
                    totalValidAmount -= differenceAmount;
                    int oldAmount = item.Amount;
                    item.Amount += differenceAmount;
                    ItemUpdated?.Invoke(this, new InventoryArgs(item.Item, oldAmount, item.Amount));
                    RenderChange(Array.IndexOf(_items, item), item);
                }
                else
                {
                    int oldAmount = item.Amount;
                    item.Amount += totalValidAmount;
                    ItemUpdated?.Invoke(this, new InventoryArgs(item.Item, oldAmount, item.Amount));
                    RenderChange(Array.IndexOf(_items, item), item);
                    totalValidAmount = 0;
                }

                if (totalValidAmount == 0) break;
            }

            // Add new item, if there is still amount left
            if (totalValidAmount > 0)
            {
                if (_items.All(a => a != null))
                {
                    if (amount - totalValidAmount == 0)
                        message = "Inventory is full.";
                    else
                        message = "Inventory is full, only added " + (amount - totalValidAmount) + " amount.";
                    return false;
                }

                var item = new InventoryItem(newItem, totalValidAmount);
                var firstOpenIndex = Array.FindIndex(_items, a => a == null);
                if (firstOpenIndex == -1) throw new Exception("Inventory is full, we shouldn't have gotten here..");
                _items[firstOpenIndex] = item;

                ItemInserted?.Invoke(this, new InventoryArgs(newItem, 0, item.Amount));
                RenderChange(firstOpenIndex, item);
            }
            return true;
        }

        /// <summary>
        /// Removes an item with the given amount from the inventory.
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="amount"></param>
        public void Remove(int itemId, int amount)
        {
            if (amount <= 0)
                throw new Exception("[ItemRemove]: Item amount cannot be zero or less.");

            var items = _items.Where(a => a != null && a.Item.Id == itemId);
            int leftOver = amount;

            foreach (var item in items)
            {
                if (leftOver >= item.Amount)
                {
                    int oldAmount = item.Amount;
                    leftOver -= item.Amount;
                    item.Amount -= item.Amount;

                    var slotIndex = Array.IndexOf(_items, item);

                    // Reset to null
                    _items[slotIndex] = null;

                    ItemRemoved?.Invoke(this, new InventoryArgs(item.Item, oldAmount, 0));
                    RenderChange(slotIndex, item);
                }
                else
                {
                    int oldAmount = item.Amount;
                    item.Amount -= leftOver;
                    leftOver = 0;
                    ItemUpdated?.Invoke(this, new InventoryArgs(item.Item, oldAmount, item.Amount));
                    RenderChange(Array.IndexOf(_items, item), item);
                    break;
                }
            }
        }

        /// <summary>
        /// Removes an item with the given amount from the inventory.
        /// </summary>
        /// <param name="oldItem"></param>
        /// <param name="amount"></param>
        public void Remove(T oldItem, int amount)
        {
            Remove(oldItem.Id, amount);
        }

        /// <summary>
        /// Peek's into all the items of this itemId from the inventory, does NOT remove them.
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public IEnumerable<T> Peek(int itemId)
        {
            return _items
                .Where(a => a != null && a.Item.Id == itemId)
                .Select(a => a.Item);
        }

        /// <summary>
        /// Returns the total amount of this item in the inventory.
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public int GetTotalAmount(int itemId)
        {
            return _items
                .Where(a => a != null && a.Item.Id == itemId)
                .Sum(a => a.Amount);
        }
    }
}
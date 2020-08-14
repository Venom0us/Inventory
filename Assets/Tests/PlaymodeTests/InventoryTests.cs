using System.Collections;
using System.Collections.Generic;
using Assets.Tests;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using VenoLib.ItemManagement;

namespace Tests
{
    public class InventoryTests
    {
        private TestItem[] _testItems = GetTestItems();
        private Canvas _canvas = GetCanvas();

        [UnityTest]
        public IEnumerator InventoryInitializationPases()
        {
            var player = SetupPlayer();

            // Skip one frame to load player
            yield return null;

            string msg;
            Assert.IsTrue(player.Inventory.Add(_testItems[0], 1, out msg));
            Assert.AreEqual(player.Inventory.GetTotalAmount(_testItems[0].Id), 1);
            Assert.IsTrue(player.Inventory.Add(_testItems[1], 163, out msg));
            Assert.AreEqual(player.Inventory.GetTotalAmount(_testItems[1].Id), 163);
        }

        [UnityTest]
        public IEnumerator InventoryAddRemoveItems()
        {
            var player = SetupPlayer();

            // Skip one frame to load player
            yield return null;

            string msg;
            Assert.IsTrue(player.Inventory.Add(_testItems[1], 60, out msg));
            Assert.AreEqual(player.Inventory.GetTotalAmount(_testItems[1].Id), 60);

            Assert.IsTrue(player.Inventory.Add(_testItems[1], 4, out msg));
            Assert.AreEqual(player.Inventory.GetTotalAmount(_testItems[1].Id), 64);

            Assert.IsTrue(player.Inventory.Add(_testItems[1], 6, out msg));
            Assert.AreEqual(player.Inventory.GetTotalAmount(_testItems[1].Id), 70);

            player.Inventory.Remove(_testItems[1].Id, 60);
            Assert.AreEqual(player.Inventory.GetTotalAmount(_testItems[1].Id), 10);
        }

        [UnityTest]
        public IEnumerator InventoryIsFull()
        {
            var player = SetupPlayer();

            // Skip one frame to load player
            yield return null;

            string msg;
            for (int i=0; i < player.InventorySize; i++)
            {
                Assert.IsTrue(player.Inventory.Add(_testItems[1], player.MaxStackSize, out msg));
                Assert.AreEqual(player.Inventory.GetTotalAmount(_testItems[1].Id), player.MaxStackSize * (i+1));
            }

            Assert.IsFalse(player.Inventory.Add(_testItems[1], player.MaxStackSize, out msg));
            Assert.AreEqual(msg, "Inventory is full.");
        }

        [UnityTest]
        public IEnumerator FillInventoryRandomItems()
        {
            var player = SetupPlayer();

            // Skip one frame to load player
            yield return null;

            string msg;
            for (int i = 0; i < player.InventorySize; i++)
            {
                Assert.IsTrue(player.Inventory.Add(_testItems[Random.Range(0, 2)], player.MaxStackSize, out msg));
            }

            var total = player.Inventory.GetTotalAmount(_testItems[0].Id) + player.Inventory.GetTotalAmount(_testItems[1].Id);
            Assert.AreEqual(total, (player.InventorySize * player.MaxStackSize));
            Assert.AreEqual(player.Inventory.Count, player.InventorySize);

            player.Inventory.Remove(_testItems[0].Id, player.MaxStackSize);
            Assert.AreEqual(player.Inventory.Count, player.InventorySize - 1);
        }

        [UnityTest]
        public IEnumerator InventoryIsFull2()
        {
            var player = SetupPlayer();

            // Skip one frame to load player
            yield return null;

            string msg;
            for (int i = 0; i < player.InventorySize -1; i++)
            {
                Assert.IsTrue(player.Inventory.Add(_testItems[1], player.MaxStackSize, out msg));
                Assert.AreEqual(player.Inventory.GetTotalAmount(_testItems[1].Id), player.MaxStackSize * (i + 1));
            }

            Assert.IsTrue(player.Inventory.Add(_testItems[1], player.MaxStackSize / 2, out msg));
            Assert.AreEqual(player.Inventory.GetTotalAmount(_testItems[1].Id), (player.MaxStackSize * (player.InventorySize-1)) + (player.MaxStackSize /2));

            Assert.IsFalse(player.Inventory.Add(_testItems[1], player.MaxStackSize, out msg));
            Assert.AreEqual(msg, "Inventory is full, only added " + (player.MaxStackSize / 2) + " amount.");
        }

        [UnityTest]
        public IEnumerator Inventory_ItemCreatedEvent_IsFired()
        {
            var player = SetupPlayer();

            // Skip one frame to load player
            yield return null;

            bool isFired = false;
            IItem eventItem = null;
            int? oldValue = null, newValue = null;
            object inv = null;
            player.Inventory.ItemInserted += (a, b) => 
            {
                isFired = true;
                inv = a;
                eventItem = b.Item;
                oldValue = b.OldAmount;
                newValue = b.NewAmount;
            };

            Assert.IsTrue(player.Inventory.Add(_testItems[0], 1, out string msg));

            Assert.IsTrue(isFired);
            Assert.AreEqual(inv, player.Inventory);
            Assert.AreEqual(eventItem, _testItems[0]);
            Assert.AreEqual(oldValue, 0);
            Assert.AreEqual(newValue, 1);
        }

        [UnityTest]
        public IEnumerator Inventory_ItemUpdatedEvent_IsFired()
        {
            var player = SetupPlayer();

            // Skip one frame to load player
            yield return null;

            bool isFired = false;
            IItem eventItem = null;
            int? oldValue = null, newValue = null;
            object inv = null;
            player.Inventory.ItemUpdated += (a, b) =>
            {
                isFired = true;
                inv = a;
                eventItem = b.Item;
                oldValue = b.OldAmount;
                newValue = b.NewAmount;
            };

            Assert.IsTrue(player.Inventory.Add(_testItems[0], 1, out string msg));
            // Trigger update event
            Assert.IsTrue(player.Inventory.Add(_testItems[0], 1, out msg));

            Assert.IsTrue(isFired);
            Assert.AreEqual(inv, player.Inventory);
            Assert.AreEqual(eventItem, _testItems[0]);
            Assert.AreEqual(oldValue, 1);
            Assert.AreEqual(newValue, 2);

            player.Inventory.Remove(_testItems[0].Id, 1);

            Assert.IsTrue(isFired);
            Assert.AreEqual(oldValue, 2);
            Assert.AreEqual(newValue, 1);
        }

        [UnityTest]
        public IEnumerator Inventory_ItemRemovedEvent_IsFired()
        {
            var player = SetupPlayer();

            // Skip one frame to load player
            yield return null;

            bool isFired = false;
            IItem eventItem = null;
            int? oldValue = null, newValue = null;
            object inv = null;
            player.Inventory.ItemRemoved += (a, b) =>
            {
                isFired = true;
                inv = a;
                eventItem = b.Item;
                oldValue = b.OldAmount;
                newValue = b.NewAmount;
            };

            Assert.IsTrue(player.Inventory.Add(_testItems[0], 1, out string msg));

            // Trigger remove event
            player.Inventory.Remove(_testItems[0].Id, 1);

            Assert.IsTrue(isFired);
            Assert.AreEqual(inv, player.Inventory);
            Assert.AreEqual(eventItem, _testItems[0]);
            Assert.AreEqual(oldValue, 1);
            Assert.AreEqual(newValue, 0);

            Assert.IsTrue(player.Inventory.Add(_testItems[0], 1, out msg));
            player.Inventory.Remove(_testItems[0].Id, 2);

            Assert.IsTrue(isFired);
            Assert.AreEqual(oldValue, 1);
            Assert.AreEqual(newValue, 0);
        }

        private static Canvas GetCanvas()
        {
            return Object.Instantiate(Resources.Load<Canvas>("Canvas"));
        }

        private static TestItem[] GetTestItems()
        {
            return Resources.LoadAll<TestItem>("TestItems");
        }

        private TestPlayer SetupPlayer()
        {
            var test = Object.Instantiate(Resources.Load<GameObject>("Players/Player")).GetComponent<TestPlayer>();
            var renderer = test.GetComponent<InventoryRenderer>();
            renderer.Canvas = _canvas;

            var slots = new List<Image>();
            foreach (Transform t in _canvas.transform.GetChild(0).GetChild(1))
            {
                slots.Add(t.GetComponent<Image>());
            }
            renderer.Slots = slots.ToArray();
            test.InventorySize = 36;
            test.MaxStackSize = 64;
            test.Initialize();
            return test;
        }
    }
}

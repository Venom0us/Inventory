using UnityEngine;
using VenoLib.ItemManagement;

namespace Assets.Tests
{
    public class TestPlayer : MonoBehaviour
    {
        public int InventorySize;
        public int MaxStackSize;
        public Inventory<TestItem> Inventory { get; private set; }

        public void Initialize()
        {
            Inventory = new Inventory<TestItem>(InventorySize, MaxStackSize, GetComponent<InventoryRenderer>());
        }
    }
}
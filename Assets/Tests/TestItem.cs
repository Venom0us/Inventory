using UnityEngine;
using VenoLib.ItemManagement;

namespace Assets.Tests
{
    [CreateAssetMenu(fileName = "TestItem", menuName = "Testing/Item")]
    public class TestItem : ScriptableObject, IItem
    {
        [SerializeField]
        private int _id;
        public int Id { get { return _id; } set { _id = value; } }

        [SerializeField]
        private Texture2D _texture;
        public Texture2D Texture { get { return _texture; } }
    }
}

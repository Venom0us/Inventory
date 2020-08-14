using UnityEngine;

namespace VenoLib.ItemManagement
{
    public interface IItem
    {
        int Id { get; set; }
        Texture2D Texture { get; }
    }
}

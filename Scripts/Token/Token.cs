using UnityEngine;


namespace CultistLike
{
    [CreateAssetMenu(menuName = "Token")]
    public class Token : ScriptableObject
    {
        public string label;
        [TextArea(3, 10)]
        public string description;
        public Sprite art;
        public Color color;

        public Slot slot;
    }
}

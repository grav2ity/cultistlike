using UnityEngine;


namespace CultistLike
{
    [CreateAssetMenu(menuName = "Token")]
    public class Token : ScriptableObject
    {
        [Tooltip("Label to display when no Act is running.")]
        public string label;
        [TextArea(3, 10)]
        [Tooltip("Description to display when no Act is running.")]
        public string description;
        public Sprite art;
        [Tooltip("Solid color used when art is not set.")]
        public Color color;

        [Tooltip("First Slot to open for this Token when no Act is running.")]
        public Slot slot;
        [Tooltip("Destroy Token after completing last Act.")]
        public bool dissolve;
        [Tooltip("Only one Token of this type can be on the table at any given time.")]
        public bool unique;
    }
}

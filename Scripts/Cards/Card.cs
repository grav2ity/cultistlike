using System.Collections.Generic;

using UnityEngine;


namespace CultistLike
{
    [CreateAssetMenu(menuName = "Card")]
    public class Card : ScriptableObject
    {
        public string cardName;
        public Sprite art;
        public Color color;
        [TextArea(3, 10)] public string description;
        public List<Aspect> aspects;
    }
}

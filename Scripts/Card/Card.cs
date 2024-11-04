using System.Collections.Generic;

using UnityEngine;


namespace CultistLike
{
    [CreateAssetMenu(menuName = "Card")]
    public class Card : Fragment
    {
        [Header("Decay")]
        public Card decayTo;
        public float lifetime;

        public override void AddToContainer(FragContainer fg) => fg.Add(this);
        public override int AdjustInContainer(FragContainer fg, int level) => fg.Adjust(this, level);
        public override void RemoveFromContainer(FragContainer fg) => fg.Remove(this);
        public override int CountInContainer(FragContainer fg) => fg.Count(this);
    }
}

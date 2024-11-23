using UnityEngine;


namespace CultistLike
{
    [CreateAssetMenu(menuName = "Card")]
    public class Card : Fragment
    {
        [Header("Decay")]
        [Tooltip("Whenever Card is created it will automatically Decay to / turn into the specified Card.")]
        public Card decayTo;
        [Tooltip("How long will it take for the Decay to complete.")]
        public float lifetime;

        public override void AddToContainer(FragContainer fg) => fg.Add(this);
        public override int AdjustInContainer(FragContainer fg, int level) => fg.Adjust(this, level);
        public override void RemoveFromContainer(FragContainer fg) => fg.Remove(this);
        public override int CountInContainer(FragContainer fg) => fg.Count(this);
    }
}

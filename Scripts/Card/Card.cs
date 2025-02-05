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

        public override void AddToTree(FragTree fg) => fg.Add(this);
        public override int AdjustInTree(FragTree fg, int level) => fg.Adjust(this, level);
        public override void RemoveFromTree(FragTree fg) => fg.Remove(this);
        public override int CountInTree(FragTree fg, bool onlyFree=false) => fg.Count(this, onlyFree);

    }
}

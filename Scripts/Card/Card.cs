﻿using UnityEngine;


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
        [Tooltip("Rule will be run when decay finishes.")]
        public Rule onDecayComplete;
        [Tooltip("Rule will be run when some other Card finishes decaying into this Card.")]
        public Rule onDecayInto;

        [Header("Unique")]
        [Tooltip("Only one instance of this Card can be in a game at any given time.")]
        public bool unique;
        // [Tooltip("Only one Card from this set can be in a game at any given time.")]
        // public Deck uniqueGroup;

        [Header("Memory")]
        public bool memoryFromFirst;

        public override void AddToTree(FragTree fg) => fg.Add(this);
        public override int AdjustInTree(FragTree fg, int level) => fg.Adjust(this, level);
        public override void RemoveFromTree(FragTree fg) => fg.Remove(this);
        public override int CountInTree(FragTree fg, bool onlyFree=false) => fg.Count(this, onlyFree);

        public bool isMutator =>
                label.Length >= 4 &&
                label[0] == '{' && label[1] == '{' &&
                label[label.Length - 1] == '}' && label[label.Length - 2] == '}';
    }
}

using System.Collections.Generic;

using UnityEngine;


namespace CultistLike
{
    [CreateAssetMenu(menuName = "Deck")]
    public class Deck : ScriptableObject
    {
        public string label;
        [Space(10)]
        [TextArea(3, 10)] public string text;
        [Space(10)]

        [Header("Fragments")]
        [Tooltip("Deck content.")]
        public List<Fragment> fragments;
        [Tooltip("Fragment to draw when Deck is empty.")]
        public Fragment defaultFragment;

        [Header("After Draw")]
        [Tooltip("Fragments added to every Fragment drawn from this Deck.")]
        public List<Fragment> tagOn;
        public Fragment memoryFragment;

        [Header("Options")]
        [Tooltip("Randomize Deck order.")]
        public bool shuffle;
        [Tooltip("Replenish fragments on exhaustion.")]
        public bool replenish;
        [Tooltip("Drawing does not remove fragments from the Deck.")]
        public bool infinite;
        public bool random;
        // public bool wrapAround;

        public Fragment Draw() => DeckManager.Instance.GetDeckInst(this).Draw();
        public Fragment DrawOffset(Fragment frag, int di) => DeckManager.Instance.GetDeckInst(this).DrawOffset(frag, di);

        public void Add(Fragment frag) => DeckManager.Instance.GetDeckInst(this).Add(frag);
        public void AddFront(Fragment frag) => DeckManager.Instance.GetDeckInst(this).AddFront(frag);
    }
}

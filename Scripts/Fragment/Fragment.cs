using System;
using System.Collections.Generic;

using UnityEngine;


namespace CultistLike
{
    public class Fragment : ScriptableObject, IFrag
    {
        public string label;
        public Sprite art;
        public Color color;
        [TextArea(3, 10)] public string description;

        [Header("Fragments")]
        public List<Fragment> fragments;

        [Header("Triggers")]
        [Tooltip("Every time an Act completes, Rules will be run if this Fragment is present.")]
        public List<Rule> rules;
        public bool oneForAll;

        [Header("Slots")]
        [Tooltip("Slot that will open inside of an Act if this Fragment is present.")]
        public List<Slot> slots;


        public virtual void AddToContainer(FragContainer fg) {}
        public virtual void RemoveFromContainer(FragContainer fg) {}
        public virtual void AdjustInContainer(FragContainer fg, int level) {}
        public virtual int CountInContainer(FragContainer fg) { return 0; }

        public virtual Fragment ToFragment() => this;
        public virtual int Count() => 1;
    }

    [Serializable]
    public class HeldFragment : IFrag
    {
        public Fragment fragment;
        public int count;

        public CardViz cardViz;


        public HeldFragment(Fragment fragment, int count)
        {
            this.fragment = fragment;
            this.count = count;
        }

        public HeldFragment(CardViz cardViz)
        {
            this.cardViz = cardViz;
        }

        public HeldFragment(Fragment fragment) : this(fragment, 1) {}
        public HeldFragment(HeldFragment fragment) : this(fragment.fragment, fragment.count) {}

        public void AdjustInList(List<HeldFragment> list, int level)
        {
            if (list != null && fragment != null)
            {
                var r = list.Find(x => x.fragment == fragment);

                if (r != null)
                {
                    r.count += level;
                    if (r.count <= 0)
                    {
                        list.Remove(r);
                    }
                }
                else
                {
                    if (level > 0)
                    {
                        list.Add(new HeldFragment(fragment, count));
                    }
                }
            }
        }
        public void AddToList(List<HeldFragment> list) => AdjustInList(list, count);
        public void RemoveFromList(List<HeldFragment> list) => AdjustInList(list, -count);
        public Fragment ToFragment() => fragment;
        public int Count() => count;
    }
}

using System;
using System.Collections.Generic;

using UnityEngine;


namespace CultistLike
{
    public class Fragment : ScriptableObject, IFrag
    {
        [Multiline] public string label;
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
        [Tooltip("Slots that will attempt to open if this Fragment is present.")]
        public List<Slot> slots;


        public virtual void AddToContainer(FragContainer fg) {}
        public virtual void RemoveFromContainer(FragContainer fg) {}
        public virtual int AdjustInContainer(FragContainer fg, int level) { return 0; }
        public virtual int CountInContainer(FragContainer fg) { return 0; }

        public virtual Fragment ToFragment() => this;
        public virtual int Count() => 1;
    }

    [Serializable]
    public class HeldFragment : IFrag
    {
        public Fragment fragment;
        public int count;

        [HideInInspector] public CardViz cardViz;


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

        public static int AdjustInList(List<HeldFragment> list, Fragment fragment, int level)
        {
            if (list != null && fragment != null)
            {
                var r = list.Find(x => x.fragment == fragment);

                if (r != null)
                {
                    int oldC = r.count;
                    r.count += level;
                    if (r.count <= 0)
                    {
                        list.Remove(r);
                    }
                    return Math.Max(0, r.count) - oldC;
                }
                else
                {
                    if (level > 0)
                    {
                        list.Add(new HeldFragment(fragment, level));
                        return level;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            return 0;
        }

        public int AddToList(List<HeldFragment> list) => HeldFragment.AdjustInList(list, fragment, count);
        public int RemoveFromList(List<HeldFragment> list) => HeldFragment.AdjustInList(list, fragment, -count);

        public Fragment ToFragment() => fragment;
        public int Count() => count;
    }
}

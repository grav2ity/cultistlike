using System;
using System.Collections.Generic;

using UnityEngine;


namespace CultistLike
{
    public class Fragment : ScriptableObject, IFrag
    {
        [Multiline] public string label;
        public Sprite art;
        [Tooltip("Solid color used when art is not set.")]
        public Color color;
        [Tooltip("Do not show inside UI fragment bar.")]
        public bool hidden;
        [TextArea(3, 10)] public string description;

        [Header("Fragments")]
        public List<Fragment> fragments;

        [Header("Triggers")]
        [Tooltip("Rules will be run on Act completion if Fragment is present.")]
        public List<Rule> rules;
        // public bool oneForAll;

        [Header("Slots")]
        [Tooltip("Slots that will attempt to open if Fragment is present.")]
        public List<Slot> slots;

        [Header("Deck")]
        public Deck deck;


        public virtual void AddToTree(FragTree fg) {}
        public virtual void RemoveFromTree(FragTree fg) {}
        public virtual int AdjustInTree(FragTree fg, int level) { return 0; }
        public virtual int CountInTree(FragTree fg, bool onlyFree=false) { return 0; }

        public virtual Fragment ToFragment() => this;
        public virtual int Count() => 1;
        public virtual bool Hidden() => hidden;
    }

    [Serializable]
    public class HeldFragment : IFrag
    {
        public Fragment fragment;
        public int count;


        public HeldFragment(Fragment fragment, int count)
        {
            this.fragment = fragment;
            this.count = count;
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
        public bool Hidden() => fragment.hidden;
    }
}

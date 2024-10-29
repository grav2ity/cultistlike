using System;
using System.Collections.Generic;

using UnityEngine;


namespace CultistLike
{
    [CreateAssetMenu(menuName = "Aspect")]
    public class Aspect : Fragment, IFrag
    {
        public override void AddToContainer(FragContainer fg) => fg.Add(this);
        public override void AdjustInContainer(FragContainer fg, int level) => fg.Adjust(this, level);
        public override void RemoveFromContainer(FragContainer fg) => fg.Remove(this);
        public override int CountInContainer(FragContainer fg) => fg.Count(this);

        public void AdjustInList(List<HeldFragment> list, int level)
        {
            if (list != null)
            {
                var r = list.Find(x => x.fragment == this);

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
                        list.Add(new HeldFragment(this, level));
                    }
                }
            }
        }

        public void AddToList(List<HeldFragment> list) => AdjustInList(list, 1);
        public void RemoveFromList(List<HeldFragment> list) => AdjustInList(list, -1);

        public override Fragment ToFragment() => this;
        public override int Count() => 1;
    }
}

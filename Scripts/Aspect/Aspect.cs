using System;
using System.Collections.Generic;

using UnityEngine;


namespace CultistLike
{
    [CreateAssetMenu(menuName = "Aspect")]
    public class Aspect : Fragment
    {
        public override void AddToContainer(FragContainer fg) => fg.Add(this);
        public override void AdjustInContainer(FragContainer fg, int level) => fg.Adjust(this, level);
        public override void RemoveFromContainer(FragContainer fg) => fg.Remove(this);
        public override int CountInContainer(FragContainer fg) => fg.Count(this);

        public void AdjustInList(List<HeldAspect> list, int level)
        {
            if (list != null)
            {
                var r = list.Find(x => x.aspect == this);

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
                        list.Add(new HeldAspect(this, level));
                    }
                }
            }
        }

        public void AddToList(List<HeldAspect> list) => AdjustInList(list, 1);
        public void RemoveFromList(List<HeldAspect> list) => AdjustInList(list, -1);
    }

    [Serializable]
    public class HeldAspect
    {
        public Aspect aspect;
        public int count;


        public HeldAspect(Aspect aspect, int count)
        {
            this.aspect = aspect;
            this.count = count;
        }

        public HeldAspect(Aspect aspect) : this(aspect, 1) {}
        public HeldAspect(HeldAspect aspect) : this(aspect.aspect, aspect.count) {}

        public void AddToList(List<HeldAspect> list) => aspect.AdjustInList(list, count);
        public void RemoveFromList(List<HeldAspect> list) => aspect.AdjustInList(list, -count);
    }
}

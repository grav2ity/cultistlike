using System.Collections.Generic;

using UnityEngine;


namespace CultistLike
{
    [CreateAssetMenu(menuName = "Aspect")]
    public class Aspect : Fragment, IFrag
    {
        public override void AddToTree(FragTree fg) => fg.Add(this);
        public override int AdjustInTree(FragTree fg, int level) => fg.Adjust(this, level);
        public override void RemoveFromTree(FragTree fg) => fg.Remove(this);
        public override int CountInTree(FragTree fg, bool onlyFree=false) => fg.Count(this, onlyFree);

        public int AdjustInList(List<HeldFragment> list, int level) => HeldFragment.AdjustInList(list, this, level);
        public int AddToList(List<HeldFragment> list) => AdjustInList(list, 1);
        public int RemoveFromList(List<HeldFragment> list) => AdjustInList(list, -1);

        public override Fragment ToFragment() => this;
        public override int Count() => 1;
    }
}

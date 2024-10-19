using UnityEngine;


namespace CultistLike
{
    public class Fragment : ScriptableObject
    {
        public string label;
        public Sprite art;
        public Color color;
        [TextArea(3, 10)] public string description;

        public virtual void AddToContainer(FragContainer fg) {}
        public virtual void RemoveFromContainer(FragContainer fg) {}
        public virtual void AdjustInContainer(FragContainer fg, int level) {}
        public virtual int CountInContainer(FragContainer fg) { return 0; }
    }
}

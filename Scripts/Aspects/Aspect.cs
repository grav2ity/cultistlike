using System;

using UnityEngine;


namespace CultistLike
{
    [CreateAssetMenu(menuName = "Aspect")]
    public class Aspect : ScriptableObject, IEquatable<Aspect>
    {
        public string aspectName;
        public Sprite art;
        public Color color;
        [TextArea(3, 10)] public string text;

        public bool Equals(Aspect other)
        {
            if (other is null)
                return false;

            return this.aspectName == other.aspectName;
        }

        public override bool Equals(object obj) => Equals(obj as Aspect);
        public override int GetHashCode() => aspectName.GetHashCode();
    }
}

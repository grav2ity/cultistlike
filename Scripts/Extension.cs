using UnityEngine;


namespace CultistLike
{
    public static class MyExtensions
    {
        public static T GetComponentInNearestParent<T>(this Transform tran,
                                                       bool includeInactive = false) where T : class?
        {
            if (tran == null)
            {
                return null;
            }

            T comp = tran.GetComponent<T>();
            if (comp != null)
            {
                return comp;
            }
            else if (tran.parent != null &&
                     (includeInactive || tran.parent.gameObject.activeInHierarchy == true))
            {
                return GetComponentInNearestParent<T>(tran.parent, includeInactive);
            }
            else
            {
                return null;
            }
        }
    }
}

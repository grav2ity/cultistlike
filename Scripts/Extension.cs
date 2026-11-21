using UnityEngine;
using UnityEditor;


namespace CultistLike
{
    public static class MyExtensions
    {
        public static T GetComponentInNearestParent<T>(this Transform tran,
                                                       bool includeInactive = false) where T : class
        {
            if (tran == null)
            {
                return null;
            }

            if (tran.TryGetComponent<T>(out var comp))
            {
                return comp;
            }
            else if (tran.parent != null &&
                     (includeInactive || tran.parent.gameObject.activeInHierarchy))
            {
                return GetComponentInNearestParent<T>(tran.parent, includeInactive);
            }
            else
            {
                return null;
            }
        }

        public static uint SaveID<T>(this T so) where T : ScriptableObject, ISave
        {
        #if UNITY_EDITOR
            var ID = so.GetID();
            if (ID != 0 && ID <= SaveManager.Instance.idBase.sos.Count)
            {
                if (SaveManager.Instance.idBase.sos[(int)ID - 1] == so)
                {
                    return ID;
                }
            }
            so.SetID(SaveManager.Instance.NewID(so));
            EditorUtility.SetDirty(so);
        #endif
            return so.GetID();
        }

        public static T SavedObject<T>(this uint ID) where T : ScriptableObject
        {
            int id = (int)ID;
            if ( id > 0 && id <= SaveManager.Instance.idBase.sos.Count)
            {
                return SaveManager.Instance.idBase.sos[id - 1] as T;
            }
            else
            {
                return null;
            }
        }
    }
}

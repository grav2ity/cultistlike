using UnityEngine;


namespace CultistLike
{
    /// <summary>
    /// Objects that can be placed on the Table.
    /// </summary>
    public abstract class Viz : Drag
    {
        protected Bounds? bounds;

        /// <summary>
        /// For discrete (e.g. Vector2Int) grid based tables.
        /// Returns extents of the object on the table beyond the center cell.
        /// Final size is (1,1) + 2*(x,y)
        /// i.e. for (1,1) object will have size (3,3)
        /// for (2,0) object will have size (5,1)
        /// </summary>
        /// <returns></returns>
        public abstract Vector2Int GetCellSize();

        /// <summary>
        /// For continuous (e.g. Vector3) tables.
        /// Returns Bounds of the object on the table.
        /// </summary>
        /// <returns></returns>
        public virtual Bounds GetBounds()
        {
            if (bounds.HasValue == false)
            {
                bounds = GetBounds(gameObject);
            }

            return bounds.Value;
        }

        private static Bounds GetBounds(GameObject go)
        {
            var renderer = go.GetComponent<Renderer>();
            var bounds = renderer != null ? renderer.bounds : new Bounds(Vector3.zero, Vector3.zero);

            if (bounds.extents.x == 0 || bounds.extents.y == 0)
            {
                bounds = new Bounds(go.transform.position, Vector3.zero);
                foreach (Transform child in go.transform)
                {
                    renderer = child.GetComponent<Renderer>();
                    if (renderer)
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }
                    else
                    {
                        bounds.Encapsulate(GetBounds(child.gameObject));
                    }
                }
            }
            return bounds;
        }
    }
}

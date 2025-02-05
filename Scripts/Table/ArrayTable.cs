using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using UnityEngine;


namespace CultistLike
{
    public class ArrayTable : Table<Vector2Int>
    {
        [Tooltip("Size of a single grid cell")]
        [SerializeField] private Vector2 cellSize;
        [Tooltip("Cell count")]
        [SerializeField] private Vector2Int cellCount;
        [Tooltip("How much to grow array when no free space is left")]
        [SerializeField] private int growStep;

        [SerializeField, HideInInspector] private SArray<Viz> array;

        private Vector3 gridCorner;

        private Vector2Int[] directions4 = new Vector2Int[]
        {
            Vector2Int.right, Vector2Int.down, Vector2Int.left, Vector2Int.up
        };


        private float planeWidth { get => cellCount.x * cellSize.x; }
        private float planeHeight { get => cellCount.y * cellSize.y; }


        public override Vector3 ToLocalPosition(Vector2Int v) => GridCoordsToLocal(v);
        public override Vector2Int FromLocalPosition(Vector3 v) => LocalToGridCoords(v);

        public override void Remove(Viz viz)
        {
            OnCardUndock(viz.gameObject);
        }

        public override void OnCardUndock(GameObject go)
        {
            base.OnCardUndock(go);

            Vector2Int v;
            if (lastLocations.TryGetValue(go, out v))
            {
                var viz = go.GetComponent<Viz>();
                if (viz == null)
                {
                    return;
                }

                array.ForV(v, viz.GetCellSize(), a => null);
            }
        }

        /// <summary>
        /// Find a free location on or in the vicinity of <c>v</c>.
        /// </summary>
        /// <param name="v">Location around which to begin search.</param>
        /// <param name="viz">Object to be placed.</param>
        /// <returns>True if <c>v</c> is set to a free location.</returns>
        public override bool FindFreeLocation(ref Vector2Int v, Viz viz)
        {
            int phase = 0;
            int steps = 0;
            var origin = v;

            var size = viz.GetCellSize();

            // Searches in an outward spiral-like movement
            while (FitsInLocation(v, size) == false)
            {
                var phase4 = phase % 4;
                var distance = 1 + phase / 4;
                var newV = v + directions4[phase4];

                if (Math.Abs(directions4[phase4].x * (newV.x - origin.x)) <= distance &&
                    Math.Abs(directions4[phase4].y * (newV.y - origin.y)) <= distance )
                    {
                        v = newV;
                        steps++;
                    }
                else
                {
                    phase++;
                }

                if (steps > cellCount.x * cellCount.y)
                {
                    Grow(growStep);
                }
            }

            return true;
        }

        /// <summary>
        /// Find free locations for multiple objects.
        /// First try consecutive locations.
        /// If not enough nonconsecutive free space can be found
        /// resize array and try again.
        /// </summary>
        /// <param name="viz">Object around which to begin search.</param>
        /// <param name="l">Objects to be placed.</param>
        /// <returns></returns>
        public override List<Vector2Int> FindFreeLocations(Viz viz, List<Viz> l)
        {
            var localP = viz.transform.position - transform.position;
            List<Vector2Int> locs = new List<Vector2Int>();
            if (FindFreeLocationsC(LocalToGridCoords(localP), l, ref locs) == true)
            {
                return locs;
            }
            else
            {
                locs.Clear();
                if (FindFreeLocationsNC(LocalToGridCoords(localP), l, ref locs) == true)
                {
                    return locs;
                }
                else
                {
                    Grow(growStep);
                    return FindFreeLocations(viz, l);
                }
            }
        }

        /// <summary>
        /// Place object on the table.
        /// </summary>
        /// <param name="viz">Object to be placed.</param>
        /// <param name="t">Location where the object will be placed.</param>
        /// <param name="moveSpeed"></param>
        /// <returns></returns>
        public override void Place(Viz viz, Vector2Int v, float moveSpeed)
        {
            base.Place(viz, v, moveSpeed);

            array.ForV(v, viz.GetCellSize(), a => viz);
        }

        public override string Save()
        {
            var save = new ArrayTableSave();
            save.fragSave = fragTree.Save();
            save.cellSize = cellSize;
            save.cellCount = cellCount;
            save.growStep = growStep;
            return JsonUtility.ToJson(save);
        }

        public override void Load(string json)
        {
            var save = new ArrayTableSave();
            JsonUtility.FromJsonOverwrite(json, save);
            fragTree.Load(save.fragSave);
            cellSize = save.cellSize;
            cellCount = save.cellCount;
            growStep = save.growStep;
            array = new SArray<Viz>(cellCount.x, cellCount.y);
            Start();
        }

        private Vector2Int NextCell(Vector2Int v, Vector2Int size)
        {
            v.x = v.x + size.x * 2 + 1;
            if (v.x + size.x >= cellCount.x)
            {
                v.x = size.x;
                v.y = v.y - size.y * 2 - 1;
                if (v.y - size.y <= 0)
                {
                    v.y = cellCount.y - size.y;
                }
            }
            return v;
        }

        private bool FindFreeLocationsC(Vector2Int v, List<Viz> l, ref List<Vector2Int> locs)
        {
            bool foundAll;
            int steps = 0;
            do
            {
                foundAll = true;
                for (int i=0; i<l.Count; i++)
                {
                    if (FitsInLocation(v, l[i].GetCellSize()) == true)
                    {
                        locs.Add(v);
                    }
                    else
                    {
                        foundAll = false;
                        locs.Clear();
                    }
                    v = NextCell(v, l[i].GetCellSize());
                    steps++;

                    if (foundAll == false)
                        break;
                }
            }
            while (foundAll == false && steps < cellCount.x * cellCount.y);

            return foundAll;
        }

        private bool FindFreeLocationsNC(Vector2Int v, List<Viz> l, ref List<Vector2Int> locs)
        {
            bool foundOne;
            int steps = 0;
            for (int i=0; i<l.Count; i++)
            {
                foundOne = false;
                while (foundOne == false && steps < cellCount.x * cellCount.y)
                {
                    foundOne = FitsInLocation(v, l[i].GetCellSize());
                    if (foundOne == true)
                    {
                        locs.Add(v);
                    }
                    v = NextCell(v, l[i].GetCellSize());
                    steps++;
                }
            }

            return steps < cellCount.x * cellCount.y;
        }

        private bool FitsInLocation(Vector2Int v, Vector2Int size) =>
            array.AllV(v, size, a => a == null);

        private string PrintArray()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter fout = new StringWriter(sb);

            for (int row = cellCount.y - 1; row >= 0 ; row--)
            {
                for (int col = 0; col < cellCount.x; col++)
                {
                    fout.Write(array[new Vector2Int(col, row)] != null ?"<color=green>X</color>" : '0');
                }
                fout.WriteLine();
            }

            return fout.ToString();
        }

        private void Awake()
        {
            array = new SArray<Viz>(cellCount.x, cellCount.y);
            fragTree = GetComponent<FragTree>();
        }

        private void Start()
        {
            gridCorner = 0.5f * (new Vector3(-planeWidth, -planeHeight, 0f));

            for(int i=0; i<transform.childCount; i++)
            {
                var child = transform.GetChild(i);

                if (child.gameObject.activeInHierarchy == true && child.GetComponent<Viz>() != null)
                {
                    OnCardDock(child.gameObject);
                }

            }
        }

        private void Grow(int i)
        {
            array = array.Grow(i, null);
            cellCount = cellCount + 2 * new Vector2Int(i, i);
            gridCorner = 0.5f * new Vector3(-planeWidth, -planeHeight, 0f);

            List<GameObject> keys = new List<GameObject>(lastLocations.Keys);
            foreach (var go in keys)
            {
                lastLocations[go] = lastLocations[go] + new Vector2Int(i, i);
            }
        }

        private Vector2Int LocalToGridCoords(Vector3 v)
        {
            Vector3 d = v - gridCorner;
            return new Vector2Int((int)Math.Floor(d.x / cellSize.x),
                                (int)Math.Floor(d.y / cellSize.y));
        }

        private Vector3 GridCoordsToLocal(Vector2Int v)
        {
            return ((new Vector3(v.x * cellSize.x, v.y * cellSize.y, 0f)) +
                    0.5f * new Vector3(cellSize.x, cellSize.y, 0f)) + gridCorner;
        }

        [Serializable]
        private class SArray<T>
        {
            [NonSerialized] public T[] array;
            [SerializeField] private int w, h;


            public SArray(int w, int h)
            {
                this.w = w;
                this.h = h;
                array = new T[w * h];
            }

            public SArray(int w, int h, T t)
            {
                this.w = w;
                this.h = h;
                array = new T[w * h];
                for (int i=0; i<array.Length; i++)
                {
                    array[i] = t;
                }
            }

            public T this[int x, int y]
            {
                get { return array[w * y + x]; }
                set { array[w * y + x] = value; }
            }

            public T this[Vector2Int v]
            {
                get { return array[w * v.y + v.x]; }
                set { array[w * v.y + v.x] = value; }
            }

            private bool WithinBounds(Vector2Int v) => v.x >= 0 && v.x < w && v.y >= 0 && v.y < h;

            public SArray<T> Grow(int i, T t)
            {
                int w2 = w + 2*i;
                int h2 = h + 2*i;
                SArray<T> newArray = new SArray<T>(w2, h2, t);

                for (int x=0; x<w; x++)
                {
                    for (int y=0; y<h; y++)
                    {
                        newArray[x+i, y+i] = this[x, y];
                    }
                }
                return newArray;
            }

            public void ForV(Vector2Int v, Vector2Int size, Func<T, T> f)
            {
                for (int x=-size.x;  x<=size.x; x++)
                {
                    for (int y=-size.y; y<=size.y; y++)
                    {
                        if (WithinBounds(v + new Vector2Int(x, y)) == true)
                            this[v + new Vector2Int(x, y)] = f(this[v + new Vector2Int(x, y)]);
                    }
                }
            }

            public bool AllV(Vector2Int v, Vector2Int size, Func<T, bool> f)
            {
                for (int x=-size.x;  x<=size.x; x++)
                {
                    for (int y=-size.y; y<=size.y; y++)
                    {
                        if (WithinBounds(v + new Vector2Int(x, y)) == false ||
                            f(this[v + new Vector2Int(x, y)]) == false)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            public void MaxV<U>(Vector2Int v, Vector2Int size, ref U u, Func<T, U> f)
                where U : IComparable<U>
            {
                U newU;
                for (int x=-size.x;  x<=size.x; x++)
                {
                    for (int y=-size.y; y<=size.y; y++)
                    {
                        if (WithinBounds(v + new Vector2Int(x, y)) == true)
                        {
                            newU = f(this[v + new Vector2Int(x, y)]);
                            if (newU.CompareTo(u) >= 0)
                            {
                                u = newU;
                            }
                        }
                    }
                }
            }
        }
    }

    public class ArrayTableSave
    {
        public FragTreeSave fragSave;
        public Vector2 cellSize;
        public Vector2Int cellCount;
        public int growStep;
    }
}

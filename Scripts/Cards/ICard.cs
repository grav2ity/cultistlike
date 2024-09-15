using UnityEngine;


namespace CultistLike
{
    public interface ICardDock
    {
        void OnCardDock(GameObject go);
        void OnCardUndock(GameObject go);
    }
}

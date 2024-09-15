using UnityEngine;


namespace CultistLike
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        public CardInfo cardInfo;


        private void Awake()
        {
            Instance = this;
        }
    }
}

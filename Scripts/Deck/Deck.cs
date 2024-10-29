using System.Collections.Generic;

using UnityEngine;


namespace CultistLike
{
    [CreateAssetMenu(menuName = "Deck")]
    public class Deck : ScriptableObject
    {
        public string label;
        [Space(10)]
        [TextArea(3, 10)] public string text;
        [Space(10)]
        public List<Fragment> fragments;
        public bool shuffle;
        public bool infinite;

        public Fragment Draw() => DeckManager.Instance.GetDeckInst(this).Draw();
        public Fragment DrawOffset(Fragment frag, int di) => DeckManager.Instance.GetDeckInst(this).DrawOffset(frag, di);
    }
}

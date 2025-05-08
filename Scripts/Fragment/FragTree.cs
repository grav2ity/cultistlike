using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEngine;


namespace CultistLike
{
    [Serializable]
    public class FragTree : MonoBehaviour
    {
        public List<HeldFragment> localFragments;
        public CardViz localCard;
        public List<CardViz> matches;

        public Fragment memoryFragment;

        public bool free;

        public Action<CardViz> onCreateCard;
        public Action ChangeEvent;


        public List<CardViz> cards
        {
            get
            {
                var list = new List<CardViz>(GetComponentsInChildren<CardViz>(true));
                if (localCard != null)
                {
                    list.Add(localCard);
                }
                return list;
            }
        }

        public List<CardViz> freeCards
        {
            get => cards.FindAll(x => x.free == true);
        }

        public List<HeldFragment> fragments
        {
            get => GetFragments(false);
        }

        public List<HeldFragment> freeFragments
        {
            get => GetFragments(true);
        }


        public List<HeldFragment> GetFragments(bool onlyFree)
        {
            List<HeldFragment> list = new List<HeldFragment>();
            var results = GetComponentsInChildren<FragTree>(true);
            foreach (var fragTree in results)
            {
                if (fragTree.enabled == true)
                {
                    if (onlyFree == false || fragTree.free == true)
                    {
                        foreach (var fragment in fragTree.localFragments)
                        {
                            HeldFragment.AdjustInList(list, fragment.fragment, fragment.count);
                        }
                    }
                }
            }
            return list;
        }

        public void Clear()
        {
            matches.Clear();
            localFragments.Clear();
        }

        public void Add(Fragment frag) => frag.AddToTree(this);
        public void Remove(Fragment frag) => frag.RemoveFromTree(this);
        public int Adjust(Fragment frag, int level) => frag.AdjustInTree(this, level);
        public int Count(Fragment frag, bool onlyFree=false) => frag.CountInTree(this, onlyFree);

        public void Add(Aspect aspect) => Adjust(aspect, 1);
        public void Remove(Aspect aspect) => Adjust(aspect, -1);
        public int Adjust(Aspect aspect, int level)
        {
            if (aspect != null)
            {
                int count = aspect.AdjustInList(localFragments, level);
                OnChange();
                return count;
            }
            else
            {
                return 0;
            }
        }


        public void Add(HeldFragment frag) => Adjust(frag.fragment, frag.count);
        public void Remove(HeldFragment frag) => Adjust(frag.fragment, -frag.count);

        public void Add(MonoBehaviour mono)
        {
            mono?.transform.SetParent(transform);
            OnChange();
        }

        public void Add(Viz viz)
        {
            viz?.Parent(transform);
        }

        public CardViz Add(Card card)
        {
            if (card != null)
            {
                var cardViz = GameManager.Instance.CreateCard(card);
                Add(cardViz);
                if (onCreateCard != null && cardViz != null)
                {
                    onCreateCard(cardViz);
                }
                return cardViz;
            }
            else
            {
                return null;
            }
        }

        public CardViz Remove(CardViz cardViz)
        {
            if (cardViz != null && cardViz.transform.IsChildOf(transform))
            {
                cardViz.transform.SetParent(null);
                matches.Remove(cardViz);
                OnChange();
                return cardViz;
            }
            else
            {
                return null;
            }
        }

        public CardViz Remove(Card card)
        {
            if (card != null)
            {
                for (int i=0; i<transform.childCount; i++)
                {
                    var cardViz = transform.GetChild(i).gameObject.GetComponent<CardViz>();
                    if (cardViz != null && cardViz.card == card)
                    {
                        return Remove(cardViz);
                    }
                }
            }
            return null;
        }

        public int Adjust(CardViz cardViz, int level)
        {
            if (cardViz != null)
            {
                if (level > 0)
                {
                    int count = 0;
                    for (int i=0; i<level; i++)
                    {
                        var newCardViz = cardViz.Duplicate();
                        if (newCardViz != null)
                        {
                            Add(newCardViz);
                            if (onCreateCard != null)
                            {
                                onCreateCard(newCardViz);
                            }
                            count++;
                        }
                    }
                    OnChange();
                    return count;
                }
                else if (level < 0)
                {
                    if (Remove(cardViz) != null)
                    {
                        OnChange();
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            return 0;
        }


        public int Adjust(Card card, int level)
        {
            if (card != null)
            {
                if (level > 0)
                {
                    int count = 0;
                    for (int i=0; i<level; i++)
                    {
                        if (Add(card) != null)
                        {
                            count++;
                        }
                    }
                    return count;
                }
                else if (level < 0)
                {
                    int count = 0;
                    while (level++ < 0)
                    {
                        if (Remove(card) != null)
                        {
                            count--;
                        }
                        else
                        {
                            break;
                        }
                    }
                    return count;
                }
            }
            return 0;
        }

        public int Adjust(Target target, int level)
        {
            if (target != null)
            {
                if (target.cards != null)
                {
                    foreach (var cardViz in target.cards)
                    {
                        Adjust(cardViz, level);
                    }
                }
                else if (target.fragment != null)
                {
                    return Adjust(target.fragment, level);
                }
            }
            return 0;
        }

        public HeldFragment Find(Aspect aspect) => fragments.Find(x => x.fragment == aspect);

        public List<CardViz> FindAll(Card card) => cards.FindAll(x => x.card == card);
        public List<CardViz> FindAll(Aspect aspect) =>
            cards.FindAll(x => x.fragTree.Count(aspect) > 0);

        public int Count(Aspect aspect, bool onlyFree=false)
        {
            var frags = onlyFree ? freeFragments : fragments;
            var hFrag = frags.Find(x => x.fragment == aspect);
            if (hFrag != null)
            {
                return hFrag.count;
            }
            else
            {
                return 0;
            }
        }
        public int Count(Card card, bool onlyFree=false) =>
            onlyFree ? freeCards.Count(x => x.card == card) : cards.Count(x => x.card == card);

        public int Count(HeldFragment frag, bool onlyFree=false) => frag != null ? Count(frag.fragment, onlyFree) : 0;

        public int Count(string fileName, bool onlyFree=false)
        {
            var frag = fragments.Find(x => x.fragment.name == fileName);
            if (frag != null)
            {
                return Count(frag, onlyFree);
            }

            var cardViz = cards.Find(x => x.card.name == fileName);
            if (cardViz != null)
            {
                return Count(cardViz.card, onlyFree);
            }

            return 0;
        }

        public FragTreeSave Save()
        {
            var save = new FragTreeSave();

            save.matches = new List<int>();
            foreach (var cardViz in matches)
            {
                save.matches.Add(cardViz.GetInstanceID());
            }

            save.localFragments = localFragments;
            save.free = free;

            return save;
        }

        public void Load(FragTreeSave save)
        {
            matches.Clear();

            foreach (var cardID in save.matches)
            {
                matches.Add(SaveManager.Instance.CardFromID(cardID));
            }

            localFragments = save.localFragments;
            free = save.free;
        }

        public void OnChange()
        {
            if (ChangeEvent != null)
            {
                ChangeEvent();
            }
            if (transform.parent != null)
            {
                transform.parent.GetComponentInParent<FragTree>()?.OnChange();
            }
        }

        public string InterpolateString(string source)
        {
            string s = Regex.Replace(source, @"\{\[(.*?)\s(==|!=|>=|<=|>|<)\s*(\d+)\s*\]\s?([\s\S]*?)\}\n?", match =>
            {
                string fragName = match.Groups[1].Value;
                string op = match.Groups[2].Value;
                string valString = match.Groups[3].Value;

                bool passed = false;

                try
                {
                    int right = Int32.Parse(valString);
                    int left = Count(fragName);

                    switch (op)
                    {
                        case "==":
                            passed = left == right;
                            break;
                        case "!=":
                            passed = left != right;
                            break;
                        case ">=":
                            passed = left >= right;
                            break;
                        case "<=":
                            passed = left <= right;
                            break;
                        case ">":
                            passed = left > right;
                            break;
                        case "<":
                            passed = left < right;
                            break;
                        default:
                            break;
                    }
                }
                catch (OverflowException)
                {
                    Debug.LogWarning("Too many digits inside {[fragment] op numeral} expression.");
                }

                if (passed == true)
                {
                    return match.Groups[4].Value;
                }
                else
                {
                    return "";
                }
            });

            return Regex.Replace(s, @"\[(.*?)\]", match =>
            {
                string key = match.Groups[1].Value;

                if (key == "MC")
                {
                    return matches.Count > 0 ? matches[0].card.label : "";
                }
                else
                {
                    return Count(key).ToString();
                }
            });
        }
    }



    [Serializable]
    public class FragTreeSave
    {
        public List<HeldFragment> localFragments;
        public List<int> matches;
        public bool free;
    }
}

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;


namespace CultistLike
{
    public class ActWindow : Drag
    {
        [Header("Layout")]
        [SerializeField] private GameObject visualsGO;
        [SerializeField] private TextMeshPro label;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private FragTree slotsFrag;
        [SerializeField] private GameObject idleSlotsGO;
        [SerializeField] private GameObject runSlotsGO;
        [SerializeField] private CardLane resultLane;
        [SerializeField] private FragmentBar aspectBar;
        [SerializeField] private FragmentBar cardBar;
        [SerializeField] private Timer _timer;
        [SerializeField] private Button okButton;
        [SerializeField] private Button collectButton;

        [Header("Slots")]
        [SerializeField] private List<SlotViz> idleSlots;
        [SerializeField] private List<SlotViz> runSlots;

        [SerializeField, HideInInspector] private bool _open;
        [SerializeField, HideInInspector] private ActStatus actStatus;
        [SerializeField, HideInInspector] private Act readyAct;

        private ActLogic actLogic;

        private bool suspendUpdates;
        private bool pendingUpdate;


        public bool open { get => _open; private set => _open = value; }
        public TokenViz tokenViz { get; private set; }

        public Timer timer => _timer;

        public FragTree slotsFragTree => slotsFrag;

        public List<CardViz> cards => actLogic.fragTree.cards;

        private List<SlotViz> slots => actStatus == ActStatus.Running ? runSlots : idleSlots;


        public bool TrySlotAndBringUp(CardViz cardViz)
        {
            if (actStatus != ActStatus.Finished)
            {
                var slotViz = AcceptsCard(cardViz);
                if (slotViz != null && slotViz.TrySlotCard(cardViz))
                {
                    BringUp();
                    return true;
                }
            }
            return false;
        }

        public SlotViz AcceptsCard(CardViz cardViz, bool onlyEmpty = false)
        {
            foreach (var slotViz in MatchingSlots(cardViz, onlyEmpty))
            {
                if (slotViz != null)
                {
                    return slotViz;
                }
            }
            return null;
        }

        public void BringUp()
        {
            Show();
            open = true;
            GameManager.Instance.OpenWindow(this);
        }

        public void Close()
        {
            switch (actStatus)
            {
                case ActStatus.Idle:
                case ActStatus.Ready:
                    ReturnCardsToTable();
                    StatusIdle();
                    break;
            }

            Hide();
            open = false;
            GameManager.Instance.CloseWindow(this);
        }

        public void GoForIt()
        {
            actLogic.RunAct(readyAct);
        }

        public void FirstSlotEmpty()
        {
            if (suspendUpdates == false)
            {
                //TODO ?? not when running
                suspendUpdates = true;
                ReturnCardsToTable();
                suspendUpdates = false;
                if (actStatus != ActStatus.Running)
                {
                    StatusIdle();
                }
            }
        }

        public void SetFragMemory(CardViz cardViz)
        {
            if (cardViz != null)
            {
                actLogic.fragTree.memoryFragment = cardViz.fragTree.memoryFragment;
            }
        }

        public void SetFragMemory(Fragment frag)
        {
            actLogic.fragTree.memoryFragment = frag;
        }

        public void ParentSlotCardsToWindow()
        {
            foreach (var slot in slots)
            {
                slot.ParentCardToWindow();
            }
        }

        public void HighlightSlots(CardViz cardViz, bool p = true)
        {
            foreach (var slotViz in MatchingSlots(cardViz))
            {
                if (slotViz.slottedCard == null || slotViz.cardLock == false)
                {
                    slotViz.SetHighlight(p);
                }
            }
        }

        public void UnhighlightSlots()
        {
            foreach (var slotViz in slots)
            {
                if (slotViz.gameObject.activeSelf)
                {
                    slotViz.SetHighlight(false);
                }
            }
        }

        // closing a slot will remove a slotted card
        // since slotted cards influence which slots are open
        // reUpdate needs to be done
        public void UpdateSlots()
        {
            if (suspendUpdates == false && actStatus != ActStatus.Finished)
            {
                suspendUpdates = true;

                var slotsToOpen = actLogic.CheckForSlots();
                var slotsToRefresh = new List<SlotViz>();

                bool reUpdate = false;
                foreach (var slotViz in slots)
                {
                    if (slotViz.open)
                    {
                        var foundSlot = slotsToOpen.Find(x => x == slotViz.slot);
                        if (foundSlot == null)
                        {
                            var cardViz = slotViz.UnslotCard();
                            slotViz.CloseSlot();
                            if (cardViz != null)
                            {
                                reUpdate = true;
                                GameManager.Instance.table.ReturnToTable(cardViz);
                            }
                        }
                        else
                        {
                            slotsToOpen.Remove(foundSlot);
                            slotsToRefresh.Add(slotViz);
                        }
                    }
                }

                if (reUpdate)
                {
                    suspendUpdates = false;
                    UpdateSlots();
                }
                else
                {
                    foreach (var slotViz in slotsToRefresh)
                    {
                        slotViz.Refresh();
                    }
                    foreach (var slot in slotsToOpen)
                    {
                        OpenSlot(slot, slots);
                    }

                    suspendUpdates = false;
                }
            }
        }

        private void CloseSlots(List<SlotViz> slots)
        {
            foreach (var slotViz in slots)
            {
                slotViz.CloseSlot();
            }
        }

        private void OpenSlot(Slot slot, List<SlotViz> slots)
        {
            foreach (var slotViz in slots)
            {
                if (slotViz.gameObject.activeSelf == false)
                {
                    slotViz.LoadSlot(slot);
                    slotViz.OpenSlot();
                    break;
                }
            }
        }

        public void SetupResultCards(List<CardViz> cards)
        {
            resultLane.ParentCards(cards);
            tokenViz.SetResultCount(cards.Count);
        }

        /// <summary>
        /// Check for new Status after card slot/unslot.
        /// </summary>
        public void Check()
        {
            switch (actStatus)
            {
                case ActStatus.Idle:
                case ActStatus.Ready:
                    AttemptReadyAct();
                    break;
                case ActStatus.Running:
                    actLogic.AttemptAltActs();
                    ApplyStatus(ActStatus.Running);
                    break;
                case ActStatus.Finished:
                    var count = resultLane.Count;
                    if (count == 0)
                    {
                        StatusIdle();
                        if (tokenViz.token.dissolve)
                        {
                            tokenViz.Dissolve();
                            Close();
                            Destroy(gameObject, 0.1f);
                        }
                    }
                    else
                    {
                        tokenViz.SetResultCount(count);
                    }
                    break;
            }
        }

        public void CollectAll()
        {
            List<Viz> l = new List<Viz>();
            foreach (var cardViz in resultLane.cards)
            {
                cardViz.ShowFace();
                if (GameManager.Instance.table.LastLocation(cardViz))
                {
                    GameManager.Instance.table.ReturnToTable(cardViz);
                }
                else
                {
                    l.Add(cardViz);
                }
            }

            GameManager.Instance.table.Place(tokenViz, l);
            resultLane.cards.Clear();

            //need this to force window update when there are no cards to claim
            Check();
        }

        public void LoadToken(TokenViz tokenViz)
        {
            if (tokenViz != null)
            {
                this.tokenViz = tokenViz;
                SetFragMemory(tokenViz.memoryFragment);

                gameObject.name = "[WINDOW] " + tokenViz.token.name;
            }
        }

        public void ApplyStatus(ActStatus actStatus)
        {
            this.actStatus = actStatus;
            switch (actStatus)
            {
                case ActStatus.Idle:
                    idleSlotsGO.SetActive(true);
                    runSlotsGO.SetActive(false);
                    resultLane.gameObject.SetActive(false);
                    okButton.interactable = false;
                    collectButton.gameObject.SetActive(false);
                    tokenViz.SetResultCount(0);
                    if (tokenViz != null)
                    {
                        label.text = tokenViz?.token?.label;
                        text.text = actLogic.TokenDescription();
                    }
                    cardBar?.gameObject.SetActive(true);
                    break;
                case ActStatus.Ready:
                    if (readyAct != null)
                    {
                        okButton.interactable = true;
                        label.text = readyAct.label;
                        text.text = actLogic.GetText(readyAct);
                        tokenViz.SetResultCount(0);
                    }
                    collectButton.gameObject.SetActive(false);
                    cardBar?.gameObject.SetActive(true);
                    break;
                case ActStatus.Running:
                    timer.gameObject.SetActive(true);
                    idleSlotsGO.SetActive(false);
                    runSlotsGO.SetActive(true);
                    okButton.interactable = false;
                    collectButton.gameObject.SetActive(false);
                    tokenViz.SetResultCount(0);
                    label.text = actLogic.label;
                    text.text = actLogic.runText;
                    cardBar?.gameObject.SetActive(true);
                    break;
                case ActStatus.Finished:
                    timer.gameObject.SetActive(false);
                    runSlotsGO.SetActive(false);
                    resultLane.gameObject.SetActive(true);
                    collectButton.interactable = true;
                    collectButton.gameObject.SetActive(true);
                    label.text = actLogic.label;
                    text.text = actLogic.endText;
                    cardBar?.gameObject.SetActive(false);
                    Check();
                    break;
            }
        }

        public void Hide()
        {
            visualsGO.SetActive(false);
            foreach (var slot in idleSlots)
            {
                slot.Hide();
            }
            foreach (var slot in runSlots)
            {
                slot.Hide();
            }
            resultLane.Hide();
        }
        public void Show()
        {
            visualsGO.SetActive(true);
            foreach (var slot in idleSlots)
            {
                slot.Show();
            }
            foreach (var slot in runSlots)
            {
                slot.Show();
            }
            resultLane.Show();
        }

        public void AddFragment(Fragment frag)
        {
            actLogic.fragTree.Add(frag);
        }

        public void RemoveFragment(Fragment frag)
        {
            actLogic.fragTree.Remove(frag);
        }

        public ActWindowSave Save()
        {
            var save = new ActWindowSave
            {
                open = open,
                actStatus = actStatus,
                readyAct = readyAct,
                position = transform.position,
                localCards = new List<int>()
            };

            for (int i=0; i<transform.childCount; i++)
            {
                var cardViz = transform.GetChild(i).gameObject.GetComponent<CardViz>();
                if (cardViz != null)
                {
                    save.localCards.Add(cardViz.GetInstanceID());
                }
            }

            save.cardLane = resultLane.Save();
            save.slots = new List<SlotVizSave>();
            foreach (var slotViz in slots)
            {
                if (slotViz.gameObject.activeSelf)
                {
                    save.slots.Add(slotViz.Save());
                }
            }

            return save;
        }

        public void Load(ActWindowSave save, TokenViz tokenViz)
        {
            transform.position = save.position;
            actStatus = save.actStatus;
            readyAct = save.readyAct;
            open = save.open;
            LoadToken(tokenViz);

            foreach (var cardID in save.localCards)
            {
                SaveManager.Instance.CardFromID(cardID).ParentTo(transform, true);
            }

            resultLane.Load(save.cardLane);

            var count = actLogic.fragTree.cards.Count;
            tokenViz.SetResultCount(count);

            for (int i=0; i<save.slots.Count && i<slots.Count; i++)
            {
                slots[i].Load(save.slots[i]);
            }

            if (save.open)
            {
                BringUp();
            }
        }

        private void AttemptReadyAct()
        {
            if (tokenViz.token != null)
            {
                readyAct = actLogic.AttemptInitialActs();

                //TODO ?? prevents ready status for acts with no tests when no card is slotted
                if (readyAct != null && idleSlots.Count > 0 && idleSlots[0].slottedCard != null)
                {
                    ApplyStatus(ActStatus.Ready);
                }
                else
                {
                    ApplyStatus(ActStatus.Idle);
                }
            }
        }

        private void StatusIdle()
        {
            actLogic.Reset();
            ApplyStatus(ActStatus.Idle);
            UpdateSlots();
        }

        private void ReturnCardsToTable()
        {
            foreach (var cardSlot in idleSlots)
            {
                if (cardSlot.slottedCard != null)
                {
                    var cardViz = cardSlot.UnslotCard();
                    GameManager.Instance.table.ReturnToTable(cardViz);
                }
            }
        }

        //TODO accessing destroyed?
        private IEnumerable<SlotViz> MatchingSlots(CardViz cardViz, bool onlyEmpty = false)
        {
            if (cardViz != null)
            {
                if (actStatus != ActStatus.Finished || resultLane.cards.Count == 0)
                {
                    foreach (var slotViz in slots)
                    {
                        if (slotViz != null && slotViz.gameObject.activeSelf && slotViz.AcceptsCard(cardViz))
                        {
                            if (onlyEmpty == false || slotViz.slottedCard == null)
                            {
                                yield return slotViz;
                            }
                        }
                    }
                }
            }
            else
            {
                yield return null;
            }
        }

        private void Awake()
        {
            actLogic = GetComponent<ActLogic>();

            var fragTree = GetComponent<FragTree>();
            fragTree.ChangeEvent += () =>
            {
                pendingUpdate = true;
            };
        }

        private void Start()
        {
            GetComponent<Drag>().draggingPlane = GameManager.Instance.windowPlane;

            timer.gameObject.SetActive(false);
            okButton.interactable = false;
            collectButton.interactable = false;

            if (open == false)
            {
                Hide();
            }

            CloseSlots(idleSlots);
            CloseSlots(runSlots);

            foreach(var c in gameObject.GetComponentsInChildren<Canvas>())
            {
                c.worldCamera = Camera.main;
            }

            if (tokenViz.autoPlay != null)
            {
                readyAct = tokenViz.autoPlay;
                actLogic.ForceAct(readyAct);
            }
            else
            {
                UpdateSlots();
                ApplyStatus(actStatus);
            }
        }

        private void Update()
        {
            if (pendingUpdate)
            {
                UpdateSlots();
                Check();

                pendingUpdate = false;
            }
        }
    }

    [Serializable]
    public class ActWindowSave
    {
        public bool open;
        public ActStatus actStatus;
        public Act readyAct;
        public List<int> localCards;
        public CardLaneSave cardLane;
        public List<SlotVizSave> slots;
        public Vector3 position;
    }

    public enum ActStatus
    {
            Idle,
            Ready,
            Running,
            Finished
    }
}

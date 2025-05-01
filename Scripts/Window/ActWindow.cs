using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;


namespace CultistLike
{
    public class ActWindow : Drag
    {
        [HideInInspector] public bool open;

        [Header("Layout")]
        [SerializeField] private GameObject visualsGO;
        [SerializeField] private TextMeshPro label;
        [SerializeField] private TextMeshProUGUI text;
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

        public ActStatus actStatus;
        [SerializeField, HideInInspector] private Act readyAct;

        private TokenViz _tokenViz;
        private ActLogic actLogic;

        private bool suspendUpdates;
        private bool pendingUpdate;


        public TokenViz tokenViz { get => _tokenViz; private set => _tokenViz = value; }
        public Timer timer { get => _timer; }

        private List<SlotViz> slots => actStatus == ActStatus.Running ? runSlots : idleSlots;
        private string runLabel => actLogic.altAct ? actLogic.altAct.label : actLogic.activeAct.label;


        public bool TrySlotAndBringUp(CardViz cardViz)
        {
            if (actStatus != ActStatus.Finished)
            {
                var slotViz = AcceptsCard(cardViz);
                if (slotViz != null && slotViz.TrySlotCard(cardViz) == true)
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
                default:
                    break;
            }

            Hide();
            open = false;
            GameManager.Instance.CloseWindow();
        }

        public void GoForIt()
        {
            actLogic.RunAct(readyAct);
        }

        public void FirstSlotEmpty()
        {
            if (suspendUpdates == false)
            {
                suspendUpdates = true;
                ReturnCardsToTable();
                suspendUpdates = false;
                if (actStatus != ActStatus.Running)
                {
                    StatusIdle();
                }
            }
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

                CloseSlots(slots);

                foreach (var slot in slotsToOpen)
                {
                    OpenSlot(slot, slots);
                }

                bool reUpdate = false;
                foreach (var slot in slots)
                {
                    if (slot.gameObject.activeSelf == false)
                    {
                        var cardViz = slot.UnslotCard();
                        if (cardViz != null)
                        {
                            reUpdate = true;
                            GameManager.Instance.table.ReturnToTable(cardViz);
                        }
                    }
                }

                suspendUpdates = false;
                if (reUpdate == true)
                {
                    UpdateSlots();
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
            resultLane.PlaceCards(cards);
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
                    actLogic.altAct = actLogic.AttemptAltActs();
                    ApplyStatus(ActStatus.Running);
                    break;
                case ActStatus.Finished:
                    var count = actLogic.fragTree.cards.Count;
                    if (count == 0)
                    {
                        StatusIdle();
                        if (tokenViz.token.dissolve == true)
                        {
                            tokenViz.Dissolve();
                            Close();
                            Destroy(this, 0.1f);
                        }
                    }
                    else
                    {
                        tokenViz.SetResultCount(count);
                    }
                    break;
                default:
                    break;
            }
        }

        public void CollectAll()
        {
            List<Viz> l = new List<Viz>();
            foreach (var cardViz in resultLane.cards)
            {
                cardViz.ShowFace();
                if (GameManager.Instance.table.LastLocation(cardViz) == true)
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
                    collectButton.interactable = false;
                    tokenViz.SetResultCount(0);
                    if (tokenViz != null)
                    {
                        label.text = tokenViz?.token?.label;
                        text.text = tokenViz?.token?.description;
                    }
                    break;
                case ActStatus.Ready:
                    if (readyAct != null)
                    {
                        okButton.interactable = true;
                        label.text = readyAct.label;
                        text.text = actLogic.fragTree.InterpolateString(readyAct.text);
                        tokenViz.SetResultCount(0);
                    }
                    break;
                case ActStatus.Running:
                    timer.gameObject.SetActive(true);
                    idleSlotsGO.SetActive(false);
                    runSlotsGO.SetActive(true);
                    okButton.interactable = false;
                    tokenViz.SetResultCount(0);
                    label.text = runLabel;
                    text.text = actLogic.runText;
                    break;
                case ActStatus.Finished:
                    timer.gameObject.SetActive(false);
                    runSlotsGO.SetActive(false);
                    resultLane.gameObject.SetActive(true);
                    collectButton.interactable = true;
                    label.text = runLabel;
                    text.text = actLogic.endText;
                    break;
                default:
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
            var save = new ActWindowSave();
            save.open = open;
            save.actStatus = actStatus;
            save.readyAct = readyAct;
            save.position = transform.position;

            save.localCards = new List<int>();
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
                if (slotViz.gameObject.activeSelf == true)
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
                SaveManager.Instance.CardFromID(cardID).ParentToWindow(transform, true);
            }

            resultLane.Load(save.cardLane);

            var count = actLogic.fragTree.cards.Count;
            tokenViz.SetResultCount(count);

            for (int i=0; i<save.slots.Count && i<slots.Count; i++)
            {
                slots[i].Load(save.slots[i]);
            }

            if (save.open == true)
            {
                BringUp();
            }
        }

        private void AttemptReadyAct()
        {
            if (tokenViz.token != null)
            {
                readyAct = actLogic.AttemptInitialActs();

                if (readyAct != null)
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

        private IEnumerable<SlotViz> MatchingSlots(CardViz cardViz, bool onlyEmpty = false)
        {
            if (cardViz != null)
            {
                if (actStatus != ActStatus.Finished)
                {
                    foreach (var slotViz in slots)
                    {
                        if (slotViz.gameObject.activeSelf == true && slotViz.AcceptsCard(cardViz) == true)
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
            if (pendingUpdate == true)
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

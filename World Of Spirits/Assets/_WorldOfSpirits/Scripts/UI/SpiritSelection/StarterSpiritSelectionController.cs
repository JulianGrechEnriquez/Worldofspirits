using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using WorldOfSpirits.Progression;
using WorldOfSpirits.Progression.Upgrades;
using WorldOfSpirits.Spirits;

namespace WorldOfSpirits.UI
{
    public sealed class StarterSpiritSelectionController : MonoBehaviour
    {
        [SerializeField] private StarterSpiritCardView cardPrefab;
        [SerializeField] private Transform cardContainer;
        [SerializeField] private List<GameObject> availableSpiritPrefabs = new List<GameObject>();
        [SerializeField] private GameObject defaultStarterSpirit;
        [SerializeField] private CanvasGroup[] gameplayHudGroups;
        [SerializeField] private TMP_Text heading;
        [SerializeField] private TMP_Text instruction;
        private readonly List<Vector3> previousHudStates = new List<Vector3>();
        private readonly List<StarterSpiritCardView> cards = new List<StarterSpiritCardView>();

        private SpiritManager spiritManager;
        private float previousTimeScale = 1f;
        private bool selectionOpen;
        public int RequiredStarterCount { get; private set; } = 1;

        private void Start()
        {
            spiritManager = FindFirstObjectByType<SpiritManager>();
            UpgradeRuntimeStats character = spiritManager != null
                ? spiritManager.GetComponent<UpgradeRuntimeStats>() : null;
            RequiredStarterCount = character != null && character.Character == UpgradeCharacter.SpiritShepherd ? 2 : 1;
            EnsureDefaultUnlocked();
            if (spiritManager == null || spiritManager.SpiritCount >= RequiredStarterCount)
            {
                gameObject.SetActive(false);
                return;
            }

            BuildUnlockedCards();
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            selectionOpen = true;
            SetGameplayHudHidden(true);
            RefreshSelection();
        }

        private void OnDisable()
        {
            SetGameplayHudHidden(false);
            if (!selectionOpen) return;
            Time.timeScale = previousTimeScale;
            selectionOpen = false;
        }

        private void SetGameplayHudHidden(bool hidden)
        {
            if (gameplayHudGroups == null) return;
            if (hidden) previousHudStates.Clear();
            for (int i = 0; i < gameplayHudGroups.Length; i++)
            {
                CanvasGroup group = gameplayHudGroups[i];
                if (hidden)
                {
                    previousHudStates.Add(group != null ? new Vector3(group.alpha, group.interactable ? 1f : 0f, group.blocksRaycasts ? 1f : 0f) : Vector3.zero);
                    if (group == null) continue;
                    group.alpha = 0f;
                    group.interactable = false;
                    group.blocksRaycasts = false;
                }
                else if (group != null && i < previousHudStates.Count)
                {
                    group.alpha = previousHudStates[i].x;
                    group.interactable = previousHudStates[i].y > 0f;
                    group.blocksRaycasts = previousHudStates[i].z > 0f;
                }
            }
            if (!hidden) previousHudStates.Clear();
        }

        private void BuildUnlockedCards()
        {
            if (cardPrefab == null || cardContainer == null)
            {
                Debug.LogError("Starter selection needs a Card Prefab and Card Container.", this);
                return;
            }

            for (int i = cardContainer.childCount - 1; i >= 0; i--)
                Destroy(cardContainer.GetChild(i).gameObject);
            cards.Clear();

            for (int i = 0; i < availableSpiritPrefabs.Count; i++)
            {
                GameObject prefab = availableSpiritPrefabs[i];
                SpiritMember member = prefab != null ? prefab.GetComponent<SpiritMember>() : null;
                // The Shepherd's starting pair must also be available on a fresh profile.
                // This does not alter the profile's persistent unlocks.
                if (member == null || member.Definition == null ||
                    (RequiredStarterCount == 1 && !SpiritUnlockProgress.IsUnlocked(member.Definition))) continue;
                StarterSpiritCardView card = Instantiate(cardPrefab, cardContainer);
                card.Bind(prefab, ChooseStarter);
                cards.Add(card);
            }
            RefreshSelection();
        }

        public void UnlockAllSpiritsForTesting()
        {
            for (int i = 0; i < availableSpiritPrefabs.Count; i++)
            {
                GameObject prefab = availableSpiritPrefabs[i];
                SpiritMember member = prefab != null ? prefab.GetComponent<SpiritMember>() : null;
                if (member != null) SpiritUnlockProgress.Unlock(member.Definition);
            }

            BuildUnlockedCards();
        }

        public void ResetSpiritUnlocksForTesting()
        {
            for (int i = 0; i < availableSpiritPrefabs.Count; i++)
            {
                GameObject prefab = availableSpiritPrefabs[i];
                SpiritMember member = prefab != null ? prefab.GetComponent<SpiritMember>() : null;
                if (member != null) SpiritUnlockProgress.Forget(member.Definition);
            }

            EnsureDefaultUnlocked();
            PlayerPrefs.Save();
            BuildUnlockedCards();
        }

        private void ChooseStarter(GameObject prefab)
        {
            if (!selectionOpen || !spiritManager.TryAddSpirit(prefab)) return;
            RefreshSelection();
            if (spiritManager.SpiritCount < RequiredStarterCount) return;
            selectionOpen = false;
            Time.timeScale = previousTimeScale;
            gameObject.SetActive(false);
        }

        private void RefreshSelection()
        {
            if (spiritManager == null) return;
            if (heading != null)
                heading.text = RequiredStarterCount == 2 ? "CHOOSE TWO STARTER SPIRITS" : "CHOOSE YOUR STARTER SPIRIT";
            if (instruction != null)
                instruction.text = RequiredStarterCount == 2
                    ? $"{spiritManager.SpiritCount}/2 selected — choose two different spirits to begin."
                    : "Choose one spirit to begin your journey.";
            StarterSpiritCardView nextCard = null;
            foreach (StarterSpiritCardView card in cards)
            {
                if (card == null) continue;
                SpiritMember member = card.SpiritPrefab != null ? card.SpiritPrefab.GetComponent<SpiritMember>() : null;
                bool selected = member != null && spiritManager.OwnsSpirit(member.Definition);
                card.SetSelected(selected);
                if (!selected && nextCard == null) nextCard = card;
            }
            if (selectionOpen && EventSystem.current != null && nextCard != null)
                EventSystem.current.SetSelectedGameObject(nextCard.SelectionButton.gameObject);
        }

        private void EnsureDefaultUnlocked()
        {
            SpiritMember member = defaultStarterSpirit != null
                ? defaultStarterSpirit.GetComponent<SpiritMember>() : null;
            if (member != null) SpiritUnlockProgress.Unlock(member.Definition);
        }
    }
}

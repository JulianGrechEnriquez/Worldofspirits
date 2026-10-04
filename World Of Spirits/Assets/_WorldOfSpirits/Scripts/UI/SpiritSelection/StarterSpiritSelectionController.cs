using System.Collections.Generic;
using UnityEngine;
using WorldOfSpirits.Progression;
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
        private readonly List<Vector3> previousHudStates = new List<Vector3>();

        private SpiritManager spiritManager;
        private float previousTimeScale = 1f;
        private bool selectionOpen;

        private void Start()
        {
            spiritManager = FindFirstObjectByType<SpiritManager>();
            EnsureDefaultUnlocked();
            if (spiritManager == null || spiritManager.SpiritCount > 0)
            {
                gameObject.SetActive(false);
                return;
            }

            BuildUnlockedCards();
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            selectionOpen = true;
            SetGameplayHudHidden(true);
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

            for (int i = 0; i < availableSpiritPrefabs.Count; i++)
            {
                GameObject prefab = availableSpiritPrefabs[i];
                SpiritMember member = prefab != null ? prefab.GetComponent<SpiritMember>() : null;
                if (member == null || !SpiritUnlockProgress.IsUnlocked(member.Definition)) continue;
                StarterSpiritCardView card = Instantiate(cardPrefab, cardContainer);
                card.Bind(prefab, ChooseStarter);
            }
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
            selectionOpen = false;
            Time.timeScale = previousTimeScale;
            gameObject.SetActive(false);
        }

        private void EnsureDefaultUnlocked()
        {
            SpiritMember member = defaultStarterSpirit != null
                ? defaultStarterSpirit.GetComponent<SpiritMember>() : null;
            if (member != null) SpiritUnlockProgress.Unlock(member.Definition);
        }
    }
}

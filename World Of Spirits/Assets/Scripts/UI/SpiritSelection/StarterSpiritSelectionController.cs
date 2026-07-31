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
        }

        private void OnDisable()
        {
            if (!selectionOpen) return;
            Time.timeScale = previousTimeScale;
            selectionOpen = false;
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

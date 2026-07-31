using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WorldOfSpirits.Progression.Upgrades;
using WorldOfSpirits.Spirits;
using WorldOfSpirits.Progression;

namespace WorldOfSpirits.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerLevelSystem), typeof(SpiritManager))]
    public sealed class ProgressionInterface : MonoBehaviour
    {
        [Header("Starter Spirits")]
        [SerializeField] private bool createRuntimeStarterSelection = true;
        [SerializeField] private List<GameObject> starterSpiritPrefabs = new List<GameObject>();
        [Tooltip("The only starter available on a fresh save. This is Fire Spirit by default.")]
        [SerializeField] private GameObject defaultStarterSpirit;

        [Header("Colours")]
        [SerializeField] private bool createRuntimeExperienceHud = true;
        [SerializeField] private Color experienceColor = new Color(0.2f, 0.85f, 1f, 1f);
        [SerializeField] private Color panelColor = new Color(0.035f, 0.055f, 0.09f, 0.96f);
        [SerializeField] private Color cardColor = new Color(0.11f, 0.16f, 0.24f, 1f);

        private PlayerLevelSystem levelSystem;
        private SpiritManager spiritManager;
        private Image experienceFill;
        private TMP_Text levelText;
        private TMP_Text experienceText;
        private GameObject starterPanel;
        private float previousTimeScale = 1f;
        private bool choosingStarter;

        private void Awake()
        {
            levelSystem = GetComponent<PlayerLevelSystem>();
            spiritManager = GetComponent<SpiritManager>();
            EnsureDefaultStarterUnlocked();
            BuildInterface();
            RefreshExperience(levelSystem.Experience, levelSystem.RequiredExperience);
        }

        private void OnEnable()
        {
            if (levelSystem == null) levelSystem = GetComponent<PlayerLevelSystem>();
            levelSystem.LevelGained += RefreshLevel;
            levelSystem.ExperienceChanged += RefreshExperience;
        }

        private void Start()
        {
            if (!createRuntimeStarterSelection) return;
            if (spiritManager.SpiritCount == 0 && starterSpiritPrefabs.Count > 0)
                OpenStarterSelection();
            else
                starterPanel.SetActive(false);
        }

        private void OnDisable()
        {
            if (levelSystem != null)
            {
                levelSystem.LevelGained -= RefreshLevel;
                levelSystem.ExperienceChanged -= RefreshExperience;
            }

            if (choosingStarter)
            {
                Time.timeScale = previousTimeScale;
                choosingStarter = false;
            }
        }

        private void RefreshLevel(int level)
        {
            if (levelText != null) levelText.text = $"LEVEL {level}";
        }

        private void RefreshExperience(float current, float required)
        {
            RefreshLevel(levelSystem.Level);
            if (experienceFill != null)
                experienceFill.fillAmount = required > 0f ? Mathf.Clamp01(current / required) : 0f;
            if (experienceText != null)
                experienceText.text = $"{Mathf.FloorToInt(current)} / {Mathf.CeilToInt(required)} XP";
        }

        private void OpenStarterSelection()
        {
            choosingStarter = true;
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            starterPanel.SetActive(true);
        }

        private void ChooseStarter(GameObject spiritPrefab)
        {
            if (!choosingStarter || spiritPrefab == null || !spiritManager.TryAddSpirit(spiritPrefab)) return;
            choosingStarter = false;
            starterPanel.SetActive(false);
            Time.timeScale = previousTimeScale;
        }

        private void BuildInterface()
        {
            if (!createRuntimeExperienceHud && !createRuntimeStarterSelection) return;

            GameObject canvasObject = CreateObject("Progression UI", transform, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 40;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            if (createRuntimeExperienceHud) BuildExperienceHud(canvasObject.transform);
            if (createRuntimeStarterSelection) BuildStarterPanel(canvasObject.transform);
        }

        private void BuildExperienceHud(Transform parent)
        {
            GameObject hud = CreateObject("Experience HUD", parent, typeof(Image));
            RectTransform hudRect = hud.GetComponent<RectTransform>();
            SetRect(hudRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(760f, 64f));
            hud.GetComponent<Image>().color = new Color(0.02f, 0.03f, 0.05f, 0.9f);

            levelText = CreateText("Level", hud.transform, "LEVEL 1", 27, TextAlignmentOptions.Center, FontStyles.Bold);
            SetRect(levelText.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(76f, 0f), new Vector2(152f, 64f));

            GameObject barBackground = CreateObject("XP Bar Background", hud.transform, typeof(Image));
            RectTransform barRect = barBackground.GetComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0f, 0.5f);
            barRect.anchorMax = new Vector2(1f, 0.5f);
            barRect.offsetMin = new Vector2(160f, -17f);
            barRect.offsetMax = new Vector2(-18f, 17f);
            barBackground.GetComponent<Image>().color = new Color(0.1f, 0.13f, 0.18f, 1f);

            GameObject fillObject = CreateObject("XP Fill", barBackground.transform, typeof(Image));
            RectTransform fillRect = fillObject.GetComponent<RectTransform>();
            Stretch(fillRect, 3f);
            experienceFill = fillObject.GetComponent<Image>();
            experienceFill.color = experienceColor;
            experienceFill.type = Image.Type.Filled;
            experienceFill.fillMethod = Image.FillMethod.Horizontal;
            experienceFill.fillOrigin = 0;

            experienceText = CreateText("XP Text", barBackground.transform, "0 / 10 XP", 22, TextAlignmentOptions.Center, FontStyles.Bold);
            Stretch(experienceText.rectTransform, 0f);
        }

        private void BuildStarterPanel(Transform parent)
        {
            starterPanel = CreateObject("Starter Spirit Selection", parent, typeof(Image));
            RectTransform panelRect = starterPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            starterPanel.GetComponent<Image>().color = panelColor;

            TMP_Text heading = CreateText("Heading", starterPanel.transform, "CHOOSE YOUR STARTER SPIRIT", 48,
                TextAlignmentOptions.Center, FontStyles.Bold);
            SetRect(heading.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(1000f, 80f));

            TMP_Text instruction = CreateText("Instruction", starterPanel.transform,
                "This spirit begins in your main slot. You can contract additional spirits while leveling up.",
                24, TextAlignmentOptions.Center, FontStyles.Normal);
            instruction.color = new Color(0.75f, 0.82f, 0.9f, 1f);
            SetRect(instruction.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -195f), new Vector2(1200f, 60f));

            int validCount = 0;
            for (int i = 0; i < starterSpiritPrefabs.Count; i++)
                if (IsStarterUnlocked(starterSpiritPrefabs[i])) validCount++;

            int columns = Mathf.Min(4, Mathf.Max(1, validCount));
            int rows = Mathf.CeilToInt(validCount / (float)columns);
            float cardWidth = 260f;
            float cardHeight = 320f;
            float spacing = 36f;
            float rowSpacing = 28f;
            float totalWidth = columns * cardWidth + Mathf.Max(0, columns - 1) * spacing;
            float totalHeight = rows * cardHeight + Mathf.Max(0, rows - 1) * rowSpacing;
            int displayIndex = 0;
            foreach (GameObject prefab in starterSpiritPrefabs)
            {
                if (!IsStarterUnlocked(prefab)) continue;
                int row = displayIndex / columns;
                int column = displayIndex % columns;
                int itemsInRow = Mathf.Min(columns, validCount - row * columns);
                float rowWidth = itemsInRow * cardWidth + Mathf.Max(0, itemsInRow - 1) * spacing;
                float x = -rowWidth * 0.5f + cardWidth * 0.5f + column * (cardWidth + spacing);
                float y = totalHeight * 0.5f - cardHeight * 0.5f - row * (cardHeight + rowSpacing) - 45f;
                CreateStarterCard(starterPanel.transform, prefab, new Vector2(x, y), new Vector2(cardWidth, cardHeight));
                displayIndex++;
            }
        }

        private void CreateStarterCard(Transform parent, GameObject spiritPrefab, Vector2 position, Vector2 size)
        {
            GameObject card = CreateObject(spiritPrefab.name + " Choice", parent, typeof(Image), typeof(Button));
            RectTransform cardRect = card.GetComponent<RectTransform>();
            SetRect(cardRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);
            Image cardImage = card.GetComponent<Image>();
            cardImage.color = cardColor;
            Button button = card.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            colors.pressedColor = new Color(0.8f, 0.9f, 1f, 1f);
            button.colors = colors;
            GameObject selectedPrefab = spiritPrefab;
            button.onClick.AddListener(() => ChooseStarter(selectedPrefab));

            SpriteRenderer previewRenderer = spiritPrefab.GetComponentInChildren<SpriteRenderer>(true);
            GameObject preview = CreateObject("Preview", card.transform, typeof(Image));
            RectTransform previewRect = preview.GetComponent<RectTransform>();
            SetRect(previewRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -103f), new Vector2(145f, 145f));
            Image previewImage = preview.GetComponent<Image>();
            previewImage.sprite = previewRenderer != null ? previewRenderer.sprite : null;
            previewImage.color = previewRenderer != null ? previewRenderer.color : Color.white;
            previewImage.preserveAspect = true;
            previewImage.raycastTarget = false;

            SpiritMember member = spiritPrefab.GetComponent<SpiritMember>();
            string spiritName = member != null && member.Definition != null ? member.Definition.SpiritName : spiritPrefab.name;
            TMP_Text nameText = CreateText("Name", card.transform, spiritName.ToUpperInvariant(), 30,
                TextAlignmentOptions.Center, FontStyles.Bold);
            SetRect(nameText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 90f), new Vector2(240f, 48f));

            string weaponName = member != null && member.Definition != null && member.Definition.RuntimeWeapon != null
                ? member.Definition.RuntimeWeapon.WeaponName : "Spirit Weapon";
            TMP_Text weaponText = CreateText("Weapon", card.transform, weaponName, 21,
                TextAlignmentOptions.Center, FontStyles.Normal);
            weaponText.color = new Color(0.65f, 0.85f, 1f, 1f);
            SetRect(weaponText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 52f), new Vector2(240f, 38f));

            TMP_Text chooseText = CreateText("Choose", card.transform, "SELECT", 22,
                TextAlignmentOptions.Center, FontStyles.Bold);
            SetRect(chooseText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 20f), new Vector2(160f, 38f));
        }

        private void EnsureDefaultStarterUnlocked()
        {
            GameObject starter = defaultStarterSpirit;
            if (starter == null)
            {
                for (int i = 0; i < starterSpiritPrefabs.Count; i++)
                {
                    if (starterSpiritPrefabs[i] != null)
                    {
                        starter = starterSpiritPrefabs[i];
                        break;
                    }
                }
            }

            SpiritMember member = starter != null ? starter.GetComponent<SpiritMember>() : null;
            if (member != null) SpiritUnlockProgress.Unlock(member.Definition);
        }

        private static bool IsStarterUnlocked(GameObject prefab)
        {
            if (prefab == null) return false;
            SpiritMember member = prefab.GetComponent<SpiritMember>();
            return member != null && SpiritUnlockProgress.IsUnlocked(member.Definition);
        }

        [ContextMenu("Reset Starter Unlocks")]
        private void ResetStarterUnlocks()
        {
            for (int i = 0; i < starterSpiritPrefabs.Count; i++)
            {
                GameObject prefab = starterSpiritPrefabs[i];
                SpiritMember member = prefab != null ? prefab.GetComponent<SpiritMember>() : null;
                if (member != null) SpiritUnlockProgress.Forget(member.Definition);
            }
            PlayerPrefs.Save();
            EnsureDefaultStarterUnlocked();
        }

        private static GameObject CreateObject(string name, Transform parent, params System.Type[] components)
        {
            GameObject item = new GameObject(name, typeof(RectTransform));
            item.transform.SetParent(parent, false);
            for (int i = 0; i < components.Length; i++) item.AddComponent(components[i]);
            return item;
        }

        private static TMP_Text CreateText(string name, Transform parent, string value, float size,
            TextAlignmentOptions alignment, FontStyles style)
        {
            GameObject item = CreateObject(name, parent, typeof(TextMeshProUGUI));
            TextMeshProUGUI text = item.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = size;
            text.alignment = alignment;
            text.fontStyle = style;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 anchoredPosition, Vector2 size)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
        }

        private static void Stretch(RectTransform rect, float inset)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.one * inset;
            rect.offsetMax = -Vector2.one * inset;
        }
    }
}

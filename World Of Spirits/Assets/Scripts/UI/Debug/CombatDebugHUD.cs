using UnityEngine;
using UnityEngine.InputSystem;
using WorldOfSpirits.Enemies;
using WorldOfSpirits.Player;
using WorldOfSpirits.Spirits;
using WorldOfSpirits.Combat;

namespace WorldOfSpirits.UI
{
    public class CombatDebugHUD : MonoBehaviour
    {
        [SerializeField] private bool showHud = true;
        [SerializeField] private Vector2 screenPosition = new Vector2(12f, 12f);
        [SerializeField, Min(0.1f)] private float enemyCountRefreshRate = 0.5f;

        private PlayerCharacter player;
        private SpiritManager spiritManager;
        private int enemyCount;
        private float nextEnemyCountRefresh;
        private GUIStyle boxStyle;
        private GUIStyle labelStyle;

        private void Awake()
        {
            player = FindFirstObjectByType<PlayerCharacter>();
            spiritManager = FindFirstObjectByType<SpiritManager>();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.f3Key.wasPressedThisFrame)
            {
                showHud = !showHud;
            }

            if (Time.unscaledTime >= nextEnemyCountRefresh)
            {
                enemyCount = 0;
                var entities = LivingEntity.ActiveEntities;
                for (int i = 0; i < entities.Count; i++)
                {
                    if (entities[i] is EnemyBase && entities[i].IsAlive)
                    {
                        enemyCount++;
                    }
                }
                nextEnemyCountRefresh = Time.unscaledTime + enemyCountRefreshRate;
            }

            if (player == null)
            {
                player = FindFirstObjectByType<PlayerCharacter>();
            }

            if (spiritManager == null)
            {
                spiritManager = FindFirstObjectByType<SpiritManager>();
            }
        }

        private void OnGUI()
        {
            if (!showHud)
            {
                return;
            }

            EnsureStyles();
            string playerText = player == null
                ? "Player: not found"
                : $"Player HP: {player.CurrentHealth:0.#}/{player.MaxHealth:0.#}\nSpeed: {player.MoveSpeed:0.##}";
            string spiritText = GetSpiritDebugText();
            string text = $"COMBAT DEBUG (F3 to hide)\n{playerText}\n{spiritText}\nEnemies: {enemyCount}\nFPS: {GetFramesPerSecond():0}";
            Vector2 size = labelStyle.CalcSize(new GUIContent(text));
            Rect box = new Rect(screenPosition.x, screenPosition.y, size.x + 24f, size.y + 20f);

            GUI.Box(box, GUIContent.none, boxStyle);
            GUI.Label(new Rect(box.x + 12f, box.y + 10f, size.x, size.y), text, labelStyle);
        }

        private static float GetFramesPerSecond()
        {
            return Time.unscaledDeltaTime > 0f ? 1f / Time.unscaledDeltaTime : 0f;
        }

        private string GetSpiritDebugText()
        {
            if (spiritManager == null)
            {
                return "Main Spirit: manager not found\nSupport 1: Empty\nSupport 2: Empty";
            }

            return $"Main Spirit: {spiritManager.GetSpiritNameAt(0)}\n" +
                   $"Support 1: {spiritManager.GetSpiritNameAt(1)}\n" +
                   $"Support 2: {spiritManager.GetSpiritNameAt(2)}";
        }

        private void EnsureStyles()
        {
            if (boxStyle != null)
            {
                return;
            }

            boxStyle = new GUIStyle(GUI.skin.box);
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                normal = { textColor = Color.white }
            };
        }
    }
}

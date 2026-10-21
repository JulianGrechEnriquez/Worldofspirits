using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WorldOfSpirits.Player;
using WorldOfSpirits.CameraSystem;
using WorldOfSpirits.Crowd;
using WorldOfSpirits.Enemies;
using WorldOfSpirits.Spawning;
using WorldOfSpirits.Progression.Upgrades;

namespace WorldOfSpirits.UI
{
    [DisallowMultipleComponent]
    public sealed class CharacterSelectionController : MonoBehaviour
    {
        [SerializeField] private PlayerCharacter spiritTamer;
        [SerializeField] private PlayerCharacter spiritShepherd;
        [SerializeField] private PlayerCharacter spiritWarrior;
        [Tooltip("These objects start disabled so gameplay initializes with the chosen player.")]
        [SerializeField] private GameObject[] gameplayObjects;
        [SerializeField] private Button tamerButton;
        [SerializeField] private Button shepherdButton;
        private bool chosen;
        [SerializeField] private PlayerCharacter spiritBeastmaster;

        private void Start()
        {
            Time.timeScale = 0f;
            if (EventSystem.current != null && tamerButton != null)
                EventSystem.current.SetSelectedGameObject(tamerButton.gameObject);
        }

        public void ChooseBeastmaster() => Choose(spiritBeastmaster);
        public void ChooseTamer() => Choose(spiritTamer);
        public void ChooseShepherd() => Choose(spiritShepherd);
        public void ChooseWarrior() => Choose(spiritWarrior);

        private void Choose(PlayerCharacter player)
        {
            if (chosen || player == null) return;
            chosen = true;
            if (spiritBeastmaster != null) spiritBeastmaster.gameObject.SetActive(player == spiritBeastmaster);
            spiritTamer.gameObject.SetActive(player == spiritTamer);
            spiritShepherd.gameObject.SetActive(player == spiritShepherd);
            if (spiritWarrior != null) spiritWarrior.gameObject.SetActive(player == spiritWarrior);

            // Bind authored references before the gameplay objects run Awake/OnEnable.
            foreach (var camera in FindObjectsByType<CameraFollow2D>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                camera.SetPlayer(player);
            foreach (var crowd in FindObjectsByType<CrowdSimulationManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                crowd.SetPlayer(player);
            foreach (var director in FindObjectsByType<SpawnDirector>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                director.SetPlayer(player.transform);
            foreach (var enemy in FindObjectsByType<EnemyBase>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                enemy.SetTarget(player.transform);
            foreach (var hud in FindObjectsByType<PlayerHealthHud>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                hud.SetPlayer(player);
            foreach (var upgrades in FindObjectsByType<UpgradeScreen>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                upgrades.SetSelectionManager(player.GetComponent<UpgradeSelectionManager>());

            Time.timeScale = 1f;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            foreach (var target in gameplayObjects)
                if (target != null) target.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using WorldOfSpirits.Player;

namespace WorldOfSpirits.Spirits
{
    public class SpiritManager : MonoBehaviour
    {
        [Header("Spirit Slots (main first)")]
        [SerializeField] private Transform mainSlot;
        [SerializeField] private List<Transform> supportSlots = new List<Transform>();

        [Header("Primary Attack")]
        [SerializeField] private Transform playerProjectileSpawner;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float rotationMoveDuration = 0.2f;
        [SerializeField] private bool logRotation;

        private readonly List<Transform> slots = new List<Transform>();
        private readonly List<SpiritMember> spirits = new List<SpiritMember>();
        private PlayerMovement playerMovement;
        private float rotationProgress = 1f;
        private Vector3[] rotationStartPositions;
        private bool isChangingFormation;

        public SpiritMember PrimarySpirit => spirits.Count > 0 ? spirits[0] : null;
        public int SpiritCount => spirits.Count;
        public bool PlayerIsMoving => playerMovement != null && playerMovement.IsMoving;
        public bool IsChangingFormation => isChangingFormation;

        public string GetSpiritNameAt(int index)
        {
            if (index < 0 || index >= spirits.Count || spirits[index] == null)
            {
                return "Empty";
            }

            return spirits[index].name.Replace("(Clone)", string.Empty).Trim();
        }

        public bool TryAddSpirit(GameObject spiritPrefab)
        {
            if (spiritPrefab == null)
            {
                Debug.LogWarning("Cannot add a spirit because no prefab was supplied.", this);
                return false;
            }

            string requestedName = CleanSpiritName(spiritPrefab.name);
            foreach (SpiritMember ownedSpirit in spirits)
            {
                if (ownedSpirit != null && CleanSpiritName(ownedSpirit.name) == requestedName)
                {
                    Debug.Log($"The player already owns {requestedName}.", this);
                    return false;
                }
            }

            if (spirits.Count >= slots.Count)
            {
                Debug.LogWarning($"Cannot add {requestedName}: all spirit slots are occupied.", this);
                return false;
            }

            Transform openSlot = slots[spirits.Count];
            GameObject spiritObject = Instantiate(spiritPrefab, openSlot);
            spiritObject.transform.localPosition = Vector3.zero;
            spiritObject.transform.localRotation = Quaternion.identity;

            SpiritMember member = spiritObject.GetComponent<SpiritMember>();
            if (member == null)
            {
                member = spiritObject.AddComponent<SpiritMember>();
            }

            spirits.Add(member);
            Debug.Log($"Added {requestedName} to spirit slot {spirits.Count}.", this);
            return true;
        }

        private static string CleanSpiritName(string spiritName)
        {
            return spiritName.Replace("(Clone)", string.Empty).Trim().ToLowerInvariant();
        }

        public bool IsPrimarySpirit(string spiritName)
        {
            return PrimarySpirit != null &&
                PrimarySpirit.name.IndexOf(spiritName, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void Awake()
        {
            playerMovement = GetComponent<PlayerMovement>();
            if (playerProjectileSpawner == null)
            {
                Transform existingSpawner = transform.Find("PlayerProjectileSpawner");
                playerProjectileSpawner = existingSpawner != null ? existingSpawner : transform;
            }

            DiscoverSlotsIfNeeded();
            BuildSpiritList();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            {
                RotateSpirits();
            }

            AnimateRotation();

            bool isMoving = playerMovement != null && playerMovement.IsMoving;
            for (int i = 0; i < spirits.Count; i++)
            {
                spirits[i].ApplyState(transform, playerProjectileSpawner, i == 0, isMoving, isChangingFormation);
            }
        }

        public void RotateSpirits()
        {
            if (spirits.Count < 2 || isChangingFormation)
            {
                return;
            }

            StartCoroutine(ChangeFormationSequence());
        }

        private IEnumerator ChangeFormationSequence()
        {
            isChangingFormation = true;

            float changeDuration = PlayFormationAnimation(false);
            if (changeDuration > 0f)
            {
                yield return new WaitForSeconds(changeDuration);
            }

            SpiritMember previousPrimary = spirits[0];
            spirits.RemoveAt(0);
            spirits.Add(previousPrimary);

            rotationStartPositions = new Vector3[spirits.Count];
            for (int i = 0; i < spirits.Count; i++)
            {
                rotationStartPositions[i] = spirits[i].transform.position;
                spirits[i].transform.SetParent(slots[i], true);
            }

            rotationProgress = rotationMoveDuration <= 0f ? 1f : 0f;
            if (logRotation)
            {
                Debug.Log($"Primary spirit changed to {PrimarySpirit.name}.", this);
            }

            while (rotationProgress < 1f)
            {
                yield return null;
            }

            float remergeDuration = PlayFormationAnimation(true);
            if (remergeDuration > 0f)
            {
                yield return new WaitForSeconds(remergeDuration);
            }

            foreach (SpiritMember spirit in spirits)
            {
                if (spirit != null)
                {
                    spirit.PlayIdleAnimation();
                }
            }

            isChangingFormation = false;
        }

        private float PlayFormationAnimation(bool remerging)
        {
            float longestDuration = 0f;
            foreach (SpiritMember spirit in spirits)
            {
                if (spirit != null)
                {
                    longestDuration = Mathf.Max(longestDuration, spirit.PlayTransitionAnimation(remerging));
                }
            }

            return longestDuration;
        }

        private void AnimateRotation()
        {
            if (rotationProgress >= 1f)
            {
                SnapSpiritsToSlots();
                return;
            }

            rotationProgress = Mathf.Min(1f, rotationProgress + Time.deltaTime / rotationMoveDuration);
            float easedProgress = Mathf.SmoothStep(0f, 1f, rotationProgress);
            for (int i = 0; i < spirits.Count; i++)
            {
                spirits[i].transform.position = Vector3.Lerp(rotationStartPositions[i], slots[i].position, easedProgress);
            }
        }

        private void SnapSpiritsToSlots()
        {
            for (int i = 0; i < spirits.Count; i++)
            {
                spirits[i].transform.localPosition = Vector3.zero;
            }
        }

        private void DiscoverSlotsIfNeeded()
        {
            if (mainSlot != null)
            {
                return;
            }

            Transform[] children = GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                string slotName = child.name.ToLowerInvariant();
                if (slotName == "main spirit slot" || slotName == "mainsprite")
                {
                    mainSlot = child;
                }
                else if (slotName.Contains("support spirit slot") ||
                         slotName.Contains("suport sprite") ||
                         slotName.Contains("support sprite"))
                {
                    supportSlots.Add(child);
                }
            }
        }

        private void BuildSpiritList()
        {
            slots.Clear();
            spirits.Clear();

            if (mainSlot != null)
            {
                slots.Add(mainSlot);
            }

            foreach (Transform supportSlot in supportSlots)
            {
                if (supportSlot != null && !slots.Contains(supportSlot))
                {
                    slots.Add(supportSlot);
                }
            }

            foreach (Transform slot in slots)
            {
                if (slot.childCount == 0)
                {
                    continue;
                }

                Transform spiritTransform = slot.GetChild(0);
                SpiritMember member = spiritTransform.GetComponent<SpiritMember>();
                if (member == null)
                {
                    member = spiritTransform.gameObject.AddComponent<SpiritMember>();
                }

                spirits.Add(member);
            }

            if (slots.Count == 0)
            {
                Debug.LogWarning("SpiritManager could not find any configured spirit slots.", this);
            }
        }
    }
}

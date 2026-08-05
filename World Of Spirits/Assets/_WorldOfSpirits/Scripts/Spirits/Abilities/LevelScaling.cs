using System;
using UnityEngine;

namespace WorldOfSpirits.Spirits
{
    [Serializable]
    public class LevelScaling
    {
        [SerializeField] private float baseValue = 1f;
        [SerializeField] private float increasePerLevel;

        public float Evaluate(int level) => baseValue + increasePerLevel * Mathf.Max(0, level - 1);
    }

    [Serializable]
    public class IntegerLevelScaling
    {
        [SerializeField, Min(0)] private int baseValue = 1;
        [SerializeField] private int increasePerLevel;

        public int Evaluate(int level) => Mathf.Max(0, baseValue + increasePerLevel * Mathf.Max(0, level - 1));
    }
}

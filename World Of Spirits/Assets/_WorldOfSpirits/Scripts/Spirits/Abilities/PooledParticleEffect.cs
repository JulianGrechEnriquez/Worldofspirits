using UnityEngine;

namespace WorldOfSpirits.Spirits
{
    /// <summary>Restarts particles cleanly when a floor effect is reused.</summary>
    public sealed class PooledParticleEffect : MonoBehaviour
    {
        private ParticleSystem[] systems;

        private void Awake()
        {
            systems = GetComponentsInChildren<ParticleSystem>(true);
        }

        private void OnEnable()
        {
            foreach (ParticleSystem system in systems)
            {
                system.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                system.Play(false);
            }
        }

        private void OnDisable()
        {
            foreach (ParticleSystem system in systems)
                system.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}

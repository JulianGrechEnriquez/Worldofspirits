using UnityEngine;

namespace WorldOfSpirits.Core
{
    public sealed class PooledSceneObject : MonoBehaviour
    {
        internal GameObject Prefab;
        internal PoolCategory Category;
        internal Transform StorageParent;
        internal int SpawnVersion;
        internal string OriginalName;
        private IScenePoolable[] callbacks;

        internal IScenePoolable[] GetCallbacks()
        {
            if (callbacks != null)
            {
                return callbacks;
            }

            MonoBehaviour[] behaviours = GetComponentsInChildren<MonoBehaviour>(true);
            int count = 0;
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IScenePoolable)
                {
                    count++;
                }
            }

            callbacks = new IScenePoolable[count];
            int callbackIndex = 0;
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IScenePoolable callback)
                {
                    callbacks[callbackIndex++] = callback;
                }
            }

            return callbacks;
        }
    }

}

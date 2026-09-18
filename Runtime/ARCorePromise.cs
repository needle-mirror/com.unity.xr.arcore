#if UNITY_6000_5_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARCore
{
    internal class ARCorePromise<T> : Promise<T>
    {
        protected override void OnKeepWaiting()
        {
            if (s_LastFrameUpdated == Time.frameCount)
                return;

            ArPrestoApi.ArPresto_update();
            s_LastFrameUpdated = Time.frameCount;
        }

        internal new void Resolve(T result)
        {
            base.Resolve(result);
        }

#if UNITY_6000_5_OR_NEWER
        [NoAutoStaticsCleanup]
#endif
        static int s_LastFrameUpdated;
    }
}

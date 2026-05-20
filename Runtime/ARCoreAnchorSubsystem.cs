using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Collections;
using Unity.XR.CoreUtils;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;
using static UnityEngine.XR.ARSubsystems.XRResultStatus;
using SerializableGuid = UnityEngine.XR.ARSubsystems.SerializableGuid;

namespace UnityEngine.XR.ARCore
{
    /// <summary>
    /// The ARCore implementation of the [XRAnchorSubsystem](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystem).
    /// Don't create this directly. Use the [SubsystemManager](xref:UnityEngine.SubsystemManager) instead.
    /// </summary>
    [Preserve]
    public sealed class ARCoreAnchorSubsystem : XRAnchorSubsystem
    {
        struct SaveRequest
        {
            internal AwaitableCompletionSource<Result<SerializableGuid>> completionSource;
            internal CancellationTokenRegistration tokenRegistration;

            internal SaveRequest(
                AwaitableCompletionSource<Result<SerializableGuid>> completionSource,
                CancellationTokenRegistration tokenRegistration)
            {
                this.completionSource = completionSource;
                this.tokenRegistration = tokenRegistration;
            }
        }

        struct LoadRequest
        {
            internal AwaitableCompletionSource<Result<XRAnchor>> completionSource;
            internal CancellationTokenRegistration tokenRegistration;

            internal LoadRequest(
                AwaitableCompletionSource<Result<XRAnchor>> completionSource,
                CancellationTokenRegistration tokenRegistration)
            {
                this.completionSource = completionSource;
                this.tokenRegistration = tokenRegistration;
            }
        }

        class ARCoreProvider : Provider
        {
            const uint k_MaxLifespanApiKey = 1;
            const uint k_MaxLifespanKeyless = 365;

            static readonly Dictionary<TrackableId, SaveRequest> s_PendingSaveRequestsByAnchorId = new();
            static readonly Dictionary<SerializableGuid, LoadRequest> s_PendingLoadRequestsByUuid = new();

            static readonly Pool.ObjectPool<AwaitableCompletionSource<Result<SerializableGuid>>> s_SaveCompletionSources =
                ObjectPoolCreateUtil.Create<AwaitableCompletionSource<Result<SerializableGuid>>>();

            static readonly Pool.ObjectPool<AwaitableCompletionSource<Result<XRAnchor>>> s_LoadCompletionSources =
                ObjectPoolCreateUtil.Create<AwaitableCompletionSource<Result<XRAnchor>>>();

            protected override bool TryInitialize()
            {
                UnityARCore_anchors_create(s_SaveAsyncCallback, s_LoadAsyncCallback);
                return true;
            }

            public override void Start() => UnityARCore_anchors_start();
            public override void Stop() => UnityARCore_anchors_stop();
            public override void Destroy() => UnityARCore_anchors_onDestroy();

            public override unsafe TrackableChanges<XRAnchor> GetChanges(XRAnchor defaultAnchor, Allocator allocator)
            {
                var context = UnityARCore_anchors_acquireChanges(
                    out var addedPtr, out var addedCount,
                    out var updatedPtr, out var updatedCount,
                    out var removedPtr, out var removedCount,
                    out var elementSize);

                try
                {
                    return new TrackableChanges<XRAnchor>(
                        addedPtr, addedCount,
                        updatedPtr, updatedCount,
                        removedPtr, removedCount,
                        defaultAnchor, elementSize,
                        allocator);
                }
                finally
                {
                    UnityARCore_anchors_releaseChanges(context);
                }
            }

            public override bool TryAddAnchor(Pose pose, out XRAnchor anchor)
            {
                return UnityARCore_anchors_tryAdd(pose, out anchor);
            }

            public override bool TryAttachAnchor(TrackableId attachedToId, Pose pose, out XRAnchor anchor)
            {
                return UnityARCore_anchors_tryAttach(attachedToId, pose, out anchor);
            }

            public override bool TryRemoveAnchor(TrackableId anchorId)
            {
                return UnityARCore_anchors_tryRemove(anchorId);
            }

            internal XRResultStatus EstimateFeatureMapQualityForHosting(
                TrackableId anchorId, ref ArFeatureMapQuality quality)
            {
                return UnityARCore_anchors_estimateFeatureMapQualityForHosting(anchorId, ref quality);
            }

            public override Awaitable<Result<SerializableGuid>> TrySaveAnchorAsync(
                TrackableId anchorId, CancellationToken cancellationToken = default)
            {
                var usingKeyless = ARCoreRuntimeSettings.Instance.authorizationType == ARCoreRuntimeSettings.AuthorizationType.Keyless;
                var lifespan = usingKeyless ? k_MaxLifespanKeyless : k_MaxLifespanApiKey;
                return TrySaveAnchorWithLifespanAsync(anchorId, lifespan, cancellationToken);
            }

            internal Awaitable<Result<SerializableGuid>> TrySaveAnchorWithLifespanAsync(
                TrackableId anchorId, uint lifespan, CancellationToken cancellationToken = default)
            {
                if (lifespan == 0)
                    throw new ArgumentException("Lifespan must be greater than 0");

                if (cancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException();

                if (s_PendingSaveRequestsByAnchorId.ContainsKey(anchorId))
                {
                    Debug.LogError($"Cannot save anchor with trackableId [{anchorId}] while saving for it is already in progress!");
                    var result = new Result<SerializableGuid>(new(StatusCode.ValidationFailure), default);
                    return AwaitableUtils<Result<SerializableGuid>>.FromResult(result);
                }

                var usingKeyless = ARCoreRuntimeSettings.Instance.authorizationType == ARCoreRuntimeSettings.AuthorizationType.Keyless;
                if (usingKeyless && lifespan > k_MaxLifespanKeyless)
                {
                    Debug.LogWarning("ARCore anchor lifespan is too long. Using default lifespan.");
                    lifespan = k_MaxLifespanKeyless;
                }
                else if (!usingKeyless && lifespan > k_MaxLifespanApiKey)
                {
                    Debug.LogWarning("ARCore anchor lifespan is too long. Using default lifespan.");
                    lifespan = k_MaxLifespanApiKey;
                }

                var tokenRegistration = cancellationToken.Register(() =>
                {
                    if (s_PendingSaveRequestsByAnchorId.Remove(anchorId, out var saveRequest))
                    {
                        saveRequest.tokenRegistration.Dispose();
                        UnityARCore_anchors_cancelSaveAnchor(anchorId);
                        saveRequest.completionSource.SetCanceled();
                        saveRequest.completionSource.Reset();
                        s_SaveCompletionSources.Release(saveRequest.completionSource);
                    }
                    else
                    {
                        Debug.LogError($"An unknown error occurred when canceling {nameof(TrySaveAnchorAsync)}.");
                    }
                });

                var completionSource = s_SaveCompletionSources.Get();
                s_PendingSaveRequestsByAnchorId[anchorId] = new SaveRequest(completionSource, tokenRegistration);

                var synchronousResultStatus = UnityARCore_anchors_trySaveAnchorAsync(anchorId, (int)lifespan);
                if (synchronousResultStatus.IsError())
                {
                    tokenRegistration.Dispose();
                    s_SaveCompletionSources.Release(completionSource);
                    s_PendingSaveRequestsByAnchorId.Remove(anchorId);
                    var result = new Result<SerializableGuid>(synchronousResultStatus, default);
                    return AwaitableUtils<Result<SerializableGuid>>.FromResult(result);
                }

                return completionSource.Awaitable;
            }

            public override Awaitable<Result<XRAnchor>> TryLoadAnchorAsync(
                SerializableGuid savedAnchorGuid, CancellationToken cancellationToken = default)
            {
                if (cancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException();

                if (s_PendingLoadRequestsByUuid.ContainsKey(savedAnchorGuid))
                {
                    Debug.LogError($"Cannot load persistent anchor GUID [{savedAnchorGuid}] while loading for it is already in progress!");
                    var result = new Result<XRAnchor>(new(StatusCode.ValidationFailure), XRAnchor.defaultValue);
                    return AwaitableUtils<Result<XRAnchor>>.FromResult(result);
                }

                var tokenRegistration = cancellationToken.Register(() =>
                {
                    if (s_PendingLoadRequestsByUuid.Remove(savedAnchorGuid, out var loadRequest))
                    {
                        loadRequest.tokenRegistration.Dispose();
                        UnityARCore_anchors_cancelLoadAnchor(savedAnchorGuid);
                        loadRequest.completionSource.SetCanceled();
                        loadRequest.completionSource.Reset();
                        s_LoadCompletionSources.Release(loadRequest.completionSource);
                    }
                    else
                    {
                        Debug.LogError($"An unknown error occurred when canceling {nameof(TryLoadAnchorAsync)}.");
                    }
                });

                var completionSource = s_LoadCompletionSources.Get();
                s_PendingLoadRequestsByUuid[savedAnchorGuid] =
                    new LoadRequest(completionSource, tokenRegistration);

                var synchronousResultStatus = UnityARCore_anchors_tryLoadAnchorAsync(savedAnchorGuid);
                if (synchronousResultStatus.IsError())
                {
                    tokenRegistration.Dispose();
                    s_LoadCompletionSources.Release(completionSource);
                    s_PendingLoadRequestsByUuid.Remove(savedAnchorGuid);
                    var result = new Result<XRAnchor>(synchronousResultStatus, XRAnchor.defaultValue);
                    return AwaitableUtils<Result<XRAnchor>>.FromResult(result);
                }

                return completionSource.Awaitable;
            }

            /// <summary>
            /// Function pointer marshaled to native API to call when <see cref="TrySaveAnchorAsync"/> is complete.
            /// </summary>
            static readonly IntPtr s_SaveAsyncCallback =
                Marshal.GetFunctionPointerForDelegate((SaveAsyncDelegate)OnSaveAsyncComplete);

            /// <summary>
            /// Function pointer marshaled to native API to call when <see cref="TryLoadAnchorAsync"/> is complete.
            /// </summary>
            static readonly IntPtr s_LoadAsyncCallback =
                Marshal.GetFunctionPointerForDelegate((LoadAsyncDelegate)OnLoadAsyncComplete);

            /// <summary>
            /// Delegate method type for <see cref="ARCoreProvider.s_SaveAsyncCallback"/>.
            /// </summary>
            delegate void SaveAsyncDelegate(TrackableId anchorId, SerializableGuid cloudAnchorId, XRResultStatus resultStatus);

            /// <summary>
            /// Delegate method type for <see cref="ARCoreProvider.s_LoadAsyncCallback"/>.
            /// </summary>
            delegate void LoadAsyncDelegate(XRAnchor anchor, SerializableGuid cloudAnchorId, XRResultStatus resultStatus);

            [MonoPInvokeCallback(typeof(SaveAsyncDelegate))]
            static async void OnSaveAsyncComplete(
                TrackableId anchorId, SerializableGuid cloudAnchorId, XRResultStatus resultStatus)
            {
                try
                {
                    await Awaitable.MainThreadAsync();

                    if (!s_PendingSaveRequestsByAnchorId.Remove(anchorId, out var saveRequest))
                    {
                        Debug.LogError($"An unknown error occurred during a system callback for {nameof(TrySaveAnchorAsync)}.");
                        return;
                    }

                    saveRequest.tokenRegistration.Dispose();
                    var completionSource = saveRequest.completionSource;
                    completionSource.SetResult(new Result<SerializableGuid>(resultStatus, cloudAnchorId));
                    completionSource.Reset();
                    s_SaveCompletionSources.Release(completionSource);
                }
                catch (OperationCanceledException)
                {
                    // do nothing
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            [MonoPInvokeCallback(typeof(LoadAsyncDelegate))]
            static async void OnLoadAsyncComplete(
                XRAnchor anchor, SerializableGuid cloudAnchorId, XRResultStatus resultStatus)
            {
                try
                {
                    // need to be on main thread to modify pending requests dictionary
                    await Awaitable.MainThreadAsync();

                    if (!s_PendingLoadRequestsByUuid.Remove(cloudAnchorId, out var loadRequest))
                    {
                        Debug.LogError($"An unknown error occurred during a system callback for {nameof(TryLoadAnchorAsync)}.");
                        return;
                    }

                    loadRequest.tokenRegistration.Dispose();
                    var completionSource = loadRequest.completionSource;
                    completionSource.SetResult(new Result<XRAnchor>(resultStatus, anchor));
                    completionSource.Reset();
                    s_LoadCompletionSources.Release(completionSource);
                }
                catch (OperationCanceledException)
                {
                    // do nothing
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            [DllImport(Constants.k_LibraryName)]
            static extern void UnityARCore_anchors_create(IntPtr saveCallback, IntPtr loadCallback);

            [DllImport(Constants.k_LibraryName)]
            static extern void UnityARCore_anchors_start();

            [DllImport(Constants.k_LibraryName)]
            static extern void UnityARCore_anchors_stop();

            [DllImport(Constants.k_LibraryName)]
            static extern void UnityARCore_anchors_onDestroy();

            [DllImport(Constants.k_LibraryName)]
            static extern unsafe void* UnityARCore_anchors_acquireChanges(
                out void* addedPtr, out int addedCount,
                out void* updatedPtr, out int updatedCount,
                out void* removedPtr, out int removedCount,
                out int elementSize);

            [DllImport(Constants.k_LibraryName)]
            static extern unsafe void UnityARCore_anchors_releaseChanges(void* changes);

            [DllImport(Constants.k_LibraryName)]
            static extern bool UnityARCore_anchors_tryAdd(Pose pose, out XRAnchor anchor);

            [DllImport(Constants.k_LibraryName)]
            static extern bool UnityARCore_anchors_tryAttach(
                TrackableId trackableToAffix, Pose pose, out XRAnchor anchor);

            [DllImport(Constants.k_LibraryName)]
            static extern bool UnityARCore_anchors_tryRemove(TrackableId anchorId);

            [DllImport(Constants.k_LibraryName)]
            static extern XRResultStatus UnityARCore_anchors_estimateFeatureMapQualityForHosting(
                TrackableId anchorId, ref ArFeatureMapQuality quality);

            [DllImport(Constants.k_LibraryName)]
            static extern XRResultStatus UnityARCore_anchors_trySaveAnchorAsync(TrackableId anchorId, int lifespan);

            [DllImport(Constants.k_LibraryName)]
            static extern XRResultStatus UnityARCore_anchors_cancelSaveAnchor(TrackableId anchorId);

            [DllImport(Constants.k_LibraryName)]
            static extern XRResultStatus UnityARCore_anchors_tryLoadAnchorAsync(SerializableGuid savedAnchorGuid);

            [DllImport(Constants.k_LibraryName)]
            static extern XRResultStatus UnityARCore_anchors_cancelLoadAnchor(SerializableGuid savedAnchorGuid);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            if (!Api.platformAndroid || !Api.loaderPresent)
                return;

            var cinfo = new XRAnchorSubsystemDescriptor.Cinfo
            {
                id = "ARCore-Anchor",
                providerType = typeof(ARCoreAnchorSubsystem.ARCoreProvider),
                subsystemTypeOverride = typeof(ARCoreAnchorSubsystem),
                supportsTrackableAttachments = true,
                supportsSynchronousAdd = true,
                supportsSaveAnchorDelegate = () => ARCoreRuntimeSettings.Instance.enableCloudAnchors,
                supportsLoadAnchorDelegate = () => ARCoreRuntimeSettings.Instance.enableCloudAnchors,
                supportsEraseAnchorDelegate = () => false,
                supportsGetSavedAnchorIdsDelegate = () => false,
                supportsAsyncCancellation = true,
            };

            XRAnchorSubsystemDescriptor.Register(cinfo);
        }

        /// <summary>
        /// Returns the quality of feature points seen in the preceding few seconds from a given anchor.
        /// Refer to ARCore docs for more information:
        /// https://developers.google.com/ar/develop/c/cloud-anchors/developer-guide#check_the_mapping_quality_of_feature_points
        /// </summary>
        /// <param name="anchorId">The ID of the anchor</param>
        /// <param name="quality">The feature map quality of the anchor</param>
        /// <returns>The result status</returns>
        public XRResultStatus EstimateFeatureMapQualityForHosting(TrackableId anchorId, ref ArFeatureMapQuality quality)
        {
            var p = (ARCoreProvider)provider;
            if (provider != null)
            {
                return p.EstimateFeatureMapQualityForHosting(anchorId, ref quality);
            }

            Debug.LogError($"{nameof(ARCoreProvider)} not found. Unable to estimate feature map quality.");
            return new XRResultStatus(XRResultStatus.StatusCode.ValidationFailure);
        }

        /// <summary>
        /// Attempts to persistently save the given anchor so that it can be loaded in a future AR session.
        /// This method takes a lifespan parameter that indicates how long the anchor should persist for.
        /// The platform may have a maximum lifespan that cannot be exceeded.
        /// </summary>
        /// <param name="anchorId">The TrackableId of the anchor to save.</param>
        /// <param name="lifespan">The lifespan (in days) of the anchor.</param>
        /// <param name="cancellationToken">An optional `CancellationToken` that you can use to cancel the operation
        /// in progress if the loaded provider <see cref="XRAnchorSubsystemDescriptor.supportsAsyncCancellation"/>.</param>
        /// <returns>The result of the async operation, containing a new persistent anchor GUID if the operation
        /// succeeded. You are responsible to <see langword="await"/> this result.</returns>
        /// <seealso cref="XRAnchorSubsystemDescriptor.supportsSaveAnchor"/>
        public Awaitable<Result<SerializableGuid>> TrySaveAnchorWithLifespanAsync(
            TrackableId anchorId, uint lifespan, CancellationToken cancellationToken = default)
        {
            var p = (ARCoreProvider)provider;
            return p.TrySaveAnchorWithLifespanAsync(anchorId, lifespan, cancellationToken);
        }
    }
}

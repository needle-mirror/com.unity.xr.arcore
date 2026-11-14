---
uid: arcore-upgrade-guide-7-0
---
# Upgrade to Google ARCore plug-in 7.0

Upgrade your project to Google ARCore plug-in 7.0.

Google ARCore plug-in 7.0 contains breaking changes which require action to successfully upgrade your project when you [upgrade the package](xref:um-upm-ui-update) from previous versions.

The following sections outline the changes you need to make to your project to use Google ARCore plug-in 7.0.

## Depth subsystem is replaced by the point cloud subsystem

Google ARCore plug-in 7.0 removes `ARCoreXRDepthSubsystem`, `ARCoreXRPointCloudSubsystem`, and `ARCoreLoader.depthSubsystem`. These APIs were deprecated in earlier versions. If your project references them, it won't compile.

To upgrade, replace `ARCoreXRDepthSubsystem` and `ARCoreXRPointCloudSubsystem` with [`ARCorePointCloudSubsystem`](xref:UnityEngine.XR.ARCore.ARCorePointCloudSubsystem), and replace `ARCoreLoader.depthSubsystem` with [`ARCoreLoader.pointCloudSubsystem`](xref:UnityEngine.XR.ARCore.ARCoreLoader.pointCloudSubsystem).

## File path-based recording and playback APIs are obsolete

The session recording and playback methods that take a file path were deprecated in version 6.1 and are obsolete in 7.0, so they produce compile errors instead of warnings. If your project uses them, it won't compile.

To upgrade session recording and playback, replace each method with its URI-based equivalent.

| Previous API | Replacement |
| :-- | :-- |
| `ARCoreSessionSubsystem.StartPlayback(string)` | [`ARCoreSessionSubsystem.StartPlaybackUri(string)`](xref:UnityEngine.XR.ARCore.ARCoreSessionSubsystem.StartPlaybackUri(System.String)) |
| `ARCoreSessionSubsystem.StopPlayback()` | [`ARCoreSessionSubsystem.StopPlaybackUri()`](xref:UnityEngine.XR.ARCore.ARCoreSessionSubsystem.StopPlaybackUri) |
| `ArSession.SetPlaybackDataset(string)` | [`ArSession.SetPlaybackDatasetUri(string)`](xref:UnityEngine.XR.ARCore.ArSession.SetPlaybackDatasetUri(System.String)) |
| `ArRecordingConfig.GetMp4DatasetFilePath(ArSession)` | [`ArRecordingConfig.GetMp4DatasetUri(ArSession)`](xref:UnityEngine.XR.ARCore.ArRecordingConfig.GetMp4DatasetUri(UnityEngine.XR.ARCore.ArSession)) |
| `ArRecordingConfig.SetMp4DatasetFilePath(ArSession, string)` | [`ArRecordingConfig.SetMp4DatasetUri(ArSession, string)`](xref:UnityEngine.XR.ARCore.ArRecordingConfig.SetMp4DatasetUri(UnityEngine.XR.ARCore.ArSession,System.String)) |

The URI-based methods take a URI instead of a file path. For more information, refer to [Session recording](xref:arcore-session-recording).

## Background shader name is obsolete

`ARCoreCameraSubsystem.backgroundShaderName` is obsolete and produces a compile error instead of a warning. If your project uses it, it won't compile.

To upgrade, use [`ARCoreCameraSubsystem.backgroundShaderNames`](xref:UnityEngine.XR.ARCore.ARCoreCameraSubsystem.backgroundShaderNames) instead.

## Additional resources

- [What's new in version 7.0](xref:arcore-whats-new)
- [Changelog](xref:arcore-changelog)

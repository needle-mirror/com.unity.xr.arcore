---
uid: arcore-whats-new
---
# What's new in version 6.0

The most significant updates in this release include:

## Added

- Added support for Image Stabilization, which helps stabilize shaky video from the camera.
- Added support for `XRCameraSubsystem.GetShaderKeywords` to `ARCoreCameraSubsystem` and `ARCoreOcclusionSubsystem`.

## Changed

- Updated to ARCore 1.37.

## Deprecated

- `ARCoreXRPointCloudSubsystem` has been deprecated and renamed to `ARCorePointCloudSubsystem` for consistency with other subsystems. Unity's API Updater should automatically convert any deprecated APIs references to the new APIs when the project is loaded into the Editor again.

## Removed

| Obsolete API                                                                 | Recommendation                                                                        |
|:------------------------------------------------------------------------------|:--------------------------------------------------------------------------------------|
| `ARCoreSettingsProvider`                                                      | This class is now deprecated. Its internal functionality is replaced by XR Management |
| `ARCoreBeforeSetConfigurationEventArgs.session`                               | Use arSession to access the session.                                                  |
| `ARCoreBeforeSetConfigurationEventArgs.config`                                | Use arConfig to access the configuration.                                             |
| `ARCoreBeforeSetConfigurationEventArgs.ARCoreBeforeSetConfigurationEventArgs` | Use ARCoreBeforeSetConfigurationEventArgs(ArSession, ArConfig) instead.               |
| `ArCameraConfig.Null`                                                         | Use default instead.                                                                  |
| `ArCameraConfig.IsNull`                                                       | Compare to null instead.                                                              |
| `ArCameraConfigFilter.Null`                                                   | Use default instead.                                                                  |
| `ArCameraConfigFilter.IsNull`                                                 | Compare to null instead.                                                              |
| `ArConfig.Null`                                                               | Use default instead.                                                                  |
| `ArConfig.IsNull`                                                             | Compare to null instead.                                                              |
| `ArSession.Null`                                                              | Use default instead.                                                                  |
| `ArSession.IsNull`                                                            | Compare to null instead.                                                              |

For a full list of changes and updates in this version, see the [ARCore XR Plug-in package changelog](xref:arcore-changelog).

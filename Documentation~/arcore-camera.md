---
uid: arcore-camera
---
# Camera

## Light estimation

ARCore light estimation can operate in two modes:

- `LightEstimationMode.AmbientIntensity`: Providers color correction and average pixel intensity information.
- `LightEstimationMode.EnvironmentalHDR`: Provides an estimated Main Light Direction, HDR Color, and the ambient SphericalHarmonicsL2 (see [SphericalHarmonicsL2](https://docs.unity3d.com/ScriptReference/Rendering.SphericalHarmonicsL2.html) for more information on Spherical Harmonics).

You can’t use both modes at the same time.

ARCore's [Face tracking](xref:UnityEngine.XR.ARCore.ARCoreFaceSubsystem) and [Environment probes](xref:UnityEngine.XR.ARSubsystems.XREnvironmentProbeSubsystem) use or affect the light estimation mode.  If one or both of these subsystems is present and `enabled`, it changes the light estimation mode behavior depending on the configuration:

| Functionality      | Supported light estimation modes                                       | Modifiable |
|--------------------|------------------------------------------------------------------------|------------|
| Face tracking      | `LightEstimationMode.AmbientIntensity`, `LightEstimationMode.Disabled` | Yes        |
| Environment probes | `LightEstimationMode.EnvironmentalHDR`                                 | No         |

* [Face tracking](xref:UnityEngine.XR.ARCore.ARCoreFaceSubsystem): ARCore doesn't support `LightEstimationMode.EnvironmentalHDR` when face tracking is enabled and rendering won't work when this mode is specified. To prevent errors, you can only set `LightEstimationMode.AmbientIntensity` or `LightEstimationMode.Disabled` when face tracking is enabled, or ARCore enforces `LightEstimationMode.Disabled`.

*  [Environment probes](xref:UnityEngine.XR.ARSubsystems.XREnvironmentProbeSubsystem): For ARCore environment probes to update the cubemap, the light estimation mode must be set to `LightEstimationMode.EnvironmentalHDR`. This also allows ARCore to take ownership of the setting.

## Camera configuration

[XRCameraConfiguration](xref:UnityEngine.XR.ARSubsystems.XRCameraConfiguration) contains an `IntPtr` field `nativeConfigurationHandle`, which is a platform-specific handle. For ARCore, this handle is the pointer to the `ArCameraConfiguration`. The native object is managed by Unity. Do not manually destroy it.

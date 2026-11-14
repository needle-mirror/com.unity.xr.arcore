---
uid: arcore-manual
---
# Google ARCore XR Plug-in

Use the Google ARCore XR Plug-in package to enable ARCore support in your [AR Foundation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@latest) project. This package implements the following AR features using ARCore 1.48:

| Feature | Description |
| :------ | :---------- |
| [Session](xref:arcore-session) | Enable, disable, and configure AR on the target platform. |
| Device tracking | Track the device's position and rotation in physical space. |
| [Camera](xref:arcore-camera) | Render images from device cameras and perform light estimation. |
| [Plane detection](xref:arcore-plane-detection) | Detect and track flat surfaces. |
| [Image tracking](xref:arcore-image-tracking) | Detect and track 2D images. |
| [Face tracking](xref:arcore-face-tracking) | Detect and track human faces. |
| [Point clouds](xref:arcore-point-clouds) | Detect and track feature points. |
| [Ray casts](xref:arcore-raycasts) | Cast rays against tracked items. |
| [Anchors](xref:arcore-anchors) | Track arbitrary points in space. |
| [Environment probes](xref:arcore-environment-probes) | Generate cubemaps of the environment. |
| [Occlusion](xref:arcore-occlusion) | Occlude AR content with physical objects and perform human segmentation. |

## Unsupported features

This package does not implement the following AR features, as they are not supported by ARCore 1.48:

| Feature | Description |
| :------ | :---------- |
| [Bounding Box detection](xref:arfoundation-bounding-box-detection) | Detect and track bounding boxes of 3D objects. |
| [Object tracking](xref:arfoundation-object-tracking) | Detect and track 3D objects. |
| [Body tracking](xref:arfoundation-body-tracking) | Detect and track a human body. |
| [Meshing](xref:arfoundation-meshing) | Generate meshes of the environment. |
| [Participants](xref:arfoundation-participant-tracking) | Track other devices in a shared AR session. |

## Install the Google ARCore XR Plug-in

To check the requirements and install this package, refer to [Install the Google ARCore plug-in](xref:arcore-install).

## Samples

You can access pre-configured sample scenes that demonstrate AR Foundation features from the [AR Foundation Samples](https://github.com/Unity-Technologies/arfoundation-samples) GitHub repository. This project also contains [ARCore samples](xref:arfoundation-samples-arcore) that demonstrate ARCore-specific functionality. To understand how to use these samples, and learn more about each scene, refer to [AR Foundation samples](xref:arfoundation-samples).

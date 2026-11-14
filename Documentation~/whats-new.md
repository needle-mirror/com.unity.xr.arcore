---
uid: arcore-whats-new
---
# What's new in version 7.0

This release includes the following significant changes:

## Breaking changes

Google ARCore plug-in 7.0 contains breaking changes:

- `ARCoreXRDepthSubsystem` and `ARCoreXRPointCloudSubsystem` are removed. Use `ARCorePointCloudSubsystem` instead.
- APIs deprecated in earlier versions are obsolete and produce compile errors, including the file path-based session recording and playback APIs.

To upgrade your project, refer to [Upgrade to Google ARCore plug-in 7.0](xref:arcore-upgrade-guide-7-0).

For a full list of changes in this version including backwards-compatible bugfixes, refer to the package [changelog](xref:arcore-changelog).

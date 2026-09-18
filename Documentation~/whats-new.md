---
uid: arcore-whats-new
---
# What's new in version 6.7

This release includes the following significant changes:

## ARCore installation

- Added `ARCoreSessionSubsystem.TryStartAsync`, an override that starts the session and, if `XRSubsystemStartOptions.InstallSoftwareIfNeeded` is set, checks whether Google Play Services for AR (ARCore) is installed and installs it if needed before starting.
- Added `ARCoreSessionSubsystem.TryInstallAsync`, which asynchronously checks whether Google Play Services for AR (ARCore) is installed on the device and attempts to install it if needed.

For a full list of changes in this version including backwards-compatible bugfixes, refer to the package [changelog](xref:arcore-changelog).

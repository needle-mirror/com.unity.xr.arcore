---
uid: arcore-whats-new
---
# What's new in version 6.1

## New features

### Vulkan Graphics API rendering support

- Added support within camera background rendering for the Vulkan Graphics API.  Refer to [Project Configuration](xref:arcore-project-config) for more information.
- Added three new session subsystem API override methods to the `ARCoreSessionSubsystem` provider class to handle Universal Render Pipeline
rendering events signaled by the `ARCommandBufferSupportRendererFeature` in order to support rendering through the Vulkan Graphics API: `ARCoreSessionSubsystem.requiresCommandBuffer`, `ARCoreSessionSubsystem.OnCommandBufferSupportEnabled`, and `ARCoreSessionSubsystem.OnCommandBufferExecute`.

For a full list of changes in this version including backwards-compatible bugfixes, refer to the package [changelog](xref:arcore-changelog).

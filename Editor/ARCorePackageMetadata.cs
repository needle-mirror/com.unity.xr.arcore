#if USE_XR_MANAGEMENT
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEngine.XR.ARCore;

using UnityEditor;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;

namespace UnityEditor.XR.ARCore
{
    class XRPackage : IXRPackage
    {
        private class ARCoreLoaderMetadata : IXRLoaderMetadata
        {
            public string loaderName { get; set; }
            public string loaderType { get; set; }
            public List<BuildTargetGroup> supportedBuildTargets { get; set; }
        }

        private class ARCorePackageMetadata : IXRPackageMetadata
        {
            public string packageName { get; set; }
            public string packageId { get; set; }
            public string settingsType { get; set; }
            public List<IXRLoaderMetadata> loaderMetadata { get; set; } 
        }

        private static IXRPackageMetadata s_Metadata = new ARCorePackageMetadata(){
                packageName = "ARCore XR Plugin",
                packageId = "com.unity.xr.arcore",
                settingsType = "UnityEngine.XR.ARCore.ARCoreLoaderSettings",
                loaderMetadata = new List<IXRLoaderMetadata>() {
                new ARCoreLoaderMetadata() {
                        loaderName = "ARCore",
                        loaderType = "UnityEngine.XR.ARCore.ARCoreLoader",
                        supportedBuildTargets = new List<BuildTargetGroup>() {
                            BuildTargetGroup.Android
                        }
                    },
                }
            };

        public IXRPackageMetadata metadata => s_Metadata;

        public bool PopulateNewSettingsInstance(ScriptableObject obj)
        {
            return true;
        }
    }
}
#endif
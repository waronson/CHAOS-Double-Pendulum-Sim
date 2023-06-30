using UnityEditor;
using UnityEngine;

public static class BuildScripts
{
#if UNITY_CLOUD_BUILD
	public static void PreExport(UnityEngine.CloudBuild.BuildManifestObject manifest)
	{
		int buildNumber = int.Parse(manifest.GetValue<string>("buildNumber"));
		PlayerSettings.iOS.buildNumber = buildNumber.ToString();
		PlayerSettings.Android.bundleVersionCode = buildNumber;
	}
#endif
}

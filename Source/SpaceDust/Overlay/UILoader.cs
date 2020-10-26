using System;
using System.IO;
using UnityEngine;


namespace SpaceDust
{
  [KSPAddon(KSPAddon.Startup.Instantly, true)]
  public class UILoader : MonoBehaviour
  {
    public static GameObject ToolbarWidgetPrefab { get; private set; }

    public static GameObject BandResourceWidgetPrefab { get; private set; }
    public static GameObject ToolbarPanelPrefab { get; private set; }
    

    private void Awake()
    {

      Utils.Log("[UILoader]: Loading UI Prefabs");
      AssetBundle prefabs = AssetBundle.LoadFromFile(Path.Combine(KSPUtil.ApplicationRootPath, "GameData/SpaceDust/UI/spacedustui.dat"));
      ToolbarWidgetPrefab = prefabs.LoadAsset("SpaceDustEnhancedResourceElement") as GameObject;
      BandResourceWidgetPrefab = prefabs.LoadAsset("BandDataWidget") as GameObject;
      ToolbarPanelPrefab = prefabs.LoadAsset("SpaceDustToolbar") as GameObject;
      Utils.Log("[UILoader]: Loaded UI Prefabs");
    }
  }
}

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceDust
{
  /// <summary>
  /// Loads UI assets
  /// </summary>
  [KSPAddon(KSPAddon.Startup.Instantly, true)]
  public class SpaceDustAssets : MonoBehaviour
  {
    public static GameObject ToolbarWidgetPrefab { get; private set; }
    public static GameObject BandResourceWidgetPrefab { get; private set; }
    public static GameObject ToolbarPanelPrefab { get; private set; }
    public static GameObject OverlayInspectorPrefab { get; private set; }
    public static Dictionary<string, Sprite> Sprites { get; private set; }

    internal static string ASSET_PATH = "GameData/SpaceDust/UI/spacedustui.dat";
    internal static string SPRITE_ATLAS_NAME = "space-dust-sprites-1";
    private void Awake()
    {

      Utils.Log("[SpaceDustAssets]: Loading UI Prefabs");
      AssetBundle prefabs = AssetBundle.LoadFromFile(Path.Combine(KSPUtil.ApplicationRootPath, ASSET_PATH));

      ToolbarWidgetPrefab = prefabs.LoadAsset("SpaceDustEnhancedResourceElement") as GameObject;
      BandResourceWidgetPrefab = prefabs.LoadAsset("BandDataWidget") as GameObject;
      ToolbarPanelPrefab = prefabs.LoadAsset("SpaceDustToolbar") as GameObject;
      OverlayInspectorPrefab = prefabs.LoadAsset("SpaceDustInspector") as GameObject;

      Utils.Log("[SpaceDustAssets]: Loaded UI Prefabs");
      /// Get the Sprite Atlas
      Sprite[] spriteSheet = prefabs.LoadAssetWithSubAssets<Sprite>(SPRITE_ATLAS_NAME);
      Sprites = new Dictionary<string, Sprite>();
      foreach (Sprite subSprite in spriteSheet)
      {
        Sprites.Add(subSprite.name, subSprite);
      }
      Utils.Log($"[SpaceDustAssets]: Loaded {Sprites.Count} sprites");
    }
  }
}

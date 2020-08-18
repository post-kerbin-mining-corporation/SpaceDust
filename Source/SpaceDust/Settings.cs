using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SpaceDust
{

  /// <summary>
  /// Static class to hold settings and configuration
  /// </summary>
  public static class Settings
  {
    // Emit UI debug messages
    public static bool DebugUI = true;
    // Emit Overlay debug messages
    public static bool DebugOverlay = true;
    // Emit module  debug messages
    public static bool DebugModules = true;


    public static int particleFieldBaseCount = 2000;
    public static float particleFieldBaseSize = 2f;
    public static string particleFieldShaderName = "KSP/Particles/Additive";
    public static string particleFieldTextureUrl = "SpaceDust/Assets/spacedust-particle-dust";
    public static string particleFieldTrailTextureUrl = "SpaceDust/Assets/particleTextureTrail";
    public static Color resourceDiscoveredColor = new Color(0.5f, 0.5f, 0.5f);
    public static Dictionary<string, Color> resourceColors = new Dictionary<string, Color>
    {
      ["XenonGas"] = new Color(0.376f, 0.655f, 0.749f),
      ["Antimatter"] = new Color(1f, 0f, 0f),
      ["ArgonGas"] = new Color(1f, 0.69f, 1f)
    };
    public static Color GetResourceColor(string resourceName)
    {
      if (Settings.resourceColors.ContainsKey(resourceName))
      {
        return Settings.resourceColors[resourceName];
      }
      return Color.white;
    }
    /// <summary>
    /// Load data from configuration
    /// </summary>
    public static void Load()
    {
      ConfigNode settingsNode;

      Utils.Log("[Settings]: Started loading");
      
      ConfigNode[] settingsNodes = GameDatabase.Instance.GetConfigNodes("SPACEDUSTSETTINGS");
      if (settingsNodes.Length > 0)
      {
        settingsNode = settingsNodes[0];
      }
      else
      {
        Utils.Log("[Settings]: Couldn't find settings file, using defaults");
      }

     
      Utils.Log("[Settings]: Finished loading");
    }
  }
}

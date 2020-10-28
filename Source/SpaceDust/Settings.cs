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
    // Emit module  debug messages
    public static bool DebugBackground = true;

    public static bool SetAllDiscovered = false;
    public static bool SetAllIdentified = false;
    public static float BaseTelescopeDiscoverRate = .0001f;
    public static float BaseDiscoverScienceReward = 5f;
    public static float BaseIdentifyScienceReward = 5f;

    public static int particleFieldBaseCount = 2000;
    public static float particleFieldBaseSize = 2f;
    public static float particleFieldMaxViewportParticleScale = 0.01f;
    public static int particleFieldMaxParticleCount = 25000;
    public static string particleFieldShaderName = "KSP/Particles/Additive";
    public static string particleFieldTextureUrl = "SpaceDust/Assets/spacedust-particle-dust";
    public static string particleFieldTrailTextureUrl = "SpaceDust/Assets/particleTextureTrail";
    public static Color resourceDiscoveredColor = new Color(0.5f, 0.5f, 0.5f);
    public static Dictionary<string, Color> resourceColors = new Dictionary<string, Color>
    {
      ["XenonGas"] = new Color(0.376f, 0.655f, 0.749f),
      ["Antimatter"] = new Color(1f, 0f, 0f),
      ["ArgonGas"] = new Color(1f, 0.69f, 1f),
      ["LqdHydrogen"] = new Color(0.33f, 0.38f, .42f),
      ["LqdHe3"] = new Color(0.313f, 0.388f, 0.38f),
      ["LqdDeuterium"] = new Color(1f, 0.69f, 1f)
    };

    public static List<string> visibleResources;

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
        settingsNode.TryGetValue("DebugUI", ref DebugUI);
        // Emit Overlay debug messages
        settingsNode.TryGetValue("DebugOverlay", ref DebugOverlay);
        // Emit module  debug messages
        settingsNode.TryGetValue("DebugModules", ref DebugModules);

        settingsNode.TryGetValue("SetAllDiscovered", ref SetAllDiscovered);
        settingsNode.TryGetValue("SetAllIdentified", ref SetAllIdentified);
        settingsNode.TryGetValue("BaseTelescopeDiscoverRate", ref BaseTelescopeDiscoverRate);
        settingsNode.TryGetValue("BaseDiscoverScienceReward", ref BaseDiscoverScienceReward);
        settingsNode.TryGetValue("BaseIdentifyScienceReward", ref BaseIdentifyScienceReward);

        settingsNode.TryGetValue("particleFieldBaseCount", ref particleFieldBaseCount);
        settingsNode.TryGetValue("particleFieldBaseSize", ref particleFieldBaseSize);
        settingsNode.TryGetValue("particleFieldShaderName", ref particleFieldShaderName);
        settingsNode.TryGetValue("particleFieldTextureUrl", ref particleFieldTextureUrl);
        settingsNode.TryGetValue("particleFieldTrailTextureUrl", ref particleFieldTrailTextureUrl);
        settingsNode.TryGetValue("particleFieldMaxParticleCount", ref particleFieldMaxParticleCount);
        settingsNode.TryGetValue("particleFieldMaxViewportParticleScale", ref particleFieldMaxViewportParticleScale);

        ConfigNode colorNode = settingsNode.GetNode("ResourceColors");
        resourceColors = new Dictionary<string, Color>();
        foreach (PartResourceDefinition defn in PartResourceLibrary.Instance.resourceDefinitions)
        {
          if (colorNode.HasValue(defn.name))
          {
            Color c = Color.white;
            colorNode.TryGetValue(defn.name, ref c);
            resourceColors.Add(defn.name, c);
          }
          else
          {
            UnityEngine.Random.InitState(defn.name.ToCharArray().Sum(x => x) % 100);
            resourceColors.Add(defn.name, UnityEngine.Random.ColorHSV());
          }
        }

        ConfigNode shownNodes = settingsNode.GetNode("ResourceVisibilities");
        visibleResources = shownNodes.GetValuesList("name");
      }
      else
      {
        Utils.Log("[Settings]: Couldn't find settings file, using defaults");
      }


      Utils.Log("[Settings]: Finished loading");
    }
  }
}

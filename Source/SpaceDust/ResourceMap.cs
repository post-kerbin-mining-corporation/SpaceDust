using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SpaceDust
{
  /// <summary>
  /// This holds the data about all resources in the game
  /// </summary>
  [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
  public class SpaceDustResourceMap : MonoBehaviour
  {
    public static SpaceDustResourceMap Instance { get; private set; }

    protected Dictionary<string, List<ResourceDistribution>> Resources;

    protected void Awake()
    {
      Instance = this;
    }
    public void Load()
    {
      ConfigNode[] resourceDistributionNodes = GameDatabase.Instance.GetConfigNodes(Settings.RESOURCE_DATA_NODE_NAME);
      Resources = new Dictionary<string, List<ResourceDistribution>>();
      List<ResourceDistribution> distros = new List<ResourceDistribution>();
      foreach (ConfigNode node in resourceDistributionNodes)
      {
        try
        {
          distros.Add(new ResourceDistribution(node));
        }
        catch (Exception)
        {
          throw new TypeLoadException(node.ToString());
        }
      }
      Utils.Log($"[SpaceDustResourceMap]: Loaded {distros.Count} resource distributions");
      List<string> bodyKeys = distros.Select(x => x.Body).Distinct().ToList();

      foreach (string bodyKey in bodyKeys)
      {
        List<ResourceDistribution> bodyDists = distros.FindAll(x => x.Body == bodyKey).ToList();
        Resources.Add(bodyKey, bodyDists);
        Utils.Log($"[SpaceDustResourceMap]: Loaded {bodyDists.Count} resource distributions for {bodyKey}");
      }
    }
    /// <summary>
    /// Samples a specified resource based on a location and return a value in t/m3
    /// </summary>
    /// <param name="ResourceName"></param>
    /// <param name="body"></param>
    /// <param name="altitude"></param>
    /// <param name="latitude"></param>
    /// <param name="longitude"></param>
    /// <returns></returns>
    public double SampleResource(string ResourceName, CelestialBody body, double altitude, double latitude, double longitude)
    {
      double sampledTotal = 0d;
      if (Resources.ContainsKey(body.name))
      {
        for (int i = 0; i < Resources[body.name].Count; i++)
        {
          if (Resources[body.name][i].ResourceName == ResourceName)
          {
            sampledTotal += Resources[body.name][i].Sample(altitude, latitude, longitude);
          }
        }
      }
      return sampledTotal;
    }

    /// <summary>
    /// Gets the list of resources available on a CelestialBody
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    public List<string> GetBodyResources(CelestialBody body)
    {
      List<string> getResources = new List<string>();
      if (Resources != null && body != null)
      {
        if (Resources.ContainsKey(body.name))
        {
          for (int i = 0; i < Resources[body.name].Count; i++)
          {
            getResources.Add(Resources[body.name][i].ResourceName);
          }
        }
      }
      return getResources;
    }

    /// <summary>
    /// Get all the Resource Bands at a given celestial body
    /// </summary>
    /// <param name="body"></param>
    /// <param name="resourceName"></param>
    /// <returns></returns>
    public List<ResourceBand> GetBodyDistributions(CelestialBody body, string resourceName)
    {
      List<ResourceBand> dist = new List<ResourceBand>();

      if (Resources.ContainsKey(body.name))
      {
        for (int i = 0; i < Resources[body.name].Count; i++)
        {
          if (Resources[body.name][i].ResourceName == resourceName)
            dist = Resources[body.name][i].Bands;

        }
      }
      return dist;
    }
  }
}

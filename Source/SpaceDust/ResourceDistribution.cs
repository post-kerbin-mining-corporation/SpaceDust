/// Defines the classes t
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SpaceDust
{
  /// Represents a continuous band of a single resource
  public class ResourceBand
  {
    double abundance = 0d;

    double minAbundance = 0d;
    double maxAbundance = 0d;
    bool useAirDensity = false;
    DistributionModel distribution;
    FalloffModelType falloffType;

    CelestialBody associatedBody;

    public ResourceBand(ConfigNode node)
    {
      node.TryGetValue("minAbundance", ref minAbundance);
      node.TryGetValue("maxAbundance", ref maxAbundance);
      node.TryGetValue("useAirDensity", ref useAirDensity);

      string distName = "Uniform";
      node.TryGetValue("distributionType", ref distName);

      if (distName == "Uniform")
        distribution = DistributionMode(new UniformDistributionModel(node));
      if (distName == "Spherical")
        distribution = DistributionMode(new SphericalDistributionModel(node));
    }

    public void Initialize()
    {
      /// Does game-time initializations
      distribution.Initialize();

      Random generator = new Random(HighLogic.currentGame.Seed);
      abundance = generator.Next(minAbundance, maxAbundance);
    }

    /// Sample the resource band
    public double Sample(double altitude, double latitude, double longitude)
    {
      double sampleResult = abundance * distribution.Sample(altitude, latitude, longitude);

      if (useAirDensity)
        sampleResult *= associatedBody.airDensity;

      return sampleReusult;
    }

    /// TODO: Make me better
    public string ToString()
    {
      return String.Format("Resource Band object");
    }
  }

  // Represents the full set of resources around a body
  public class ResourceDistribution
  {
    string resourceName;
    string body;

    List<ResourceBand> resourceBands;

    public ResourceDistribution(ConfigNode node)
    {
      resourceBands = new List<ResourceBand>();
      ConfigNode[] nodes = node.GetNodes("RESOURCEBAND");
      for (int i=0; i < nodes.Length; i++)
      {
        resourceBands.Add(new ResourceBand(nodes[i]));
      }

    }

    public void Initialize()
    {
      for (int i=0; i < resourceBands.Count; i++)
      {
        resourceBands[i].Initialize();
      }
    }

    public double Sample(double altitude, double latitude, double longitude)
    {
      double sampleResult = 0d;
      for (int i = 0; i< resourceBands.Count ;i++)
      {
        sampleResult += resourceBands[i].Sample(altitude, latitude, longitude);
      }
      return sampleResult;
    }
  }
}

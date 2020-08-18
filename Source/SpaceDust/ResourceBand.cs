using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceDust
{
  /// Represents a continuous band of a single resource
  public class ResourceBand
  {

    public string name = "GenericBand";
    public string ResourceName { get; private set; }
    public double Abundance { get; private set; }

    public bool AlwaysDiscovered = false;
    public bool AlwaysIdentified = false;

    double minAbundance = 0d;
    double maxAbundance = 0d;
    bool useAirDensity = false;

    FloatCurve densityCurve;
    CelestialBody associatedBody;
    
    public DistributionModel Distribution { get; private set; }

    public ResourceBand(string resourceName, ConfigNode node)
    {
      ResourceName = resourceName;
      string distName = "Uniform";
      densityCurve = new FloatCurve();
      
      node.TryGetValue("name", ref name);
      node.TryGetValue("minAbundance", ref minAbundance);
      node.TryGetValue("maxAbundance", ref maxAbundance);
      node.TryGetValue("useAirDensity", ref useAirDensity);

      node.TryGetValue("alwaysDiscovered", ref AlwaysDiscovered);
      node.TryGetValue("alwaysIdentified", ref AlwaysIdentified);

      node.TryGetValue("distributionType", ref distName);
      if (useAirDensity)
      {
        densityCurve.Load(node.GetNode("densityCurve"));
      }

      if (distName == "Uniform")
        Distribution = new UniformDistributionModel(node) as DistributionModel;
      if (distName == "Spherical")
        Distribution = new SphericalDistributionModel(node) as DistributionModel;
      
      
    }

    public void Initialize(string bodyName)
    {
      /// Does game-time initializations
      Distribution.Initialize();

      
      UnityEngine.Random.InitState(HighLogic.fetch.currentGame.Seed);
      Abundance = UnityEngine.Random.Range((float)minAbundance, (float)maxAbundance);
      foreach (CelestialBody b in FlightGlobals.Bodies)
      {
        if (bodyName == b.bodyName)
        {
          associatedBody = b;
          Distribution.BodyRadius = b.Radius;
        }
      }

      if (AlwaysDiscovered)
        SpaceDustScenario.Instance.DiscoverResourceBand( ResourceName, name, associatedBody);
      if (AlwaysIdentified)
        SpaceDustScenario.Instance.IdentifyResourceBand( ResourceName, name, associatedBody);


    }

    public bool CheckDistanceToCenter(double testAltitude, double proximityThreshold)
    {
      if (Math.Abs(testAltitude - Distribution.Center()) < proximityThreshold)
        return true;
      return false;
    }

    /// Sample the resource band
    public double Sample(double altitude, double latitude, double longitude)
    {
      double sampleResult = Abundance * Distribution.Sample(altitude, latitude, longitude);

      if (useAirDensity)
        sampleResult *= densityCurve.Evaluate((float)associatedBody.GetPressureAtm(altitude));

      return sampleResult;
    }

    /// TODO: Make me better
    public string ToString()
    {
      return String.Format($"Resource Band(Abundance={Abundance}, Distribution)");
    }
  }
}

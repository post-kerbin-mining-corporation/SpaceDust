using System.Collections.Generic;

namespace SpaceDust
{
  /// <summary>
  /// Represents the full set of bands for a single resource around a single body
  /// </summary>
  public class ResourceDistribution
  {
    public string ResourceName;
    public string Body;
    public List<ResourceBand> Bands { get; private set; }


    private const string RESOURCENAME_PARAMETER_NAME = "resourceName";
    private const string BODY_PARAMETER_NAME = "body";
    private const string BAND_NODE_NAME = "RESOURCEBAND";

    public ResourceDistribution(ConfigNode node)
    {

      node.TryGetValue(RESOURCENAME_PARAMETER_NAME, ref ResourceName);
      node.TryGetValue(BODY_PARAMETER_NAME, ref Body);


      Bands = new List<ResourceBand>();
      ConfigNode[] nodes = node.GetNodes(BAND_NODE_NAME);
      for (int i=0; i < nodes.Length; i++)
      {
        Bands.Add(new ResourceBand(ResourceName, nodes[i]));
      }
      Initialize();
    }

    public void Initialize()
    {
      for (int i=0; i < Bands.Count; i++)
      {
        Bands[i].Initialize(Body);
      }
    }

    /// <summary>
    /// Sample all the bands at this coordinate
    /// </summary>
    /// <param name="altitude"></param>
    /// <param name="latitude"></param>
    /// <param name="longitude"></param>
    /// <returns></returns>
    public double Sample(double altitude, double latitude, double longitude)
    {
      double sampleResult = 0d;
      for (int i = 0; i< Bands.Count ;i++)
      {
        
        sampleResult += Bands[i].Sample(altitude, latitude, longitude);
      }
      return sampleResult;
    }
  }
}

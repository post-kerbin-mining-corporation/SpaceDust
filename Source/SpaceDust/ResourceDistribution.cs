/// Defines the classes t
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SpaceDust
{
  

  // Represents the full set of resources around a body
  public class ResourceDistribution
  {
    public string ResourceName;
    public string Body;

    
    public List<ResourceBand> Bands { get { return resourceBands; } }

    List<ResourceBand> resourceBands;

    public ResourceDistribution(ConfigNode node)
    {

      node.TryGetValue("resourceName", ref ResourceName);
      node.TryGetValue("body", ref Body);


      resourceBands = new List<ResourceBand>();
      ConfigNode[] nodes = node.GetNodes("RESOURCEBAND");
      for (int i=0; i < nodes.Length; i++)
      {
        resourceBands.Add(new ResourceBand(ResourceName, nodes[i]));
      }
      Initialize();
    }

    public void Initialize()
    {
      for (int i=0; i < resourceBands.Count; i++)
      {
        resourceBands[i].Initialize(Body);
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

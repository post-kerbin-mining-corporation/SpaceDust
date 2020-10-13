using KSP.Localization;
using System.Collections.Generic;

namespace SpaceDust
{

  public class SpaceDustDiscoveryData
  {
    public string ResourceName = "noResource";
    public string BodyName = "noBody";
    public string BandName = "noBand";
    public bool Discovered = false;
    public bool Identified = false;


    public SpaceDustDiscoveryData()
    { }
    public SpaceDustDiscoveryData(string resourceName, string bandName, string bodyName, bool isDiscovered, bool isIdentified)
    {
      ResourceName = resourceName;
      BodyName = bodyName;
      BandName = bandName;
      Discovered = isDiscovered;
      Identified = isIdentified;
    }
    public SpaceDustDiscoveryData(ConfigNode node)
    {
      Load(node);
    }
    public void Load(ConfigNode node)
    {
      node.TryGetValue("resourceName", ref ResourceName);
      node.TryGetValue("bodyName", ref BodyName);
      node.TryGetValue("bandName", ref BandName);
      node.TryGetValue("discovered", ref Discovered);
      node.TryGetValue("identified", ref Identified);
    }

    public ConfigNode Save()
    {
      ConfigNode node = new ConfigNode();
      node.name = "DISCOVERYDATA";
      node.AddValue("resourceName", ResourceName);
      node.AddValue("bodyName", BodyName);
      node.AddValue("bandName", BandName);
      node.AddValue("discovered", Discovered);
      node.AddValue("identified", Identified);
      return node;
    }

    public new string ToString()
    {
      return $"DiscoveryData(Band {BandName} around {BodyName} containing {ResourceName}: Discovered  = {Discovered},Identified = {Identified})";
    }
  }

  [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.SPACECENTER, GameScenes.FLIGHT, GameScenes.TRACKSTATION, GameScenes.EDITOR)]
  public class SpaceDustScenario : ScenarioModule
  {
    public static SpaceDustScenario Instance { get; private set; }


    protected List<SpaceDustDiscoveryData> distributionData;

    public override void OnAwake()
    {
      Utils.Log("[SpaceDustScenario]: Awake");
      Instance = this;
      base.OnAwake();
    }

    public override void OnLoad(ConfigNode node)
    {
      Utils.Log("[SpaceDustScenario]: Started Loading");
      base.OnLoad(node);
      if (distributionData == null) distributionData = new List<SpaceDustDiscoveryData>();
      foreach (ConfigNode saveNode in node.GetNodes("DISCOVERYDATA"))
      {
        distributionData.Add(new SpaceDustDiscoveryData(saveNode));
      }
      Utils.Log("[SpaceDustScenario]: Done Loading");
    }

    public override void OnSave(ConfigNode node)
    {
      Utils.Log("[SpaceDustScenario]: Started Saving");
      base.OnSave(node);
      
      if (distributionData != null)
      {
        foreach (SpaceDustDiscoveryData data in distributionData)
        {
          //node.AddNode(data.Save());
          //Utils.Log(data.ToString());
        }
      }
      Utils.Log(node.ToString());
      Utils.Log("[SpaceDustScenario]: Done Saving");
    }
    public bool IsAnyDiscovered(string resourceName, CelestialBody b)
    {
      foreach (ResourceBand band in SpaceDustResourceMap.Instance.GetBodyDistributions(b, resourceName))
      {
        if (IsDiscovered(resourceName, band.name, b.bodyName))
          return true;
      }
      return false;
       
    }
    public bool IsDiscovered(string resourceName, string bandName, CelestialBody b)
    {
      return IsDiscovered(resourceName, bandName, b.bodyName);
    }
    public bool IsDiscovered(string resourceName, string bandName,string bodyName)
    {
      if (Settings.SetAllDiscovered)
        return true;
      if (distributionData.Find(x => 
      (x.ResourceName == resourceName) &&
      (x.BandName == bandName) &&
      (x.BodyName == bodyName) && 
      x.Discovered) == null)
        return false;
      return true;
    }
    public bool IsAnyIdentified(string resourceName, CelestialBody b)
    {
      if (Settings.SetAllIdentified)
        return true;
      foreach (ResourceBand band in SpaceDustResourceMap.Instance.GetBodyDistributions(b, resourceName))
      {
        if (IsIdentified(resourceName, band.name, b.bodyName))
          return true;
      }
      return false;

    }
    public bool IsIdentified(string resourceName, string bandName, CelestialBody b)
    {
      return IsIdentified(resourceName, bandName, b.bodyName);
    }
    public bool IsIdentified(string resourceName, string bandName, string bodyName)
    {
      if (Settings.SetAllIdentified)
        return true;
      if (distributionData.Find(x =>
      (x.ResourceName == resourceName) && 
      (x.BodyName == bodyName) &&
      (x.BandName == bandName) &&
      x.Identified) == null)
        return false;
      return true;
    }
    public void DiscoverResourceBandsAtBody(string resourceName, CelestialBody b)
    {
    
      foreach (ResourceBand band in SpaceDustResourceMap.Instance.GetBodyDistributions(b, resourceName))
      {

        DiscoverResourceBand(resourceName, band.name, b.bodyName);
      }
    }
    public void DiscoverResourceBand(string resourceName, string bandName, CelestialBody b)
    {
      DiscoverResourceBand(resourceName, bandName, b.bodyName);
    }
    public void DiscoverResourceBand(string resourceName, string bandName, string bodyName)
    {
      if (!IsDiscovered(resourceName, bandName, bodyName))
      {
        List<SpaceDustDiscoveryData> toDiscover = distributionData.FindAll(x => 
        (x.ResourceName == resourceName) &&
        (x.BandName == bandName) &&
        (x.BodyName == bodyName));

        if (toDiscover == null || toDiscover.Count == 0)
        {
          distributionData.Add(new SpaceDustDiscoveryData(resourceName, bandName, bodyName, true, false));
        }
        else
        {
          foreach (SpaceDustDiscoveryData data in toDiscover)
          {
            data.Discovered = true;
          }
        }
        if (HighLogic.LoadedSceneIsFlight)
        {
          ScreenMessage msg = new ScreenMessage(Localizer.Format("#LOC_SpaceDust_Message_Discovery", resourceName, bodyName), 5f, ScreenMessageStyle.UPPER_CENTER);
          ScreenMessages.PostScreenMessage(msg);
        }
        Utils.Log($"[SpaceDustScenario]: Discovered {resourceName} in {bandName} at {bodyName}");
      }
    }
    public void IdentifyResourceBandsAtBody(string resourceName, CelestialBody b)
    {
      foreach (ResourceBand band in SpaceDustResourceMap.Instance.GetBodyDistributions(b, resourceName))
      {

        IdentifyResourceBand(resourceName, band.name, b.bodyName);
      }
    }
    public void IdentifyResourceBand(string resourceName, string bandName, CelestialBody b)
    {
      IdentifyResourceBand(resourceName, bandName, b.bodyName);
    }
    public void IdentifyResourceBand(string resourceName, string bandName, string bodyName)
    {
      if (!IsIdentified(resourceName, bandName, bodyName))
      {
        List<SpaceDustDiscoveryData> toIdentify = distributionData.FindAll(x =>
        (x.ResourceName == resourceName) &&
        (x.BandName == bandName) &&
        (x.BodyName == bodyName));

        if (toIdentify == null || toIdentify.Count == 0)
        {
          distributionData.Add(new SpaceDustDiscoveryData(resourceName, bandName, bodyName, true, true));
        }
        else
        {
          foreach (SpaceDustDiscoveryData data in toIdentify)
          {
            data.Discovered = true;
            data.Identified = true;
          }
        }
        if (HighLogic.LoadedSceneIsFlight)
        {
          ScreenMessage msg = new ScreenMessage(Localizer.Format("#LOC_SpaceDust_Message_Identified", resourceName, bodyName), 5f, ScreenMessageStyle.UPPER_CENTER);
          ScreenMessages.PostScreenMessage(msg);
        }
        Utils.Log($"[SpaceDustScenario]: Identified {resourceName} in {bandName} at {bodyName}");
      }
    }
  }
}

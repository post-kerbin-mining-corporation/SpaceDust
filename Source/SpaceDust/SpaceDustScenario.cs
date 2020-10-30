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
    public float discoveryPercent = 0f;
    public float identifyPercent = 0f;


    public SpaceDustDiscoveryData()
    { }
    public SpaceDustDiscoveryData(string resourceName, string bandName, string bodyName, bool isDiscovered, bool isIdentified)
    {
      ResourceName = resourceName;
      BodyName = bodyName;
      BandName = bandName;
      Discovered = isDiscovered;
      Identified = isIdentified;
      if (isDiscovered)
        discoveryPercent = 100f;
      if (isIdentified)
        identifyPercent = 100f;
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
      node.TryGetValue("discoveryPercent", ref discoveryPercent);
      node.TryGetValue("identifyPercent", ref identifyPercent);
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
      node.AddValue("discoveryPercent", discoveryPercent);
      node.AddValue("identifyPercent", identifyPercent);
      return node;
    }

    public new string ToString()
    {
      return $"DiscoveryData(Band {BandName} around {BodyName} containing {ResourceName}: Discovered  = {discoveryPercent}% ({Discovered}),Identified = {identifyPercent}% ({Identified}))";
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
          node.AddNode(data.Save());
        }
      }
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
    public bool IsDiscovered(string resourceName, string bandName, string bodyName)
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
        DiscoverResourceBand(resourceName, band, b.bodyName);
      }
    }
    public void DiscoverResourceBand(string resourceName, ResourceBand band, CelestialBody b)
    {
      DiscoverResourceBand(resourceName, band, b.bodyName);
    }
    public void DiscoverResourceBand(string resourceName, ResourceBand band, string bodyName)
    {
      if (!IsDiscovered(resourceName, band.name, bodyName))
      {
        List<SpaceDustDiscoveryData> toDiscover = distributionData.FindAll(x =>
        (x.ResourceName == resourceName) &&
        (x.BandName == band.name) &&
        (x.BodyName == bodyName));

        if (toDiscover == null || toDiscover.Count == 0)
        {
          distributionData.Add(new SpaceDustDiscoveryData(resourceName, band.name, bodyName, true, false));
        }
        else
        {
          foreach (SpaceDustDiscoveryData data in toDiscover)
          {
            data.Discovered = true;
            data.discoveryPercent = 100;
          }
        }
        if (HighLogic.LoadedSceneIsFlight)
        {
          ScreenMessage msg = new ScreenMessage(Localizer.Format("#LOC_SpaceDust_Message_Discovery", resourceName, bodyName), 5f, ScreenMessageStyle.UPPER_CENTER);
          ScreenMessages.PostScreenMessage(msg);
        }
        Utils.Log($"[SpaceDustScenario]: Discovered {resourceName} in {band.name} at {bodyName}");

        if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX)
        {
          float scienceValue = FlightGlobals.GetBodyByName(bodyName).scienceValues.InSpaceHighDataValue * band.identifyScienceReward;
          Utils.Log($"[SpaceDustScenario]: Added {scienceValue} science because  {band.name} at {bodyName} was discovered");

          ResearchAndDevelopment.Instance.AddScience(scienceValue, TransactionReasons.ScienceTransmission);
        }
      }
    }
    public void IdentifyResourceBandsAtBody(string resourceName, CelestialBody b)
    {
      foreach (ResourceBand band in SpaceDustResourceMap.Instance.GetBodyDistributions(b, resourceName))
      {

        IdentifyResourceBand(resourceName, band, b.bodyName);
      }
    }
    public void IdentifyResourceBand(string resourceName, ResourceBand band, CelestialBody b)
    {
      IdentifyResourceBand(resourceName, band, b.bodyName);
    }
    public void IdentifyResourceBand(string resourceName, ResourceBand band, string bodyName)
    {
      if (!IsIdentified(resourceName, band.name, bodyName))
      {
        List<SpaceDustDiscoveryData> toIdentify = distributionData.FindAll(x =>
        (x.ResourceName == resourceName) &&
        (x.BandName == band.name) &&
        (x.BodyName == bodyName));

        if (toIdentify == null || toIdentify.Count == 0)
        {
          distributionData.Add(new SpaceDustDiscoveryData(resourceName, band.name, bodyName, true, true));
        }
        else
        {
          foreach (SpaceDustDiscoveryData data in toIdentify)
          {
            data.Discovered = true;
            data.discoveryPercent = 100;
            data.Identified = true;
            data.identifyPercent = 100;
          }
        }
        if (HighLogic.LoadedSceneIsFlight)
        {
          ScreenMessage msg = new ScreenMessage(Localizer.Format("#LOC_SpaceDust_Message_Identified", resourceName, bodyName), 5f, ScreenMessageStyle.UPPER_CENTER);
          ScreenMessages.PostScreenMessage(msg);
        }
        Utils.Log($"[SpaceDustScenario]: Identified {resourceName} in {band.name} at {bodyName}");
        if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX)
        {
          float scienceValue = FlightGlobals.GetBodyByName(bodyName).scienceValues.InSpaceHighDataValue * band.identifyScienceReward;
          Utils.Log($"[SpaceDustScenario]: Added {scienceValue} science because  {band.name} at {bodyName} was identified");

          ResearchAndDevelopment.Instance.AddScience(scienceValue, TransactionReasons.ScienceTransmission);
        }
       
      }
    }

    public void AddDiscoveryAtBody(string resourceName, CelestialBody b, float amount)
    {
      foreach (ResourceBand band in SpaceDustResourceMap.Instance.GetBodyDistributions(b, resourceName))
      {
        AddDiscoveryAtBand(resourceName, band, b.bodyName, amount);
      }
    }
    public void AddDiscoveryAtBand(string resourceName, ResourceBand band, CelestialBody b, float amount)
    {
      AddDiscoveryAtBand(resourceName, band, b.bodyName, amount);
    }
    public void AddDiscoveryAtBand(string resourceName, ResourceBand band, string bodyName, float amount)
    {
      if (!IsDiscovered(resourceName, band.name, bodyName))
      {
        List<SpaceDustDiscoveryData> toDiscover = distributionData.FindAll(x =>
          (x.ResourceName == resourceName) &&
          (x.BandName == band.name) &&
          (x.BodyName == bodyName));

        if (toDiscover == null || toDiscover.Count == 0)
        {
          distributionData.Add(new SpaceDustDiscoveryData(resourceName, band.name, bodyName, false, false));

        }
        else
        {
          foreach (SpaceDustDiscoveryData data in toDiscover)
          {
            data.discoveryPercent += amount * band.RemoteDiscoveryScale;
            if (data.discoveryPercent >= 100f)
            {
              DiscoverResourceBand(resourceName, band, bodyName);
            }
          }
        }
      }
    }
    public void AddIdentifyAtBody(string resourceName, CelestialBody b, float amount)
    {
      foreach (ResourceBand band in SpaceDustResourceMap.Instance.GetBodyDistributions(b, resourceName))
      {

        AddIdentifyAtBand(resourceName, band, b.bodyName, amount);
      }
    }
    public void AddIdentifyAtBand(string resourceName, ResourceBand band, CelestialBody b, float amount)
    {
      AddIdentifyAtBand(resourceName, band, b.bodyName, amount);
    }
    public void AddIdentifyAtBand(string resourceName, ResourceBand band, string bodyName, float amount)
    {
      if (!IsIdentified(resourceName, band.name, bodyName))
      {
        List<SpaceDustDiscoveryData> toIdentify = distributionData.FindAll(x =>
        (x.ResourceName == resourceName) &&
        (x.BandName == band.name) &&
        (x.BodyName == bodyName));

        if (toIdentify == null || toIdentify.Count == 0)
        {
          distributionData.Add(new SpaceDustDiscoveryData(resourceName, band.name, bodyName, true, true));
        }
        else
        {
          foreach (SpaceDustDiscoveryData data in toIdentify)
          {
            if (data.Discovered)
            {
              data.identifyPercent += amount * band.RemoteDiscoveryScale;
            }
            if (data.identifyPercent >= 100f)
            {
              IdentifyResourceBand(resourceName, band, bodyName);
            }
          }
        }

      }
    }

    public float GetSurveyProgressAtBody(string resourceName, CelestialBody b)
    {
      float maxProgress = 0f;
      float progress = 0f;
      foreach (ResourceBand band in SpaceDustResourceMap.Instance.GetBodyDistributions(b, resourceName))
      {
        maxProgress += 200f;
        progress += GetBandSurveyProgress(resourceName, band.name, b.bodyName);
      }
      if (maxProgress > 0)
        return 100f;
      return progress / maxProgress * 100f;
    }

    public float GetBandSurveyProgress(string resourceName, string bandName, string bodyName)
    {

      List<SpaceDustDiscoveryData> toEval = distributionData.FindAll(x =>
      (x.ResourceName == resourceName) &&
      (x.BandName == bandName) &&
      (x.BodyName == bodyName));

      if (toEval == null || toEval.Count == 0)
      {
        return 0f;
      }
      else
      {
        foreach (SpaceDustDiscoveryData data in toEval)
        {
          return data.identifyPercent + data.discoveryPercent;
        }
      }
      if (HighLogic.LoadedSceneIsFlight)
      {
        ScreenMessage msg = new ScreenMessage(Localizer.Format("#LOC_SpaceDust_Message_Identified", resourceName, bodyName), 5f, ScreenMessageStyle.UPPER_CENTER);
        ScreenMessages.PostScreenMessage(msg);
      }
      Utils.Log($"[SpaceDustScenario]: Identified {resourceName} in {bandName} at {bodyName}");
      return 0f;
    }
  }
}

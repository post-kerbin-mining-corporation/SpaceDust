using KSP.Localization;
using System.Collections.Generic;

namespace SpaceDust
{
  /// <summary>
  /// Manages persistence and tracking of discovery data
  /// </summary>
  [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.SPACECENTER, GameScenes.FLIGHT, GameScenes.TRACKSTATION, GameScenes.EDITOR)]
  public class SpaceDustScenario : ScenarioModule
  {
    public static SpaceDustScenario Instance { get; private set; }


    protected List<SpaceDustDiscoveryData> distributionData;

    private const string BAND_DISCOVERED_MESSAGE_KEY = "#LOC_SpaceDust_Message_Discovery";
    private const string BAND_IDENTIFIED_MESSAGE_KEY = "#LOC_SpaceDust_Message_Identified";

    public override void OnAwake()
    {
      Utils.Log("[SpaceDustScenario]: Awake");
      Instance = this;
      base.OnAwake();
    }
    /// <summary>
    /// Loads all the discovery data from the persistence node
    /// </summary>
    /// <param name="node"></param>
    public override void OnLoad(ConfigNode node)
    {
      Utils.Log("[SpaceDustScenario]: Started Loading");
      base.OnLoad(node);
      if (distributionData == null) distributionData = new List<SpaceDustDiscoveryData>();
      foreach (ConfigNode saveNode in node.GetNodes(Settings.PERSISTENCE_DATA_NODE_NAME))
      {
        distributionData.Add(new SpaceDustDiscoveryData(saveNode));
      }
      Utils.Log("[SpaceDustScenario]: Done Loading");
    }

    /// <summary>
    /// Saves the discovery data to the persistence node
    /// </summary>
    /// <param name="node"></param>
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

    /// <summary>
    /// Returns true if any resource has been discovered around a given CB
    /// </summary>
    /// <param name="resourceName"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public bool IsAnyDiscovered(string resourceName, CelestialBody b)
    {
      foreach (ResourceBand band in SpaceDustResourceMap.Instance.GetBodyDistributions(b, resourceName))
      {
        if (IsDiscovered(resourceName, band.name, b.bodyName))
          return true;
      }
      return false;

    }

    /// <summary>
    /// Returns true if a particular resource band has been discovered around a body
    /// </summary>
    /// <param name="resourceName"></param>
    /// <param name="bandName"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public bool IsDiscovered(string resourceName, string bandName, CelestialBody b)
    {
      return IsDiscovered(resourceName, bandName, b.bodyName);
    }

    /// <summary>
    /// Returns true if a particular resource band has been discovered around a body by name
    /// </summary>
    /// <param name="resourceName"></param>
    /// <param name="bandName"></param>
    /// <param name="bodyName"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Returns true if a resource has been identified around a body
    /// </summary>
    /// <param name="resourceName"></param>
    /// <param name="b"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Returns true if a particular resource band has been Identified around a body
    /// </summary>
    /// <param name="resourceName"></param>
    /// <param name="bandName"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public bool IsIdentified(string resourceName, string bandName, CelestialBody b)
    {
      return IsIdentified(resourceName, bandName, b.bodyName);
    }

    /// <summary>
    /// Returns true if a particular resource band has been Identified around a body by name
    /// </summary>
    /// <param name="resourceName"></param>
    /// <param name="bandName"></param>
    /// <param name="bodyName"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Set all bands containing the specified resource to be Discovered around a body
    /// </summary>
    /// <param name="resourceName"></param>
    /// <param name="b"></param>
    public void DiscoverResourceBandsAtBody(string resourceName, CelestialBody b)
    {
      foreach (ResourceBand band in SpaceDustResourceMap.Instance.GetBodyDistributions(b, resourceName))
      {
        DiscoverResourceBand(resourceName, band, b.bodyName, true);
      }
    }
    /// <summary>
    /// Sets a specific band around a body to be Discovered
    /// </summary>
    /// <param name="resourceName"></param>
    /// <param name="band"></param>
    /// <param name="b"></param>
    public void DiscoverResourceBand(string resourceName, ResourceBand band, CelestialBody b)
    {
      DiscoverResourceBand(resourceName, band, b.bodyName, true);
    }
    /// <summary>
    /// Sets a specific band around a body to be Discovered with the addition of some Science points
    /// </summary>
    /// <param name="resourceName"></param>
    /// <param name="band"></param>
    /// <param name="b"></param>
    /// <param name="addScience"></param>
    public void DiscoverResourceBand(string resourceName, ResourceBand band, CelestialBody b, bool addScience)
    {
      DiscoverResourceBand(resourceName, band, b.bodyName, addScience);
    }
    /// <summary>
    /// Sets a specific band around a body by name to be Discovered with the addition of some Science points
    /// </summary>
    /// <param name="resourceName"></param>
    /// <param name="band"></param>
    /// <param name="bodyName"></param>
    /// <param name="addScience"></param>
    public void DiscoverResourceBand(string resourceName, ResourceBand band, string bodyName, bool addScience)
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
          ScreenMessage msg = new ScreenMessage(Localizer.Format(BAND_DISCOVERED_MESSAGE_KEY, resourceName, bodyName), 5f, ScreenMessageStyle.UPPER_CENTER);
          ScreenMessages.PostScreenMessage(msg);
        }
        Utils.Log($"[SpaceDustScenario]: Discovered {resourceName} in {band.name} at {bodyName}");

        if (addScience && (HighLogic.CurrentGame.Mode == Game.Modes.CAREER || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX))
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

        IdentifyResourceBand(resourceName, band, b.bodyName, true);
      }
    }
    public void IdentifyResourceBand(string resourceName, ResourceBand band, CelestialBody b, bool addScience)
    {
      IdentifyResourceBand(resourceName, band, b.bodyName, addScience);
    }
    public void IdentifyResourceBand(string resourceName, ResourceBand band, string bodyName, bool addScience)
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
          ScreenMessage msg = new ScreenMessage(Localizer.Format(BAND_IDENTIFIED_MESSAGE_KEY, resourceName, bodyName), 5f, ScreenMessageStyle.UPPER_CENTER);
          ScreenMessages.PostScreenMessage(msg);
        }
        Utils.Log($"[SpaceDustScenario]: Identified {resourceName} in {band.name} at {bodyName}");
        if (addScience && (HighLogic.CurrentGame.Mode == Game.Modes.CAREER || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX))
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
              DiscoverResourceBand(resourceName, band, bodyName, true);
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
              IdentifyResourceBand(resourceName, band, bodyName, true);
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

    /// <summary>
    /// Get how discovered a band around a body is
    /// </summary>
    /// <param name="resourceName"></param>
    /// <param name="bandName"></param>
    /// <param name="bodyName"></param>
    /// <returns></returns>
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
        ScreenMessage msg = new ScreenMessage(Localizer.Format(BAND_IDENTIFIED_MESSAGE_KEY, resourceName, bodyName), 5f, ScreenMessageStyle.UPPER_CENTER);
        ScreenMessages.PostScreenMessage(msg);
      }
      Utils.Log($"[SpaceDustScenario]: Identified {resourceName} in {bandName} at {bodyName}");
      return 0f;
    }
  }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using KSP.Localization;

namespace SpaceDust
{
  public enum DiscoverMode
  {
    None,
    Local,
    SOI,
    Altitude
  }

  public class ScannedResource
  {
    public string Name = "";
    public DiscoverMode DiscoverMode;
    public DiscoverMode IdentifyMode;
    public double LocalThreshold = 0.01;
    public double DiscoverRange = 70000;
    public double IdentifyRange = 30000;

    public ScannedResource(ConfigNode c)
    {
      Load(c);
    }
    public void Load(ConfigNode c)
    {
      c.TryGetValue("name", ref Name);
      c.TryGetEnum<DiscoverMode>("DiscoverMode", ref DiscoverMode, DiscoverMode.Local);
      c.TryGetEnum<DiscoverMode>("IdentifyMode", ref IdentifyMode, DiscoverMode.Local);
      c.TryGetValue("LocalThreshold", ref LocalThreshold);
      c.TryGetValue("DiscoverRange", ref DiscoverRange);
      c.TryGetValue("IdentifyRange", ref IdentifyRange);


    }
    public string GenerateInfoString()
    {
      string msg = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustScanner_Info_Resource", Name);
      switch (DiscoverMode)
      {
        case DiscoverMode.Local:
          msg += Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustScanner_Info_Discovers_Local");
            break;
        case DiscoverMode.SOI:
          msg += Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustScanner_Info_Discovers_SOI");
          break;
        case DiscoverMode.Altitude:
          msg += Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustScanner_Info_Discovers_Altitude", (DiscoverRange/1000.0).ToString("F0"));
          break;
      }
      switch (IdentifyMode)
      {
        case DiscoverMode.Local:
          msg += Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustScanner_Info_Identifies_Local");
          break;
        case DiscoverMode.SOI:
          msg += Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustScanner_Info_Identifies_SOI");
          break;
        case DiscoverMode.Altitude:
          msg += Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustScanner_Info_Identifies", (IdentifyRange / 1000.0).ToString("F0"));
          break;
      }



      return msg;
    }
  }
  public class ModuleSpaceDustScanner : PartModule
  {
    // Am i enabled?
    [KSPField(isPersistant = true)]
    public bool Enabled = false;

    // Cost per second to run the scanner
    [KSPField(isPersistant = true)]
    public float PowerCost = 1f;

    // Current cost to run the scanner
    [KSPField(isPersistant = true)]
    public float CurrentPowerConsumption = 1f;

    // UI field for showing scan status
    [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = false, guiName = "#LOC_SpaceDust_ModuleSpaceDustScanner_Field_Resources")]
    public string ScannerUI = "";




    [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "#LOC_SpaceDust_ModuleSpaceDustScanner_Event_EnableScanner", active = true)]
    public void EnableScanner()
    {
      Enabled = true;
    }
    [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "#LOC_SpaceDust_ModuleSpaceDustScanner_Event_DisableScanner", active = false)]
    public void DisableScanner()
    {
      Enabled = false;
    }


    private List<ScannedResource> resources;



    public override string GetModuleDisplayName()
    {
      return Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustScanner_DisplayName");
    }

    public override string GetInfo()
    {
      string msg = "";
      msg += Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustScanner_Info_Header");

      foreach (ScannedResource res in resources)
      {
        msg += res.GenerateInfoString();
      }
      return msg;
    }


    public override void OnStart(StartState state)
    {
      base.OnStart(state);
      if (HighLogic.LoadedSceneIsFlight)
      {

      }
    }

    public override void OnLoad(ConfigNode node)
    {
      base.OnLoad(node);
      if (resources == null || resources.Count == 0)
      {
        resources = new List<ScannedResource>();
        foreach (ConfigNode resNode in node.GetNodes("SCANNED_RESOURCE"))
        {
          resources.Add(new ScannedResource(resNode));
        }
      }
    }

    public override void OnUpdate()
    {
      if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
      {
        if (Events["EnableScanner"].active == Enabled || Events["DisableScanner"].active != Enabled)
        {
          Events["DisableScanner"].active = Enabled;
          Events["EnableScanner"].active = !Enabled;
        }
      }
    }

    void FixedUpdate()
    {
      if (HighLogic.LoadedSceneIsFlight)
      {
        string message = "";
        if (Enabled)
        {
          CurrentPowerConsumption = -PowerCost;
          // check power
          if (part.RequestResource(PartResourceLibrary.ElectricityHashcode,
            (double)(PowerCost * TimeWarp.fixedDeltaTime)) >= (PowerCost * TimeWarp.fixedDeltaTime))
          {
            // do scanning
            for (int i = 0; i < resources.Count; i++)
            {
              double resourceSample = SpaceDustResourceMap.Instance.SampleResource(resources[i].Name,
                part.vessel.mainBody,
                vessel.altitude,
                vessel.latitude,
                vessel.longitude);
              // This mod discovers all bands at the body
              if (resources[i].DiscoverMode == DiscoverMode.SOI)
              {
                SpaceDustScenario.Instance.DiscoverResourceBandsAtBody(resources[i].Name, vessel.mainBody);
              }
              // This mode discovers modes if we are close enough 
              if (resources[i].DiscoverMode == DiscoverMode.Altitude)
              {
                List<ResourceBand> bands = SpaceDustResourceMap.Instance.GetBodyDistributions(vessel.mainBody, resources[i].Name);
                {
                  foreach (ResourceBand band in bands)
                  {
                    if (band.CheckDistanceToCenter(vessel.altitude, resources[i].DiscoverRange))
                    {
                      SpaceDustScenario.Instance.DiscoverResourceBand(resources[i].Name, band.name, vessel.mainBody);
                    }
                  }
                }
              }
              // This mod discovers all bands at the body
              if (resources[i].DiscoverMode == DiscoverMode.Local)
              {
                List<ResourceBand> bands = SpaceDustResourceMap.Instance.GetBodyDistributions(vessel.mainBody, resources[i].Name);
                {
                  foreach (ResourceBand band in bands)
                  {
                    if (band.Sample(vessel.altitude, vessel.latitude, vessel.longitude) > resources[i].LocalThreshold)
                    {
                      SpaceDustScenario.Instance.DiscoverResourceBand(resources[i].Name, band.name, vessel.mainBody);
                    }
                  }
                }
              }
              // This mod discovers all bands at the body
              if (resources[i].IdentifyMode == DiscoverMode.SOI)
              {

                SpaceDustScenario.Instance.IdentifyResourceBandsAtBody(resources[i].Name, vessel.mainBody);

              }
              // This mode discovers modes if we are close enough 
              if (resources[i].IdentifyMode == DiscoverMode.Altitude)
              {
                List<ResourceBand> bands = SpaceDustResourceMap.Instance.GetBodyDistributions(vessel.mainBody, resources[i].Name);
                {
                  foreach (ResourceBand band in bands)
                  {
                    if (band.CheckDistanceToCenter(vessel.altitude, resources[i].IdentifyRange))
                    {
                      SpaceDustScenario.Instance.IdentifyResourceBand(resources[i].Name, band.name, vessel.mainBody);
                    }
                  }
                }
              }
              // This mod discovers all bands at the body
              if (resources[i].IdentifyMode == DiscoverMode.Local)
              {
                List<ResourceBand> bands = SpaceDustResourceMap.Instance.GetBodyDistributions(vessel.mainBody, resources[i].Name);
                {
                  foreach (ResourceBand band in bands)
                  {
                    if (band.Sample(vessel.altitude, vessel.latitude, vessel.longitude) > resources[i].LocalThreshold)
                    {
                      SpaceDustScenario.Instance.IdentifyResourceBand(resources[i].Name, band.name, vessel.mainBody);
                    }
                  }
                }
              }

              if (message != "")
                message += "\n";
              message += Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustScanner_Field_Resources_SingleSample", resources[i].Name, resourceSample.ToString("F2"));
            }

          }
          else
          {
            message = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustScanner_Field_Resources_NoPower");
          }

        }
        else
        {
          CurrentPowerConsumption = 0f;
          message = Localizer.Format(("#LOC_SpaceDust_ModuleSpaceDustScanner_Field_Disabled"));
        }
        ScannerUI = message;
      }
      else
      {
        CurrentPowerConsumption = -PowerCost;
      }
    }
  }
}

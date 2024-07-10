using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using KSP.Localization;

namespace SpaceDust
{

  public class ModuleSpaceDustScanner : PartModule
  {
    /// Am i enabled?
    [KSPField(isPersistant = true)]
    public bool Enabled = false;

    /// Cost per second to run the scanner
    [KSPField(isPersistant = true)]
    public float PowerCost = 1f;

    /// Minimum EC to leave when harvesting
    [KSPField(isPersistant = false)]
    public float minResToLeave = 0.1f;

    /// Am i enabled?
    [KSPField(isPersistant = false)]
    public bool ScanInSpace = true;

    /// Am i enabled?
    [KSPField(isPersistant = false)]
    public bool ScanInAtmosphere = true;

    /// Current cost to run the scanner
    [KSPField(isPersistant = true)]
    public float CurrentPowerConsumption = 1f;

    /// UI field for showing scan status
    [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = false, guiName = "#LOC_SpaceDust_ModuleSpaceDustScanner_Field_Resources")]
    public string ScannerUI = "";

    [KSPField(isPersistant = false)]
    public string ScanAnimationName;

    [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "#LOC_SpaceDust_ModuleSpaceDustScanner_Event_EnableScanner", active = true)]
    public void EnableScanner()
    {
      Enabled = true;
    }
    [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "#LOC_SpaceDust_ModuleSpaceDustScanner_Event_DisableScanner", active = false)]
    public void DisableScanner()
    {
      Enabled = false;
    }

    /// ACTIONS
    [KSPAction(guiName = "#LOC_SpaceDust_ModuleSpaceDustScanner_Action_Enable")]
    public void EnableAction(KSPActionParam param) { EnableScanner(); }

    [KSPAction(guiName = "#LOC_SpaceDust_ModuleSpaceDustScanner_Action_Disable")]
    public void DisableAction(KSPActionParam param) { DisableScanner(); }

    [KSPAction(guiName = "#LOC_SpaceDust_ModuleSpaceDustScanner_Action_Toggle")]
    public void ToggleAction(KSPActionParam param)
    {
      if (Enabled) DisableScanner(); else EnableScanner();
    }

    private AnimationState[] scanState;
    private List<ScannedResource> resources;

    public override string GetModuleDisplayName()
    {
      return Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustScanner_DisplayName");
    }

    public override string GetInfo()
    {
      string msg = "";
      msg += Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustScanner_Info_Header", PowerCost.ToString("F1"));

      foreach (ScannedResource res in resources)
      {
        msg += res.GenerateInfoString();
      }
      return msg;
    }


    public override void OnStart(StartState state)
    {
      base.OnStart(state);
      if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
      {
        if (ScanAnimationName != "")
        {
          scanState = Utils.SetUpAnimation(ScanAnimationName, part);
          if (Enabled)
          {
            foreach (AnimationState anim in scanState)
            {
              anim.speed = 0f;
              anim.normalizedTime = 1f;
            }
          }
          else
          {
            foreach (AnimationState anim in scanState)
            {
              anim.speed = 0f;
              anim.normalizedTime = 0f;
            }
          }
        }
        if (resources == null || resources.Count == 0)
        {
          ConfigNode node = Utils.GetModuleConfigNode(part, moduleName);
          if (node != null)
            OnLoad(node);
        }
        foreach (ScannedResource res in resources)
        {
          res.density = PartResourceLibrary.Instance.GetDefinition(res.Name).density;
        }
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

    public void Update()
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

    public bool CheckScanSituation()
    {
      double atmDensity = vessel.atmDensity;
      if (atmDensity > 0d)
      {
        return ScanInAtmosphere;
      }
      else
      {
        return ScanInSpace;
      }
    }

    void FixedUpdate()
    {
      HandleAnimation();
      if (HighLogic.LoadedSceneIsFlight)
      {
        string message = "";
        if (Enabled)
        {
          CurrentPowerConsumption = -PowerCost;
          vessel.GetConnectedResourceTotals(PartResourceLibrary.ElectricityHashcode, out double currentEC, out double maxEC);
          double chargeRequest = PowerCost * TimeWarp.fixedDeltaTime;


          if (!CheckScanSituation())
          {
            message = "Cannot scan in this situation";
          }
          else
          {
            // check power
            if (currentEC > chargeRequest + minResToLeave)
            {
              double consumption = part.RequestResource(PartResourceLibrary.ElectricityHashcode, chargeRequest);
              if (consumption >= chargeRequest - 0.0001)

              {
                // do scanning
                for (int i = 0; i < resources.Count; i++)
                {
                  double resourceSample = SpaceDustResourceMap.Instance.SampleResource(resources[i].Name,
                    part.vessel.mainBody,
                    vessel.altitude + part.vessel.mainBody.Radius,
                    vessel.latitude,
                    vessel.longitude);

                  // This mode discovers all bands at the body
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
                          SpaceDustScenario.Instance.DiscoverResourceBand(resources[i].Name, band, vessel.mainBody);
                        }
                      }
                    }
                  }
                  // This mode discovers all bands at the body
                  if (resources[i].DiscoverMode == DiscoverMode.Local)
                  {
                    List<ResourceBand> bands = SpaceDustResourceMap.Instance.GetBodyDistributions(vessel.mainBody, resources[i].Name);
                    {
                      foreach (ResourceBand band in bands)
                      {
                        if (band.Sample(vessel.altitude, vessel.latitude, vessel.longitude) > resources[i].LocalThreshold)
                        {
                          SpaceDustScenario.Instance.DiscoverResourceBand(resources[i].Name, band, vessel.mainBody);
                        }
                      }
                    }
                  }
                  // This mode discovers all bands at the body
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
                        if (band.CheckDistanceToCenter(vessel.altitude + vessel.mainBody.Radius, resources[i].IdentifyRange))
                        {
                          SpaceDustScenario.Instance.IdentifyResourceBand(resources[i].Name, band, vessel.mainBody, true);
                        }
                      }
                    }
                  }
                  // This mode discovers all bands at the body
                  if (resources[i].IdentifyMode == DiscoverMode.Local)
                  {
                    List<ResourceBand> bands = SpaceDustResourceMap.Instance.GetBodyDistributions(vessel.mainBody, resources[i].Name);
                    {
                      foreach (ResourceBand band in bands)
                      {
                        if (band.Sample(vessel.altitude, vessel.latitude, vessel.longitude) > resources[i].LocalThreshold)
                        {
                          SpaceDustScenario.Instance.IdentifyResourceBand(resources[i].Name, band, vessel.mainBody, true);
                        }
                      }
                    }
                  }

                  if (message != "")
                    message += "\n";
                  message += Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustScanner_Field_Resources_SingleSample", resources[i].Name, resourceSample.ToString("G2"));
                }

              }
              else
              {
                message = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustScanner_Field_Resources_NoPower");
              }
            }
            else
            {
              message = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustScanner_Field_Resources_NoPower");
            }
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

    void HandleAnimation()
    {
      if (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight)
        if (ScanAnimationName != "")
        {
          if (Enabled)
          {

            foreach (AnimationState anim in scanState)
            {
              anim.speed = 1f;
              anim.normalizedTime = Mathf.Clamp(anim.normalizedTime, 0f, 1f);
            }
          }
          else
          {

            foreach (AnimationState anim in scanState)
            {
              anim.speed = -1f;
              anim.normalizedTime = Mathf.Clamp(anim.normalizedTime, 0f, 1f);
            }
          }
        }
    }
  }
}

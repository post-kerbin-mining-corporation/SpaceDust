using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KSP.Localization;
using System.Security.Permissions;

namespace SpaceDust
{

  public class ModuleSpaceDustTelescope : PartModule
  {
    // Am i enabled?
    [KSPField(isPersistant = true)]
    public bool Enabled = false;

    // Cost per second to run the telescope
    [KSPField(isPersistant = true)]
    public float PowerCost = 1f;

    // Current scan target
    [KSPField(isPersistant = true)]
    public string Target = "";

    // Current cost to run the scanner
    [KSPField(isPersistant = true)]
    public float CurrentPowerConsumption = 1f;

    // Current cost to run the scanner
    [KSPField(isPersistant = true)]
    public int Slots = 2;

    [KSPField(isPersistant = false)]
    public string ScanAnimationName;

    // UI field for showing scan status
    [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = false, guiName = "#LOC_SpaceDust_ModuleSpaceDustTelescope_Field_Status_Title")]
    public string ScannerUI = "";



    [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "#LOC_SpaceDust_ModuleSpaceDustTelescope_Event_EnableTelescope", active = true)]
    public void EnableTelescope()
    {
      Enabled = true;
    }
    [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "#LOC_SpaceDust_ModuleSpaceDustTelescope_Event_DisableTelescope", active = false)]
    public void DisableTelescope()
    {
      Enabled = false;
    }


    private AnimationState[] scanState;
    private List<ModuleSpaceDustTelescopeSlot> instrumentSlots;

    public override string GetModuleDisplayName()
    {
      return Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustTelescope_DisplayName");
    }

    public override string GetInfo()
    {
      string msg = "";
      msg += Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustTelescope_Info_Header", PowerCost.ToString("F1"));

      return msg;
    }

    public override void OnAwake()
    {
      base.OnAwake();
      instrumentSlots = new List<ModuleSpaceDustTelescopeSlot>();
    }
    public override void OnStart(StartState state)
    {
      base.OnStart(state);

      instrumentSlots = this.GetComponentsInChildren<ModuleSpaceDustTelescopeSlot>().ToList();
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
      }
    }


    public override void OnUpdate()
    {
      if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
      {
        if (Events["EnableTelescope"].active == Enabled || Events["DisableTelescope"].active != Enabled)
        {
          Events["DisableTelescope"].active = Enabled;
          Events["EnableTelescope"].active = !Enabled;


        }
      }
    }
    void FixedUpdate()
    {
      if (HighLogic.LoadedSceneIsFlight)
      {
        if (Enabled)
        {
          CurrentPowerConsumption = -PowerCost;
          double amt = part.RequestResource(PartResourceLibrary.ElectricityHashcode,
            (double)(PowerCost * TimeWarp.fixedDeltaTime));
          // check power
          if (amt > 0.00001)
          {
            ITargetable target = part.vessel.targetObject;

            if (target != null)
            {
              try
              {
                CelestialBody targetBody;
                targetBody = (CelestialBody)target;
                // do scanning
                ScannerUI = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustTelescope_Field_Status_Observing", part.vessel.targetObject.GetDisplayName());

                for (int i = 0; i < instrumentSlots.Count; i++)
                {
                  instrumentSlots[i].SetUIState(true);
                  instrumentSlots[i].Scan((CelestialBody)part.vessel.targetObject);

                }

              }
              catch (InvalidCastException)
              {
                for (int i = 0; i < instrumentSlots.Count; i++)
                {
                  instrumentSlots[i].SetUIState(false);
                }
                ScannerUI = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustTelescope_Field_Status_NoTarget");
              }

            }
            else
            {
              for (int i = 0; i < instrumentSlots.Count; i++)
              {
                instrumentSlots[i].SetUIState(false);
              }
              ScannerUI = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustTelescope_Field_Status_NoTarget");
            }

          }


          if (ScanAnimationName != "")
          {
            foreach (AnimationState anim in scanState)
            {
              anim.speed = 1f;
              anim.normalizedTime = Mathf.Clamp(anim.normalizedTime, 0f, 1f);
            }
          }

          else
          {
            for (int i = 0; i < instrumentSlots.Count; i++)
            {
              instrumentSlots[i].SetUIState(false);
            }
            ScannerUI = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustTelescope_Field_Status_NoPower");

          }

        }
        else
        {
          CurrentPowerConsumption = 0f;
          ScannerUI = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustTelescope_Field_Status_Disabled");
          for (int i = 0; i < instrumentSlots.Count; i++)
          {
            instrumentSlots[i].SetUIState(false);
          }
          if (ScanAnimationName != "")
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

  public class ModuleSpaceDustTelescopeSlot : PartModule
  {

    // Cost per second to run the telescope
    [KSPField(isPersistant = false)]
    public float PowerCost = 1f;

    [KSPField(isPersistant = false)]
    public string ResourceName = "";

    [KSPField(isPersistant = false)]
    public string slotID = "";

    [KSPField(isPersistant = false)]
    public string InstrumentName = "";

    [KSPField(isPersistant = false)]
    public bool Discovers = true;

    [KSPField(isPersistant = false)]
    public bool Identifies = true;

    [KSPField(isPersistant = false)]
    public double MaxRange = 50000000000d;

    [KSPField(isPersistant = false)]
    public double DiscoverRate = 1d;

    [KSPField(isPersistant = false)]
    public double IdentifyRate = 1d;

    // UI field for showing scan status
    [KSPField(isPersistant = false, guiActive = false, guiActiveEditor = false, guiName = "#LOC_SpaceDust_ModuleSpaceDustTelescope_Field_Instrument_Title")]
    public string ScannerUI = "";

    //public override string GetModuleDisplayName()
    //{
    //  return Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustTelescope_Instrument_DisplayName");
    //}

    //public override string GetInfo()
    //{
    //  string msg = "";
    //  msg += Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustTelescope_Instrument_Info");

    //  return msg;
   // }

    public void SetUIState(bool state)
    {
      if (ResourceName != "" || ResourceName != "None")
      {
        Fields["ScannerUI"].guiActive = state;
        Fields["ScannerUI"].guiName = Localizer.Format(InstrumentName);
      }
      else
      {
        Fields["ScannerUI"].guiActive = false;
      }
    }

    public void Scan(CelestialBody targetBody)
    {
      double dist = Vector3d.Distance(part.vessel.GetWorldPos3D(), targetBody.position);
      if (dist > MaxRange)
        ScannerUI = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustTelescope_Field_Instrument_OutOfRange", targetBody.bodyDisplayName);
      else
      {
        if (Discovers)
        {
          SpaceDustScenario.Instance.AddDiscoveryAtBody(ResourceName, targetBody, (float)DiscoverRate * TimeWarp.fixedDeltaTime);
        }
        if (Identifies)
        {
          SpaceDustScenario.Instance.AddIdentifyAtBody(ResourceName, targetBody, (float)IdentifyRate * TimeWarp.fixedDeltaTime);
        }

        ScannerUI = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustTelescope_Field_Instrument_Scanning");
        //ScannerUI = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustTelescope_Field_Instrument_Progress",
        //  SpaceDustScenario.Instance.GetSurveyProgressAtBody(ResourceName, targetBody).ToString("F1"));
      }
    }

  }
}

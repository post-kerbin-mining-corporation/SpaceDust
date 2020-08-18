using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KSP.Localization;

namespace SpaceDust
{
  public enum HarvesterType
  {
    Atmosphere,
    Exosphere,
    Omni
  }
  public class HarvestedResource
  {
    public string Name;
    // The basic efficiency, applied at local V = 0
    public float BaseEfficiency;
    // The velocity to use when the intake is static
    public float IntakeVelocityStatic;
    // Maps how well the intake works as velocity increases. 0 = nothing, 1= baseEfficiency
    public FloatCurve IntakeVelocityScale;
    
    public HarvesterType HarvestType;
    public String HarvestIntakeTransformName;

    public Transform HarvestIntakeTransform;

    public HarvestedResource() { }
    public HarvestedResource(ConfigNode node) { Load(node); }
    public void Load(ConfigNode node)
    {

    }

    public string GenerateInfoString()
    {
      return "";
    }
  }
  public class ModuleSpaceDustHarvester : PartModule
  {

    // Am i enabled?
    [KSPField(isPersistant = true)]
    public bool Enabled = false;

    // Cost per second to run the miner
    [KSPField(isPersistant = true)]
    public float PowerCost = 1f;

    // Current cost to run the miner
    [KSPField(isPersistant = true)]
    public float CurrentPowerConsumption = 1f;

    // UI field for showing scan status
    [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = false, guiName = "#LOC_SpaceDust_ModuleSpaceDustHarvester_Field_Resources")]
    public string ScannerUI = "";

    // UI field for showing scan status
    [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = false, guiName = "#LOC_SpaceDust_ModuleSpaceDustHarvester_Field_Scoop")]
    public string ScoopUI = "";


    [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "#LOC_SpaceDust_ModuleSpaceDustHarvester_Event_EnableScanner", active = true)]
    public void EnableHarvester()
    {
      Enabled = true;
    }
    [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "#LOC_SpaceDust_ModuleSpaceDustHarvester_Event_DisableScanner", active = false)]
    public void DisableHarvester()
    {
      Enabled = false;
    }

    protected List<HarvestedResource> resources;

    public override string GetModuleDisplayName()
    {
      return Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustHarvester_DisplayName");
    }

    public override string GetInfo()
    {
      string msg = "";
      msg += String.Format("Scans for atmospheric and exospheric resources. \n\n <b>Detectable Resources</b>:\n");

      foreach (HarvestedResource res in resources)
      {
        msg += res.GenerateInfoString();
      }
      return msg;
    }

    public override void OnLoad(ConfigNode node)
    {
      base.OnLoad(node);
      if (resources == null || resources.Count == 0)
      {
        resources = new List<HarvestedResource>();
        foreach (ConfigNode resNode in node.GetNodes("HARVESTED_RESOURCE"))
        {
          resources.Add(new HarvestedResource(resNode));
        }
      }
    }

    public override void OnUpdate()
    {
      if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
      {
        if (Events["EnableHarvester"].active == Enabled || Events["DisableHarvester"].active != Enabled)
        {
          Events["DisableHarvester"].active = Enabled;
          Events["EnableHarvester"].active = !Enabled;
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
            DoFocusedHarvesting();
          }
          else
          {
            message = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustHarvester_Field_Resources_NoPower");
          }

        }
        else
        {
          CurrentPowerConsumption = 0f;
          message = Localizer.Format(("#LOC_SpaceDust_ModuleSpaceDustHarvester_Field_Disabled"));
        }
        ScannerUI = message;
      }
      else
      {
        CurrentPowerConsumption = -PowerCost;
      }
    }
    void DoFocusedHarvesting()
    {

    }
  }

  
}

using KSP.Localization;
using System;
using System.Collections.Generic;
using UnityEngine;

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
    [KSPField(isPersistant = false)]
    public float PowerCost = 1f;

    // Current cost to run the miner
    [KSPField(isPersistant = true)]
    public float CurrentPowerConsumption = 1f;

    // The velocity to use when the intake is static
    [KSPField(isPersistant = false)]
    public float IntakeSpeedStatic = 0f;
    // Maps how well the intake works as velocity increases. 0 = nothing, 1= baseEfficiency

    [KSPField(isPersistant = false)]
    public FloatCurve IntakeVelocityScale;

    [KSPField(isPersistant = false)]
    public HarvesterType HarvestType;

    [KSPField(isPersistant = false)]
    public String HarvestIntakeTransformName;

    


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
    protected Transform HarvestIntakeTransform;

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

    public override void OnStart(StartState state)
    {
      base.OnStart(state);
      if (HarvestIntakeTransformName != "")
      {
        HarvestIntakeTransform = part.FindModelTransform(HarvestIntakeTransformName);
      }
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
      double efficiencyMultiplier = 1d;
      if (HarvestType == HarvesterType.Atmosphere && part.vessel.atmDensity > 0.0001d)
      {
        Vector3d worldVelocity = part.vessel.srf_velocity;
        Vector3 intakeVector;
        if (HarvestIntakeTransform == null)
          intakeVector = this.transform.forward;
        else
          intakeVector = HarvestIntakeTransform.forward;

        double dot = Vector3d.Dot(worldVelocity, intakeVector);
        float intakeSpeed = (float)(worldVelocity.magnitude * Math.Abs(dot) + IntakeSpeedStatic);
        efficiencyMultiplier = IntakeVelocityScale.Evaluate(intakeSpeed);
        ScoopUI = $"worldVel: {worldVelocity}\nintakeSPeed: {intakeSpeed}\ndot: {dot}\n Effic {efficiencyMultiplier}";
      }
      for (int i = 0; i < resources.Count; i++)
      {
        double resourceSample = SpaceDustResourceMap.Instance.SampleResource(resources[i].Name,
          part.vessel.mainBody,
          vessel.altitude,
          vessel.latitude,
          vessel.longitude);

        
      }
    }
  }


}

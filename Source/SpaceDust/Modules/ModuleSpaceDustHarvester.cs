using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public double MinHarvestValue = 0.0001d;
    public double density = 0.05;


    public HarvestedResource() { }
    public HarvestedResource(ConfigNode node) { Load(node); }
    public void Load(ConfigNode node)
    {
      node.TryGetValue("Name", ref Name);
      node.TryGetValue("BaseEfficiency", ref BaseEfficiency);
      node.TryGetValue("MinHarvestValue", ref MinHarvestValue);
      density = PartResourceLibrary.Instance.GetDefinition(Name).density;
    }

    public string GenerateInfoString()
    {
      return Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustHarvester_Info_Resource", Name, (BaseEfficiency*100f).ToString("F1"));
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

    // The velocity to use when the intake is static
    [KSPField(isPersistant = false)]
    public float IntakeArea = 0f;

    // Maps how well the intake works as velocity increases. 0 = nothing, 1= baseEfficiency

    [KSPField(isPersistant = false)]
    public FloatCurve IntakeVelocityScale;

    [KSPField(isPersistant = false)]
    public HarvesterType HarvestType;

    [KSPField(isPersistant = false)]
    public String HarvestIntakeTransformName;

    [KSPField(isPersistant = false)]
    public String HarvestAnimationName;

    [KSPField(isPersistant = false)]
    public String LoopAnimationName;

    [KSPField(isPersistant = false)]
    public String HeatModuleName;


    // UI field for showing scan status
    [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = false, guiName = "#LOC_SpaceDust_ModuleSpaceDustHarvester_Field_Resources")]
    public string ScannerUI = "";

    // UI field for showing scan status
    [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = false, guiName = "#LOC_SpaceDust_ModuleSpaceDustHarvester_Field_IntakeSpeed")]
    public string IntakeSpeed = "";

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

    public List<HarvestedResource> resources;
    protected Transform HarvestIntakeTransform;
    private AnimationState[] harvestState;
    private AnimationState[] loopState;
    private IScalarModule scalarHeatModule;

    public override string GetModuleDisplayName()
    {
      return Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustHarvester_DisplayName");
    }

    public override string GetInfo()
    {
      string msg = "";
      msg += Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustHarvester_Info_Header", HarvestType.ToString(), IntakeSpeedStatic.ToString("F1"), PowerCost.ToString("F1"));

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
      if (LoopAnimationName != "")
        loopState = Utils.SetUpAnimation(LoopAnimationName, part);

      if (HarvestAnimationName != "")
      {
        harvestState = Utils.SetUpAnimation(HarvestAnimationName, part);
        
        if (Enabled)
        {
          foreach (AnimationState anim in harvestState)
          {
            anim.speed = 0f;
            anim.normalizedTime = 1f;
          }
        }
        else
        {
          foreach (AnimationState anim in harvestState)
          {
            anim.speed = 0f;
            anim.normalizedTime = 0f;
          }
        }
      }
      if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
      {
        if (resources == null || resources.Count == 0)
        {
          ConfigNode node = GameDatabase.Instance.GetConfigs("PART").
              Single(c => part.partInfo.name == c.name).config.
              GetNodes("MODULE").Single(n => n.GetValue("name") == moduleName);
          OnLoad(node);
        }
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
        if (Enabled && LoopAnimationName != "")
        {
          foreach (AnimationState anim in loopState)
          {
            anim.wrapMode = WrapMode.Loop;
            anim.speed = 1f;
          }
        }
        else if (LoopAnimationName != "")
        {
          foreach (AnimationState anim in loopState)
          {
            anim.wrapMode = WrapMode.Loop;
            anim.speed = 0f;
          }
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
            (double)(PowerCost * TimeWarp.fixedDeltaTime)) > 0.00001f)
          {
            Fields["IntakeSpeed"].guiActive = true;
            Fields["ScoopUI"].guiActive = true;

            DoFocusedHarvesting();
            message = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustHarvester_Field_Resources_Harvesting");
          }
          else
          {
            message = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustHarvester_Field_Resources_NoPower");
            Fields["ScoopUI"].guiActive = false;
            Fields["IntakeSpeed"].guiActive = false;
          }
          if (HarvestAnimationName != "")
          {
            foreach (AnimationState anim in harvestState)
            {
              anim.speed = 1f;
              anim.normalizedTime = Mathf.Clamp(anim.normalizedTime, 0f, 1f);
            }
          }

        }
        else
        {
          CurrentPowerConsumption = 0f;
          message = Localizer.Format(("#LOC_SpaceDust_ModuleSpaceDustHarvester_Field_Resources_Disabled"));
          Fields["ScoopUI"].guiActive = false;
          Fields["IntakeSpeed"].guiActive = false;
          if (HarvestAnimationName != "")
          {
            foreach (AnimationState anim in harvestState)
            {
              anim.speed = -1f;
              anim.normalizedTime = Mathf.Clamp(anim.normalizedTime, 0f, 1f);
            }
          }
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
      
      if (HarvestType == HarvesterType.Atmosphere && part.vessel.atmDensity > 0.0001d)
      {
        if (part.vessel.atmDensity < 0.0001d)
        {
          ScoopUI = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustHarvester_Field_Scoop_NeedsAtmo");
          return;
        }
        Vector3d worldVelocity = part.vessel.srf_velocity;
        double mach = part.vessel.mach;

        Vector3 intakeVector;
        if (HarvestIntakeTransform == null)
          intakeVector = this.transform.forward;
        else
          intakeVector = HarvestIntakeTransform.forward;

        double dot = Vector3d.Dot(worldVelocity, intakeVector);
        float intakeVolume = (float)(worldVelocity.magnitude * MathExtensions.Clamp(dot, 0d, 1d) * IntakeVelocityScale.Evaluate((float)mach) + IntakeSpeedStatic) * IntakeArea;
        IntakeSpeed = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustHarvester_Field_IntakeSpeed_Normal", Utils.ToSI(intakeVolume, "G2"));
        ScoopUI = "";
  
        for (int i = 0; i < resources.Count; i++)
        {
          double resourceSample = SpaceDustResourceMap.Instance.SampleResource(resources[i].Name,
            part.vessel.mainBody,
            vessel.altitude + part.vessel.mainBody.Radius,
            vessel.latitude,
            vessel.longitude);

          if (resourceSample > resources[i].MinHarvestValue)
          {
            double resAmt = resourceSample * intakeVolume *  1d/resources[i].density *resources[i].BaseEfficiency;
            if (ScoopUI != "")
              ScoopUI += "\n";
            ScoopUI += Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustHarvester_Field_Scoop_Resource", resources[i].Name, resAmt.ToString("G5"));
            part.RequestResource(resources[i].Name, -resAmt * TimeWarp.fixedDeltaTime, ResourceFlowMode.ALL_VESSEL, false);
          }
          
        }
        if (ScoopUI == "")
          ScoopUI = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustHarvester_Field_Scoop_Resource_None");
      }

      if (HarvestType == HarvesterType.Exosphere && part.vessel.atmDensity < 0.0001d)
      {
        if (part.vessel.atmDensity > 0.0001d)
        {
          ScoopUI = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustHarvester_Field_Scoop_NeedsVacuum");
          return;
        }

        

        double orbitSpeedAtAlt = Math.Sqrt(part.vessel.mainBody.gravParameter/ (part.vessel.altitude + part.vessel.mainBody.Radius));

        Vector3d worldVelocity = part.vessel.obt_velocity;
        Vector3 intakeVector;
        if (HarvestIntakeTransform == null)
          intakeVector = this.transform.forward;
        else
          intakeVector = HarvestIntakeTransform.forward;
        
        
        double dot = Vector3d.Dot(worldVelocity.normalized, intakeVector.normalized);
        float intakeVolume = (float)(worldVelocity.magnitude * MathExtensions.Clamp(dot, 0d, 1d) + IntakeSpeedStatic) * IntakeArea;
        IntakeSpeed = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustHarvester_Field_IntakeSpeed_Normal", intakeVolume.ToString("G2"));

        ScoopUI = "";

        for (int i = 0; i < resources.Count; i++)
        {
          double resourceSample = SpaceDustResourceMap.Instance.SampleResource(resources[i].Name,
            part.vessel.mainBody,
            vessel.altitude + part.vessel.mainBody.Radius,
            vessel.latitude,
            vessel.longitude);

          if (resourceSample * intakeVolume * resources[i].BaseEfficiency > resources[i].MinHarvestValue)
          {
            double resAmt = resourceSample * intakeVolume * 1d / resources[i].density * resources[i].BaseEfficiency;
            ScoopUI += Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustHarvester_Field_Scoop_Resource", resources[i].Name, resAmt.ToString("G3"));
            part.RequestResource(resources[i].Name, -resAmt * TimeWarp.fixedDeltaTime, ResourceFlowMode.ALL_VESSEL, false);
          }

        }
        if (ScoopUI == "")
          ScoopUI = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustHarvester_Field_Scoop_Resource_None");
      }

    }
  }


}

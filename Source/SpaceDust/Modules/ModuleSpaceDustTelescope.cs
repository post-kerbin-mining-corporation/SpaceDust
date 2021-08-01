using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KSP.Localization;
using System.Security.Permissions;

namespace SpaceDust
{

  public class InstrumentSlot
  {
    public string SlotName;
    public string InstrumentName = "None";


    public SpaceDustInstrument Instrument;
    public InstrumentSlot(ConfigNode node)
    {
      node.TryGetValue("name", ref SlotName);
      node.TryGetValue("Instrument", ref InstrumentName);
      if (InstrumentName != "None")
        Instrument = SpaceDustInstruments.Instance.GetInstrument(InstrumentName);
    }
    public InstrumentSlot(string name, string inst)
    {
      SlotName = name;
      InstrumentName = inst;
      if (InstrumentName != "None")
        Instrument = SpaceDustInstruments.Instance.GetInstrument(InstrumentName);
    }

    public ConfigNode Save()
    {
      ConfigNode c = new ConfigNode("SLOT");
      c.AddValue("name", SlotName);
      c.AddValue("Instrument", InstrumentName);
      return c;
    }
  }

  public class ModuleSpaceDustTelescope : PartModule
  {
    // Am i enabled?
    [KSPField(isPersistant = true)]
    public bool Enabled = false;

    // Cost per second to run the telescope
    [KSPField(isPersistant = true)]
    public float PowerCost = 1f;

    // Minimum EC to leave when harvesting
    [KSPField(isPersistant = false)]
    public float minResToLeave = 0.1f;

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
    // UI field for showing scan modifier
    [KSPField(isPersistant = false, guiActive = false, guiActiveEditor = false, guiName = "#LOC_SpaceDust_ModuleSpaceDustTelescope_Field_Modifier_Title")]
    public string ModifierUI = "";

    // UI field for showing scan status
    [KSPField(isPersistant = false, guiActive = false, guiActiveEditor = false, guiName = "#LOC_SpaceDust_ModuleSpaceDustTelescope_Field_Instrument_None")]
    public string InstrumentUI_1 = "";
    // UI field for showing scan status
    [KSPField(isPersistant = false, guiActive = false, guiActiveEditor = false, guiName = "#LOC_SpaceDust_ModuleSpaceDustTelescope_Field_Instrument_None")]
    public string InstrumentUI_2 = "";
    // UI field for showing scan status
    [KSPField(isPersistant = false, guiActive = false, guiActiveEditor = false, guiName = "#LOC_SpaceDust_ModuleSpaceDustTelescope_Field_Instrument_None")]
    public string InstrumentUI_3 = "";
    // UI field for showing scan status
    [KSPField(isPersistant = false, guiActive = false, guiActiveEditor = false, guiName = "#LOC_SpaceDust_ModuleSpaceDustTelescope_Field_Instrument_None")]

    public string InstrumentUI_4 = "";
    // Size of the lens
    [KSPField(isPersistant = false)]
    public double ObjectiveDiameter = 1.8d;

    // Size of the lens
    [KSPField(isPersistant = false)]
    public double FieldOfView = 1.8d;

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
    private List<InstrumentSlot> instrumentSlots;

    public override string GetModuleDisplayName()
    {
      return Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustTelescope_DisplayName");
    }

    public override string GetInfo()
    {
      string msg = "";
      msg += Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustTelescope_Info_Header", PowerCost.ToString("F1"), (Mathf.Rad2Deg * FieldOfView).ToString("F2"), ObjectiveDiameter.ToString("F1"));

      return msg;
    }
    public override void OnLoad(ConfigNode node)
    {
      base.OnLoad(node);
      if (instrumentSlots == null) instrumentSlots = new List<InstrumentSlot>();


      foreach (ConfigNode resNode in node.GetNodes("SLOT"))
      {
        InstrumentSlot newSlot = new InstrumentSlot(resNode);
        // If a slot with this name exists, replace it, else create it
        if (instrumentSlots.FirstOrDefault(x => newSlot.SlotName == x.SlotName) != null)
        {
          instrumentSlots[instrumentSlots.IndexOf(instrumentSlots.Find(x => newSlot.SlotName == x.SlotName))] = newSlot;
        }
        else
        {
          instrumentSlots.Add(newSlot);
        }
      }
      for (int i = 0; i < instrumentSlots.Count; i++)
      {
        if (instrumentSlots[i].Instrument != null)
        {
          Fields[$"InstrumentUI_{i + 1}"].guiName = instrumentSlots[i].Instrument.Title;
        }

      }
    }

    public override void OnSave(ConfigNode node)
    {
      base.OnSave(node);
      if (instrumentSlots != null)
        for (int i = 0; i < instrumentSlots.Count; i++)
        {
          if (instrumentSlots[i] != null)
            node.AddNode(instrumentSlots[i].Save());
        }

    }
    public override void OnAwake()
    {
      base.OnAwake();

    }
    public void Start()
    {


      if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
      {
        if (instrumentSlots == null || instrumentSlots.Count == 0)
        {
          ConfigNode node = Utils.GetModuleConfigNode(part, moduleName);
          if (node != null)
            OnLoad(node);
        }

        for (int i = 0; i < instrumentSlots.Count; i++)
        {
          if (instrumentSlots[i].Instrument != null)
          {
            Fields[$"InstrumentUI_{i + 1}"].guiName = instrumentSlots[i].Instrument.Title;
          }

        }
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
    void SetScanUI(bool on)
    {
      for (int i = 0; i < instrumentSlots.Count; i++)
      {
        if (instrumentSlots[i].Instrument != null)
          Fields[$"InstrumentUI_{i + 1}"].guiActive = on;
        else
          Fields[$"InstrumentUI_{i + 1}"].guiActive = false;
      }
    }
    void FixedUpdate()
    {
      if (HighLogic.LoadedSceneIsFlight)
      {
        if (Enabled)
        {
          CurrentPowerConsumption = -PowerCost;

          vessel.GetConnectedResourceTotals(PartResourceLibrary.ElectricityHashcode, out double currentEC, out double maxEC);
          double chargeRequest = PowerCost * TimeWarp.fixedDeltaTime;

          float angle = 0f;
          CelestialBody obscuringBody = null;

          // check power
          if (currentEC > chargeRequest + minResToLeave)
          {
            double consumption = part.RequestResource(PartResourceLibrary.ElectricityHashcode, chargeRequest);
            if (consumption >= chargeRequest - 0.0001)

            {
              ITargetable target = part.vessel.targetObject;

              if (target != null)
              {

                try
                {
                  CelestialBody targetBody;
                  targetBody = (CelestialBody)target;
                  Target = targetBody.name;

                  if (!Utils.CalculateBodyLOS(this.vessel, targetBody, transform, out angle, out obscuringBody))
                  {
                    SetScanUI(false);
                    //Fields["ModifierUI"].guiActive = false;
                    ScannerUI = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustTelescope_Field_Status_Blocked", obscuringBody.GetDisplayName());
                    return;
                  }

                  if (this.vessel.atmDensity > 0.0001d)
                  {


                    //ModifierUI = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustTelescope_Field_Modifier_InAtmo", atmosphereScale);
                    //Fields["ModifierUI"].guiActive = true;
                  }
                  else
                  {
                    // Fields["ModifierUI"].guiActive = false;
                  }


                  // do scanning
                  ScannerUI = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustTelescope_Field_Status_Observing", part.vessel.targetObject.GetDisplayName());
                  SetScanUI(true);
                  for (int i = 0; i < instrumentSlots.Count; i++)
                  {
                    if (instrumentSlots[i].Instrument != null)
                    {

                      string response = instrumentSlots[i].Instrument.Scan(part.vessel, (CelestialBody)part.vessel.targetObject, ObjectiveDiameter, FieldOfView, TimeWarp.fixedDeltaTime);
                      Fields[$"InstrumentUI_{i + 1}"].SetValue(response, this);
                    }

                  }

                }
                catch (InvalidCastException)
                {
                  Target = "";
                  SetScanUI(false);
                  // Fields["ModifierUI"].guiActive = false;
                  ScannerUI = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustTelescope_Field_Status_NoTarget");
                }

              }
              else
              {
                Target = "";
                SetScanUI(false);
                ScannerUI = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustTelescope_Field_Status_NoTarget");
              }

            }
            else
            {
              Target = "";
              SetScanUI(false);
              ScannerUI = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustTelescope_Field_Status_NoPower");
            }
          }
          else
          {
            Target = "";
            SetScanUI(false);
            ScannerUI = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustTelescope_Field_Status_NoPower");

          }
          if (ScanAnimationName != "")
          {
            foreach (AnimationState anim in scanState)
            {
              anim.speed = 1f;
              anim.normalizedTime = Mathf.Clamp(anim.normalizedTime, 0f, 1f);
            }
          }

        }
        else
        {
          // Fields["ModifierUI"].guiActive = false;
          CurrentPowerConsumption = 0f;
          ScannerUI = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustTelescope_Field_Status_Disabled");
          SetScanUI(false);
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

}

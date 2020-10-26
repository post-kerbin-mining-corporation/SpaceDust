using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SpaceDust
{
  public class SpaceDustTelescopeBackground
  {
    Vessel ves;
    ProtoPartModuleSnapshot protoTelescopeModule;
    PartModule prefabScopeModule;

    ModuleSpaceDustTelescope telescope;

    bool Enabled = false;
    CelestialBody TargetBody;
    List<SpaceDustInstrument> selectedInstruments;

    public SpaceDustTelescopeBackground(Vessel vessel,
      ProtoPartModuleSnapshot protoScope,
      PartModule prefabScope)
    {
      ves = vessel;
      protoTelescopeModule = protoScope;
      prefabScopeModule = prefabScope;

      telescope = prefabScope as ModuleSpaceDustTelescope;
      

      bool.TryParse(protoTelescopeModule.moduleValues.GetValue("Enabled"), out Enabled);
      string tgtString = protoTelescopeModule.moduleValues.GetValue("Target");
      if (tgtString != "")
      {
        TargetBody = FlightGlobals.GetBodyByName(tgtString);
      }

      Utils.Log($"[SDBGT]: {ves.name}: {tgtString}, {TargetBody}, {Enabled}");
      selectedInstruments = new List<SpaceDustInstrument>();
      foreach (ConfigNode c in protoTelescopeModule.moduleValues.GetNodes("SLOT"))
      {
        string instName = c.GetValue("Instrument");
        Utils.Log($"[SDBGT]: {instName}");
        if (instName != "None")
          selectedInstruments.Add(SpaceDustInstruments.Instance.GetInstrument(instName));
      }

      
    }

    public void Process(float timeStep)
    {
      if (Enabled && TargetBody != null)
      {
        for (int i = 0; i<selectedInstruments.Count; i++)
        {
          selectedInstruments[i].Scan(ves, TargetBody, telescope.ObjectiveDiameter, telescope.FieldOfView, timeStep);
        }
      }
  
    }
  }
}

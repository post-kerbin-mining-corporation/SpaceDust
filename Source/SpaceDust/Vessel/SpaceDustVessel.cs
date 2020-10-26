using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace SpaceDust
{
  public class SpaceDustVessel : VesselModule
  {

    public bool Ready { get { return vesselDataReady; } }

    #region PrivateVariables

    bool vesselDataReady = false;
    List<SpaceDustHarvesterBackground> backgroundHarvesters;
    List<SpaceDustTelescopeBackground> backgroundTelescopes;
    #endregion

    protected override void OnStart()
    {
      base.OnStart();

      // These events need to trigger a refresh
      GameEvents.onVesselGoOnRails.Add(new EventData<Vessel>.OnEvent(RefreshVesselData));
      GameEvents.onVesselWasModified.Add(new EventData<Vessel>.OnEvent(RefreshVesselData));
    }

    void OnDestroy()
    {
      // Clean up events when the item is destroyed
      GameEvents.onVesselGoOnRails.Remove(RefreshVesselData);
      GameEvents.onVesselWasModified.Remove(RefreshVesselData);
    }
    void FixedUpdate()
    {
      if (HighLogic.LoadedSceneIsGame && !vesselDataReady)
      {
        if (!vessel.loaded)
        {
          
          RefreshVesselData();
        }
      }
      if (!vessel.loaded && vesselDataReady)
      {
        foreach (SpaceDustHarvesterBackground bg in backgroundHarvesters)
        {
          bg.Process(TimeWarp.fixedDeltaTime);
        }
        foreach (SpaceDustTelescopeBackground bg in backgroundTelescopes)
        {
          bg.Process(TimeWarp.fixedDeltaTime);
        }
      }
    }

    /// <summary>
    /// Referesh the data, given a Vessel event
    /// </summary>
    protected void RefreshVesselData(Vessel eventVessel)
    {
      //if (Settings.DebugModules)
        //Utils.Log(String.Format("[{0}]: Refreshing VesselData from Vessel event", this.GetType().Name));
      RefreshVesselData();
    }
    /// <summary>
    /// Referesh the data, given a ConfigNode event
    /// </summary>
    protected void RefreshVesselData(ConfigNode node)
    {
     // if (Settings.DebugBackground)
       // Utils.Log(String.Format("[{0}]: Refreshing VesselData from save node event", this.GetType().Name));
      RefreshVesselData();
    }


    /// <summary>
    /// Referesh the data classes
    /// </summary>
    protected void RefreshVesselData()
    {

      if (vessel == null || vessel.protoVessel == null)
        return;

      backgroundHarvesters = new List<SpaceDustHarvesterBackground>();
      backgroundTelescopes = new List<SpaceDustTelescopeBackground>();
      foreach (ProtoPartSnapshot protoPart in vessel.protoVessel.protoPartSnapshots)
      {
        // Find telescope components


        Part part_prefab = PartLoader.getPartInfoByName(protoPart.partName).partPrefab;
        var module_prefabs = part_prefab.FindModulesImplementing<PartModule>();

        ProtoPartModuleSnapshot telescopeProto = protoPart.modules.Find(x => x.moduleName == "ModuleSpaceDustTelescope");
        if (telescopeProto != null)
        {
          if (Settings.DebugBackground)
            Utils.Log($"[SpaceDustBackgroundSim]: Found ModuleSpaceDustTelescope on {vessel.name} that is unloaded, adding to background simulation");

         
          var telescopePrefab = module_prefabs.Find(x => x.moduleName == "ModuleSpaceDustTelescope");
          

          SpaceDustTelescopeBackground protoTelescope = new SpaceDustTelescopeBackground(vessel, telescopeProto,telescopePrefab);
          backgroundTelescopes.Add(protoTelescope);
        }

        ProtoPartModuleSnapshot harvesterProto = protoPart.modules.Find(x => x.moduleName == "ModuleSpaceDustHarvester");
        if (harvesterProto != null)
        {
          if (Settings.DebugBackground)
            Utils.Log($"[SpaceDustBackgroundSim]:Found ModuleSpaceDustHarvester on {vessel.name} that is unloaded, adding to background simulation");

          var harvestPrefab = module_prefabs.Find(x => x.moduleName == "ModuleSpaceDustHarvester");
          SpaceDustHarvesterBackground protoHarvester = new SpaceDustHarvesterBackground(vessel, harvesterProto, harvestPrefab);
          backgroundHarvesters.Add(protoHarvester);

        }

      }

      vesselDataReady = true;

      if (Settings.DebugBackground)
      {

      }

    }
  }


}

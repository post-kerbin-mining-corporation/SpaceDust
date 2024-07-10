using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceDust
{
  public class SpaceDustHarvesterBackground
  {
    Vessel ves;
    ProtoPartModuleSnapshot protoMiner;
    PartModule moduleMiner;
    ModuleSpaceDustHarvester harvester;

    public SpaceDustHarvesterBackground(Vessel vessel, ProtoPartModuleSnapshot protoModule, PartModule partModule)
    {
      ves = vessel;
      protoMiner = protoModule;
      moduleMiner = partModule;
      harvester = moduleMiner as ModuleSpaceDustHarvester;

      foreach (HarvestedResource res in harvester.resources)
      {
        res.density = PartResourceLibrary.Instance.GetDefinition(res.Name).density;
        }
    }

    void AddBackgroundResources(ProtoVessel protoVessel, string resourceName, double amountToAdd)
    {
      // Iterate through all parts, adding the amount harvested
      foreach (ProtoPartSnapshot p in ves.protoVessel.protoPartSnapshots)
      {
        foreach (ProtoPartResourceSnapshot r in p.resources)
        {
          if (r.resourceName == resourceName)
          {
            //Utils.Log($"Start {r.amount}/{r.maxAmount}");
            double capacity = r.maxAmount - r.amount;

            if (capacity >= amountToAdd)
            {

              r.amount = r.amount + amountToAdd;
              // Utils.Log($"Added {amountToAdd} to protopart");
              amountToAdd = 0d;
            }
            else
            {
              r.amount = r.maxAmount;
              amountToAdd -= capacity;
            }
            // Utils.Log($"Start {r.amount}/{r.maxAmount}");
          }
          r.UpdateConfigNodeAmounts();
        }

      }

    }
    public void Process(float timeStep)
    {
      // Utils.Log($"[SpaceDustHarvesterBackground]: tring to harvest {harvester.resources.Count} resources");

      //Utils.Log($"[SpaceDustHarvesterBackground]: type {harvester.HarvestType}, density {ves.mainBody.GetPressureAtm(ves.altitude)}, Vsrf {ves.srf_velocity}");
      if (bool.Parse(protoMiner.moduleValues.GetValue("Enabled")))
      {
        if (harvester.HarvestType == HarvestType.Atmosphere && ves.mainBody.GetPressureAtm(ves.altitude) > 0.000d)
        {

          Vector3d worldVelocity = ves.srf_velocity;
          double mach = ves.mach;


          double dot = 1.0d;
          float intakeVolume = (float)(worldVelocity.magnitude * MathExtensions.Clamp(dot, 0d, 1d) * harvester.IntakeVelocityScale.Evaluate((float)mach) + harvester.IntakeSpeedStatic) * harvester.IntakeArea;


          for (int i = 0; i < harvester.resources.Count; i++)
          {
            double resourceSample = SpaceDustResourceMap.Instance.SampleResource(harvester.resources[i].Name,
              ves.mainBody,
              ves.altitude + ves.mainBody.Radius,
              ves.latitude,
              ves.longitude);

            if (resourceSample > harvester.resources[i].MinHarvestValue)
            {
              double amountToAdd = resourceSample * intakeVolume * harvester.resources[i].BaseEfficiency * timeStep;
              AddBackgroundResources(ves.protoVessel, harvester.resources[i].Name, amountToAdd);
            }
          }
        }
        if (harvester.HarvestType == HarvestType.Exosphere && ves.mainBody.GetPressureAtm(ves.altitude) == 0d)
        {

          Vector3d worldVelocity = ves.orbitDriver.vel;
          double dot = 1d;
          float intakeVolume = (float)(worldVelocity.magnitude * MathExtensions.Clamp(dot, 0d, 1d) + harvester.IntakeSpeedStatic) * harvester.IntakeArea;

          for (int i = 0; i < harvester.resources.Count; i++)
          {
            double resourceSample = SpaceDustResourceMap.Instance.SampleResource(harvester.resources[i].Name,
              ves.mainBody,
              ves.altitude + ves.mainBody.Radius,
              ves.latitude,
              ves.longitude);
            
            if (resourceSample >= harvester.resources[i].MinHarvestValue)
            {
              double amountToAdd = resourceSample * intakeVolume * 1d / harvester.resources[i].density * harvester.resources[i].BaseEfficiency * timeStep;

              //Utils.Log($"[SpaceDustHarvesterBackground] sampled {harvester.resources[i].Name} @ {resourceSample}, " +
              //$"minH {harvester.resources[i].MinHarvestValue}," +
              //$"effic {harvester.resources[i].BaseEfficiency}," +
              //$"volume {intakeVolume}," +
              //$"area {harvester.IntakeArea}," +
              //$"speedstatic {harvester.IntakeSpeedStatic}," +
              //$"worldvel {worldVelocity.magnitude}");

              
              //Utils.Log($"[SpaceDustHarvesterBackground] sampled {harvester.resources[i].Name} @ {resourceSample}. Harvesting {amountToAdd} at step {timeStep}");
              AddBackgroundResources(ves.protoVessel, harvester.resources[i].Name, amountToAdd);
            }

          }
        }
      }
    }
  }
}



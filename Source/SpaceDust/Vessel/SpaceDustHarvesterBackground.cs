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

    }

    public void Process(float timeStep)
    {
      // Utils.Log($"[SpaceDustHarvesterBackground]: tring to harvest {harvester.resources.Count} resources");

      //Utils.Log($"[SpaceDustHarvesterBackground]: type {harvester.HarvestType}, density {ves.mainBody.GetPressureAtm(ves.altitude)}, Vsrf {ves.srf_velocity}");
      if (bool.Parse(protoMiner.moduleValues.GetValue("Enabled")))
      {
        if (harvester.HarvestType == HarvesterType.Atmosphere && ves.mainBody.GetPressureAtm(ves.altitude) > 0.0001d)
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
              double resAmt = resourceSample * intakeVolume * harvester.resources[i].BaseEfficiency * timeStep;
              foreach (ProtoPartSnapshot p in ves.protoVessel.protoPartSnapshots)
              {
                foreach (ProtoPartResourceSnapshot r in p.resources)
                {
                  if (r.amount < r.maxAmount + resAmt)
                  {
                    r.amount = r.amount + resAmt;
                    resAmt = 0d;
                  }
                  else
                  if (r.amount < r.maxAmount)
                  {
                    resAmt = resAmt - (r.maxAmount - r.amount);
                    r.amount = r.maxAmount;
                  }
                }
              }
            }
          }
        }
        if (harvester.HarvestType == HarvesterType.Exosphere && ves.mainBody.GetPressureAtm(ves.altitude) < 0.0001d)
        {

          Vector3d worldVelocity = ves.obt_velocity;
          double mach = ves.mach;


         
          double dot = 1d;
          float intakeVolume = (float)(worldVelocity.magnitude * MathExtensions.Clamp(dot, 0d, 1d) + harvester.IntakeSpeedStatic) * harvester.IntakeArea;

          for (int i = 0; i < harvester.resources.Count; i++)
          {
            double resourceSample = SpaceDustResourceMap.Instance.SampleResource(harvester.resources[i].Name,
              ves.mainBody,
              ves.altitude + ves.mainBody.Radius,
              ves.latitude,
              ves.longitude);

            if (resourceSample > harvester.resources[i].MinHarvestValue)
            {
              double resAmt = resourceSample * intakeVolume * harvester.resources[i].BaseEfficiency * timeStep;
              foreach (ProtoPartSnapshot p in ves.protoVessel.protoPartSnapshots)
              {
                foreach (ProtoPartResourceSnapshot r in p.resources)
                {
                  if (r.amount < r.maxAmount + resAmt)
                  {
                    r.amount = r.amount + resAmt;
                    resAmt = 0d;
                  }
                  else
                  if (r.amount < r.maxAmount)
                  {
                    resAmt = resAmt - (r.maxAmount - r.amount);
                    r.amount = r.maxAmount;
                  }
                }
              }
            }

          }
        }
      }
    }
  }
}



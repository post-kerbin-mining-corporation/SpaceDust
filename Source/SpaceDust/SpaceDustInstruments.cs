using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KSP.Localization;
namespace SpaceDust
{
  [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
  public class SpaceDustInstruments : MonoBehaviour
  {
    public static SpaceDustInstruments Instance { get; private set; }

    public Dictionary<string, SpaceDustInstrument> Instruments;


    protected void Awake()
    {
      Instance = this;
    }
    public void Load()
    {
      ConfigNode[] SpaceDustInstrument = GameDatabase.Instance.GetConfigNodes("SPACEDUST_INSTRUMENT");
      Instruments = new Dictionary<string, SpaceDustInstrument>();
      foreach (ConfigNode node in SpaceDustInstrument)
      {
        SpaceDustInstrument newInst = new SpaceDustInstrument(node);
        Instruments.Add(newInst.Name, newInst);
        Utils.LogError($"[SpaceDustInstruments]: Added {newInst.Name} to database");
      }
      Utils.Log($"[SpaceDustInstruments]: Loaded {Instruments.Count} telescope instruments");
    }

    public SpaceDustInstrument GetInstrument(string name)
    {
      if (!Instruments.ContainsKey(name))
      {
        Utils.LogError($"[SpaceDustInstruments]: no defined instrument named {name} exists");
        return null;
      }
      return Instruments[name];
    }
  }

  public class SpaceDustInstrument
  {

    public string Name;
    public string Title;
    public string ResourceName;
    public bool Discovers;
    public bool Identifies;
    public double Wavelength;
    public double Sensitivity;
    public SpaceDustInstrument(ConfigNode node)
    {
      Load(node);
    }

    public void Load(ConfigNode node)
    {
      node.TryGetValue("Name", ref Name);
      node.TryGetValue("Title", ref Title);
      node.TryGetValue("ResourceName", ref ResourceName);
      node.TryGetValue("Discovers", ref Discovers);
      node.TryGetValue("Identifies", ref Identifies);
      node.TryGetValue("Wavelength", ref Wavelength);
      node.TryGetValue("Sensitivity", ref Sensitivity);

      Wavelength *= 1E-9;
      Title = Localizer.Format(Title);
    }


    public string Scan(Vessel ves, CelestialBody targetBody, double objectiveDiam, double FOV, float timeStep)
    {
      string results = "";
      double angRes = 1.22d * Wavelength / objectiveDiam;
      double targetAngularSize = 2d * Math.Atan((targetBody.Radius * 2d) / (2d * Vector3d.Distance(ves.GetWorldPos3D(), targetBody.position)));

      double clampedTargetSize = MathExtensions.Clamp(targetAngularSize, 0d, FOV);

      double pxSize = clampedTargetSize / angRes;

      if (targetAngularSize < angRes)
        results = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustTelescope_Field_Instrument_OutOfRange", targetBody.bodyDisplayName);
      else
      {
        
        float toDiscover = (float)(pxSize* Settings.BaseTelescopeDiscoverRate*Sensitivity/100f);
        if (Discovers)
        {
          SpaceDustScenario.Instance.AddDiscoveryAtBody(ResourceName, targetBody, toDiscover * timeStep);
        }
        if (Identifies)
        {
          SpaceDustScenario.Instance.AddIdentifyAtBody(ResourceName, targetBody, toDiscover * timeStep);
        }

        results = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustTelescope_Field_Instrument_Scanning",
          
          (toDiscover).ToString("F3"));
        //ScannerUI = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustTelescope_Field_Instrument_Progress",
        //  SpaceDustScenario.Instance.GetSurveyProgressAtBody(ResourceName, targetBody).ToString("F1"));
      }
      return results;
    }

  }
}

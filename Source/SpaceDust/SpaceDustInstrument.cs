using System;
using KSP.Localization;
namespace SpaceDust
{
  /// <summary>
  /// Represents an Instrument used to discover distributions remotely, e.g. with a telescope
  /// </summary>
  public class SpaceDustInstrument
  {

    public string Name;
    public string Title;
    public string ResourceName;
    public bool Discovers;
    public bool Identifies;
    public double Wavelength;
    public double Sensitivity;
    public FloatCurve AtmosphereEffect;

    private const string OUT_OF_RANGE_KEY = "#LOC_SpaceDust_ModuleSpaceDustTelescope_Field_Instrument_OutOfRange";
    private const string SCANNING_KEY = "#LOC_SpaceDust_ModuleSpaceDustTelescope_Field_Instrument_Scanning";

    private const string NAME_PARAMETER_NAME = "Name";
    private const string TITLE_PARAMETER_NAME = "Title";
    private const string RESOURCENAME_PARAMETER_NAME = "ResourceName";
    private const string DISCOVERS_PARAMETER_NAME = "Discovers";
    private const string IDENTIFIES_PARAMETER_NAME = "Identifies";
    private const string SENSITIVITY_PARAMETER_NAME = "Sensitivity";
    private const string WAVELENGTH_PARAMETER_NAME = "Wavelength";
    private const string ATMOSPHERE_PARAMETER_NAME = "AtmosphereEffect";

    public SpaceDustInstrument(ConfigNode node)
    {
      Load(node);
    }

    public void Load(ConfigNode node)
    {
      node.TryGetValue(NAME_PARAMETER_NAME, ref Name);
      node.TryGetValue(TITLE_PARAMETER_NAME, ref Title);
      node.TryGetValue(RESOURCENAME_PARAMETER_NAME, ref ResourceName);
      node.TryGetValue(DISCOVERS_PARAMETER_NAME, ref Discovers);
      node.TryGetValue(IDENTIFIES_PARAMETER_NAME, ref Identifies);
      node.TryGetValue(WAVELENGTH_PARAMETER_NAME, ref Wavelength);
      node.TryGetValue(SENSITIVITY_PARAMETER_NAME, ref Sensitivity);

      // Configure the default curve
      AtmosphereEffect = new FloatCurve();
      AtmosphereEffect.Add(0f, 1f);
      AtmosphereEffect.Add(70000f, 5f);
      AtmosphereEffect.Add(500000f, 0f);
      ConfigNode floatCurveNode = new ConfigNode();
      if (node.TryGetNode(ATMOSPHERE_PARAMETER_NAME, ref floatCurveNode))
        AtmosphereEffect.Load(floatCurveNode);

      /// Convert the wavelength
      Wavelength *= 1E-9;
      Title = Localizer.Format(Title);
    }

    /// <summary>
    /// Use the instrument to scan
    /// </summary>
    /// <param name="ves"></param>
    /// <param name="targetBody"></param>
    /// <param name="objectiveDiam"></param>
    /// <param name="FOV"></param>
    /// <param name="timeStep"></param>
    /// <returns></returns>
    public string Scan(Vessel ves, CelestialBody targetBody, double objectiveDiam, double FOV, float timeStep)
    {
      string results;
      double angRes = 1.22d * Wavelength / objectiveDiam;
      double targetAngularSize = 2d * Math.Atan((targetBody.Radius * 2d) / (2d * Vector3d.Distance(ves.GetWorldPos3D(), targetBody.position)));

      double clampedTargetSize = MathExtensions.Clamp(targetAngularSize, 0d, FOV);

      double pxSize = clampedTargetSize / angRes;

      if (targetAngularSize < angRes)
        results = Localizer.Format(OUT_OF_RANGE_KEY, targetBody.bodyDisplayName);
      else
      {
        float atmosphereScale = 0f;
        if (ves.atmDensity > 0.00001)
        {
          atmosphereScale = (float)(Utils.CalculateAirMass(ves, targetBody) * ves.mainBody.atmosphereDepth * ves.mainBody.atmDensityASL);
        }

        float toDiscover = (float)(pxSize * Settings.BaseTelescopeDiscoverRate * Sensitivity / 100f * AtmosphereEffect.Evaluate((float)atmosphereScale));
        if (Discovers)
        {
          SpaceDustScenario.Instance.AddDiscoveryAtBody(ResourceName, targetBody, toDiscover * timeStep);
        }
        if (Identifies)
        {
          SpaceDustScenario.Instance.AddIdentifyAtBody(ResourceName, targetBody, toDiscover * timeStep);
        }

        results = Localizer.Format(SCANNING_KEY, (toDiscover).ToString("F3"));
      }
      return results;
    }

  }
}

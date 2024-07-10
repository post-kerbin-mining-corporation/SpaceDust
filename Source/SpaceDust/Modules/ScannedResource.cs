using KSP.Localization;

namespace SpaceDust
{
  /// <summary>
  /// The kind of discovery a scanner does
  /// </summary>
  public enum DiscoverMode
  {
    None,  
    Local,
    SOI,
    Altitude
  }

  /// <summary>
  /// Represents a resource scanning configuration
  /// </summary>
  public class ScannedResource
  {
    public string Name = "";
    public DiscoverMode DiscoverMode;
    public DiscoverMode IdentifyMode;
    public double LocalThreshold = 0.01;
    public double DiscoverRange = 70000;
    public double IdentifyRange = 30000;
    public double density = 0.05;

    private const string DISCOVER_MODE_PARAMETER_NAME = "DiscoverMode";
    private const string IDENTIFY_MODE_PARAMETER_NAME = "IdentifyMode";
    private const string DISCOVER_RANGE_PARAMETER_NAME = "DiscoverRange";
    private const string IDENTIFY_RANGE_PARAMETER_NAME = "IdentifyRange";
    private const string LOCAL_THRESHOLD_PARAMETER_NAME = "LocalThreshold";

    public ScannedResource(ConfigNode c)
    {
      Load(c);
    }

    public void Load(ConfigNode c)
    {
      c.TryGetValue("name", ref Name);
      c.TryGetEnum<DiscoverMode>(DISCOVER_MODE_PARAMETER_NAME, ref DiscoverMode, DiscoverMode.Local);
      c.TryGetEnum<DiscoverMode>(IDENTIFY_MODE_PARAMETER_NAME, ref IdentifyMode, DiscoverMode.Local);
      c.TryGetValue(LOCAL_THRESHOLD_PARAMETER_NAME, ref LocalThreshold);
      c.TryGetValue(DISCOVER_RANGE_PARAMETER_NAME, ref DiscoverRange);
      c.TryGetValue(IDENTIFY_RANGE_PARAMETER_NAME, ref IdentifyRange);
    }

    /// <summary>
    /// Generates the info string for the part tooltip in the parts picker
    /// </summary>
    /// <returns></returns>
    public string GenerateInfoString()
    {
      string msg = Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustScanner_Info_Resource", Name);
      msg += " ";
      switch (DiscoverMode)
      {
        case DiscoverMode.Local:
          msg += Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustScanner_Info_Discovers_Local");
          break;
        case DiscoverMode.SOI:
          msg += Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustScanner_Info_Discovers_SOI");
          break;
        case DiscoverMode.Altitude:
          msg += Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustScanner_Info_Discovers_Altitude", (DiscoverRange / 1000.0).ToString("F0"));
          break;
      }
      msg += " ";
      switch (IdentifyMode)
      {
        case DiscoverMode.Local:
          msg += Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustScanner_Info_Identifies_Local");
          break;
        case DiscoverMode.SOI:
          msg += Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustScanner_Info_Identifies_SOI");
          break;
        case DiscoverMode.Altitude:
          msg += Localizer.Format("#LOC_SpaceDust_ModuleSpaceDustScanner_Info_Identifies_Altitude", (IdentifyRange / 1000.0).ToString("F0"));
          break;
      }
      return msg;
    }
  }
}

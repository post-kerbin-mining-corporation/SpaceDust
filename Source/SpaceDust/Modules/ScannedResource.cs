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

    public ScannedResource(ConfigNode c)
    {
      Load(c);
    }

    public void Load(ConfigNode c)
    {
      c.TryGetValue("name", ref Name);
      c.TryGetEnum<DiscoverMode>("DiscoverMode", ref DiscoverMode, DiscoverMode.Local);
      c.TryGetEnum<DiscoverMode>("IdentifyMode", ref IdentifyMode, DiscoverMode.Local);
      c.TryGetValue("LocalThreshold", ref LocalThreshold);
      c.TryGetValue("DiscoverRange", ref DiscoverRange);
      c.TryGetValue("IdentifyRange", ref IdentifyRange);
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

namespace SpaceDust
{
  /// <summary>
  /// Represents persistent data about any discovery state of a resource band at a body
  /// </summary>
  public class SpaceDustDiscoveryData
  {
    public string ResourceName = "noResource";
    public string BodyName = "noBody";
    public string BandName = "noBand";
    public bool Discovered = false;
    public bool Identified = false;
    public float discoveryPercent = 0f;
    public float identifyPercent = 0f;

    private const string RESOURCENAME_PARAMETER_NAME = "resourceName";
    private const string BODYNAME_PARAMETER_NAME = "bodyName";
    private const string BANDNAME_PARAMETER_NAME = "bandName";
    private const string DISCOVERED_PARAMETER_NAME = "discovered";
    private const string IDENTIFIED_PARAMETER_NAME = "identified";
    private const string DISCOVERED_PERCENT_PARAMETER_NAME = "discoveryPercent";
    private const string IDENTIFIED_PERCENT_PARAMETER_NAME = "identifyPercent";

    public SpaceDustDiscoveryData()
    { }

    public SpaceDustDiscoveryData(string resourceName, string bandName, string bodyName, bool isDiscovered, bool isIdentified)
    {
      ResourceName = resourceName;
      BodyName = bodyName;
      BandName = bandName;
      Discovered = isDiscovered;
      Identified = isIdentified;

      if (isDiscovered)
      {
        discoveryPercent = 100f;
      }
      if (isIdentified)
      {
        identifyPercent = 100f;
      }
    }

    public SpaceDustDiscoveryData(ConfigNode node)
    {
      Load(node);
    }

    /// <summary>
    /// Load from a ConfigNode
    /// </summary>
    /// <param name="node"></param>
    public void Load(ConfigNode node)
    {
      node.TryGetValue(RESOURCENAME_PARAMETER_NAME, ref ResourceName);
      node.TryGetValue(BODYNAME_PARAMETER_NAME, ref BodyName);
      node.TryGetValue(BANDNAME_PARAMETER_NAME, ref BandName);
      node.TryGetValue(DISCOVERED_PARAMETER_NAME, ref Discovered);
      node.TryGetValue(IDENTIFIED_PARAMETER_NAME, ref Identified);
      node.TryGetValue(DISCOVERED_PERCENT_PARAMETER_NAME, ref discoveryPercent);
      node.TryGetValue("", ref identifyPercent);
    }

    /// <summary>
    /// Save to a ConfigNode
    /// </summary>
    /// <returns></returns>
    public ConfigNode Save()
    {
      ConfigNode node = new ConfigNode();
      node.name = Settings.PERSISTENCE_DATA_NODE_NAME;
      node.AddValue(RESOURCENAME_PARAMETER_NAME, ResourceName);
      node.AddValue(BODYNAME_PARAMETER_NAME, BodyName);
      node.AddValue(BANDNAME_PARAMETER_NAME, BandName);
      node.AddValue(DISCOVERED_PARAMETER_NAME, Discovered);
      node.AddValue(IDENTIFIED_PARAMETER_NAME, Identified);
      node.AddValue(DISCOVERED_PERCENT_PARAMETER_NAME, discoveryPercent);
      node.AddValue(IDENTIFIED_PERCENT_PARAMETER_NAME, identifyPercent);
      return node;
    }

    public new string ToString()
    {
      return $"DiscoveryData(Band {BandName} around {BodyName} containing {ResourceName}: Discovered  = {discoveryPercent}% ({Discovered}),Identified = {identifyPercent}% ({Identified}))";
    }
  }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceDust
{
  /// <summary>
  /// Class for loading and managing the set of Space Dust Instruments
  /// </summary>
  [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
  public class SpaceDustInstruments : MonoBehaviour
  {
    public static SpaceDustInstruments Instance { get; private set; }
    public Dictionary<string, SpaceDustInstrument> Instruments;

    protected void Awake()
    {
      Instance = this;
    }
    /// <summary>
    /// Load all the instrument data
    /// </summary>
    public void Load()
    {

      ConfigNode[] SpaceDustInstrument = GameDatabase.Instance.GetConfigNodes(Settings.INSTRUMENT_DATA_NODE_NAME);
      Instruments = new Dictionary<string, SpaceDustInstrument>();
      foreach (ConfigNode node in SpaceDustInstrument)
      {
        try
        {
          SpaceDustInstrument newInst = new SpaceDustInstrument(node);
          Instruments.Add(newInst.Name, newInst);
          Utils.Log($"[SpaceDustInstruments]: Added {newInst.Name} to database", LogType.Loading);
        }
        catch (Exception)
        {
          throw new TypeLoadException(node.ToString());
        }
      }
      Utils.Log($"[SpaceDustInstruments]: Loaded {Instruments.Count} telescope instruments", LogType.Loading);

    }

    /// <summary>
    /// Get an Instrument by name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public SpaceDustInstrument GetInstrument(string name)
    {
      if (Instruments == null) Load();

      if (!Instruments.ContainsKey(name))
      {
        Utils.LogError($"[SpaceDustInstruments]: no defined instrument named {name} exists");
        return null;
      }
      return Instruments[name];
    }
  }

}

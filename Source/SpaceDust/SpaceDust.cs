using System;
using UnityEngine;

namespace SpaceDust
{
  /// <summary>
  /// Main SpaceDust class, loads all data up
  /// </summary>
  [KSPAddon(KSPAddon.Startup.EveryScene, false)]
  public class SpaceDust : MonoBehaviour
  {
    public static SpaceDust Instance { get; private set; }

    protected void Awake()
    {
      Instance = this;
    }
    protected void Start()
    {
      Settings.Load();
      if (HighLogic.LoadedSceneIsGame)
      {
        try
        {
          SpaceDustInstruments.Instance.Load();
        }
        catch (TypeLoadException except)
        {
          Utils.LogError($"[SpaceDust] Critical error loading SpaceDustInstrument from confignode {except.Message}");
        }
        try
        {
          SpaceDustResourceMap.Instance.Load();
        }
        catch (TypeLoadException except)
        {
          Utils.LogError($"[SpaceDust] Critical error loading SpaceDustResource from configNode {except.Message}");
        }
      }
    }
  }
}

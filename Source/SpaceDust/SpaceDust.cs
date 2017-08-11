using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SpaceDust
{


  [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.SPACECENTER, GameScenes.FLIGHT, GameScenes.TRACKSTATION, GameScenes.EDITOR)]
  public class SpaceDustScenario : ScenarioModule
  {
    public override void OnAwake()
    {
      Utils.Log("[SpaceDustScenario]: Awake");
      Instance = this;
      base.OnAwake();
    }

    public override void OnLoad(ConfigNode node)
    {
      Utils.Log("[SpaceDustScenario]: Started Loading");
      base.OnLoad(node);
      SpaceDustResourceMap.Instance.Load();

      Utils.Log("[SpaceDustScenario]: Done Loading");
    }
  }


  [KSPAddon(KSPAddon.Startup.EveryScene, false)]
  public class SpaceDustResourceMap : MonoBehaviour
  {
    private static SpaceDustResourceMap instance;
    public static SpaceDustResourceMap Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SpaceDust>();
                if (instance == null)
                {
                    GameObject obj = new SpaceDustResourceMap();
                    instance = obj.AddComponent<SpaceDust>();
                }
            }
            return instance;
        }
    }

    

    public void Load()
    {
      ConfigNode resourceNodes = GameDatabase.Instance.GetConfigs("SPACEDUST_RESOURCE");
    }


  }
}

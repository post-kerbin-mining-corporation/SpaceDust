using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP.UI.Screens;
using KSP.UI;
using UnityEngine;

namespace SpaceDust
{
  [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
  public class ToolbarUI : MonoBehaviour
  {
    public static ToolbarUI Instance { get; private set; }
    // Control Vars
    protected static bool showWindow = false;


    // Panel
    protected ToolbarPanel toolbarPanel;

    // Stock toolbar button
    protected string toolbarUIIconURLOff = "SpaceDust/UI/toolbar_off";
    protected string toolbarUIIconURLOn = "SpaceDust/UI/toolbar_on";
    protected static ApplicationLauncherButton stockToolbarButton = null;

    // Data
    Dictionary<string, bool> resourceVisibilities;

    protected virtual void Awake()
    {
      if (HighLogic.LoadedSceneHasPlanetarium)
      {
        GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
        GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);

        GameEvents.OnMapEntered.Add(new EventVoid.OnEvent(OnMapEntered));
        GameEvents.OnMapExited.Add(new EventVoid.OnEvent(OnMapExited));
        GameEvents.OnMapFocusChange.Add(new EventData<MapObject>.OnEvent(OnMapFocusChange));
        GameEvents.onGUIApplicationLauncherUnreadifying.Add(new EventData<GameScenes>.OnEvent(OnGUIAppLauncherUnreadifying));
      }
      resourceVisibilities = new Dictionary<string, bool>();
      Instance = this;
    }



    public void OnMapEntered()
    {
      if (Settings.DebugUI)
        Utils.Log($"[ToolbarUI] Entering map view, focus on {PlanetariumCamera.fetch.target.DisplayName}");
      CelestialBody body = PlanetariumCamera.fetch.target.celestialBody;
      if (body == null)
      {
        body = PlanetariumCamera.fetch.target.vessel.mainBody;
      }
      RefreshResources(body);
    }
    public void OnMapExited()
    {
      if (Settings.DebugUI)
        Utils.Log($"[ToolbarUI] Exiting map view");
      if (toolbarPanel != null)
        toolbarPanel.SetVisible(false);
    }
    public void OnMapFocusChange(MapObject mapObject)
    {
      if (Settings.DebugUI)
        Utils.Log($"[ToolbarUI] Changed focus to {mapObject.GetName()}");
      if (mapObject != null)

      {
        CelestialBody body = mapObject.celestialBody;
        if (body == null)
        {
          body = mapObject.vessel.mainBody;
        }
        RefreshResources(body);
      }
    }

    public void RefreshResources(CelestialBody body)
    {
      DestroyResources();
      List<string> bodyResources = SpaceDustResourceMap.Instance.GetBodyResources(body);
      if (bodyResources.Count > 0)
      {
        foreach (string res in bodyResources)
        {
          if (Settings.visibleResources.Contains(res))
          {
            if (!resourceVisibilities.ContainsKey(res))
              resourceVisibilities.Add(res, false);

            if (toolbarPanel != null)
              toolbarPanel.AddResourceEntry(body, res, SpaceDustResourceMap.Instance.GetBodyDistributions(body, res), resourceVisibilities[res]);
          }
        }
      }
    }

    public void SetResourceVisible(string resourceName, bool shown)
    {
      if (resourceVisibilities.ContainsKey(resourceName))
        resourceVisibilities[resourceName] = shown;
    }
    public bool IsVisible(string resourceName)
    {
      if (resourceVisibilities.ContainsKey(resourceName))
        return resourceVisibilities[resourceName];

      return false;
    }
    protected void DestroyResources()
    {
      if (toolbarPanel != null)
        toolbarPanel.RemoveResourceEntries();
    }

    public void Start()
    {

      if (ApplicationLauncher.Ready)
        OnGUIAppLauncherReady();


    }

    protected void CreateToolbarPanel()
    {
      GameObject newUIPanel = (GameObject)Instantiate(Assets.ToolbarPanelPrefab, Vector3.zero, Quaternion.identity);
      newUIPanel.transform.SetParent(UIMasterController.Instance.appCanvas.transform);
      newUIPanel.transform.localPosition = Vector3.zero;
      toolbarPanel = newUIPanel.AddComponent<ToolbarPanel>();
      toolbarPanel.SetVisible(false);
    }
    protected void DestroyToolbarPanel()
    {
      if (toolbarPanel != null && toolbarPanel.gameObject != null)
      {
        Destroy(toolbarPanel.gameObject);
      }
    }


    public void ToggleAppLauncher()
    {
      showWindow = !showWindow;
      toolbarPanel.SetVisible(showWindow);
      if (showWindow)
      {
        MapOverlay.Instance.SetVisible(true);
      }
      else
      {
        MapOverlay.Instance.SetVisible(false);
      }

    }

    void Update()
    {
      if (showWindow && toolbarPanel)
      {

        if (HighLogic.LoadedSceneHasPlanetarium)
        {
          toolbarPanel.rect.position = stockToolbarButton.GetAnchorUL() - new Vector3(toolbarPanel.rect.rect.width + 50f, toolbarPanel.rect.rect.height, 0f);
        }
        
      }
    }

    #region Stock Toolbar Methods
    public void OnDestroy()
    {
      // Remove the stock toolbar button
      GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
      if (stockToolbarButton != null)
      {
        ApplicationLauncher.Instance.RemoveModApplication(stockToolbarButton);
      }

      GameEvents.OnMapEntered.Remove(OnMapEntered);
      GameEvents.OnMapFocusChange.Remove(OnMapFocusChange);

      GameEvents.OnMapExited.Remove(OnMapExited);
      
    }

    protected void OnToolbarButtonToggle()
    {
      stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture(showWindow ? toolbarUIIconURLOn : toolbarUIIconURLOff, false));
      ToggleAppLauncher();
    }


    protected void OnGUIAppLauncherReady()
    {
      showWindow = false;
      if (ApplicationLauncher.Ready && stockToolbarButton == null)
      {
        stockToolbarButton = ApplicationLauncher.Instance.AddModApplication(
            OnToolbarButtonToggle,
            OnToolbarButtonToggle,
            DummyVoid,
            DummyVoid,
            DummyVoid,
            DummyVoid,
            ApplicationLauncher.AppScenes.MAPVIEW | ApplicationLauncher.AppScenes.TRACKSTATION,
            (Texture)GameDatabase.Instance.GetTexture(toolbarUIIconURLOff, false));
      }
      CreateToolbarPanel();
    }

    protected void OnGUIAppLauncherDestroyed()
    {
      if (stockToolbarButton != null)
      {
        ApplicationLauncher.Instance.RemoveModApplication(stockToolbarButton);
        stockToolbarButton = null;
      }
      DestroyToolbarPanel();
    }


    protected void OnGUIAppLauncherUnreadifying(GameScenes scene)
    {


      DestroyToolbarPanel();
    }

    protected void onAppLaunchToggleOff()
    {
      stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture(toolbarUIIconURLOff, false));
    }

    protected void DummyVoid() { }

    public void ResetAppLauncher()
    {
      if (stockToolbarButton == null)
      {
        stockToolbarButton = ApplicationLauncher.Instance.AddModApplication(
            OnToolbarButtonToggle,
            OnToolbarButtonToggle,
            DummyVoid,
            DummyVoid,
            DummyVoid,
            DummyVoid,
            ApplicationLauncher.AppScenes.MAPVIEW | ApplicationLauncher.AppScenes.TRACKSTATION,
            (Texture)GameDatabase.Instance.GetTexture(toolbarUIIconURLOff, false));
      }

    }
    #endregion


  }
}

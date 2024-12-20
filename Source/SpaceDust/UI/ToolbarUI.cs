﻿using System.Collections.Generic;
using KSP.UI.Screens;
using KSP.UI;
using UnityEngine;

namespace SpaceDust
{
  /// <summary>
  /// Creates the toolbar panel and manages the app launcher
  /// </summary>
  [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
  public class ToolbarUI : MonoBehaviour
  {
    public static ToolbarUI Instance { get; private set; }
    // Control Vars
    protected static bool showWindow = false;
    protected static bool pinnedOn = false;

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
        GameEvents.onGUIApplicationLauncherUnreadifying.Add(new EventData<GameScenes>.OnEvent(OnGUIAppLauncherUnreadifying));

        GameEvents.OnMapEntered.Add(new EventVoid.OnEvent(OnMapEntered));
        GameEvents.OnMapExited.Add(new EventVoid.OnEvent(OnMapExited));
        GameEvents.OnMapFocusChange.Add(new EventData<MapObject>.OnEvent(OnMapFocusChange));
      }
      resourceVisibilities = new Dictionary<string, bool>();
      Instance = this;
    }

    public void Start()
    {
      if (ApplicationLauncher.Ready)
      {
        OnGUIAppLauncherReady();
      }
    }

    public void OnMapEntered()
    {
      Utils.Log($"[ToolbarUI] Entering map view, focus on {PlanetariumCamera.fetch.target.DisplayName}", LogType.UI);
      CelestialBody body = PlanetariumCamera.fetch.target.celestialBody;
      if (body == null)
      {
        body = PlanetariumCamera.fetch.target.vessel.mainBody;
      }
      RefreshResources(body);
    }

    public void OnMapExited()
    {
      Utils.Log($"[ToolbarUI] Exiting map view", LogType.UI);
      if (toolbarPanel != null)
        toolbarPanel.SetVisible(false);
    }
    public void OnMapFocusChange(MapObject mapObject)
    {

      Utils.Log($"[ToolbarUI] Changed focus to {mapObject.GetName()}", LogType.UI);
      if (mapObject != null)
      {
        CelestialBody body = mapObject.celestialBody;
        if (body == null)
        {
          body = mapObject.orbit.referenceBody;
        }
        RefreshResources(body);
      }
    }

    /// <summary>
    /// Refreshes the resource display
    /// </summary>
    /// <param name="body"></param>
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

    /// <summary>
    /// Sets a resource to be visible in the toolbar panel
    /// </summary>
    /// <param name="resourceName"></param>
    /// <param name="shown"></param>
    public void SetResourceVisible(string resourceName, bool shown)
    {
      if (resourceVisibilities.ContainsKey(resourceName))
        resourceVisibilities[resourceName] = shown;
    }

    /// <summary>
    /// Is a resource visible in the toolbar panel?
    /// </summary>
    /// <param name="resourceName"></param>
    /// <returns></returns>
    public bool IsVisible(string resourceName)
    {
      if (resourceVisibilities.ContainsKey(resourceName))
        return resourceVisibilities[resourceName];

      return false;
    }

    /// <summary>
    /// Just nuke the entries
    /// </summary>
    protected void DestroyResources()
    {
      if (toolbarPanel != null)
        toolbarPanel.RemoveResourceEntries();
    }

    /// <summary>
    /// Creates the toolbar panel
    /// </summary>
    protected void CreateToolbarPanel()
    {
      GameObject newUIPanel = (GameObject)Instantiate(SpaceDustAssets.ToolbarPanelPrefab, Vector3.zero, Quaternion.identity);
      newUIPanel.transform.SetParent(UIMasterController.Instance.appCanvas.transform);
      newUIPanel.transform.localPosition = Vector3.zero;
      toolbarPanel = newUIPanel.AddComponent<ToolbarPanel>();
      toolbarPanel.SetVisible(false);
    }
    /// <summary>
    /// Destroys the toolbar panel
    /// </summary>
    protected void DestroyToolbarPanel()
    {
      if (toolbarPanel != null && toolbarPanel.gameObject != null)
      {
        Destroy(toolbarPanel.gameObject);
      }
    }

    /// <summary>
    /// Hover state 
    /// </summary>
    public void SetHoverState(bool on)
    {
      if (pinnedOn)
        return;

      showWindow = on;
      toolbarPanel.SetVisible(showWindow);
    }

    /// <summary>
    /// Clicked state
    /// </summary>
    /// <param name="on"></param>
    public void SetClickedState(bool on)
    {
      pinnedOn = on;
      showWindow = pinnedOn;

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
          if (HighLogic.LoadedSceneIsFlight)
          {
            toolbarPanel.rect.pivot = new Vector2(1, 1);
            toolbarPanel.rect.position = stockToolbarButton.GetAnchorUL();
          }
          else
          {
            toolbarPanel.rect.pivot = new Vector2(1, 0);
            toolbarPanel.rect.position = stockToolbarButton.GetAnchorUR();
          }
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
      GameEvents.onGUIApplicationLauncherDestroyed.Remove(OnGUIAppLauncherDestroyed);
      GameEvents.onGUIApplicationLauncherUnreadifying.Remove(OnGUIAppLauncherUnreadifying);

      GameEvents.OnMapEntered.Remove(OnMapEntered);
      GameEvents.OnMapFocusChange.Remove(OnMapFocusChange);
      GameEvents.OnMapExited.Remove(OnMapExited);

    }

    protected void OnGUIAppLauncherReady()
    {
      showWindow = false;
      if (ApplicationLauncher.Ready && stockToolbarButton == null)
      {
        stockToolbarButton = ApplicationLauncher.Instance.AddModApplication(
          OnToolbarButtonOn,
          OnToolbarButtonOff,
          OnToolbarButtonHover,
          OnToolbarButtonHoverOut,
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

    protected void OnToolbarButtonHover()
    {
      stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture(showWindow ? toolbarUIIconURLOn : toolbarUIIconURLOff, false));
      SetHoverState(true);
    }
    protected void OnToolbarButtonHoverOut()
    {
      stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture(showWindow ? toolbarUIIconURLOn : toolbarUIIconURLOff, false));
      SetHoverState(false);
    }
    protected void OnToolbarButtonOn()
    {
      stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture(showWindow ? toolbarUIIconURLOn : toolbarUIIconURLOff, false));
      SetClickedState(true);
    }
    protected void OnToolbarButtonOff()
    {
      stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture(showWindow ? toolbarUIIconURLOn : toolbarUIIconURLOff, false));
      SetClickedState(false);
    }
    protected void DummyVoid() { }

    public void ResetAppLauncher()
    {
      if (stockToolbarButton == null)
      {
        stockToolbarButton = ApplicationLauncher.Instance.AddModApplication(
          OnToolbarButtonOn,
          OnToolbarButtonOff,
          OnToolbarButtonHover,
          OnToolbarButtonHoverOut,
          DummyVoid,
          DummyVoid,
          ApplicationLauncher.AppScenes.MAPVIEW | ApplicationLauncher.AppScenes.TRACKSTATION,
          (Texture)GameDatabase.Instance.GetTexture(toolbarUIIconURLOff, false));
      }
    }
    #endregion
  }
}

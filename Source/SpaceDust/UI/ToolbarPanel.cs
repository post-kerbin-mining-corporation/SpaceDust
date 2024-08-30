using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using KSP.Localization;

namespace SpaceDust
{
  /// <summary>
  /// The toolbar panel that is shown when clicking on the app bar
  /// </summary>
  public class ToolbarPanel : MonoBehaviour
  {
    public bool active = false;
    public RectTransform rect;
    public Text noneText;
    public Text panelTitle;
    public Text resourceHeader;
    public RectTransform resourceList;

    public List<ToolbarResourceElement> resourceEntries;

    void Awake()
    {
      rect = this.transform as RectTransform;
      resourceEntries = new List<ToolbarResourceElement>();

      noneText = Utils.FindChildOfType<Text>("NoResourcesObject", transform);
      panelTitle = Utils.FindChildOfType<Text>("PanelTitleText", transform);
      resourceHeader = Utils.FindChildOfType<Text>("HeaderText", transform);
      resourceList = transform.FindDeepChild("ResourceList").transform as RectTransform;
    }

    public void Start()
    {
      panelTitle.text = Localizer.Format("#LOC_SpaceDust_UI_PanelTitle");
      resourceHeader.text = Localizer.Format("#LOC_SpaceDust_UI_ResourceAreaHeader");
      noneText.text = Localizer.Format("#LOC_SpaceDust_UI_NoResources");
    }

    /// <summary>
    /// Sets the panel visibility
    /// </summary>
    /// <param name="state"></param>
    public void SetVisible(bool state)
    {
      active = state;
      rect.gameObject.SetActive(state);
    }

    public void Update()
    {
      bool hasVisibleResources = false;
      if (resourceEntries != null && resourceEntries.Count > 0)
      {
        for (int i = resourceEntries.Count - 1; i >= 0; i--)
        {
          if (resourceEntries[i].active)
          {
            hasVisibleResources = true;
          }
        }
      }

      noneText.gameObject.SetActive(!hasVisibleResources);
    }

    /// <summary>
    /// Removes all the toolbar resource entries
    /// </summary>
    public void RemoveResourceEntries()
    {
      Utils.Log($"[ToolbarPanel] Clearing all entries", LogType.UI);
      if (resourceEntries != null && resourceEntries.Count > 0)
      {
        for (int i = resourceEntries.Count - 1; i >= 0; i--)
        {
          Destroy(resourceEntries[i].gameObject);
        }
        resourceEntries.Clear();
      }
      noneText.gameObject.SetActive(true);
    }

    /// <summary>
    /// Creates a resource entry for a set of Bands
    /// </summary>
    /// <param name="body"></param>
    /// <param name="resourceName"></param>
    /// <param name="bands"></param>
    /// <param name="shown"></param>
    public void AddResourceEntry(CelestialBody body, string resourceName, List<ResourceBand> bands, bool shown)
    {

      Utils.Log($"[ToolbarPanel]: Adding a new resource element for {resourceName}", LogType.UI);

      noneText.gameObject.SetActive(false);
      GameObject newElement = (GameObject)Instantiate(SpaceDustAssets.ToolbarWidgetPrefab, Vector3.zero, Quaternion.identity);

      newElement.transform.SetParent(resourceList);
      ToolbarResourceElement res = newElement.AddComponent<ToolbarResourceElement>();

      res.AssignResource(body, resourceName, bands, shown);

      resourceEntries.Add(res);
      Utils.Log($"[ToolbarPanel] Added a new resource entry for {resourceName}", LogType.UI);

    }
  }
}

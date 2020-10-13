using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UnityEngine;


namespace SpaceDust
{
  public class ToolbarPanel: MonoBehaviour
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

      noneText = transform.FindDeepChild("NoResourcesObject").GetComponent<Text>();
      panelTitle = transform.FindDeepChild("PanelTitleText").GetComponent<Text>();
      resourceHeader = transform.FindDeepChild("HeaderText").GetComponent<Text>();
      resourceList = transform.FindDeepChild("ResourceList").transform as RectTransform;
      
    }
    public void Start()
    {
      panelTitle.text = "Space Dust";
      resourceHeader.text = "Harvestable Resources";
      noneText.text = "No resources discovered!";
    }

    public void SetVisible(bool state)
    {
      active = state;
      rect.gameObject.SetActive(state);
    }

    public void RemoveResourceEntries()
    {
      Utils.Log($"[ToolbarPanel] Clearing all entries");
      if (resourceEntries != null && resourceEntries.Count > 0)
      {
        for (int i= resourceEntries.Count-1; i >= 0;i--)
        {
          Destroy(resourceEntries[i].gameObject);
        }
        resourceEntries.Clear();
      }
      noneText.gameObject.SetActive(true);
    }

    public void AddResourceEntry(CelestialBody body, string resourceName, List<ResourceBand> bands, bool shown)
    {
      Utils.Log($"[ToolbarPanel]: Adding a new resource element for {resourceName}");
      noneText.gameObject.SetActive(false);
      GameObject newElement = (GameObject)Instantiate(UILoader.ToolbarWidgetPrefab, Vector3.zero, Quaternion.identity);
      
      newElement.transform.SetParent(resourceList);
      //newUIPanel.transform.localPosition = Vector3.zero;
      ToolbarResourceElement res = newElement.AddComponent<ToolbarResourceElement>();

      res.SetResource(body, resourceName, bands, shown);

      resourceEntries.Add(res);
      Utils.Log($"[ToolbarPanel] Added a new resource entry for {resourceName}");
    }
  }
}

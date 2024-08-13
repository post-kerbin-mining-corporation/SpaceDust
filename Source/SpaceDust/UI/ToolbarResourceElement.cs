using SpaceDust.Overlay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using KSP.Localization;

namespace SpaceDust
{
  public class ToolbarResourceElement : MonoBehaviour
  {
    public bool active = false;
    public RectTransform rect;
    public Toggle visibleToggle;
    public Image resourceColor;
    public Text resourceName;

    public List<BandResourceElement> bandWidgets;

    private string resName = "";
    private const string RESOURCE_UNKNOWN_KEY = "#LOC_SpaceDust_UI_UnknownResource";

    void Awake()
    {
      FindElements();
    }

    void FindElements()
    {
      rect = this.transform as RectTransform;

      resourceColor = transform.FindDeepChild("Swatch").GetComponent<Image>();
      resourceName = transform.FindDeepChild("Label").GetComponent<Text>();

      visibleToggle = transform.GetComponent<Toggle>();
      visibleToggle.onValueChanged.AddListener(delegate { ToggleResource(); });

      bandWidgets = new List<BandResourceElement>();
    }

    /// <summary>
    /// Toggles overlay visibility for this resource
    /// </summary>
    public void ToggleResource()
    {
      MapOverlay.Instance.SetResourceVisible(resName, visibleToggle.isOn);
      ToolbarUI.Instance.SetResourceVisible(resName, visibleToggle.isOn);

      foreach (BandResourceElement bandElement in bandWidgets)
      {
        if (SpaceDustScenario.Instance.IsDiscovered(resName, bandElement.associatedBand.name, bandElement.associatedBody))
        {
          bandElement.SetVisible(true);
        }
        else
        {
          bandElement.SetVisible(false);
        }
      }
    }

    /// <summary>
    /// Assigns a resource for this UI widget
    /// </summary>
    /// <param name="body"></param>
    /// <param name="ResourceName"></param>
    /// <param name="bands"></param>
    /// <param name="shown"></param>
    public void AssignResource(CelestialBody body, string ResourceName, List<ResourceBand> bands, bool shown)
    {
      if (resourceColor == null) FindElements();

      resName = ResourceName;
      bool anyID = SpaceDustScenario.Instance.IsAnyIdentified(ResourceName, body);
      bool anyDiscover = SpaceDustScenario.Instance.IsAnyDiscovered(ResourceName, body);
      resourceColor.enabled = anyID;

      if (anyID)
      {
        resourceName.text = ResourceName;
        resourceColor.color = Settings.GetResourceColor(ResourceName);
        SetVisible(true);
      }
      else if (anyDiscover)
      {
        resourceColor.color = Settings.resourceDiscoveredColor;
        resourceName.text = Localizer.Format(RESOURCE_UNKNOWN_KEY);
        SetVisible(true);
      }
      else
      {
        SetVisible(false);
      }

      foreach (ResourceBand band in bands)
      {
        GameObject bandElement = (GameObject)Instantiate(SpaceDustAssets.BandResourceWidgetPrefab, Vector3.zero, Quaternion.identity);
        bandElement.transform.SetParent(rect);

        BandResourceElement bandResElement = bandElement.AddComponent<BandResourceElement>();
        bandResElement.AssignBand(body, band);
        bandWidgets.Add(bandResElement);

        /// bands are visible in the UI if they have been discovered
        if (SpaceDustScenario.Instance.IsDiscovered(ResourceName, band.name, body))
        {
          bandResElement.SetVisible(true);
        }
        else
        {
          bandResElement.SetVisible(false);
        }
      }
      visibleToggle.isOn = shown;
      MapOverlay.Instance.SetResourceVisible(resName, visibleToggle.isOn);
      ToolbarUI.Instance.SetResourceVisible(resName, visibleToggle.isOn);
    }

    /// <summary>
    /// Sets if this resource is visible in the UI
    /// </summary>
    /// <param name="state"></param>
    public void SetVisible(bool state)
    {
      active = state;
      rect.gameObject.SetActive(state);
    }
  }
}

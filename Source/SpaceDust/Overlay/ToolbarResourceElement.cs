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
  public class ToolbarResourceElement: MonoBehaviour
  {
    public bool active = false;
    public RectTransform rect;
    public Toggle visibleToggle;
    public Image resourceColor;
    public Text resourceName;

    public List<BandResourceElement> bandWidgets;
    string resName = "";

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
     // Utils.Log($"Finding {resourceColor} {resourceName} {visibleToggle}");
    }

    public void Start()
    {
    }

    public void ToggleResource()
    {
      MapOverlay.Instance.SetResourceVisible(resName, visibleToggle.isOn);
      ToolbarUI.Instance.SetResourceVisible(resName, visibleToggle.isOn);

      foreach (BandResourceElement wdget in bandWidgets)
      {
        if (SpaceDustScenario.Instance.IsIdentified(resName, wdget.associatedBand.name, wdget.associatedBody))
          wdget.SetVisible(visibleToggle.isOn);
        else
          wdget.SetVisible(false);
      }
    }

    public void SetResource(CelestialBody body, string ResourceName, List<ResourceBand> bands, bool shown)
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
        resourceName.text = Localizer.Format("#LOC_SpaceDust_UI_UnknownResource");
        SetVisible(true);
      }
      else
      {
        SetVisible(false);
      }

      foreach( ResourceBand b in bands)
      {
        GameObject newElement = (GameObject)Instantiate(Assets.BandResourceWidgetPrefab, Vector3.zero, Quaternion.identity);

        newElement.transform.SetParent(rect);
        //newUIPanel.transform.localPosition = Vector3.zero;
        BandResourceElement res = newElement.AddComponent<BandResourceElement>();
        res.SetBand(body, b);
        bandWidgets.Add(res);

        if (anyID && shown)
          res.SetVisible(true);
        else
          res.SetVisible(false);
      }

      visibleToggle.isOn = shown;
      MapOverlay.Instance.SetResourceVisible(resName, visibleToggle.isOn);
      ToolbarUI.Instance.SetResourceVisible(resName, visibleToggle.isOn);
    }
    public void SetVisible(bool state)
    {
      active = state;
      rect.gameObject.SetActive(state);
    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceDust
{
  public class ToolbarResourceElement: MonoBehaviour
  {
    public bool active = false;
    public RectTransform rect;
    public Toggle visibleToggle;
    public Image resourceColor;
    public Text resourceName;

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

      Utils.Log($"Finding {resourceColor} {resourceName} {visibleToggle}");
    }

    public void Start()
    {
    }

    public void ToggleResource()
    {
      MapOverlay.Instance.SetResourceVisible(resName, visibleToggle.isOn);
    }

    public void SetResource(string ResourceName, bool discovered, bool identified)
    {
      if (resourceColor == null) FindElements();

      resName = ResourceName; 
      resourceColor.enabled = identified;
      

      if (identified)
      {
        resourceName.text = ResourceName;
        resourceColor.color = Settings.GetResourceColor(ResourceName);
      }
      else if (discovered)
      {
        resourceColor.color = Settings.resourceDiscoveredColor;
        resourceName.text = "?";
      }
      else
      {
        
      }
    }
    public void SetVisible(bool state)
    {
      active = state;
      rect.gameObject.SetActive(state);
    }
  }
}

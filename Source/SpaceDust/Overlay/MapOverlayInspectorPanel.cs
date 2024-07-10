using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KSP.Localization;

namespace SpaceDust
{
  /// <summary>
  /// UI element for the mouse-following tooltip inspector
  /// </summary>
  public class MapOverlayInspectorPanel : MonoBehaviour
  {
    public bool Enabled { get; private set; }

    protected GameObject inspectorObj;
    protected Image icon;
    protected Text resourceName;
    protected Text concentration;
    protected RectTransform rect;

    protected ResourceBand currentBand;

    private const string BAND_CONCENTRATION_KEY = "#LOC_SpaceDust_UI_BandData";
    private const string BAND_UNKNOWN_KEY = "#LOC_SpaceDust_UI_UnknownResource";

    /// <summary>
    /// Map band type to the correct sprite 
    /// </summary>
    private Dictionary<HarvestType, string> SpriteMap = new Dictionary<HarvestType, string>()
      {
        {HarvestType.Atmosphere, "icon-atmo"},
        {HarvestType.Exosphere, "icon-orbit"},
      };

    protected void Awake()
    {
      inspectorObj = this.gameObject;
      rect = this.GetComponent<RectTransform>();
      rect.anchorMin = rect.anchorMax = Vector2.one * 0.5f;
      rect.pivot = Vector2.zero;
      icon = Utils.FindChildOfType<Image>("Icon", transform);
      resourceName = Utils.FindChildOfType<Text>("Name", transform);
      concentration = Utils.FindChildOfType<Text>("Concentration", transform);
      Enabled = true;
      SetVisible(false);
    }

    /// <summary>
    /// Sets the panel as visible
    /// </summary>
    /// <param name="state"></param>
    public void SetVisible(bool state)
    {
      if (Enabled != state)
      {
        Enabled = state;
        inspectorObj.SetActive(state);
      }
    }

    /// <summary>
    /// Sets the panel to show data for a band that has been fully Identified
    /// </summary>
    /// <param name="band"></param>
    public void SetInspectIdentified(ResourceBand band)
    {
      if (band != currentBand)
      {
        resourceName.text = band.ResourceName;
        if (!concentration.enabled)
        {
          concentration.enabled = true;
        }
        double smple = band.Abundance / PartResourceLibrary.Instance.GetDefinition(band.ResourceName).density;
        concentration.text = Localizer.Format(BAND_CONCENTRATION_KEY, smple.ToString("G3"));
        icon.sprite = SpaceDustAssets.Sprites[SpriteMap[band.BandType]];
        currentBand = band;
      }
    }

    /// <summary>
    /// Sets the panel to show data for a band that has not been identified
    /// </summary>
    /// <param name="band"></param>
    public void SetInspectUnidentified(ResourceBand band)
    {
      if (band != currentBand)
      {
        resourceName.text = Localizer.Format(BAND_UNKNOWN_KEY);
        if (concentration.enabled)
        {
          concentration.enabled = false;
          icon.sprite = SpaceDustAssets.Sprites[SpriteMap[band.BandType]];
        }
        currentBand = band;
      }
    }

    /// <summary>
    /// Sets the canvas position of the inspector
    /// </summary>
    /// <param name="position"></param>
    public void SetPosition(Vector2 position)
    {
      rect.anchoredPosition = position;
    }
  }
}

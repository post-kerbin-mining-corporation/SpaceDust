using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using KSP.Localization;

namespace SpaceDust.Overlay
{
  /// <summary>
  /// This widget shows the concentration data for a single Band under a resource in the UI
  /// </summary>
  public class BandResourceElement : MonoBehaviour
  {

    public bool active = false;
    public RectTransform rect;
    public Text bandName;
    public Text concentration;
    public Image icon;

    public ResourceBand associatedBand;
    public CelestialBody associatedBody;

    private const string BAND_CONCENTRATION_KEY = "#LOC_SpaceDust_UI_BandData";
    private const string BAND_UNKNOWN_KEY = "#LOC_SpaceDust_UI_UnknownBand";

    private Dictionary<HarvestType, string> SpriteMap = new Dictionary<HarvestType, string>()
      {
        {HarvestType.Atmosphere, "icon-atmo"},
        {HarvestType.Exosphere, "icon-orbit"},
      };


    void Awake()
    {
      FindElements();
    }

    protected void FindElements()
    {
      rect = this.transform as RectTransform;
      bandName = Utils.FindChildOfType<Text>("Title", transform);
      concentration = Utils.FindChildOfType<Text>("Conc", transform);
      icon = Utils.FindChildOfType<Image>("Icon", transform);
    }

    /// <summary>
    /// Assigns the band that this widget will show
    /// </summary>
    /// <param name="body"></param>
    /// <param name="bnd"></param>
    public void AssignBand(CelestialBody body, ResourceBand bnd)
    {

      if (bandName == null) FindElements();

      associatedBand = bnd;
      associatedBody = body;
      SetBandData(associatedBody, associatedBand);
    }

    /// <summary>
    /// Set up visible UI data based on the band
    /// </summary>
    /// <param name="body"></param>
    /// <param name="bnd"></param>
    protected void SetBandData(CelestialBody body, ResourceBand bnd)
    {
      if (SpaceDustScenario.Instance.IsIdentified(bnd.ResourceName, bnd.name, body))
      {
        icon.enabled = true;
        bandName.enabled = true;
        concentration.enabled = true;

        bandName.text = bnd.title;
        double smple = bnd.Abundance / PartResourceLibrary.Instance.GetDefinition(bnd.ResourceName).density;
        concentration.text = Localizer.Format(BAND_CONCENTRATION_KEY, smple.ToString("G3"));
        icon.sprite = SpaceDustAssets.Sprites[SpriteMap[bnd.BandType]];
      }
      else if (SpaceDustScenario.Instance.IsDiscovered(bnd.ResourceName, bnd.name, body))
      {
        icon.enabled = true;
        bandName.enabled = true;
        concentration.enabled = true;
        bandName.text = bnd.title;
        concentration.text = Localizer.Format(BAND_UNKNOWN_KEY);
        icon.sprite = SpaceDustAssets.Sprites[SpriteMap[bnd.BandType]];
      }
      else
      {
        icon.enabled = false;
        bandName.enabled = false;
        concentration.enabled = false;
      }
    }

    /// <summary>
    /// Sets whether this band widget should be visible
    /// </summary>
    /// <param name="state"></param>
    public void SetVisible(bool state)
    {
      active = state;
      rect.gameObject.SetActive(state);
      if (state)
      {
        SetBandData(associatedBody, associatedBand);
      }
    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using KSP.Localization;

namespace SpaceDust.Overlay
{
  public class BandResourceElement : MonoBehaviour
  {

    public bool active = false;
    public RectTransform rect;
    public Text bandName;
    public Text concentration;

    public ResourceBand associatedBand;
    public CelestialBody associatedBody;

    void Awake()
    {
      FindElements();
    }

    void FindElements()
    {
      rect = this.transform as RectTransform;

      bandName = transform.FindDeepChild("Title").GetComponent<Text>();
      concentration = transform.FindDeepChild("Conc").GetComponent<Text>();

    }

    public void Start()
    {
    }
    public void SetBand(CelestialBody body, ResourceBand bnd)
    {

      if (bandName == null) FindElements();

      associatedBand = bnd;
      associatedBody = body;
      SetState(associatedBody,associatedBand);
    }

    void SetState(CelestialBody body, ResourceBand bnd)
    {

      if (SpaceDustScenario.Instance.IsIdentified(bnd.ResourceName, bnd.name, body))
      {
        bandName.enabled = true;
        concentration.enabled = true;
        bandName.text = bnd.title;
        concentration.text = Localizer.Format("#LOC_SpaceDust_UI_BandData", bnd.Abundance.ToString("G3"));
      }
      else if (SpaceDustScenario.Instance.IsDiscovered(bnd.ResourceName, bnd.name, body))
      {

        bandName.enabled = true;
        concentration.enabled = true;
        bandName.text = bnd.title;
        concentration.text = Localizer.Format("#LOC_SpaceDust_UI_UnknownBand");
      }
      else
      {
        bandName.enabled = false;
        concentration.enabled = false;
      }
    }
    public void SetVisible(bool state)
    {
      active = state;
      rect.gameObject.SetActive(state);
      if (state)
      {
        SetState(associatedBody, associatedBand);
      }  
    }
  }
}

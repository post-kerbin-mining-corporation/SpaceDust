using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SpaceDust
{
  [KSPAddon(KSPAddon.Startup.AllGameScenes,false)]
  public class MapOverlay: MonoBehaviour
  {
    public static MapOverlay Instance { get; private set; }

    List<ParticleField> drawnFields;

    protected void Awake()
    {
      Instance = this;
      drawnFields = new List<ParticleField>();
      GameEvents.OnMapEntered.Add(new EventVoid.OnEvent(OnMapEntered));
      GameEvents.OnMapExited.Add(new EventVoid.OnEvent(OnMapExited));
      GameEvents.OnMapFocusChange.Add(new EventData<MapObject>.OnEvent( OnMapFocusChange));
      GameEvents.onGameSceneLoadRequested.Add(new EventData<GameScenes>.OnEvent(onGameSceneLoadRequested));
    }
    protected void OnDestroy()
    {
      GameEvents.OnMapEntered.Remove(OnMapEntered);
      GameEvents.OnMapExited.Remove(OnMapExited);
      GameEvents.OnMapFocusChange.Remove(OnMapFocusChange);
      GameEvents.onGameSceneLoadRequested.Remove(onGameSceneLoadRequested);
    }
    public void onGameSceneLoadRequested(GameScenes scenes)
    {
      if (Settings.DebugOverlay)
        Utils.Log($"[MapOverlay] Load Requested");
      RemoveBodyFields();
    }
    public void OnMapEntered()
    {
      if (Settings.DebugOverlay)
        Utils.Log($"[MapOverlay] Entering map view, focus on {PlanetariumCamera.fetch.target.name}");
      CelestialBody body = PlanetariumCamera.fetch.target.celestialBody;
      if (body == null)
      {
        body = PlanetariumCamera.fetch.target.vessel.mainBody;
      }
      RegenerateBodyFields(body);
    }
    public void OnMapExited()
    {
      if (Settings.DebugOverlay)
        Utils.Log($"[MapOverlay] Exiting map view");

      RemoveBodyFields();
    }

    public void OnMapFocusChange(MapObject mapObject)
    {
      if (Settings.DebugOverlay)
        Utils.Log($"[MapOverlay] Changed focus to {mapObject.name}");
      if (mapObject != null)

      {
        CelestialBody body = mapObject.celestialBody;
        if (body == null)
        {
          body = mapObject.vessel.mainBody;
        }
        RegenerateBodyFields(body);
      }
    }
    public void SetVisible(bool state)
    {
      foreach (ParticleField p in drawnFields)
      {
          if (state)
          p.SetVisible(ToolbarUI.Instance.IsVisible(p.resName));
        else
          p.SetVisible(state);
      }
    }

    public void SetResourceVisible(string resourceName, bool state)
    {
      foreach (ParticleField p in drawnFields)
      {
        if (p.resName == resourceName)
        {
          p.SetVisible(state);
        }
      }
    }
    void RegenerateBodyFields(CelestialBody targetObject)
    {
      RemoveBodyFields();
      List<string> bodyResources = SpaceDustResourceMap.Instance.GetBodyResources(targetObject);
      foreach (string resName in bodyResources)
      {
        if (Settings.visibleResources.Contains(resName))
          GenerateBodyField(resName, targetObject);
      }
    }

    void RemoveBodyFields()
    {
      if (Settings.DebugOverlay)
        Utils.Log($"[MapOverlay]: Removing body fields");
      for (int i = drawnFields.Count - 1; i >= 0; i--)
      {
        Destroy(drawnFields[i].gameObject);
      }
      drawnFields.Clear();
    }

    void GenerateBodyField(string resourceName, CelestialBody body)
    {



      Utils.Log($"[MapOverlay]: Generating fields for {resourceName} around {body.name}");
      foreach (ResourceBand b in SpaceDustResourceMap.Instance.GetBodyDistributions(body, resourceName))
      {
        bool discovered = SpaceDustScenario.Instance.IsDiscovered(resourceName, b.name, body);
        bool ided = SpaceDustScenario.Instance.IsIdentified(resourceName,b.name, body);
        GameObject fieldObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ParticleField field = fieldObj.AddComponent<ParticleField>();

        if (HighLogic.LoadedSceneIsFlight)
        {
          Utils.Log($"[MapOverlay]: Generating fields for {b.name} FLIGHT");
          field.transform.SetParent(body.MapObject.trf);
          field.transform.localScale = Vector3.one;
          field.transform.localPosition = Vector3.zero;
        }
        if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
        {
          Utils.Log($"[MapOverlay]: Generating fields for {b.name} TRACKER BRO");
          field.transform.SetParent(body.MapObject.trf);
          field.transform.localScale = Vector3.one;
          field.transform.localPosition = Vector3.zero;
        }

        if (discovered || ided)
          field.CreateField(b, resourceName, discovered, ided, body.MapObject.transform.position);

        field.SetVisible(ToolbarUI.Instance.IsVisible(resourceName));
        drawnFields.Add(field);

        Utils.Log($"[MapOverlay]: Generated field for {body.name}.{b.name}");

      }
      
    }

  }
}

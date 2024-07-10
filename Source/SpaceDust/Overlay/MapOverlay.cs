using System.Collections.Generic;
using UnityEngine;
using KSP.UI;

namespace SpaceDust
{
  /// <summary>
  /// Draws the particle-y map overlay
  /// </summary>
  [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
  public class MapOverlay : MonoBehaviour
  {
    public static MapOverlay Instance { get; private set; }

    private MapOverlayInspectorPanel inspector;
    private CelestialBody focusedBody;
    private List<ParticleField> drawnFields;
    private Camera mapCamera;
    private RaycastHit hit;
    private Ray ray;
    private LayerMask raycastMask;

    protected void Awake()
    {
      Instance = this;
      drawnFields = new List<ParticleField>();
      raycastMask = LayerMask.GetMask("Scaled Scenery");
      GameEvents.OnMapEntered.Add(new EventVoid.OnEvent(OnMapEntered));
      GameEvents.OnMapExited.Add(new EventVoid.OnEvent(OnMapExited));
      GameEvents.OnMapFocusChange.Add(new EventData<MapObject>.OnEvent(OnMapFocusChange));
      GameEvents.onGameSceneLoadRequested.Add(new EventData<GameScenes>.OnEvent(onGameSceneLoadRequested));
    }

    /// <summary>
    /// Create the mouse-following inspector panel
    /// </summary>
    protected void CreateInspectorPanel()
    {
      GameObject newUIPanel = (GameObject)Instantiate(SpaceDustAssets.OverlayInspectorPrefab, Vector3.zero, Quaternion.identity);
      newUIPanel.transform.SetParent(UIMasterController.Instance.mainCanvas.transform, false);

      inspector = newUIPanel.AddComponent<MapOverlayInspectorPanel>();
      inspector.SetVisible(false);

      if (HighLogic.LoadedSceneIsFlight)
      {
        raycastMask = LayerMask.GetMask("MapFX");
      }
      else
      {
        raycastMask = LayerMask.GetMask("Scaled Scenery");
      }
    }

    /// <summary>
    /// Destroy the inspector panel
    /// </summary>
    protected void DestroyInspectorPanel()
    {
      Destroy(inspector.gameObject);
      inspector = null;
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
      focusedBody = PlanetariumCamera.fetch.target.celestialBody;
      mapCamera = PlanetariumCamera.fetch.GetCameraTransform().GetComponent<Camera>();
      if (focusedBody == null)
      {
        focusedBody = PlanetariumCamera.fetch.target.vessel.mainBody;
      }
      RegenerateBodyFields(focusedBody);
      CreateInspectorPanel();
    }
    public void OnMapExited()
    {
      if (Settings.DebugOverlay)
        Utils.Log($"[MapOverlay] Exiting map view");

      RemoveBodyFields();
      DestroyInspectorPanel();
    }

    public void OnMapFocusChange(MapObject mapObject)
    {
      if (Settings.DebugOverlay)
        Utils.Log($"[MapOverlay] Changed focus to {mapObject.name}");
      if (mapObject != null)

      {
        focusedBody = mapObject.celestialBody;
        if (focusedBody == null)
        {
          focusedBody = mapObject.orbit.referenceBody;
        }
        RegenerateBodyFields(focusedBody);
      }
    }
    /// <summary>
    /// Sets the overlay as visible or invisible
    /// </summary>
    /// <param name="state"></param>
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

    /// <summary>
    /// Sets a resource as visible or not
    /// </summary>
    /// <param name="resourceName"></param>
    /// <param name="state"></param>
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

    /// <summary>
    /// Deletes and recreates the fields for a body
    /// </summary>
    /// <param name="targetObject"></param>
    private void RegenerateBodyFields(CelestialBody targetObject)
    {
      RemoveBodyFields();
      List<string> bodyResources = SpaceDustResourceMap.Instance.GetBodyResources(targetObject);
      foreach (string resName in bodyResources)
      {
        if (Settings.visibleResources.Contains(resName))
          GenerateBodyField(resName, targetObject);
      }
    }

    /// <summary>
    /// Deletes the fields for a body
    /// </summary>
    private void RemoveBodyFields()
    {
      if (Settings.DebugOverlay)
        Utils.Log($"[MapOverlay]: Removing body fields");
      for (int i = drawnFields.Count - 1; i >= 0; i--)
      {
        Destroy(drawnFields[i].gameObject);
      }
      drawnFields.Clear();
    }

    /// <summary>
    /// Creates the overlay fields for a body
    /// </summary>
    /// <param name="resourceName"></param>
    /// <param name="body"></param>
    void GenerateBodyField(string resourceName, CelestialBody body)
    {
      Utils.Log($"[MapOverlay]: Generating fields for {resourceName} around {body.name}");
      foreach (ResourceBand b in SpaceDustResourceMap.Instance.GetBodyDistributions(body, resourceName))
      {
        bool discovered = SpaceDustScenario.Instance.IsDiscovered(resourceName, b.name, body);
        bool ided = SpaceDustScenario.Instance.IsIdentified(resourceName, b.name, body);
        GameObject fieldObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fieldObj.name = $"ParticleField_{body.name}_{b.name}_{resourceName}";
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

    ResourceBand sampledBand;

    Transform hitTransform;
    FieldCollision hitCollisionBand;

    public void Update()
    {
      if (HighLogic.LoadedScene == GameScenes.TRACKSTATION || (HighLogic.LoadedScene == GameScenes.FLIGHT && MapView.MapIsEnabled))
      {
        if (inspector != null)
        {
          ray = mapCamera.ScreenPointToRay(Input.mousePosition);
          if (Physics.Raycast(ray, out hit, 10000f, raycastMask))
          {
            /// If this is a different object than last hit we should reget the data
            //Debug.Log($"{hitTransform}");
            if (hitTransform != hit.transform)
            {
              
              hitTransform = hit.transform;
              Transform parent = hitTransform.parent;
              if (parent != null)
              {

                hitCollisionBand = parent.GetComponent<FieldCollision>();
                if (hitCollisionBand != null)
                {
                  sampledBand = hitCollisionBand.AssociatedBand;
                  if (SpaceDustScenario.Instance.IsIdentified(sampledBand.ResourceName, sampledBand.name, focusedBody))
                  {
                    inspector.SetInspectIdentified(sampledBand);
                  }
                  else
                  {
                    inspector.SetInspectUnidentified(sampledBand);
                  }
                }
              }
            }

            if (hitCollisionBand != null)
            {
              Vector2 position;
              RectTransformUtility.ScreenPointToLocalPointInRectangle(UIMasterController.Instance.mainCanvas.GetComponent<RectTransform>(), Input.mousePosition, UIMasterController.Instance.uiCamera, out position);
              inspector.SetPosition(position);
              inspector.SetVisible(true);
            }
            else
            {
              inspector.SetVisible(false);
            }
          }
          else
          {
            hitTransform = null;
            hitCollisionBand = null;
            inspector.SetVisible(false);

          }

        }
      }
    }
  }
}

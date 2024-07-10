using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SpaceDust
{
  public class FieldCollision : MonoBehaviour
  {

    public ResourceBand AssociatedBand { get; private set; }
    private DistributionMesh Mesh;
    private MeshRenderer rend;
    private MeshFilter filt;

    Collider fieldCollider;
    public void CreateCollision(ResourceBand band, Transform parent)
    {
      GameObject go = new GameObject($"FieldCollider_{band.name}_{band.ResourceName}");
      go.transform.SetParent(parent, false);
      go.transform.localEulerAngles = new Vector3(90f, 0, 0);
      go.transform.localScale = Vector3.one * ScaledSpace.InverseScaleFactor * 10f;
      //rend = go.AddComponent<MeshRenderer>();
      //filt = go.AddComponent<MeshFilter>();
      MeshCollider mCol = go.AddComponent<MeshCollider>();

      if ((SphericalDistributionModel)band.Distribution != null)
      {
        SphericalDistributionMesh sMesh = new SphericalDistributionMesh((SphericalDistributionModel)band.Distribution);
        sMesh.GenerateMesh(7, 48);

        Mesh = sMesh;
      }

      //filt.mesh = Mesh.Mesh;
      mCol.sharedMesh = Mesh.Mesh;
      //SphereCollider Scol = go.AddComponent<SphereCollider>();
      if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
      {
        go.layer = 10;
      }
      else
      {
        go.layer = 24;
      }

      //Scol.radius = ((float)band.Distribution.MaxSize() / ScaledSpace.ScaleFactor) * 10f;
      AssociatedBand = band;
      fieldCollider = mCol as Collider;
    }

    public void SetEnabled(bool on)
    {
      if (fieldCollider != null)
      {
        fieldCollider.enabled = on;
      }
    }
  }
}

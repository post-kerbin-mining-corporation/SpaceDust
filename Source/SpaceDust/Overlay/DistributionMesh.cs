using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace SpaceDust
{
  public class DistributionMesh
  {
    public Mesh Mesh { get; protected set; }
    public int LatitudeRingCount { get; protected set; }
    public int SideCount { get; protected set; }
    public DistributionMesh() { }
    public virtual void GenerateMesh(int rings, int sides) 
    {
      SideCount = sides;
      LatitudeRingCount = rings;
      Mesh = new Mesh { name = $"fancy mesh" };
    }
  }

  public class SphericalDistributionMesh : DistributionMesh
  {
    private SphericalDistributionModel model;

    public SphericalDistributionMesh(SphericalDistributionModel toModel)
    {
      model = toModel;
    }

    public override void GenerateMesh(int rings, int sides)
    {
      base.GenerateMesh(rings, sides);
      

      Vector3[] verts = GenerateVerts(model.minimumLatitude >= 90f && model.maximumAltitude <= 90f);
      int[] tris = GenerateTriangles(model.minimumLatitude >= 90f && model.maximumAltitude <= 90f);

      Mesh.vertices = verts;
      Mesh.triangles = tris;
    }

    private Vector3[] GenerateVerts(bool onlyOuter)
    {
      float totalHeightDegrees = (float)(model.maximumLatitude - model.minimumLatitude);
      List<Vector3> verts = new List<Vector3>();

      for (int i = 0; i < LatitudeRingCount; i++)
      {
        float fraction = i / (float)(LatitudeRingCount - 1);
        float ringLat = (float)model.minimumLatitude + totalHeightDegrees * fraction;
        for (int k = 0; k < SideCount; k++)
        {
          fraction = k / (float)(SideCount);
          verts.Add(Utils.Pol2Cart(
            new Vector3(
              (float)(model.BodyRadius + model.maximumAltitude), 
              Mathf.Deg2Rad * ringLat, 
              Mathf.Deg2Rad * (360f * fraction))));
        }
      }
      if (!onlyOuter)
      {
        for (int i = 0; i < LatitudeRingCount; i++)
        {
          float fraction = i / (float)(LatitudeRingCount - 1);
          float ringLat = (float)model.minimumLatitude + totalHeightDegrees * fraction;
          Vector3[] ring = new Vector3[SideCount];
          for (int k = 0; k < SideCount; k++)
          {
            fraction = k / (float)(SideCount);
            verts.Add(Utils.Pol2Cart(new Vector3((float)(model.BodyRadius + model.minimumAltitude), Mathf.Deg2Rad * ringLat, Mathf.Deg2Rad * (360f * fraction))));
          }
        }
      }
      return verts.ToArray();
    }
    private int[] GenerateTriangles(bool onlyOuter)
    {

      List<int> triangles = new List<int>();
      int i1, i2, i3, i4, i5, i6;
      int f1, f2, f3, f4, f5, f6;
      // Generate outer shell
      for (int i = 0; i < LatitudeRingCount - 1; i++)
      {
        for (int k = 0; k < SideCount - 1; k++)
        {
          i1 = k + i * (SideCount);
          i2 = k + i * (SideCount) + 1;
          i3 = k + (i + 1) * (SideCount);
          triangles.Add(i1);
          triangles.Add(i2);
          triangles.Add(i3);

          i4 = k + (i + 1) * (SideCount);
          i5 = k + i * (SideCount) + 1;
          i6 = k + (i + 1) * (SideCount) + 1;
          triangles.Add(i4);
          triangles.Add(i5);
          triangles.Add(i6);
        }
        f1 = (SideCount - 1) + i * (SideCount);
        f2 = 0 + i * (SideCount);
        f3 = (SideCount - 1) + (i + 1) * (SideCount);
        triangles.Add(f1);
        triangles.Add(f2);
        triangles.Add(f3);
        f4 = (SideCount - 1) + (i + 1) * (SideCount);
        f5 = 0 + i * (SideCount);
        f6 = 0 + (i + 1) * (SideCount);
        triangles.Add(f4);
        triangles.Add(f5);
        triangles.Add(f6);
      }
      if (!onlyOuter)
      {
        int vertOffset = SideCount * LatitudeRingCount;
        // Generate inner shell
        for (int i = 0; i < LatitudeRingCount - 1; i++)
        {
          for (int k = 0; k < SideCount - 1; k++)
          {
            i1 = vertOffset + k + i * (SideCount);
            i2 = vertOffset + k + i * (SideCount) + 1;
            i3 = vertOffset + k + (i + 1) * (SideCount);
            triangles.Add(i1);
            triangles.Add(i3);
            triangles.Add(i2);

            i4 = vertOffset + k + (i + 1) * (SideCount);
            i5 = vertOffset + k + i * (SideCount) + 1;
            i6 = vertOffset + k + (i + 1) * (SideCount) + 1;
            triangles.Add(i4);
            triangles.Add(i6);
            triangles.Add(i5);
          }
          // Last tri
          f1 = vertOffset + (SideCount - 1) + i * (SideCount);
          f2 = vertOffset + 0 + i * (SideCount);
          f3 = vertOffset + (SideCount - 1) + (i + 1) * (SideCount);
          triangles.Add(f1);
          triangles.Add(f3);
          triangles.Add(f2);
          f4 = vertOffset + (SideCount - 1) + (i + 1) * (SideCount);
          f5 = vertOffset + 0 + i * (SideCount);
          f6 = vertOffset + 0 + (i + 1) * (SideCount);
          triangles.Add(f4);
          triangles.Add(f6);
          triangles.Add(f5);

        }

        // generate upper deck
        int ring1Idx = (int)Mathf.Floor(LatitudeRingCount / 2f) + 1;
        int ring2Idx = LatitudeRingCount * 2 - 1;
        for (int k = 0; k < SideCount - 1; k++)
        {
          i1 = k + ring1Idx * (SideCount);
          i2 = k + ring1Idx * (SideCount) + 1;
          i3 = k + ring2Idx * (SideCount);
          triangles.Add(i1);
          triangles.Add(i2);
          triangles.Add(i3);

          i4 = k + ring2Idx * (SideCount);
          i5 = k + ring1Idx * (SideCount) + 1;
          i6 = k + ring2Idx * (SideCount) + 1;
          triangles.Add(i4);
          triangles.Add(i5);
          triangles.Add(i6);
        }
        f1 = (SideCount - 1) + ring1Idx * (SideCount);
        f2 = 0 + ring1Idx * (SideCount);
        f3 = (SideCount - 1) + ring2Idx * (SideCount);
        triangles.Add(f1);
        triangles.Add(f2);
        triangles.Add(f3);
        f4 = (SideCount - 1) + ring2Idx * (SideCount);
        f5 = 0 + ring1Idx * (SideCount);
        f6 = 0 + ring2Idx * (SideCount);
        triangles.Add(f4);
        triangles.Add(f5);
        triangles.Add(f6);

        // generate lower deck
        ring2Idx = ring1Idx + 1;
        ring1Idx = 0;
        for (int k = 0; k < SideCount - 1; k++)
        {
          i1 = k + ring1Idx * (SideCount);
          i2 = k + ring1Idx * (SideCount) + 1;
          i3 = k + ring2Idx * (SideCount);
          triangles.Add(i1);
          triangles.Add(i3);
          triangles.Add(i2);

          i4 = k + ring2Idx * (SideCount);
          i5 = k + ring1Idx * (SideCount) + 1;
          i6 = k + ring2Idx * (SideCount) + 1;
          triangles.Add(i4);
          triangles.Add(i6);
          triangles.Add(i5);
        }
        f1 = (SideCount - 1) + ring1Idx * (SideCount);
        f2 = 0 + ring1Idx * (SideCount);
        f3 = (SideCount - 1) + ring2Idx * (SideCount);
        triangles.Add(f1);
        triangles.Add(f3);
        triangles.Add(f2);
        f4 = (SideCount - 1) + ring2Idx * (SideCount);
        f5 = 0 + ring1Idx * (SideCount);
        f6 = 0 + ring2Idx * (SideCount);
        triangles.Add(f4);
        triangles.Add(f6);
        triangles.Add(f5);

      }
      return triangles.ToArray();
    }
  }
}

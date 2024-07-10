using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace SpaceDust
{
  public static class Utils
  {

    public static void Log(string str)
    {
      Debug.Log("[SpaceDust]" + str);
    }
    public static void LogError(string str)
    {
      Debug.LogError("[SpaceDust]" + str);
    }
    public static void LogWarning(string str)
    {
      Debug.LogWarning("[SpaceDust]" + str);
    }

    /// <summary>
    /// Get a reference in a child of a type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public static T FindChildOfType<T>(string name, Transform parent)
    {
      T result = default(T);
      try
      {
        result = parent.FindDeepChild(name).GetComponent<T>();
      }
      catch (NullReferenceException e)
      {
        Debug.LogError($"Couldn't find {name} in children of {parent.name}");
      }
      return result;
    }
    // This function loads up some animationstates
    public static AnimationState[] SetUpAnimation(string animationName, Part part)
    {
      var states = new List<AnimationState>();
      foreach (var animation in part.FindModelAnimators(animationName))
      {
        var animationState = animation[animationName];
        animationState.speed = 0;
        animationState.enabled = true;
        // Clamp this or else weird things happen
        animationState.wrapMode = WrapMode.ClampForever;
        animation.Blend(animationName);
        states.Add(animationState);
      }
      // Convert
      return states.ToArray();
    }
    public static ConfigNode GetModuleConfigNode(Part part, string moduleName)
    {
      try
      {
        if (part == null || part.partInfo == null || part.partInfo.partConfig == null)
          return null;
        return part.partInfo.partConfig.GetNodes("MODULE").First(n => n.GetValue("name") == moduleName);
      }
      catch (Exception)
      {
        Utils.LogError($"Couldn't get module config for {moduleName} on {part.partInfo.name}");
        return null;
      }
    }

    public static string ToSI(double d, string format = null)
    {
      if (d == 0.0)
        return d.ToString(format);

      char[] incPrefixes = new[] { 'k', 'M', 'G', 'T', 'P', 'E', 'Z', 'Y' };
      char[] decPrefixes = new[] { 'm', '\u03bc', 'n', 'p', 'f', 'a', 'z', 'y' };

      int degree = Mathf.Clamp((int)Math.Floor(Math.Log10(Math.Abs(d)) / 3), -8, 8);
      if (degree == 0)
        return d.ToString(format) + " ";

      double scaled = d * Math.Pow(1000, -degree);

      char? prefix = null;

      switch (Math.Sign(degree))
      {
        case 1: prefix = incPrefixes[degree - 1]; break;
        case -1: prefix = decPrefixes[-degree - 1]; break;
      }

      return scaled.ToString(format) + " " + prefix;
    }

    /// <summary>
    /// Transform cartesian to spherical coordinates
    /// </summary>
    /// <param name="cart"></param>
    /// <returns></returns>
    public static Vector3 Cart2Sphere(Vector3 cart)
    {
      float r = Mathf.Sqrt(cart.x * cart.x + cart.y * cart.y + cart.z * cart.z);
      float phi = Mathf.Atan2(cart.y, cart.x);
      float theta = Mathf.Acos(cart.z / r);

      return new Vector3(r, phi, theta);
    }
    public static Vector3 Pol2Cart(Vector3 pol)
    {
      float x = pol.x * Mathf.Cos(pol.y) * Mathf.Cos(pol.z);
      float y = pol.x * Mathf.Cos(pol.y) * Mathf.Sin(pol.z);
      float z = pol.x * Mathf.Sin(pol.y);
      return new Vector3(x, y, z);
    }

    public static bool CalculateBodyLOS(Vessel ves, CelestialBody target, Transform refXForm, out float angle, out CelestialBody obscuringBody)
    {
      bool targetVisible = true;
      angle = 0f;
      obscuringBody = null;

      CelestialBody currentBody = FlightGlobals.currentMainBody;

      angle = Vector3.Angle(refXForm.forward, target.transform.position - refXForm.position);

      if (currentBody != target)
      {

        Vector3 vT = target.position - ves.GetWorldPos3D();
        Vector3 vC = currentBody.position - ves.GetWorldPos3D();
        // if true, behind horizon plane
        if (Vector3.Dot(vT, vC) > (vC.sqrMagnitude - currentBody.Radius * currentBody.Radius))
        {
          // if true, obscured
          if ((Mathf.Pow(Vector3.Dot(vT, vC), 2) / vT.sqrMagnitude) > (vC.sqrMagnitude - currentBody.Radius * currentBody.Radius))
          {
            targetVisible = false;
            obscuringBody = currentBody;
          }
        }
      }

      return targetVisible;
    }

    public static double CalculateAirMass(Vessel ves, CelestialBody target)
    {

      double rUnit = ves.mainBody.Radius / ves.mainBody.atmosphereDepth;
      double yUnit = ves.altitude / ves.mainBody.atmosphereDepth;

      
      double zenithAngle = Vector3d.Angle(ves.GetWorldPos3D() - ves.mainBody.position,  target.position - ves.mainBody.position)*Mathf.Deg2Rad;
      
      double x = Math.Sqrt(Math.Pow(rUnit + yUnit, 2) * Math.Pow(Math.Cos(zenithAngle), 2) + 2d * rUnit * (1d - yUnit) - yUnit * yUnit + 1d) - (rUnit + yUnit) * Math.Cos(zenithAngle);

      return x;
    }
  }
  public static class MathExtensions
  {
    // Source: http://stackoverflow.com/a/2683487/1455541
    public static T Clamp<T>(T val, T min, T max) where T : IComparable<T>
    {
      if (val.CompareTo(min) < 0) return min;
      else if (val.CompareTo(max) > 0) return max;
      else return val;
    }
  }
  public static class TransformDeepChildExtension
  {
    //Breadth-first search
    public static Transform FindDeepChild(this Transform aParent, string aName)
    {
      Queue<Transform> queue = new Queue<Transform>();
      queue.Enqueue(aParent);
      while (queue.Count > 0)
      {
        var c = queue.Dequeue();
        if (c.name == aName)
          return c;
        foreach (Transform t in c)
          queue.Enqueue(t);
      }
      return null;
    }



    /*
    //Depth-first search
    public static Transform FindDeepChild(this Transform aParent, string aName)
    {
        foreach(Transform child in aParent)
        {
            if(child.name == aName )
                return child;
            var result = child.FindDeepChild(aName);
            if (result != null)
                return result;
        }
        return null;
    }
    */
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SpaceDust
{
   public enum FalloffType
   {
     None, Linear
   }

 
  [System.Serializable]
  // A base DistributionModel from which all models should derive
  public class DistributionModel
  {
    public double BodyRadius = 0d;
    public DistributionModel(ConfigNode node)
    { }

    /// <summary>
    /// 
    /// </summary>
    public virtual void Initialize()
    { }

    public virtual void Load(ConfigNode node)
    { }

    public virtual double MaxSize() { return 0d; }

    public virtual double Center() { return 0d; }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="altitude"></param>
    /// <param name="latitude"></param>
    /// <param name="longitude"></param>
    /// <returns></returns>
    public virtual double Sample(double altitude, double latitude, double longitude)
    {
      return 1d;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public virtual string ToString()
    {
      return $"(Basic Distribution Model)";
    }
  }

  // A Uniform distribution is the same everywhere
  public class UniformDistributionModel : DistributionModel
  {
    public UniformDistributionModel(ConfigNode node):base(node)
    { }

    public override double MaxSize() { return 0d; }

    public override double Center() { return 0d; }

    /// <summary>
    /// 
    /// </summary>
    public override void Initialize()
    { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="altitude"></param>
    /// <param name="latitude"></param>
    /// <param name="longitude"></param>
    /// <returns></returns>
    public override double Sample(double altitude, double latitude, double longitude)
    {
      return 1d;
    }

    public override string ToString()
    {
      return "Uniform Distribution Model( no parameters)";
    }
  }

  // A Spherical distribution is a spherical shell
  [System.Serializable]
  public class SphericalDistributionModel : DistributionModel
  {
    public int seed;
    public double minimumLatitude = -90d;
    public double maximumLatitude = 90d;
    public double centerLatitude = 0d;
    public float latitudeVariability;
    public FalloffType latitudeFalloff;

    public double minimumAltitude = 0d;
    public double maximumAltitude = 100000d;
    public double centerAltitude = 0d;
    public float altitudeVariability;
    public double altitudeSquish = 0d;
    public FalloffType altitudeFalloff;


    public SphericalDistributionModel(ConfigNode node):base (node)
    {
      Load(node);
    }

    public override void Load(ConfigNode node)
    {
      node.TryGetValue("latUpperBound", ref maximumLatitude);
      node.TryGetValue("latLowerBound", ref minimumLatitude);
      node.TryGetValue("latPeak", ref centerLatitude);
      node.TryGetValue("latVariability", ref latitudeVariability);

      node.TryGetValue("altLowerBound", ref minimumAltitude);
      node.TryGetValue("altUpperBound", ref maximumAltitude);
      node.TryGetValue("altPeak", ref centerAltitude);
      node.TryGetValue("altVariability", ref altitudeVariability);
      node.TryGetValue("altitudeSquish", ref altitudeSquish);
      node.TryGetEnum<FalloffType>("altFalloffType", ref altitudeFalloff, FalloffType.None);
      node.TryGetEnum<FalloffType>("latFalloffType", ref latitudeFalloff, FalloffType.None);


      minimumAltitude *= Settings.GameScale;
      centerAltitude *= Settings.GameScale;
      maximumAltitude *= Settings.GameScale;
      altitudeVariability *= Settings.GameScale;
    }

    public override double MaxSize() { return maximumAltitude + BodyRadius; }

    public override double Center() { return centerAltitude + BodyRadius; }

    /// <summary>
    /// 
    /// </summary>
    public override void Initialize()
    {
      UnityEngine.Random.InitState(HighLogic.fetch.currentGame.Seed);

      minimumLatitude += UnityEngine.Random.Range(-latitudeVariability, latitudeVariability);
      maximumLatitude += UnityEngine.Random.Range(-latitudeVariability, latitudeVariability);
      centerLatitude += UnityEngine.Random.Range(-latitudeVariability, latitudeVariability);

      minimumAltitude += UnityEngine.Random.Range(-altitudeVariability, altitudeVariability);
      maximumAltitude += UnityEngine.Random.Range(-altitudeVariability, altitudeVariability);
      centerAltitude += UnityEngine.Random.Range(-altitudeVariability, altitudeVariability);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="altitude"></param>
    /// <param name="latitude"></param>
    /// <param name="longitude"></param>
    /// <returns></returns>
    public override double Sample(double altitude, double latitude, double longitude)
    {

      //altitude = altitude * (altitudeSquish * Math.Cos(latitude * Mathf.Deg2Rad));
      double latFactor = CalculateLatFactor(latitude);
      double altFactor = CalculateAltFactor(altitude);

      //Utils.Log($"{altitude}, {latitude}, {longitude}, {latFactor}, {altFactor}");

      return altFactor * latFactor;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="altitude"></param>
    /// <returns></returns>
    double CalculateAltFactor(double altitude)
    {
      altitude -= BodyRadius;
      if (altitudeFalloff == FalloffType.Linear)
      {
        if (altitude > centerAltitude)
          return 1d - UtilMath.Clamp((altitude - centerAltitude) / (maximumAltitude - centerAltitude ), 0d, 1d);
        else
          return 1d - UtilMath.Clamp( (altitude - centerAltitude) / (minimumAltitude - centerAltitude), 0d, 1d);
       
      }
      else if (altitudeFalloff == FalloffType.None)
      {
        if (altitude >= minimumAltitude  && altitude <= maximumAltitude )
          return 1d;
        else
          return 0d;
      }
      return 0d;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="latitude"></param>
    /// <returns></returns>
    double CalculateLatFactor(double latitude)
    {
      double latFactor = 0f; ;
      if (latitudeFalloff == FalloffType.Linear)
      {

        if (latitude > centerLatitude)
          latFactor = 1d - (latitude - centerLatitude) / (maximumLatitude - centerLatitude);
        else
          latFactor = 1d - (latitude - centerLatitude) / (minimumLatitude - centerLatitude);
      }
      else if (latitudeFalloff == FalloffType.None)
      {
        if (latitude >= minimumLatitude && latitude <= maximumLatitude)
          latFactor = 1d;
        else
          latFactor = 0d;
      }
      return latFactor;
    }

    public override string ToString()
    {
      return String.Format("Spherical Distribution(Altitude Range: {0:F1}-{1:F1} km, center {2:F1} km Latitude Range: {3:F1}-{4:F1} km, center {5:F1} km)",
        minimumAltitude, maximumAltitude, centerAltitude, minimumLatitude, maximumLatitude, centerLatitude);
    }
  }

}

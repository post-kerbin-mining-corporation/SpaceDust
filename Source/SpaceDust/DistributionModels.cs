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

  // A base DistributionModel from which all models should derive
  public class DistributionModel
  {
    public DistributionModel()
    {}

    /// Called by the Distribution object on game load. Should set Seed parameters
    public virtual void Initialize()
    {}

    /// Called by parts/UIs to sample concentrations
    /// Should return a scalar value representing the fraction of the maximum abundance possible
    public virtual double Sample(double altitude, double latitude, double longitude)
    {}

    public virtual string ToString()
    {}
  }

  // A Uniform distribution is the same everywhere
  public class UniformDistributionModel
  {
    public UniformDistributionModel()
    {}

    public override void Initialize()
    {}

    /// Should return a scalar value representing the fraction of the maximum abundance possible
    public override double Sample(double altitude, double latitude, double longitude)
    {
      return 1d
    }

    public override string ToString()
    {
      return "Uniform Distribution";
    }
  }

  // A Spherical distribution is a spherical shell
  public class SphericalDistributionModel
  {
    double minimumLatitude = -90d;
    double maximumLatitude = 90d;
    double centerLatitude = 0d;
    FalloffType latitudeFalloff;

    double minimumAltitude = 0d
    double maximumAltitude = 70d;
    double centerAltitude = 0d;
    FalloffType altitudeFalloff;


    public SphericalDistributionModel(ConfigNode node)
    {
       node.TryGetValue("latUpperBound", ref minimumLatitude);
       node.TryGetValue("latLowerBound", ref maximumLatitude);
       node.TryGetValue("latPeak", ref centerLatitude);
       node.TryGetValue("latVariability", ref latitudeVariability);

       node.TryGetValue("altLowerBound", ref minimumAltitude);
       node.TryGetValue("altUpperBound", ref maximumAltitude);
       node.TryGetValue("altPeak", ref centerAltitude);
       node.TryGetValue("altVariability", ref altitudeVariability);

       string altFalloffName = "Linear";
       string latFalloffName = "None";

       node.TryGetValue("altFalloffType", ref altFalloffName);
       node.TryGetValue("latFalloffType", ref latFalloffName);

        altitudeFalloff = Enum.Parse(typeof(FalloffType), altFalloffName);
        latitudeFalloff = Enum.Parse(typeof(FalloffType), latFalloffName);
    }

    public override void Initialize()
    {
      Random generator = new Random(HighLogic.currentGame.Seed);
      minimumLatitude += generator.Next(-latitudeVariability, latitudeVariability);
      maximumLatitude += generator.Next(-latitudeVariability, latitudeVariability);
      centerLatitude += generator.Next(-latitudeVariability, latitudeVariability);

      minimumAltitude += generator.Next(-altitudeVariability, altitudeVariability);
      maximumAltitude += generator.Next(-altitudeVariability, altitudeVariability);
      centerAltitude += generator.Next(-altitudeVariability, altitudeVariability);
    }

    /// Should return a scalar value representing the fraction of the maximum abundance possible
    public override double Sample(double altitude, double latitude, double longitude)
    {
      double latFactor = CalculateLatFactor(latitude);
      double altFactor = CalculateAltFactor(altitude);

      return latFactor*altFactor;
    }

    double CalculateAltFactor(double altitude)
    {
      if (altitudeFalloff == FalloffType.Linear)
      {
        if (maximumAltitude - altitude > centerAltitude):
          return 1.0d - (altitude - centerAltitude)/(maximumAltitude - centerAltitude);
        else
          return(altitude - minimumAltitude)/(centerAltitude - miniumAltitude);
      }
      else if (altitudeFalloff == FalloffType.Linear)
      {
        if (altitude >= minimumAltitude && altitude <= maximumAltitude)
          return 1d;
        else
          return 0d;
      }
      return 0d;
    }

    double CalculateLatFactor(double latitude)
    {
      if (latitudeeFalloff == FalloffType.Linear)
      {
        if (maximumLatitude - latitude > centerLatitude):
          latFactor = 1.0d - (latitude - centerLatitude)/(maximumLatitude - centerLatitude);
        else
          latFactor = (latitude - minimumAltitude)/(centerLatitude - minimumLatitude);
      } else if (latitudeFalloff == FalloffType.None)
      {
        if (latitute >= minimumLatitude && latitude <= maximumLatitude)
          return 1d;
        else
          return 0d;
      }
    }

    public string ToString()
    {
      return String.Format("Spherical Distribution:\n -Altitude Range: {0:F1}-{1:F1} km, center {2:F1} km\n" +
        "-Latitude Range: {3:F1}-{4:F1} km, center {5:F1} km",
        minimumAltitude, maximumAltitude, centerAltitude, minimumLatitude, maximumLatitude, centerLatitude);
    }
  }
}

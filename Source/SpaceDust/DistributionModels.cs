using System;
namespace SpaceDust
{
  public enum FalloffType
  {
    None, Linear
  }

  /// <summary>
  /// Represents a distribution model that controls how the concentration of resources in a Band are positionally distributed
  /// </summary>
  [System.Serializable]
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
    /// Sample the distribution at a point
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
    /// ToSTring
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return $"(Basic Distribution Model)";
    }
  }

  /// <summary>
  /// This is a Uniform distribution that is 100% there if you're in it, no matter where
  /// </summary>
  public class UniformDistributionModel : DistributionModel
  {
    public UniformDistributionModel(ConfigNode node) : base(node)
    { }

    /// <summary>
    /// The size of the distribution
    /// </summary>
    /// <returns></returns>
    public override double MaxSize() { return 0d; }

    /// <summary>
    /// The center of the distribution
    /// </summary>
    /// <returns></returns>
    public override double Center() { return 0d; }

    public override void Initialize()
    { }

    /// <summary>
    /// Sample the distribution at a point
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

  /// <summary>
  /// A Spherical distribution is a shell around a position with minimum and maximum altitude and latitudes
  /// </summary>
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

    private const string LAT_UPPER_PARAMETER_NAME = "latUpperBound";
    private const string LAT_LOWER_PARAMETER_NAME= "latLowerBound";
    private const string LAT_PEAK_PARAMETER_NAME = "latPeak";
    private const string LAT_VARIABILITY_PARAMETER_NAME = "latVariability";
    private const string LAT_FALLOFF_PARAMETER_NAME = "latFalloffType";

    private const string ALT_UPPER_PARAMETER_NAME = "altLowerBound";
    private const string ALT_LOWER_PARAMETER_NAME = "altUpperBound";
    private const string ALT_PEAK_PARAMETER_NAME = "altPeak";
    private const string ALT_VARIABILITY_PARAMETER_NAME = "altVariability";
    private const string ALT_SQUISH_PARAMETER_NAME = "altitudeSquish";
    private const string ALT_FALLOFF_PARAMETER_NAME = "altFalloffType";

    public SphericalDistributionModel(ConfigNode node) : base(node)
    {
      Load(node);
    }

    public override void Load(ConfigNode node)
    {
      node.TryGetValue(LAT_UPPER_PARAMETER_NAME, ref maximumLatitude);
      node.TryGetValue(LAT_LOWER_PARAMETER_NAME, ref minimumLatitude);
      node.TryGetValue(LAT_PEAK_PARAMETER_NAME, ref centerLatitude);
      node.TryGetValue(LAT_VARIABILITY_PARAMETER_NAME, ref latitudeVariability);
      node.TryGetEnum<FalloffType>(LAT_FALLOFF_PARAMETER_NAME, ref latitudeFalloff, FalloffType.None);

      node.TryGetValue(ALT_UPPER_PARAMETER_NAME, ref minimumAltitude);
      node.TryGetValue(ALT_LOWER_PARAMETER_NAME, ref maximumAltitude);
      node.TryGetValue(ALT_PEAK_PARAMETER_NAME, ref centerAltitude);
      node.TryGetValue(ALT_VARIABILITY_PARAMETER_NAME, ref altitudeVariability);
      node.TryGetValue(ALT_SQUISH_PARAMETER_NAME, ref altitudeSquish);
      node.TryGetEnum<FalloffType>(ALT_FALLOFF_PARAMETER_NAME, ref altitudeFalloff, FalloffType.None);
      
      minimumAltitude *= Settings.GameScale;
      centerAltitude *= Settings.GameScale;
      maximumAltitude *= Settings.GameScale;
      altitudeVariability *= Settings.GameScale;
    }

    /// <summary>
    /// The size of the distribution
    /// </summary>
    /// <returns></returns>
    public override double MaxSize() { return maximumAltitude + BodyRadius; }
    /// <summary>
    /// The center of the distribution
    /// </summary>
    /// <returns></returns>
    public override double Center() { return centerAltitude + BodyRadius; }

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
    /// Sample the distribution at a point
    /// </summary>
    /// <param name="altitude"></param>
    /// <param name="latitude"></param>
    /// <param name="longitude"></param>
    /// <returns></returns>
    public override double Sample(double altitude, double latitude, double longitude)
    {
      double latFactor = CalculateLatFactor(latitude);
      double altFactor = CalculateAltFactor(altitude);

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
          return 1d - UtilMath.Clamp((altitude - centerAltitude) / (maximumAltitude - centerAltitude), 0d, 1d);
        else
          return 1d - UtilMath.Clamp((altitude - centerAltitude) / (minimumAltitude - centerAltitude), 0d, 1d);

      }
      else if (altitudeFalloff == FalloffType.None)
      {
        if (altitude >= minimumAltitude && altitude <= maximumAltitude)
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

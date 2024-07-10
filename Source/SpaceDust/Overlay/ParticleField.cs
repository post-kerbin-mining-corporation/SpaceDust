using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceDust
{
  /// <summary>
  /// This is a single Particle Field that models a Distribution of a single Resource for a CelestialBody
  /// </summary>
  public class ParticleField : MonoBehaviour
  {

    public string resName = "";
    public ResourceBand Band { get; private set; }

    public FieldCollision fieldCollision;

    private ParticleSystem particleField;
    private ParticleSystem.Particle[] particleBuffer;
    private ParticleSystemRenderer particleRenderer;
    private Color particleColor;
    private bool fieldGenerated = false;

    private Transform xform;
    private float spinRate = 2f;
    private int targetCount = 1;



    void Awake()
    {
      xform = transform;
      particleField = gameObject.AddComponent<ParticleSystem>();
      particleRenderer = gameObject.GetComponent<ParticleSystemRenderer>();
      fieldCollision = gameObject.AddComponent<FieldCollision>();

      var em = particleField.emission;
      em.enabled = false;
      var main = particleField.main;
      main.simulationSpace = ParticleSystemSimulationSpace.Local;
      main.playOnAwake = false;
      main.loop = false;
      main.maxParticles = Settings.particleFieldMaxParticleCount;

      particleColor = Color.white;
      if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
      {
        particleField.gameObject.layer = 10;
      }
      else
      {
        particleField.gameObject.layer = 24;
      }
    }

    float ticker = 0;
    void Update()
    {

      if (fieldGenerated)
      {
        ticker += Time.fixedDeltaTime;
        if (ticker > 0.2f)
        {
          int numParticlesAlive = particleField.GetParticles(particleBuffer);

          if (numParticlesAlive < targetCount)
          {

            List<ParticleSystem.Particle> newParticles = GenerateParticles(targetCount - numParticlesAlive);

            particleBuffer.SetRange(newParticles, Mathf.Clamp(numParticlesAlive - 1, 0, numParticlesAlive - 1));
            particleField.SetParticles(particleBuffer);
          }

          ticker = 0f;
        }

        xform.Rotate(Vector3.up, Time.deltaTime * spinRate, Space.Self);
      }
    }


    public void SetVisible(bool state)
    {
      particleRenderer.enabled = state;
      fieldCollision.SetEnabled(state);
    }

    public void CreateField(ResourceBand band,
      string resourceName,
      bool discovered,
      bool identified,
      Vector3 scaledSpacePosition)
    {
      targetCount = (int)(Mathf.Clamp(Settings.particleFieldBaseCount * band.ParticleCountScale, 1f, Settings.particleFieldMaxParticleCount));
      spinRate = band.ParticleRotateRate;


      resName = resourceName;
      Band = band;
      particleRenderer.maxParticleSize = Settings.particleFieldMaxViewportParticleScale;
      particleRenderer.material = new Material(Shader.Find(Settings.particleFieldShaderName));
      particleRenderer.material.mainTexture = (Texture)GameDatabase.Instance.GetTexture(Settings.particleFieldTextureUrl, false);

      if (identified)
        particleColor = Settings.GetResourceColor(resourceName);
      else
        particleColor = Settings.resourceDiscoveredColor;

      fieldCollision.CreateCollision(band, xform);

      List<ParticleSystem.Particle> particles = GenerateParticles(targetCount);
      particleField.SetParticles(particles.ToArray(), targetCount);

      particleBuffer = new ParticleSystem.Particle[particleField.main.maxParticles];
      fieldGenerated = true;
    }

    /// <summary>
    /// Generate the particles in the field according to the band distribution rules
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    List<ParticleSystem.Particle> GenerateParticles(int count)
    {
      List<ParticleSystem.Particle> particles = new List<ParticleSystem.Particle>();
      int i = 0;
      int iterations = 0;

      while (i < count && iterations < count * 10)
      {
        Vector3 pos = UnityEngine.Random.insideUnitSphere * ((float)Band.Distribution.MaxSize() / ScaledSpace.ScaleFactor);
        Vector3 sphericalPos = Utils.Cart2Sphere(new Vector3(pos.z, pos.x, pos.y));

        float sampled = (float)Band.Distribution.Sample(sphericalPos.x * ScaledSpace.ScaleFactor, 90d - sphericalPos.z * Mathf.Rad2Deg, sphericalPos.y * Mathf.Rad2Deg);

        if (sampled >= 0.0001f)
        {
          ParticleSystem.Particle p = new ParticleSystem.Particle();
          p.position = pos;
          float scaled = (Mathf.Log10(sampled) + 8f) / 10;
          p.startColor = new Color(particleColor.r, particleColor.g, particleColor.b, scaled * 0.25f);
          p.startSize = Settings.particleFieldBaseSize * scaled * 20;
          p.startLifetime = UnityEngine.Random.Range(25f, 50f);
          p.remainingLifetime = UnityEngine.Random.Range(25, 50f);
          p.rotation = UnityEngine.Random.Range(0f, 360f);
          particles.Add(p);
          i++;
        }
        iterations++;

      }
      return particles;
    }

   

  }
}

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
    ParticleSystem particleField;
    ParticleSystem.Particle[] particleBuffer;
    ParticleSystemRenderer particleRenderer;
    Color particleColor;
    bool fieldGenerated = false;
    public ResourceBand resBand;
    Transform xform;
    float spinRate = 2f;
    void Awake()
    {
      xform = transform;
      particleField = gameObject.AddComponent<ParticleSystem>();
      particleRenderer = gameObject.GetComponent<ParticleSystemRenderer>();
      

      var em = particleField.emission;
      em.enabled = false;
      var main = particleField.main;
      main.simulationSpace = ParticleSystemSimulationSpace.Local;
      main.playOnAwake = false;
      main.loop = false;

    
      particleColor = Color.white;
      particleField.gameObject.layer = 24 ;
    }

    float ticker = 0;
    void FixedUpdate()
    {
      
      if ( fieldGenerated)
      {
        ticker += Time.fixedDeltaTime;
        if (ticker > 0.2f)
        {
          int numParticlesAlive = particleField.GetParticles(particleBuffer);

          if (numParticlesAlive < Settings.particleFieldBaseCount)
          {
            List<ParticleSystem.Particle> newParticles = GenerateParticles(Settings.particleFieldBaseCount - numParticlesAlive);
            particleBuffer.SetRange(newParticles, numParticlesAlive - 1);
            particleField.SetParticles(particleBuffer);
          }
          
          ticker = 0f;
        }

        xform.Rotate(Vector3.up, Time.fixedDeltaTime * spinRate, Space.Self);
      }
    }

    void ConfigureTrails()
    {
      var trails = particleField.trails;
      trails.enabled = true;
      trails.ratio = 1;
      trails.lifetime = 0.011f;
      trails.textureMode = ParticleSystemTrailTextureMode.Stretch;
      trails.sizeAffectsWidth = true;
      trails.widthOverTrail = 1f;
      trails.mode = ParticleSystemTrailMode.PerParticle;
      trails.worldSpace = true;
      trails.inheritParticleColor = true;
      trails.dieWithParticles = true;

      particleRenderer.renderMode = ParticleSystemRenderMode.None;
      particleRenderer.trailMaterial = new Material(Shader.Find(Settings.particleFieldShaderName));
      particleRenderer.trailMaterial.mainTexture = (Texture)GameDatabase.Instance.GetTexture(Settings.particleFieldTextureUrl, false);
    }

    public void SetVisible(bool state)
    {
      particleRenderer.enabled = state;
    }

    public void CreateField(ResourceBand band, 
      string resourceName, 
      bool discovered, 
      bool identified, 
      Vector3 scaledSpacePosition)
    {
      resName = resourceName;
      resBand = band;
      particleRenderer.material = new Material(Shader.Find(Settings.particleFieldShaderName));
      particleRenderer.material.mainTexture = (Texture)GameDatabase.Instance.GetTexture(Settings.particleFieldTextureUrl, false);

      if (identified)
        particleColor = Settings.GetResourceColor(resourceName);
      else
        particleColor = Settings.resourceDiscoveredColor;

      List<ParticleSystem.Particle> particles = GenerateParticles(Settings.particleFieldBaseCount);
      particleField.SetParticles(particles.ToArray(), Settings.particleFieldBaseCount);

      particleBuffer = new ParticleSystem.Particle[particleField.main.maxParticles];
      fieldGenerated = true;
    }
    List<ParticleSystem.Particle> GenerateParticles(int count)
    {
      
      List<ParticleSystem.Particle> particles = new List<ParticleSystem.Particle>();
      int i = 0;
      int iterations = 0;
      
      while (i < count && iterations < count * 10)
      {

        Vector3 pos = UnityEngine.Random.insideUnitSphere * (float)resBand.Distribution.MaxSize() / ScaledSpace.ScaleFactor;
        Vector3 sphericalPos = Cart2Sphere(new Vector3(pos.z, pos.x, pos.y));
        float sampled = (float)resBand.Distribution.Sample(sphericalPos.x* ScaledSpace.ScaleFactor, sphericalPos.z * Mathf.Rad2Deg, sphericalPos.y * Mathf.Rad2Deg);
        
        if (sampled >= 0.001f)
        {
          ParticleSystem.Particle p = new ParticleSystem.Particle();
          p.position = pos;
          
          p.startColor = new Color(particleColor.r, particleColor.g, particleColor.b, sampled*0.5f);
          p.startSize = Settings.particleFieldBaseSize*Mathf.Clamp01(sampled*2f);
          p.startLifetime = UnityEngine.Random.Range(25f, 50f);
          p.remainingLifetime = UnityEngine.Random.Range(25, 50f);
          p.rotation = UnityEngine.Random.Range(0f,360f);
          particles.Add(p);
          i++;
        }
        iterations++;

      }
      return particles;
    }

    Vector3 Cart2Sphere(Vector3 cart)
    {
      float r = Mathf.Sqrt(cart.x * cart.x + cart.y * cart.y + cart.z * cart.z);
      float phi = Mathf.Atan2(cart.y, cart.x);
      float theta = Mathf.Acos(cart.z / r);

      return new Vector3(r, phi, theta);
    }
    
  }
}

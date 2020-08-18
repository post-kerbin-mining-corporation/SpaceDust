using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KSP.Localization;

namespace SpaceDust
{
  public class ModuleSpaceDustTelescope: PartModule
  {
    // Am i enabled?
    [KSPField(isPersistant = true)]
    public bool Enabled = false;

    // Cost per second to run the telescope
    [KSPField(isPersistant = true)]
    public float PowerCost = 1f;


  }
}

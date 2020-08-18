using System.Collections.Generic;
using UnityEngine;

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

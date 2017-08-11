using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SpaceDust
{
    public static class Utils
    {

        public static void Log(string str)
        {
            Debug.Log("[SpaceDust]: " + str);
        }
        public static void LogError(string str)
        {
            Debug.LogError("[SpaceDust]: " + str);
        }
        public static void LogWarning(string str)
        {
            Debug.LogWarning("[SpaceDust]: " + str);
        }

        public static FloatCurve GetValue(ConfigNode node, string nodeID, FloatCurve defaultValue)
        {
            if (node.HasNode(nodeID))
            {
                FloatCurve theCurve = new FloatCurve();
                ConfigNode[] nodes = node.GetNodes(nodeID);
                for (int i = 0; i < nodes.Length; i++)
                {
                    string[] valueArray = nodes[i].GetValues("key");

                    for (int l = 0; l < valueArray.Length; l++)
                    {
                        string[] splitString = valueArray[l].Split(' ');
                        Vector2 v2 = new Vector2(float.Parse(splitString[0]), float.Parse(splitString[1]));
                        theCurve.Add(v2.x, v2.y, 0, 0);
                    }
                }
                Debug.Log(theCurve.Evaluate(0f));
                return theCurve;
            }
            Debug.Log("default");
            return defaultValue;
        }
    }
}

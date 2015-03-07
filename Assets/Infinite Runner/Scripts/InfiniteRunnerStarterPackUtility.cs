using UnityEngine;
using System.Collections;

namespace InfiniteRunner
{
    /*
     * A small class for common functions.
     */
    public class InfiniteRunnerStarterPackUtility
    {
        public static void ActiveRecursively(Transform obj, bool active)
        {
            foreach (Transform child in obj) {
                InfiniteRunnerStarterPackUtility.ActiveRecursively(child, active);
            }
            obj.gameObject.SetActive(active);
        }

        public static Vector3 CalculateBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            // quadratic bezier curve formula: p0(1-t)^2+2p1t(1-t)+p2t^2
            float u = 1 - t;
            return ((u * u) * p0) + (2 * t * u * p1) + (t * t * p2);
        }
    }
}
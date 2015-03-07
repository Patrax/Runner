using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using InfiniteRunner.InfiniteObjects;

namespace InfiniteRunner.InfiniteGenerator
{
    /*
     * A static class which will show the editor insector for the Platform Placement Rules
     */
    public class PlatformPlacementRuleInspector : Editor
    {

        public static bool showPlatforms(ref List<PlatformPlacementRule> platformPlacementRules, bool linkedPlatform)
        {
            GUILayout.Label(string.Format("Platforms {0}", (linkedPlatform ? "Linked" : "Avoided")), "BoldLabel");
            if (platformPlacementRules == null || platformPlacementRules.Count == 0) {
                GUILayout.Label(string.Format("No platforms {0}", (linkedPlatform ? "linked" : "avoided")));
                return false;
            }

            PlatformPlacementRule platformPlacementRule;
            for (int i = 0; i < platformPlacementRules.Count; ++i) {
                platformPlacementRule = platformPlacementRules[i];

                // quick cleanup if the platform has gone null
                if (platformPlacementRule.platform == null) {
                    platformPlacementRules.RemoveAt(i);
                    return true;
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label(string.Format("  {0}", platformPlacementRule.platform.name));
                if (GUILayout.Button("Remove")) {
                    platformPlacementRules.RemoveAt(i);
                    return true;
                }
                GUILayout.EndHorizontal();
            }

            return false;
        }

        public static int AddPlatform(ref List<PlatformPlacementRule> platformPlacementRules, InfiniteObject platform, bool linkedPlatform)
        {
            if (platformPlacementRules == null) {
                platformPlacementRules = new List<PlatformPlacementRule>();
            }
            // Make sure there aren't any duplicates
            for (int i = 0; i < platformPlacementRules.Count; ++i) {
                if (platformPlacementRules[i].platform == platform)
                    return 2;
            }

            platformPlacementRules.Add(new PlatformPlacementRule(platform, linkedPlatform));
            return 0;
        }
    }
}
using UnityEngine;
using UnityEditor;

namespace InfiniteRunner.InfiniteGenerator
{
    /*
     * Infinite Object Generator inspector editor - mainly used to show the custom Distance Value List inspector
     */
    [CustomEditor(typeof(InfiniteObjectGenerator))]
    public class InfiniteObjectGeneratorInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            InfiniteObjectGenerator infiniteObjectGenerator = (InfiniteObjectGenerator)target;
            GUILayout.Label("No Collidable Probabilities", "BoldLabel");
            DistanceValueList distanceProbabilityList = infiniteObjectGenerator.noCollidableProbability;
            if (DistanceValueListInspector.ShowLoopToggle(ref distanceProbabilityList, DistanceValueType.Probability)) {
                infiniteObjectGenerator.noCollidableProbability = distanceProbabilityList;
                EditorUtility.SetDirty(target);
            }
            DistanceValueListInspector.ShowDistanceValues(ref distanceProbabilityList, DistanceValueType.Probability);

            if (DistanceValueListInspector.ShowAddNewValue(ref distanceProbabilityList, DistanceValueType.Probability)) {
                infiniteObjectGenerator.noCollidableProbability = distanceProbabilityList;
                EditorUtility.SetDirty(target);
            }
        }
    }
}
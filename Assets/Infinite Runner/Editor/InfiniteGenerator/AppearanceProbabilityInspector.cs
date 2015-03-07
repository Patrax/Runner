using UnityEngine;
using UnityEditor;

namespace InfiniteRunner.InfiniteGenerator
{
    /*
     * This editor script will allow you to quickly add appearance probabilities while ensuring the values will work within the game
     */
    [CustomEditor(typeof(AppearanceProbability))]
    public class AppearanceProbabilityInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            AppearanceProbability appearanceProbability = ((AppearanceProbability)target);

            DistanceValueList occurProbabilities = appearanceProbability.occurProbabilities;
            GUILayout.Label("Occur Probabilities", "BoldLabel");
            if (DistanceValueListInspector.ShowLoopToggle(ref occurProbabilities, DistanceValueType.Probability)) {
                appearanceProbability.occurProbabilities = occurProbabilities;
                EditorUtility.SetDirty(target);
            }
            DistanceValueListInspector.ShowDistanceValues(ref occurProbabilities, DistanceValueType.Probability);

            DistanceValueList noOccurProbabilities = appearanceProbability.noOccurProbabilities;
            GUILayout.Label("No Occur Probabilities", "BoldLabel");
            if (DistanceValueListInspector.ShowLoopToggle(ref noOccurProbabilities, DistanceValueType.Probability)) {
                appearanceProbability.noOccurProbabilities = noOccurProbabilities;
                EditorUtility.SetDirty(target);
            }
            DistanceValueListInspector.ShowDistanceValues(ref noOccurProbabilities, DistanceValueType.Probability);

            if (DistanceValueListInspector.ShowAddNewValue(ref occurProbabilities, ref noOccurProbabilities)) {
                appearanceProbability.occurProbabilities = occurProbabilities;
                appearanceProbability.noOccurProbabilities = noOccurProbabilities;
                EditorUtility.SetDirty(target);
            }
        }
    }
}
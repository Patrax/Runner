using UnityEngine;
using UnityEditor;
using InfiniteRunner.InfiniteGenerator;

namespace InfiniteRunner.Player
{
    /*
     * Player Controller inspector editor - mainly used to show the custom Distance Value List inspector
     */
    [CustomEditor(typeof(PlayerController))]
    public class PlayerControllerInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            PlayerController playerController = (PlayerController)target;

            GUILayout.Label("Forward Speeds", "BoldLabel");
            DistanceValueList forwardSpeedList = playerController.forwardSpeeds;
            if (DistanceValueListInspector.ShowLoopToggle(ref forwardSpeedList, DistanceValueType.Speed)) {
                playerController.forwardSpeeds = forwardSpeedList;
                EditorUtility.SetDirty(target);
            }
            DistanceValueListInspector.ShowDistanceValues(ref forwardSpeedList, DistanceValueType.Speed);

            if (DistanceValueListInspector.ShowAddNewValue(ref forwardSpeedList, DistanceValueType.Speed)) {
                playerController.forwardSpeeds = forwardSpeedList;
                EditorUtility.SetDirty(target);
            }
        }
    }
}
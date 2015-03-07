// To prevent any NGUI scripts from being used comment out the following line:
//#define COMPILE_NGUI

using UnityEngine;
using UnityEditor;
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
using UnityEngine.UI;
#endif

namespace InfiniteRunner.Game
{
    /*
     * Custom inspector to show either the NGUI or uGUI fields
     */
    [CustomEditor(typeof(GUIManager))]
    public class GUIManagerInspector : Editor
    {
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
        private static string[] guiTypes = { "NGUI", "uGUI" };
#endif
        private static GUIContent guiContent = new GUIContent();

        public override void OnInspectorGUI()
        {
            GUIManager guiManager = target as GUIManager;
            if (guiManager == null) {
                return;
            }

            SerializedObject serializedObject = new SerializedObject(guiManager);
            EditorGUI.BeginChangeCheck();

#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
            int guiTypeIndex = EditorGUILayout.Popup("GUI Type", guiManager.useuGUI ? 1 : 0, guiTypes);
            guiManager.useuGUI = guiTypeIndex == 1;
#endif
            DrawProperty(serializedObject, "clickDelay", "Click Delay");

            GUILayout.Label("Main Menu", "BoldLabel");
            DrawProperty(serializedObject, "mainMenuPanel", "Main Menu Panel");
            DrawProperty(serializedObject, "logoPanel", "Logo Panel");

            GUILayout.Label("In Game", "BoldLabel");
            DrawProperty(serializedObject, "inGameLeftPanel", "Left Panel");
            DrawProperty(serializedObject, "inGameTopPanel", "Top Panel");
            DrawProperty(serializedObject, "inGameRightPanel", "Right Panel");
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
            if (guiManager.useuGUI) {
                DrawProperty(serializedObject, "inGameScoreText", "Score Text");
                DrawProperty(serializedObject, "inGameCoinsText", "Coins Text");
                DrawProperty(serializedObject, "inGameSecondaryCoinsText", "Secondary Coins Text");
                DrawProperty(serializedObject, "inGameActivePowerUpImage", "Active Power Up Image");
                DrawProperty(serializedObject, "inGamePowerUpSprites", "Power Up Sprites");
            } else {
#endif
#if COMPILE_NGUI
                DrawProperty(serializedObject, "inGameScore", "Score Label");
                DrawProperty(serializedObject, "inGameCoins", "Coins Label");
                DrawProperty(serializedObject, "inGameSecondaryCoins", "Secondary Coins Label");
                DrawProperty(serializedObject, "inGameActivePowerUp", "Active Power Up Sprite");
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
            }
#endif
            DrawProperty(serializedObject, "inGameActivePowerUpCutoffMaterial", "Active Power Up Cutoff Material");
            DrawProperty(serializedObject, "inGamePlayAnimation", "Play Animations");
            DrawProperty(serializedObject, "inGamePlayAnimationName", "Play Animation Name");
            
            GUILayout.Label("Pause", "BoldLabel");
            DrawProperty(serializedObject, "pausePanel", "Panel");
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
            if (guiManager.useuGUI) {
                DrawProperty(serializedObject, "pauseScoreText", "Score Text");
                DrawProperty(serializedObject, "pauseCoinsText", "Coins Text");
                DrawProperty(serializedObject, "pauseSecondaryCoinsText", "Secondary Coins Text");
            } else {
#endif
#if COMPILE_NGUI
                DrawProperty(serializedObject, "pauseScore", "Score Label");
                DrawProperty(serializedObject, "pauseCoins", "Coins Label");
                DrawProperty(serializedObject, "pauseSecondaryCoins", "Secondary Coins Label");
#endif

#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
            }
#endif

            GUILayout.Label("Revive", "BoldLabel");
            DrawProperty(serializedObject, "revivePanel", "Panel");
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
            if (guiManager.useuGUI) {
                DrawProperty(serializedObject, "reviveDescriptionText", "Description Text");
            } else {
#endif
#if COMPILE_NGUI
                DrawProperty(serializedObject, "reviveDescription", "Description Label");
#endif

#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
            }
#endif
            DrawProperty(serializedObject, "revivePlayAnimation", "Play Animation");
            DrawProperty(serializedObject, "revivePlayAnimationName", "Play Animation Name");
            DrawProperty(serializedObject, "reviveYesButton", "Yes Button");

            GUILayout.Label("End Game", "BoldLabel");
            DrawProperty(serializedObject, "endGamePanel", "Panel");
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
            if (guiManager.useuGUI) {
                DrawProperty(serializedObject, "endGameScoreText", "Score Text");
                DrawProperty(serializedObject, "endGameCoinsText", "Coins Text");
                DrawProperty(serializedObject, "endGameMultiplierText", "Multiplier Text");
            } else {
#endif
#if COMPILE_NGUI
                DrawProperty(serializedObject, "endGameScore", "Score Label");
                DrawProperty(serializedObject, "endGameCoins", "Coins Label");
                DrawProperty(serializedObject, "endGameMultiplier", "Multiplier Label");
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
            }
#endif
            DrawProperty(serializedObject, "endGamePlayAnimation", "Play Animation");
            DrawProperty(serializedObject, "endGamePlayAnimationName", "Play Animation Name");

            GUILayout.Label("Store", "BoldLabel");
            DrawProperty(serializedObject, "storePanel", "Panel");
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
            if (guiManager.useuGUI) {
                DrawProperty(serializedObject, "storePowerUpSelectionButtonText", "Power Up Selection Button Text");
                DrawProperty(serializedObject, "storeTitleText", "Score Text");
                DrawProperty(serializedObject, "storeDescriptionText", "Description Text");
                DrawProperty(serializedObject, "storeCoinsText", "Coins Text");
            } else {
#endif
#if COMPILE_NGUI
                DrawProperty(serializedObject, "storePowerUpSelectionButton", "Power Up Selection Button Label");
                DrawProperty(serializedObject, "storeTitle", "Score Label");
                DrawProperty(serializedObject, "storeDescription", "Description Label");
                DrawProperty(serializedObject, "storeCoins", "Coins Label");
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
            }
#endif
            DrawProperty(serializedObject, "storeBackToMainMenuButton", "Back To Main Menu GameObject");
            DrawProperty(serializedObject, "storeBackToEndGameButton", "Back To End Game GameObject");
            DrawProperty(serializedObject, "storeBuyButton", "Buy Button GameObject");
            DrawProperty(serializedObject, "storePowerUpPreviewTransform", "Power Up Preview Transform");
            DrawProperty(serializedObject, "storeCharacterPreviewTransform", "Character Preview Transform");

            GUILayout.Label("Stats", "BoldLabel");
            DrawProperty(serializedObject, "statsPanel", "Panel");
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
            if (guiManager.useuGUI) {
                DrawProperty(serializedObject, "statsHighScoreText", "High Score Text");
                DrawProperty(serializedObject, "statsCoinsText", "Coins Text");
                DrawProperty(serializedObject, "statsSecondaryCoinsText", "Secondary Coins Text");
                DrawProperty(serializedObject, "statsPlayCountText", "Play Count Text");
            } else {
#endif
#if COMPILE_NGUI
                DrawProperty(serializedObject, "statsHighScore", "High Score Label");
                DrawProperty(serializedObject, "statsCoins", "Coins Label");
                DrawProperty(serializedObject, "statsSecondaryCoins", "Secondary Coins Label");
                DrawProperty(serializedObject, "statsPlayCount", "Play Count Label");
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
            }
#endif

            GUILayout.Label("Missions", "BoldLabel");
            DrawProperty(serializedObject, "missionsPanel", "Panel");
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
            if (guiManager.useuGUI) {
                DrawProperty(serializedObject, "tutorialLabelText", "Tutorial Label Text");
                DrawProperty(serializedObject, "missionsActiveMission1Text", "Active Mission 1 Text");
                DrawProperty(serializedObject, "missionsActiveMission2Text", "Active Mission 2 Text");
                DrawProperty(serializedObject, "missionsActiveMission3Text", "Active Mission 3 Text");
            } else {
#endif
#if COMPILE_NGUI
                DrawProperty(serializedObject, "missionsScoreMultiplier", "Score Multiplier Label");
                DrawProperty(serializedObject, "missionsActiveMission1", "Active Mission 1 Label");
                DrawProperty(serializedObject, "missionsActiveMission2", "Active Mission 2 Label");
                DrawProperty(serializedObject, "missionsActiveMission3", "Active Mission 3 Label");
#endif

#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
            }
#endif
            DrawProperty(serializedObject, "missionsBackToMainMenuButton", "Back To Main Menu Button GameObject");
            DrawProperty(serializedObject, "missionsBackToEndGameButton", "Back To End Game Button GameObject");

            GUILayout.Label("In Game Missions", "BoldLabel");
            DrawProperty(serializedObject, "inGameMissionsPanel", "Panel");
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
            if (guiManager.useuGUI) {
                DrawProperty(serializedObject, "inGameMissionsMissionCompleteText", "Mission Complete Text");
            } else {
#endif
#if COMPILE_NGUI
                DrawProperty(serializedObject, "inGameMissionsMissionComplete", "Mission Complete Label");
#endif

#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
            }
#endif

            GUILayout.Label("Tutorial", "BoldLabel");
            DrawProperty(serializedObject, "tutorialPanel", "Panel");
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
            if (guiManager.useuGUI) {
                DrawProperty(serializedObject, "tutorialLabelText", "Tutorial Label Text");
            } else {
#endif
#if COMPILE_NGUI
                DrawProperty(serializedObject, "tutorialLabel", "Tutorial Label");
#endif

#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
            }
#endif
            DrawProperty(serializedObject, "inGameMissionsPlayAnimation", "Play Animation");
            DrawProperty(serializedObject, "inGameMissionsPlayAnimationName", "Play Animation Name");

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawProperty(SerializedObject serializedObject, string propertyPath, string label)
        {
            SerializedProperty serializedProperty = serializedObject.FindProperty(propertyPath);
            if (serializedProperty != null) {
                guiContent.text = label;
                EditorGUILayout.PropertyField(serializedProperty, guiContent, true);
            }
        }
    }
}
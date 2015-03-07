using UnityEngine;
using UnityEditor;
using System;

namespace InfiniteRunner.Game
{
    /*
     * An inspector class which allows you to easily set the static data
     */
    [CustomEditor(typeof(StaticData))]
    public class StaticDataInspector : Editor
    {
        public void OnEnable()
        {
            StaticData staticData = (StaticData)target;
            if (staticData.totalPowerUpLevels == 0) {
                staticData.totalPowerUpLevels = 1;
            }

            int powerUpCount = (int)PowerUpTypes.None;
            if (staticData.powerUpTitle == null || staticData.powerUpTitle.Length != staticData.totalPowerUpLevels || staticData.powerUpTitle.Length != powerUpCount) {
                string[] powerUpTitle = staticData.powerUpTitle;
                string[] powerUpDescription = staticData.powerUpDescription;
                GameObject[] powerUpPrefab = staticData.powerUpPrefab;
                float[] powerUpLength = staticData.powerUpLength;
                int[] powerUpCost = staticData.powerUpCost;

                staticData.powerUpTitle = new string[(int)PowerUpTypes.None];
                staticData.powerUpDescription = new string[(int)PowerUpTypes.None];
                staticData.powerUpPrefab = new GameObject[(int)PowerUpTypes.None];
                staticData.powerUpLength = new float[(int)PowerUpTypes.None * (staticData.totalPowerUpLevels + 1)];
                staticData.powerUpCost = new int[(int)PowerUpTypes.None * staticData.totalPowerUpLevels];

                if (powerUpTitle != null) {
                    for (int i = 0; i < powerUpTitle.Length; ++i) {
                        if (staticData.powerUpTitle.Length == i) {
                            break;
                        }
                        staticData.powerUpTitle[i] = powerUpTitle[i];
                        staticData.powerUpDescription[i] = powerUpDescription[i];
                        staticData.powerUpPrefab[i] = powerUpPrefab[i];
                    }
                    for (int i = 0; i < powerUpLength.Length; ++i) {
                        if (staticData.powerUpLength.Length == i) {
                            break;
                        }
                        staticData.powerUpLength[i] = powerUpLength[i];
                    }
                    for (int i = 0; i < powerUpCost.Length; ++i) {
                        if (staticData.powerUpCost.Length == i) {
                            break;
                        }
                        staticData.powerUpCost[i] = powerUpCost[i];
                    }
                }
                EditorUtility.SetDirty(staticData);
            }

            if (staticData.characterCount == 0) {
                staticData.characterCount = 1;
            }
            if (staticData.characterTitle == null || staticData.characterTitle.Length != staticData.characterCount) {
                staticData.characterTitle = new string[staticData.characterCount];
                staticData.characterDescription = new string[staticData.characterCount];
                staticData.characterCost = new int[staticData.characterCount];
                staticData.characterPrefab = new GameObject[staticData.characterCount];
                EditorUtility.SetDirty(staticData);
            }

            if (staticData.missionDescription == null || staticData.missionDescription.Length != (int)MissionType.None) {
                staticData.missionDescription = new string[(int)MissionType.None];
                staticData.missionCompleteText = new string[(int)MissionType.None];
                staticData.missionGoal = new int[(int)MissionType.None];
                EditorUtility.SetDirty(staticData);
            }

            if (staticData.missionDescription.Length != staticData.missionCompleteText.Length) {
                staticData.missionCompleteText = new string[(int)MissionType.None];
            }
        }

        public override void OnInspectorGUI()
        {
            StaticData staticData = (StaticData)target;

            // Characters:
            GUILayout.Label("Characters", "BoldLabel");

            for (int i = 0; i < staticData.characterCount; ++i) {
                GUILayout.Label(string.Format("Character{0}", i), "BoldLabel");
                staticData.characterTitle[i] = EditorGUILayout.TextField("Title", staticData.characterTitle[i]);
                staticData.characterDescription[i] = EditorGUILayout.TextField("Description", staticData.characterDescription[i]);
                staticData.characterCost[i] = EditorGUILayout.IntField("Cost", staticData.characterCost[i]);

                GUILayout.BeginHorizontal();
                GUILayout.Label("Prefab");
                staticData.characterPrefab[i] = EditorGUILayout.ObjectField(staticData.characterPrefab[i], typeof(GameObject), false, GUILayout.Width(152)) as GameObject;
                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Character")) {
                addCharacter();
            }
            if (staticData.characterCount > 1 && GUILayout.Button("Remove Character")) {
                removeCharacter();
            }

            GUILayout.Space(20);

            // Reviving:
            GUILayout.Label("Revive", "BoldLabel");
            staticData.reviveSecondaryCoinCost = EditorGUILayout.IntField("Secondary Coins Cost", staticData.reviveSecondaryCoinCost);
            GUILayout.Space(20);

            // Chase:
            GUILayout.Label("Chase Object", "BoldLabel");
            GUILayout.BeginHorizontal();
            GUILayout.Label("Prefab");
            staticData.chaseObjectPrefab = EditorGUILayout.ObjectField(staticData.chaseObjectPrefab, typeof(GameObject), false, GUILayout.Width(152)) as GameObject;
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            // Power Ups:
            GUILayout.Label("Power Ups", "BoldLabel");

            int powerUpLevels = staticData.totalPowerUpLevels;
            for (int i = 0; i < (int)PowerUpTypes.None; ++i) {
                GUILayout.Label(Enum.GetName(typeof(PowerUpTypes), i), "BoldLabel");
                staticData.powerUpTitle[i] = EditorGUILayout.TextField("Title", staticData.powerUpTitle[i]);
                staticData.powerUpDescription[i] = EditorGUILayout.TextField("Description", staticData.powerUpDescription[i]);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Prefab");
                staticData.powerUpPrefab[i] = EditorGUILayout.ObjectField(staticData.powerUpPrefab[i], typeof(GameObject), false, GUILayout.Width(152)) as GameObject;
                GUILayout.EndHorizontal();
                for (int j = 0; j < powerUpLevels; ++j) {
                    staticData.powerUpCost[(i * powerUpLevels) + j] = EditorGUILayout.IntField(string.Format("Level {0} Cost", j), staticData.powerUpCost[(i * powerUpLevels) + j]);
                    staticData.powerUpLength[(i * (powerUpLevels + 1)) + j + 1] = EditorGUILayout.FloatField(string.Format("Level {0} Length", j), staticData.powerUpLength[(i * (powerUpLevels + 1)) + j + 1]);
                }
            }
            if (GUILayout.Button("Add Level")) {
                addPowerUpLevel();
            }
            if (staticData.totalPowerUpLevels > 1 && GUILayout.Button("Remove Level")) {
                removePowerUpLevel();
            }

            GUILayout.Space(20);

            // Missions:
            GUILayout.Label("Missions", "BoldLabel");

            for (int i = 0; i < (int)MissionType.None; ++i) {
                GUILayout.Label(Enum.GetName(typeof(MissionType), i), "BoldLabel");
                staticData.missionDescription[i] = EditorGUILayout.TextField("Description", staticData.missionDescription[i]);
                staticData.missionCompleteText[i] = EditorGUILayout.TextField("Mission Complete", staticData.missionCompleteText[i]);
                staticData.missionGoal[i] = EditorGUILayout.IntField("Goal", staticData.missionGoal[i]);
            }
        }

        // add a new level to the end of each power up type
        private void addPowerUpLevel()
        {
            StaticData staticData = (StaticData)target;

            int powerUpLevels = staticData.totalPowerUpLevels;
            int[] cost = new int[staticData.powerUpCost.Length + (int)PowerUpTypes.None];
            float[] length = new float[staticData.powerUpLength.Length + (int)PowerUpTypes.None];
            for (int i = 0; i < (int)PowerUpTypes.None; ++i) {
                for (int j = 0; j < powerUpLevels + 1; ++j) {
                    if (j == powerUpLevels) {
                        cost[i * (powerUpLevels + 1) + j] = 0;
                        length[(i * (powerUpLevels + 2)) + j + 1] = 0;
                    } else {
                        cost[i * (powerUpLevels + 1) + j] = staticData.powerUpCost[i * powerUpLevels + j];
                        length[(i * (powerUpLevels + 2)) + j + 1] = staticData.powerUpLength[(i * (powerUpLevels + 1)) + j + 1];
                    }
                }
            }

            staticData.powerUpCost = cost;
            staticData.powerUpLength = length;
            staticData.totalPowerUpLevels += 1;

            EditorUtility.SetDirty(staticData);
        }

        // remove the last level from each power up type
        private void removePowerUpLevel()
        {
            StaticData staticData = (StaticData)target;

            int powerUpLevels = staticData.totalPowerUpLevels;
            int[] cost = new int[staticData.powerUpCost.Length - (int)PowerUpTypes.None];
            float[] length = new float[staticData.powerUpLength.Length - (int)PowerUpTypes.None];
            for (int i = 0; i < (int)PowerUpTypes.None; ++i) {
                for (int j = 0; j < powerUpLevels - 1; ++j) {
                    cost[i * (powerUpLevels - 1) + j] = staticData.powerUpCost[i * powerUpLevels + j];
                    length[(i * powerUpLevels) + j + 1] = staticData.powerUpLength[(i * (powerUpLevels + 1)) + j + 1];
                }
            }

            staticData.powerUpCost = cost;
            staticData.powerUpLength = length;
            staticData.totalPowerUpLevels -= 1;

            EditorUtility.SetDirty(staticData);
        }

        private void addCharacter()
        {
            StaticData staticData = (StaticData)target;

            int characterCount = staticData.characterCount;
            string[] title = new string[characterCount + 1];
            string[] description = new string[characterCount + 1];
            int[] cost = new int[characterCount + 1];
            GameObject[] prefab = new GameObject[characterCount + 1];
            for (int i = 0; i < characterCount; ++i) {
                title[i] = staticData.characterTitle[i];
                description[i] = staticData.characterDescription[i];
                cost[i] = staticData.characterCost[i];
                prefab[i] = staticData.characterPrefab[i];
            }

            staticData.characterTitle = title;
            staticData.characterDescription = description;
            staticData.characterCost = cost;
            staticData.characterPrefab = prefab;
            staticData.characterCount += 1;

            EditorUtility.SetDirty(staticData);
        }

        private void removeCharacter()
        {
            StaticData staticData = (StaticData)target;

            int characterCount = staticData.characterCount;
            string[] title = new string[characterCount - 1];
            string[] description = new string[characterCount - 1];
            int[] cost = new int[characterCount - 1];
            GameObject[] prefab = new GameObject[characterCount - 1];
            for (int i = 0; i < characterCount - 1; ++i) {
                title[i] = staticData.characterTitle[i];
                description[i] = staticData.characterDescription[i];
                cost[i] = staticData.characterCost[i];
                prefab[i] = staticData.characterPrefab[i];
            }

            staticData.characterTitle = title;
            staticData.characterDescription = description;
            staticData.characterCost = cost;
            staticData.characterPrefab = prefab;
            staticData.characterCount -= 1;

            EditorUtility.SetDirty(staticData);
        }
    }
}
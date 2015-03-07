using UnityEngine;
using System.Collections;

namespace InfiniteRunner.Game
{
    /*
     * Static data is a singleton class which holds any data which is not directly game related, such as the power up length and cost
     */
    public class StaticData : MonoBehaviour
    {
        static public StaticData instance;

        public int reviveSecondaryCoinCost;

        public int totalPowerUpLevels;
        public string[] powerUpTitle;
        public string[] powerUpDescription;
        public GameObject[] powerUpPrefab;
        public float[] powerUpLength;
        public int[] powerUpCost;

        public int characterCount;
        public string[] characterTitle;
        public string[] characterDescription;
        public int[] characterCost;
        public GameObject[] characterPrefab;

        public string[] missionDescription;
        public string[] missionCompleteText;
        public int[] missionGoal;

        public GameObject chaseObjectPrefab;

        public void Awake()
        {
            instance = this;
        }

        public string GetPowerUpTitle(PowerUpTypes powerUpType)
        {
            return powerUpTitle[(int)powerUpType];
        }

        public string GetPowerUpDescription(PowerUpTypes powerUpType)
        {
            return powerUpDescription[(int)powerUpType];
        }

        public GameObject GetPowerUpPrefab(PowerUpTypes powerUpType)
        {
            return powerUpPrefab[(int)powerUpType];
        }

        public float GetPowerUpLength(PowerUpTypes powerUpType, int level)
        {
            return powerUpLength[((int)powerUpType * (totalPowerUpLevels + 1)) + level];
        }

        public int GetPowerUpCost(PowerUpTypes powerUpType, int level)
        {
            return powerUpCost[((int)powerUpType * totalPowerUpLevels) + level];
        }

        public int GetTotalPowerUpLevels()
        {
            return totalPowerUpLevels;
        }

        public string GetCharacterTitle(int character)
        {
            return characterTitle[character];
        }

        public string GetCharacterDescription(int character)
        {
            return characterDescription[character];
        }

        public int GetCharacterCost(int character)
        {
            return characterCost[character];
        }

        public GameObject GetCharacterPrefab(int character)
        {
            return characterPrefab[character];
        }

        public string GetMissionDescription(MissionType missionType)
        {
            return missionDescription[(int)missionType];
        }

        public string GetMissionCompleteText(MissionType missionType)
        {
            return missionCompleteText[(int)missionType];
        }

        public int GetMissionGoal(MissionType missionType)
        {
            return missionGoal[(int)missionType];
        }

        public GameObject GetChaseObjectPrefab()
        {
            return chaseObjectPrefab;
        }
    }
}
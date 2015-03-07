using UnityEngine;
using System.Collections;

namespace InfiniteRunner.Game
{
    /**
     * After each run GameOver() will be called and the MissionManager will check the active missions to determine if they have been satisifed. If they are satisfied, the scoreMultiplier
     * is incremented by scoreMultiplierIncrement and that value is multiplied by the points to give you your final score.
     * 
     * ID                       Description
     * NoviceRunner             run for 500 points
     * CompetentRunner          run for 1500 points
     * ExpertRunner             run for 5000 points
     * RunnerComplete           running complete
     * NoviceCoinCollector      collect 50 coins
     * CompetentCoinCollector   collect 150 coins
     * ExpertCoinCollector      collect 500 coins
     * CoinCollectorComplete    coin collector complete
     * NovicePlayCount          play 5 games
     * CompetentPlayCount       play 15 games
     * ExpertPlayCount          play 50 games
     * PlayCountComplete        play count complete
     **/
    public enum MissionType
    {
        NoviceRunner, CompetentRunner, ExpertRunner, RunnerComplete, NoviceCoinCollector, CompetentCoinCollector, ExpertCoinCollector,
        CoinCollectorComplete, NovicePlayCount, CompetentPlayCount, ExpertPlayCount, PlayCountComplete, None
    }
    public class MissionManager : MonoBehaviour
    {
        static public MissionManager instance;

        // callback for any class that is interested when a mission is complete (such as the social manager)
        public delegate void MissionCompleteHandler(MissionType missionType);
        public event MissionCompleteHandler OnMissionComplete;

        // The amount the score should be multiplied by each time a challenge is complete
        public float scoreMultiplierIncrement;

        // Should a GUI notification pop up when the mission has been completed?
        public bool instantGUINotification = true;
        // If instant gui notification is enabled, this value determines how often the missions are checked for completeness
        public float instantGUINotificationUpdateInterval = 0.1f;
        private WaitForSeconds instantNotificationDelay;

        private MissionType[] activeMissions;
        private float scoreMultiplier;

        private DataManager dataManager;
        private GUIManager guiManager;

        public void Awake()
        {
            instance = this;
        }

        public void Start()
        {
            dataManager = DataManager.instance;

            activeMissions = new MissionType[3]; // 3 active missions at a time
            scoreMultiplier = 1;
            for (int i = 0; i < activeMissions.Length; ++i) {
                activeMissions[i] = (MissionType)PlayerPrefs.GetInt(string.Format("Mission{0}", i), -1);
                // there are no active missions if the game hasn't been started yet
                if ((int)activeMissions[i] == -1) {
                    activeMissions[i] = (MissionType)(i * 4); // 4 missions per set
                }
                scoreMultiplier += ((int)activeMissions[i] % 4) * scoreMultiplierIncrement;
            }

            if (instantGUINotification) {
                guiManager = GUIManager.instance;
                instantNotificationDelay = new WaitForSeconds(instantGUINotificationUpdateInterval);
                GameManager.instance.OnStartGame += StartGame;
            }
        }

        private void StartGame()
        {
            GameManager.instance.OnPauseGame += GamePaused;

            if (instantGUINotification) {
                StartCoroutine("InGameCheckForCompletedMissions");
            }
        }

        // Check for any completed missions while the game is running. If a mission is complete then show it in the gui
        private IEnumerator InGameCheckForCompletedMissions()
        {
            while (true) {
                yield return instantNotificationDelay;

                MissionType completedMission;
                if ((completedMission = CheckForCompletedMissions(true)) != MissionType.None) {
                    guiManager.ShowInGameMissionCompletePanel(dataManager.GetMissionCompleteText(completedMission));
                }
            }
        }

        public void GameOver()
        {
            CheckForCompletedMissions(false);
            GameManager.instance.OnPauseGame -= GamePaused;
            StopCoroutine("InGameCheckForCompletedMissions");
        }

        // loop through the active missions and determine if the previous run satisfied the mission requirements
        private MissionType CheckForCompletedMissions(bool inGame)
        {
            for (int i = 0; i < activeMissions.Length; ++i) {
                switch (activeMissions[i]) {
                    case MissionType.NoviceRunner:
                    case MissionType.CompetentRunner:
                    case MissionType.ExpertRunner:
                        if (dataManager.GetScore(false) >= dataManager.GetMissionGoal(activeMissions[i])) {
                            MissionType completeMission = activeMissions[i];
                            MissionComplete(activeMissions[i]);
                            return completeMission;
                        }
                        break;
                    case MissionType.NoviceCoinCollector:
                    case MissionType.CompetentCoinCollector:
                    case MissionType.ExpertCoinCollector:
                        if (dataManager.GetLevelCoins(true) >= dataManager.GetMissionGoal(activeMissions[i])) {
                            MissionType completeMission = activeMissions[i];
                            MissionComplete(activeMissions[i]);
                            return completeMission;
                        }
                        break;
                    case MissionType.NovicePlayCount:
                    case MissionType.CompetentPlayCount:
                    case MissionType.ExpertPlayCount:
                        // play count doesn't get incremented until after CheckForCompletedMissions is called 
                        if (dataManager.GetPlayCount() + (inGame ? 0 : 1) >= dataManager.GetMissionGoal(activeMissions[i])) {
                            MissionType completeMission = activeMissions[i];
                            MissionComplete(activeMissions[i]);
                            return completeMission;
                        }
                        break;
                }
            }
            return MissionType.None;
        }

        private void MissionComplete(MissionType missionType)
        {
            if (((int)missionType - 3) % 4 != 0) { // don't increment if the player has already reached the max
                int missionSet = (int)missionType / 4;
                activeMissions[missionSet] = missionType + 1;
                scoreMultiplier += scoreMultiplierIncrement;
                PlayerPrefs.SetInt(string.Format("Mission{0}", missionSet), (int)activeMissions[missionSet]);
            }

            if (OnMissionComplete != null) {
                OnMissionComplete(missionType);
            }
        }

        public float GetScoreMultiplier()
        {
            return scoreMultiplier;
        }

        public MissionType GetMission(int mission)
        {
            return activeMissions[mission];
        }

        private void GamePaused(bool paused)
        {
            if (instantGUINotification) {
                if (paused) {
                    StopCoroutine("InGameCheckForCompletedMissions");
                } else {
                    StartCoroutine("InGameCheckForCompletedMissions");
                }
            }
        }
    }
}
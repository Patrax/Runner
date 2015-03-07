using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.GameCenter;

namespace InfiniteRunner.Game
{
    // the type of achievement earned
    public enum SocialAchievementID
    {
        ExpertRunner, ExpertCoinCollector, ExpertPlayer
    };

    /**
     * The social manager is responsible for interacting with Unity's Social API. Currently it only supports Game Center on iOS and the Mac.
     */
    public class SocialManager : MonoBehaviour
    {
        static public SocialManager instance = null;

        // callback after the user has been authenticated (can be used to enable the game center icon, for example)
        public delegate void SocialAuthenticationHandler(bool isAuthenticated);
        public event SocialAuthenticationHandler onSocialAuthentication;

        // bundle identifier for the app. must be registered within iTunes Connect
        public string bundleIdentifier;

        public string facebookGroup;
        public string twitterID;

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            if (!isUserAuthenticated())
                Social.localUser.Authenticate(ProcessAuthentication);

            MissionManager.instance.OnMissionComplete += MissionComplete;
        }

        //------------------------------------------------------------------
        // Game Center
        //------------------------------------------------------------------

        public bool isUserAuthenticated()
        {
#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_STANDALONE_OSX)
		    return Social.localUser.authenticated;
#else
            return false;
#endif
        }

        public void ShowLeaderboard()
        {
            if (isUserAuthenticated()) {
                Social.ShowLeaderboardUI();
            }
        }

        public void ShowAchievements()
        {
            if (isUserAuthenticated()) {
                Social.ShowAchievementsUI();
            }
        }

        public void RecordScore(int score)
        {
            if (isUserAuthenticated()) {
                Social.ReportScore(score, string.Format("{0}.score", bundleIdentifier), success =>
                {
                    Debug.Log(string.Format("score {0} reported {1}", score, (success ? "successfully" : "unsuccessfully")));
                });
            }
        }

        public void RecordAchievement(SocialAchievementID achievement, float progress)
        {
            if (isUserAuthenticated()) {
                Social.ReportProgress(AchievementIDtoString(achievement), progress, success =>
                {
                    Debug.Log(string.Format("Achievement {0} recorded {1}", achievement, (success ? "successfully" : "unsuccessfully")));
                });
            }
        }

        public void MissionComplete(MissionType missionType)
        {
            // award an achievement if the player finishes all of the missions of a certain type
            // the mission is finished if it is evenly divisible by 3
            if ((int)missionType % 3 == 0) {
                switch (missionType) {
                    case MissionType.ExpertRunner:
                        RecordAchievement(SocialAchievementID.ExpertRunner, 100.0f);
                        break;
                    case MissionType.ExpertCoinCollector:
                        RecordAchievement(SocialAchievementID.ExpertCoinCollector, 100.0f);
                        break;
                    case MissionType.ExpertPlayCount:
                        RecordAchievement(SocialAchievementID.ExpertPlayer, 100.0f);
                        break;
                }
            }
        }

        private string AchievementIDtoString(SocialAchievementID id)
        {
            string achievement = "";
            switch (id) {
                case SocialAchievementID.ExpertRunner:
                    achievement = string.Format("{0}.expertrunner", bundleIdentifier);
                    break;
                case SocialAchievementID.ExpertCoinCollector:
                    achievement = string.Format("{0}.expertcoincollector", bundleIdentifier);
                    break;
                case SocialAchievementID.ExpertPlayer:
                    achievement = string.Format("{0}.expertplayer", bundleIdentifier);
                    break;
            }
            return achievement;
        }

        // Callbacks
        private void ProcessAuthentication(bool success)
        {
            if (success) {
#if !UNITY_EDITOR
			    GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);
#endif
            }

            if (onSocialAuthentication != null)
                onSocialAuthentication(success);
        }

        public void OpenFacebook()
        {
            Application.OpenURL(string.Format("https://facebook.com/{0}", facebookGroup));
        }

        public void OpenTwitter()
        {
            Application.OpenURL(string.Format("https://twitter.com/{0}", twitterID));
        }
    }
}
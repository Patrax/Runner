using UnityEngine;

namespace InfiniteRunner.Game
{
    /*
     * The data manager is a singleton which manages the data across games. It will persist any permanent data such as the
     * total number of coins or power up level
     */
    public class DataManager : MonoBehaviour
    {
        static public DataManager instance;

        public float scoreMult;

        private float score;
        private int totalCoins;
        private int levelCoins;
        private int totalSecondaryCoins;
        private int levelSecondaryCoins;
        private int collisions;
        private bool hasBeenRevived;

        private int[] currentPowerupLevel;

        private GUIManager guiManager;
        private SocialManager socialManager;
        private MissionManager missionManager;
        private StaticData staticData;

        public void Awake()
        {
            instance = this;
        }

        public void Start()
        {
            guiManager = GUIManager.instance;
            socialManager = SocialManager.instance;
            missionManager = MissionManager.instance;
            staticData = StaticData.instance;

            score = 0;
            levelCoins = 0;
            levelSecondaryCoins = 0;
            collisions = 0;
            hasBeenRevived = false;
            totalCoins = PlayerPrefs.GetInt("Coins", 0);
            totalSecondaryCoins = PlayerPrefs.GetInt("SecondaryCoins", 0);

            currentPowerupLevel = new int[(int)PowerUpTypes.None];
            for (int i = 0; i < (int)PowerUpTypes.None; ++i) {
                currentPowerupLevel[i] = PlayerPrefs.GetInt(string.Format("PowerUp{0}", i), 0);
                if (currentPowerupLevel[i] == 0 && GameManager.instance.enableAllPowerUps) {
                    currentPowerupLevel[i] = 1;
                }
            }

            // first character is always available
            PurchaseCharacter(0);
        }

        public void AddToScore(float amount)
        {
            score += (amount * scoreMult);
            guiManager.SetInGameScore(Mathf.RoundToInt(score));
        }

        public int GetScore()
        {
            return GetScore(false);
        }

        public int GetScore(bool withMultiplier)
        {
            return Mathf.RoundToInt(score * (withMultiplier ? missionManager.GetScoreMultiplier() : 1));
        }

        public void ObstacleCollision()
        {
            collisions++;
        }

        public int GetCollisionCount()
        {
            return collisions;
        }

        public void AddToCoins(int amount, bool primaryCoin)
        {
            if (primaryCoin) {
                levelCoins += amount;
                guiManager.SetInGameCoinCount(levelCoins, true);
            } else {
                levelSecondaryCoins += amount;
                guiManager.SetInGameCoinCount(levelSecondaryCoins, false);
            }
        }

        public int GetLevelCoins(bool primaryCoin)
        {
            if (primaryCoin) {
                return levelCoins;
            } else {
                return levelSecondaryCoins;
            }
        }

        public void AdjustTotalCoins(int amount, bool primaryCoin)
        {
            if (primaryCoin) {
                totalCoins += amount;
                PlayerPrefs.SetInt("Coins", totalCoins);
            } else {
                totalSecondaryCoins += amount;
                PlayerPrefs.SetInt("SecondaryCoins", totalSecondaryCoins);
            }
        }

        public int GetTotalCoins(bool primaryCoin)
        {
            if (primaryCoin) {
                return totalCoins;
            } else {
                return totalSecondaryCoins;
            }
        }

        public int GetHighScore()
        {
            return PlayerPrefs.GetInt("HighScore", 0);
        }

        public int GetPlayCount()
        {
            return PlayerPrefs.GetInt("PlayCount");
        }

        public bool CanRevive()
        {
            return !hasBeenRevived;
        }

        public bool CanPurchaseRevive()
        {
            return levelSecondaryCoins + totalSecondaryCoins >= staticData.reviveSecondaryCoinCost;
        }

        public int GetReviveCost()
        {
            return staticData.reviveSecondaryCoinCost;
        }

        public void Revive()
        {
            // Use all of the level specialty coins first
            int remaining = Mathf.Min(levelSecondaryCoins, staticData.reviveSecondaryCoinCost);
            levelSecondaryCoins -= remaining;
            if (staticData.reviveSecondaryCoinCost - remaining > 0) {
                totalSecondaryCoins -= (staticData.reviveSecondaryCoinCost - remaining);
                PlayerPrefs.SetInt("SecondaryCoins", totalSecondaryCoins);
            }
            guiManager.SetInGameCoinCount(levelSecondaryCoins, false);

            // can only revive once
            hasBeenRevived = true;
        }

        public void GameOver()
        {
            // save the high score, coin count, and play count
            if (GetScore() > GetHighScore()) {
                PlayerPrefs.SetInt("HighScore", GetScore());
                socialManager.RecordScore(GetScore());
            }

            totalCoins += levelCoins;
            PlayerPrefs.SetInt("Coins", totalCoins);

            totalSecondaryCoins += levelSecondaryCoins;
            PlayerPrefs.SetInt("SecondaryCoins", totalSecondaryCoins);

            int playCount = PlayerPrefs.GetInt("PlayCount", 0);
            playCount++;
            PlayerPrefs.SetInt("PlayCount", playCount);
        }

        public void ResetValues()
        {
            score = 0;
            levelCoins = 0;
            levelSecondaryCoins = 0;
            collisions = 0;
            hasBeenRevived = false;

            guiManager.SetInGameScore(Mathf.RoundToInt(score));
            guiManager.SetInGameCoinCount(levelCoins, true);
            guiManager.SetInGameCoinCount(levelSecondaryCoins, false);
        }

        public int GetCharacterCount()
        {
            return staticData.characterCount;
        }

        public string GetCharacterTitle(int character)
        {
            return staticData.GetCharacterTitle(character);
        }

        public string GetCharacterDescription(int character)
        {
            return staticData.GetCharacterDescription(character);
        }

        public int GetCharacterCost(int character)
        {
            if (PlayerPrefs.GetInt(string.Format("CharacterPurchased{0}", character), 0) == 1)
                return -1; // -1 cost if the character is already purchased
            return staticData.GetCharacterCost(character);
        }

        public void PurchaseCharacter(int character)
        {
            PlayerPrefs.SetInt(string.Format("CharacterPurchased{0}", character), 1);
        }

        public void SetSelectedCharacter(int character)
        {
            if (PlayerPrefs.GetInt(string.Format("CharacterPurchased{0}", character), 0) == 1) {
                PlayerPrefs.SetInt("SelectedCharacter", character);
            }
        }

        public int GetSelectedCharacter()
        {
            return PlayerPrefs.GetInt("SelectedCharacter", 0);
        }

        public GameObject GetCharacterPrefab(int character)
        {
            return staticData.GetCharacterPrefab(character);
        }

        public GameObject GetChaseObjectPrefab()
        {
            return staticData.GetChaseObjectPrefab();
        }

        public string GetPowerUpTitle(PowerUpTypes powerUpType)
        {
            return staticData.GetPowerUpTitle(powerUpType);
        }

        public string GetPowerUpDescription(PowerUpTypes powerUpType)
        {
            return staticData.GetPowerUpDescription(powerUpType);
        }

        public GameObject GetPowerUpPrefab(PowerUpTypes powerUpType)
        {
            return staticData.GetPowerUpPrefab(powerUpType);
        }

        public int GetPowerUpLevel(PowerUpTypes powerUpTypes)
        {
            return currentPowerupLevel[(int)powerUpTypes];
        }

        public float GetPowerUpLength(PowerUpTypes powerUpType)
        {
            return staticData.GetPowerUpLength(powerUpType, currentPowerupLevel[(int)powerUpType]);
        }

        public int GetPowerUpCost(PowerUpTypes powerUpType)
        {
            if (currentPowerupLevel[(int)powerUpType] < staticData.GetTotalPowerUpLevels()) {
                return staticData.GetPowerUpCost(powerUpType, currentPowerupLevel[(int)powerUpType]);
            }
            return -1; // out of power up upgrades
        }

        public void UpgradePowerUp(PowerUpTypes powerUpType)
        {
            currentPowerupLevel[(int)powerUpType]++;
            PlayerPrefs.SetInt(string.Format("PowerUp{0}", (int)powerUpType), currentPowerupLevel[(int)powerUpType]);
        }

        public int GetMissionGoal(MissionType missionType)
        {
            return staticData.GetMissionGoal(missionType);
        }

        public string GetMissionDescription(MissionType missionType)
        {
            return staticData.GetMissionDescription(missionType);
        }

        public string GetMissionCompleteText(MissionType missionType)
        {
            return staticData.GetMissionCompleteText(missionType);
        }
    }
}
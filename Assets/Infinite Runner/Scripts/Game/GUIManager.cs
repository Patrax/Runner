// To prevent any NGUI scripts from being used comment out the following line:
//#define COMPILE_NGUI

using UnityEngine;
using System.Collections;
using InfiniteRunner.Player;
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
using UnityEngine.UI;
using UnityEngine.EventSystems;
#endif

namespace InfiniteRunner.Game
{
    /*
     * The GUI manager is a singleton class which manages the NGUI objects
     */
    public enum GUIState { MainMenu, InGame, EndGame, Store, Stats, Pause, Tutorial, Missions, Revive, InGameMissions, Inactive }
    public enum TutorialType { Jump, Slide, Strafe, Attack, Turn, GoodLuck }
    public class GUIManager : MonoBehaviour
    {
        static public GUIManager instance;

        public float clickDelay = 0.2f;

#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
        public bool useuGUI = false;
#endif

        public GameObject mainMenuPanel;
        public GameObject logoPanel;
        public GameObject inGameLeftPanel;
        public GameObject inGameTopPanel;
        public GameObject inGameRightPanel;
        public GameObject revivePanel;
        public GameObject endGamePanel;
        public GameObject storePanel;
        public GameObject statsPanel;
        public GameObject missionsPanel;
        public GameObject inGameMissionsPanel;
        public GameObject pausePanel;
        public GameObject tutorialPanel;

        // in game:
#if COMPILE_NGUI
        public UILabel inGameScore;
        public UILabel inGameCoins;
        public UILabel inGameSecondaryCoins;
        public UISprite inGameActivePowerUp;
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
        public Text inGameScoreText;
        public Text inGameCoinsText;
        public Text inGameSecondaryCoinsText;
        public Image inGameActivePowerUpImage;
        public Sprite[] inGamePowerUpSprites;
#endif
        public Material inGameActivePowerUpCutoffMaterial;
        public Animation[] inGamePlayAnimation;
        public string inGamePlayAnimationName;
        private WaitForSeconds inGamePowerUpProgressWaitForSeconds;
        private CoroutineData inGamePowerUpData;
        private bool powerUpActive;
        private float lastClickTime;

        // pause:
#if COMPILE_NGUI
        public UILabel pauseScore;
        public UILabel pauseCoins;
        public UILabel pauseSecondaryCoins;
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
        public Text pauseScoreText;
        public Text pauseCoinsText;
        public Text pauseSecondaryCoinsText;
#endif

        // revive:
#if COMPILE_NGUI
        public UILabel reviveDescription;
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
        public Text reviveDescriptionText;
#endif
        public Animation revivePlayAnimation;
        public string revivePlayAnimationName;
        public GameObject reviveYesButton;

        // end game:
#if COMPILE_NGUI
        public UILabel endGameScore;
        public UILabel endGameCoins;
        public UILabel endGameMultiplier;
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
        public Text endGameScoreText;
        public Text endGameCoinsText;
        public Text endGameMultiplierText;
#endif
        public Animation endGamePlayAnimation;
        public string endGamePlayAnimationName;

        // store:
        public GameObject storeBackToMainMenuButton;
        public GameObject storeBackToEndGameButton;
#if COMPILE_NGUI
        public UILabel storePowerUpSelectionButton;
        public UILabel storeTitle;
        public UILabel storeDescription;
        public UILabel storeCoins;
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
        public Text storePowerUpSelectionButtonText;
        public Text storeTitleText;
        public Text storeDescriptionText;
        public Text storeCoinsText;
#endif
        public GameObject storeBuyButton;
        private bool storeSelectingPowerUp;
        private int storeItemIndex;

        public Transform storePowerUpPreviewTransform;
        public Transform storeCharacterPreviewTransform;
        private GameObject storeItemPreview;

        // stats:
#if COMPILE_NGUI
        public UILabel statsHighScore;
        public UILabel statsCoins;
        public UILabel statsSecondaryCoins;
        public UILabel statsPlayCount;
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
        public Text statsHighScoreText;
        public Text statsCoinsText;
        public Text statsSecondaryCoinsText;
        public Text statsPlayCountText;
#endif

        // tutorial:
#if COMPILE_NGUI
        public UILabel tutorialLabel;
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
        public Text tutorialLabelText;
#endif

        // missions:
        public GameObject missionsBackToMainMenuButton;
        public GameObject missionsBackToEndGameButton;
#if COMPILE_NGUI
        public UILabel missionsScoreMultiplier;
        public UILabel missionsActiveMission1;
        public UILabel missionsActiveMission2;
        public UILabel missionsActiveMission3;
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
        public Text missionsScoreMultiplierText;
        public Text missionsActiveMission1Text;
        public Text missionsActiveMission2Text;
        public Text missionsActiveMission3Text;
#endif

        // in game missions:
#if COMPILE_NGUI
        public UILabel inGameMissionsMissionComplete;
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
        public Text inGameMissionsMissionCompleteText;
#endif
        public Animation inGameMissionsPlayAnimation;
        public string inGameMissionsPlayAnimationName;

        private GUIState guiState;
        private bool tutorialShown = false;

        private GameManager gameManager;
        private DataManager dataManager;
        private MissionManager missionManager;
        private CoinGUICollection coinGUICollection;

        public void Awake()
        {
            instance = this;
        }

        public void Start()
        {
            gameManager = GameManager.instance;
            dataManager = DataManager.instance;
            missionManager = MissionManager.instance;
            coinGUICollection = CoinGUICollection.instance;

            guiState = GUIState.MainMenu;
            inGamePowerUpData = new CoroutineData();
            gameManager.OnPauseGame += GamePaused;
            lastClickTime = -clickDelay;

            // hide everything except the main menu
            mainMenuPanel.SetActive(true);
            logoPanel.SetActive(true);
            inGameLeftPanel.SetActive(false);
            inGameTopPanel.SetActive(false);
            inGameRightPanel.SetActive(false);
            if (revivePanel != null)
                revivePanel.SetActive(false);
            endGamePanel.SetActive(false);
            storePanel.SetActive(false);
            statsPanel.SetActive(false);
            missionsPanel.SetActive(false);
            if (inGameMissionsPanel != null)
                inGameMissionsPanel.SetActive(false);
            pausePanel.SetActive(false);
            tutorialPanel.SetActive(false);
        }

        public bool CanClick()
        {
            bool canClick = lastClickTime + clickDelay < Time.time;
            lastClickTime = Time.time;
            return canClick;
        }

        public void ShowGUI(GUIState state)
        {
            switch (state) {
                case GUIState.InGame:
                    if (tutorialShown) {
                        tutorialPanel.SetActive(true);
                    }
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
                    if (useuGUI) {
                        inGameActivePowerUpImage.gameObject.SetActive(powerUpActive);
                        EventSystem.current.SetSelectedGameObject(null);
                    } else {
#endif    
#if COMPILE_NGUI
                        inGameActivePowerUp.gameObject.SetActive(powerUpActive);
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
                    }
#endif
                    break;
                case GUIState.EndGame:
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
                    if (useuGUI) {
                        endGameScoreText.text = dataManager.GetScore(true).ToString();
                        endGameCoinsText.text = dataManager.GetLevelCoins(true).ToString();
                        endGameMultiplierText.text = string.Format("{0}x", missionManager.GetScoreMultiplier());
                    } else {
#endif
#if COMPILE_NGUI
                        endGameScore.text = dataManager.GetScore(true).ToString();
                        endGameCoins.text = dataManager.GetLevelCoins(true).ToString();
                        endGameMultiplier.text = string.Format("{0}x", missionManager.GetScoreMultiplier());
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
                    }
#endif

                    // only need to show the animation if we are coming from in game or revive
                    if (guiState == GUIState.InGame || guiState == GUIState.Revive) {
                        endGamePanel.SetActive(true);

                        endGamePlayAnimation.enabled = true;
                        endGamePlayAnimation.Stop();
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
                        if (useuGUI) {
                            endGamePlayAnimation[endGamePlayAnimationName].speed = -1;
                            endGamePlayAnimation[endGamePlayAnimationName].time = endGamePlayAnimation[endGamePlayAnimationName].length;
                        } else {
#endif
#if COMPILE_NGUI
                            endGamePlayAnimation[endGamePlayAnimationName].speed = 1;
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
                        }
#endif
                        endGamePlayAnimation.Play(endGamePlayAnimationName);
                        for (int i = 0; i < inGamePlayAnimation.Length; ++i) {
                            inGamePlayAnimation[i].Stop();
                            inGamePlayAnimation[i][inGamePlayAnimationName].speed = 1;
                            inGamePlayAnimation[i].enabled = true;
                            inGamePlayAnimation[i].Play(inGamePlayAnimationName);
                        }
#if COMPILE_NGUI
                        // NGUI 2 uses UIButtonPlayAnimation. NGUI 3 uses UIPlayAnimation. When the NGUI 3 version of the play animation activates it automatically disables
                        // the animation component. We don't want that so we are just going to disable the NGUI component.
                        System.Type playAnimationType = System.Type.GetType("UIPlayAnimation, Assembly-CSharp");
                        if (playAnimationType != null) {
                            var playAnimations = endGamePanel.GetComponentsInChildren(playAnimationType);
                            for (int i = 0; i < playAnimations.Length; ++i) {
                                ((MonoBehaviour)playAnimations[i]).enabled = false;
                            }
                            // enable the buttons again after the animation is done playing
                            StartCoroutine(EnableComponents(endGamePlayAnimation[endGamePlayAnimationName].length, playAnimations));
                        }
#endif
                    }
                    break;

                case GUIState.Store:
                    // go back to the correct menu that we came from
                    if (guiState == GUIState.MainMenu) {
                        storeBackToEndGameButton.SetActive(false);
                        storeBackToMainMenuButton.SetActive(true);
                    } else if (guiState == GUIState.EndGame) {
                        storeBackToMainMenuButton.SetActive(false);
                        storeBackToEndGameButton.SetActive(true);
                    }
                    storeSelectingPowerUp = false;
                    storeItemIndex = dataManager.GetSelectedCharacter();
                    RefreshStoreGUI();
                    RefreshStoreItem();
                    break;

                case GUIState.Pause:
                    if (tutorialShown) {
                        tutorialPanel.SetActive(false);
                    }
                    if (inGameMissionsPanel.activeSelf) {
                        // speed up the animation so the panel will disappear quicker
                        inGameMissionsPlayAnimation[inGameMissionsPlayAnimationName].speed = 2;
                    }
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
                    if (useuGUI) {
                        pauseScoreText.text = dataManager.GetScore().ToString();
                        pauseCoinsText.text = (dataManager.GetLevelCoins(true) + coinGUICollection.GetAnimatingCoins()).ToString();
                        pauseSecondaryCoinsText.text = dataManager.GetLevelCoins(false).ToString();
                    } else {
#endif
#if COMPILE_NGUI
                        pauseScore.text = dataManager.GetScore().ToString();
                        pauseCoins.text = (dataManager.GetLevelCoins(true) + coinGUICollection.GetAnimatingCoins()).ToString();
                        pauseSecondaryCoins.text = dataManager.GetLevelCoins(false).ToString();
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
                    }
#endif
                    break;

                case GUIState.Stats:
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
                    if (useuGUI) {
                        statsHighScoreText.text = dataManager.GetHighScore().ToString();
                        statsCoinsText.text = dataManager.GetTotalCoins(true).ToString();
                        statsSecondaryCoinsText.text = dataManager.GetTotalCoins(false).ToString();
                        statsPlayCountText.text = dataManager.GetPlayCount().ToString();
                    } else {
#endif
#if COMPILE_NGUI
                        statsHighScore.text = dataManager.GetHighScore().ToString();
                        statsCoins.text = dataManager.GetTotalCoins(true).ToString();
                        statsSecondaryCoins.text = dataManager.GetTotalCoins(false).ToString();
                        statsPlayCount.text = dataManager.GetPlayCount().ToString();
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
                    }
#endif
                    break;

                case GUIState.Missions:
                    if (guiState == GUIState.MainMenu) {
                        missionsBackToEndGameButton.SetActive(false);
                        missionsBackToMainMenuButton.SetActive(true);
                    } else { // coming from GUIState.EndGame
                        missionsBackToMainMenuButton.SetActive(false);
                        missionsBackToEndGameButton.SetActive(true);
                    }
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
                    if (useuGUI) {
                        missionsScoreMultiplierText.text = string.Format("{0}x", missionManager.GetScoreMultiplier());
                        missionsActiveMission1Text.text = dataManager.GetMissionDescription(missionManager.GetMission(0));
                        missionsActiveMission2Text.text = dataManager.GetMissionDescription(missionManager.GetMission(1));
                        missionsActiveMission3Text.text = dataManager.GetMissionDescription(missionManager.GetMission(2));
                    } else {
#endif
#if COMPILE_NGUI
                        missionsScoreMultiplier.text = string.Format("{0}x", missionManager.GetScoreMultiplier());
                        missionsActiveMission1.text = dataManager.GetMissionDescription(missionManager.GetMission(0));
                        missionsActiveMission2.text = dataManager.GetMissionDescription(missionManager.GetMission(1));
                        missionsActiveMission3.text = dataManager.GetMissionDescription(missionManager.GetMission(2));
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
                    }
#endif
                    break;

                case GUIState.Revive:
                    revivePanel.SetActive(true);
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
                    if (useuGUI) {
                        reviveDescriptionText.text = "Do you want to use " + dataManager.GetReviveCost() + " diamonds to revive?\n\nYou have " + (dataManager.GetTotalCoins(false) + dataManager.GetLevelCoins(false)).ToString() + " diamonds.";
                    } else {
#endif
#if COMPILE_NGUI
                        reviveDescription.text = "Do you want to use " + dataManager.GetReviveCost() + " diamonds to revive?\n\nYou have " + (dataManager.GetTotalCoins(false) + dataManager.GetLevelCoins(false)).ToString() + " diamonds.";

                        // Deactivate the yes animation if the player doesn't have enough coins. NGUI3 automatically starts with it deactivated
                        if (!dataManager.CanPurchaseRevive()) {
                            System.Type animationType = System.Type.GetType("UIButtonPlayAnimation, Assembly-CSharp");
                            if (animationType != null) {
                                var playAnimation = reviveYesButton.GetComponent(animationType);
                                ((MonoBehaviour)playAnimation).enabled = false;
                            }
                        }
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
                    }
#endif
                    revivePlayAnimation.enabled = true;
                    revivePlayAnimation.Stop();
                    revivePlayAnimation[revivePlayAnimationName].speed = 1;
                    revivePlayAnimation.Play(revivePlayAnimationName);
                    break;
            }

            guiState = state;
        }

        private IEnumerator EnableComponents(float length, Component[] components)
        {
            yield return new WaitForSeconds(length);

            for (int i = 0; i < components.Length; ++i) {
                ((MonoBehaviour)components[i]).enabled = true;
            }
        }

        public void SetInGameScore(int score)
        {
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
            if (useuGUI) {
                inGameScoreText.text = score.ToString();
            } else {
#endif
#if COMPILE_NGUI
                inGameScore.text = score.ToString();
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
            }
#endif
        }

        public void SetInGameCoinCount(int coins, bool primaryCoins)
        {
            if (primaryCoins) {
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
                if (useuGUI) {
                    inGameCoinsText.text = coins.ToString();
                } else {
#endif
#if COMPILE_NGUI
                    inGameCoins.text = coins.ToString();
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
                }
#endif
            } else {
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
                if (useuGUI) {
                    inGameSecondaryCoinsText.text = coins.ToString();
                } else {
#endif
#if COMPILE_NGUI
                    inGameSecondaryCoins.text = coins.ToString();
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
                }
#endif
            }
        }

        public void ActivatePowerUp(PowerUpTypes powerUpType, bool active, float length)
        {
            if (active) {
                inGameActivePowerUpCutoffMaterial.SetFloat("_Cutoff", 0.0f);
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
                if (useuGUI) {
                    inGameActivePowerUpImage.sprite = inGamePowerUpSprites[(int)powerUpType];
                } else {
#endif
#if COMPILE_NGUI
                    inGameActivePowerUp.spriteName = powerUpType.ToString();
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
                }
#endif

                if (inGamePowerUpProgressWaitForSeconds == null) {
                    inGamePowerUpProgressWaitForSeconds = new WaitForSeconds(0.05f);
                }

                inGamePowerUpData.duration = length;
                StartCoroutine("UpdatePowerUpProgress");
            } else {
                StopCoroutine("UpdatePowerUpProgress");
            }
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
            if (useuGUI) {
                inGameActivePowerUpImage.gameObject.SetActive(active);
            } else {
#endif
#if COMPILE_NGUI
                inGameActivePowerUp.gameObject.SetActive(active);
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
            }
#endif
            powerUpActive = active;
        }

        private IEnumerator UpdatePowerUpProgress()
        {
            inGamePowerUpData.startTime = Time.time;
            float cutoff = inGameActivePowerUpCutoffMaterial.GetFloat("_Cutoff");
            float cutoffStep = 0.05f / inGamePowerUpData.duration;
            while (cutoff < 1) {
                cutoff += cutoffStep;
                inGameActivePowerUpCutoffMaterial.SetFloat("_Cutoff", cutoff);
                yield return inGamePowerUpProgressWaitForSeconds;
            }
        }

        public void ShowInGameMissionCompletePanel(string text)
        {
            // if a completed mission is already being shown don't start another one.
            if (inGameMissionsPanel.activeSelf) {
                return;
            }

            inGameMissionsPanel.SetActive(true);
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
            if (useuGUI) {
                inGameMissionsMissionCompleteText.text = text;
            } else {
#endif
#if COMPILE_NGUI
                inGameMissionsMissionComplete.text = text;
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
            }
#endif

            inGameMissionsPlayAnimation.enabled = true;
            inGameMissionsPlayAnimation.Stop();
            inGameMissionsPlayAnimation[inGameMissionsPlayAnimationName].speed = 1;
            inGameMissionsPlayAnimation.Play(inGameMissionsPlayAnimationName);
        }

        // This function is called when the in game mission panel hides after showing the completed mission (this is done through an animation event)
        // The function is called through a coroutine because when the panel is disabled NGUI destroys an object which must be destroyed through
        // the regular update loop rather then an animation event
        public IEnumerator InGameMissionPanelHidden()
        {
            yield return null;
            inGameMissionsPanel.SetActive(false);
        }

        public void ShowTutorial(bool show, TutorialType tutorial)
        {
            tutorialShown = show;
            tutorialPanel.SetActive(show);
            string text = "";
            if (show) {
                switch (tutorial) {
                    case TutorialType.Jump:
#if (UNITY_IPHONE || UNITY_ANDROID || UNITY_BLACKBERRY || UNITY_WP8)
                        text = "Swipe up to jump";
#else
                        text = "Press the up arrow\nto jump";
#endif
                        break;
                    case TutorialType.Slide:
#if (UNITY_IPHONE || UNITY_ANDROID || UNITY_BLACKBERRY || UNITY_WP8)
                        text = "Swipe down to slide";
#else
                        text = "Press the down arrow\nto slide";
#endif
                        break;
                    case TutorialType.Strafe:
#if (UNITY_IPHONE || UNITY_ANDROID || UNITY_BLACKBERRY || UNITY_WP8)
                        text = "Tilt to turn";
#else
                        text = "Left and right arrows\nwill change slots";
#endif
                        break;
                    case TutorialType.Attack:
#if (UNITY_IPHONE || UNITY_ANDROID || UNITY_BLACKBERRY || UNITY_WP8)
                        text = "Tap to attack";
#else
                        text = "Attack with the\nleft mouse button";
#endif
                        break;
                    case TutorialType.Turn:
#if (UNITY_IPHONE || UNITY_ANDROID || UNITY_BLACKBERRY || UNITY_WP8)
                        text = "Swipe left or right\nto turn";
#else
                        text = "Left and right arrows\nwill also turn";
#endif
                        break;
                    case TutorialType.GoodLuck:
                        text = "Good luck!";
                        break;
                }
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
                if (useuGUI) {
                    tutorialLabelText.text = text;
                } else {
#endif
#if COMPILE_NGUI
                    tutorialLabel.text = text;
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
                }
#endif
            }
        }

        // something has changed (item change, purchase, etc). Need to refresh the gui text
        public void RefreshStoreGUI()
        {
            int cost = -1;
            if (storeSelectingPowerUp) {
                PowerUpTypes powerUp = (PowerUpTypes)storeItemIndex;
                cost = dataManager.GetPowerUpCost(powerUp);
                storeBuyButton.SetActive(cost != -1);
                
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
                if (useuGUI) {
                    storeTitleText.text = dataManager.GetPowerUpTitle(powerUp);
                    storeDescriptionText.text = dataManager.GetPowerUpDescription(powerUp);
                    storePowerUpSelectionButtonText.text = "Characters";
                } else {
#endif
#if COMPILE_NGUI
                    storeTitle.text = dataManager.GetPowerUpTitle(powerUp);
                    storeDescription.text = dataManager.GetPowerUpDescription(powerUp);
                    storePowerUpSelectionButton.text = "Characters";
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
                }
#endif
            } else { // characters
                cost = dataManager.GetCharacterCost(storeItemIndex);
                storeBuyButton.SetActive(cost != -1);

#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
                if (useuGUI) {
                    storeTitleText.text = dataManager.GetCharacterTitle(storeItemIndex);
                    storeDescriptionText.text = dataManager.GetCharacterDescription(storeItemIndex);
                    storePowerUpSelectionButtonText.text = "Power Ups";
                } else {
#endif
#if COMPILE_NGUI
                    storeTitle.text = dataManager.GetCharacterTitle(storeItemIndex);
                    storeDescription.text = dataManager.GetCharacterDescription(storeItemIndex);
                    storePowerUpSelectionButton.text = "Power Ups";
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
                }
#endif
            }
            
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
            if (useuGUI) {
                storeCoinsText.text = string.Format("{0}  ({1} Coins Available)", (cost == -1 ? "Purchased" : string.Format("Cost: {0}", cost.ToString())), dataManager.GetTotalCoins(true));
            } else {
#endif
#if COMPILE_NGUI
                storeCoins.text = string.Format("{0}  ({1} Coins Available)", (cost == -1 ? "Purchased" : string.Format("Cost: {0}", cost.ToString())), dataManager.GetTotalCoins(true));
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
            }
#endif
        }

        public void TogglePowerUpVisiblity()
        {
            storeSelectingPowerUp = !storeSelectingPowerUp;
            storeItemIndex = 0;
            RefreshStoreGUI();
            RefreshStoreItem();
        }

        // rotate through showing a preview of the item
        public void RotateStoreItem(bool next)
        {
            if (storeSelectingPowerUp) {
                storeItemIndex = (storeItemIndex + (next ? 1 : -1)) % (int)PowerUpTypes.None;
                if (storeItemIndex < 0) {
                    storeItemIndex = (int)PowerUpTypes.None - 1;
                }
            } else {
                storeItemIndex = (storeItemIndex + (next ? 1 : -1)) % dataManager.GetCharacterCount();
                if (storeItemIndex < 0) {
                    storeItemIndex = dataManager.GetCharacterCount() - 1;
                }
                GameManager.instance.SelectCharacter(storeItemIndex);
            }
            RefreshStoreGUI();
            RefreshStoreItem();
        }

        // show a preview of the new item
        private void RefreshStoreItem()
        {
            if (storeItemPreview != null) {
                Destroy(storeItemPreview);
            }
            Transform activePreviewTransform = null;
            if (storeSelectingPowerUp) {
                storeItemPreview = GameObject.Instantiate(dataManager.GetPowerUpPrefab((PowerUpTypes)storeItemIndex)) as GameObject;
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
                if (useuGUI) {
                    storeItemPreview.transform.parent = storePowerUpPreviewTransform.parent;
                }
#endif
                storeItemPreview.transform.localScale = storePowerUpPreviewTransform.localScale;
                activePreviewTransform = storePowerUpPreviewTransform;
            } else {
                // don't want to override PlayerController.instance when the new player controller gets instantiated
                PlayerController playerController = PlayerController.instance;
                storeItemPreview = GameObject.Instantiate(dataManager.GetCharacterPrefab(storeItemIndex)) as GameObject;
                storeItemPreview.GetComponent<PlayerController>().ActivatePowerUp(PowerUpTypes.CoinMagnet, false);
                PlayerController.instance = playerController;
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
                if (useuGUI) {
                    storeItemPreview.transform.parent = storeCharacterPreviewTransform.parent;
                }
#endif
                storeItemPreview.transform.localScale = storeCharacterPreviewTransform.localScale; // set the character to be the same scale as the preview transform
                activePreviewTransform = storeCharacterPreviewTransform;
            }
            storeItemPreview.transform.position = activePreviewTransform.position;
            storeItemPreview.transform.rotation = activePreviewTransform.rotation;


            // disable all of the componenets to prevent any scripts from running. Also set the layer so only the GUI camera can see it
            int layer = 0;
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
            if (useuGUI) {
                layer = LayerMask.NameToLayer("UI");
                storeItemPreview.AddComponent<CanvasRenderer>();
            } else {
#endif
#if COMPILE_NGUI
                layer = LayerMask.NameToLayer("GUI");
#endif
#if !(UNITY_4_3 || UNITY_4_4 || UNITY_4_5)
            }
#endif
            storeItemPreview.gameObject.layer = layer;
            ChangeLayers(storeItemPreview.transform, layer);
            foreach (Behaviour childCompnent in storeItemPreview.GetComponentsInChildren<Behaviour>()) {
                childCompnent.enabled = false;
            }
            if (storeItemPreview.GetComponent<Rigidbody>()) {
                storeItemPreview.GetComponent<Rigidbody>().isKinematic = true;
            }
            PlayerAnimation playerAnimation = null;
            if ((playerAnimation = storeItemPreview.GetComponent<PlayerAnimation>()) != null) {
                if (playerAnimation.useMecanim) {
                    storeItemPreview.GetComponent<Animator>().enabled = true;
                    storeItemPreview.GetComponent<Animator>().SetFloat("Speed", 0);
                } else {
                    storeItemPreview.GetComponent<Animation>().enabled = true;
                    playerAnimation.Idle();
                }
            }
        }

        // recursively change all of the child layers
        private void ChangeLayers(Transform obj, int layer)
        {
            foreach (Transform child in obj) {
                child.gameObject.layer = layer;
                ChangeLayers(child, layer);
            }
        }

        public void PurchaseStoreItem()
        {
            if (storeSelectingPowerUp) {
                gameManager.UpgradePowerUp((PowerUpTypes)storeItemIndex);
            } else {
                gameManager.PurchaseCharacter(storeItemIndex);
                gameManager.SelectCharacter(storeItemIndex);
            }
            RefreshStoreGUI();
        }

        public void RemoveStoreItemPreview()
        {
            if (storeItemPreview != null) {
                Destroy(storeItemPreview);
            }
        }

        public void GamePaused(bool paused)
        {
            if (powerUpActive) {
                if (paused) {
                    StopCoroutine("UpdatePowerUpProgress");
                    inGamePowerUpData.CalcuateNewDuration();
                } else {
                    StartCoroutine("UpdatePowerUpProgress");
                }
            }
        }

        public void GameOver()
        {
            if (tutorialPanel.activeSelf) {
                tutorialPanel.SetActive(false);
            }
            if (inGameMissionsPanel.activeSelf) {
                // speed up the animation so the panel will disappear quicker
                inGameMissionsPlayAnimation[inGameMissionsPlayAnimationName].speed = 2;
            }
        }
    }
}
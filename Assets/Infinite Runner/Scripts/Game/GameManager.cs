using UnityEngine;
using System.Collections;
using InfiniteRunner.InfiniteGenerator;
using InfiniteRunner.InfiniteObjects;
using InfiniteRunner.Player;

namespace InfiniteRunner.Game
{
    /*
     * CoroutineData is a quick class used to save off any timinig information needed when the game is paused.
     */
    public class CoroutineData
    {
        public float startTime;
        public float duration;
        public CoroutineData() { startTime = 0; duration = 0; }
        public void CalcuateNewDuration() { duration -= Time.time - startTime; }
    }

    /*
     * The game manager is a singleton which manages the game state. It coordinates with all of the other classes to tell them
     * when to start different game states such as pausing the game or ending the game.
     */
    public enum GameOverType { Wall, JumpObstacle, DuckObstacle, Pit, Quit };
    public class GameManager : MonoBehaviour
    {
        static public GameManager instance;

        public delegate void GenericHandler();
        public event GenericHandler OnPlayerSpawn;
        public event GenericHandler OnStartGame;
        public delegate void PauseHandler(bool paused);
        public event PauseHandler OnPauseGame;

        public bool godMode;
        public bool enableAllPowerUps;
        public bool showTutorial;
        public bool runInBackground;
        public bool allowRevives;

        private int activeCharacter;
        private GameObject character;

        private bool gamePaused;
        private bool gameActive;

        private InfiniteObjectGenerator infiniteObjectGenerator;
        private PlayerController playerController;
        private GUIManager guiManager;
        private DataManager dataManager;
        private AudioManager audioManager;
        private PowerUpManager powerUpManager;
        private MissionManager missionManager;
        private InputController inputController;
        private ChaseController chaseController;
        private CameraController cameraController;
        private CoinGUICollection coinGUICollection;

        public void Awake()
        {
            instance = this;
        }

        public void Start()
        {
            infiniteObjectGenerator = InfiniteObjectGenerator.instance;
            guiManager = GUIManager.instance;
            dataManager = DataManager.instance;
            audioManager = AudioManager.instance;
            powerUpManager = PowerUpManager.instance;
            missionManager = MissionManager.instance;
            inputController = InputController.instance;
            cameraController = CameraController.instance;
            coinGUICollection = CoinGUICollection.instance;

            Application.runInBackground = runInBackground;
            activeCharacter = -1;
            SpawnCharacter();
            SpawnChaseObject();
        }

        private void SpawnCharacter()
        {
            if (activeCharacter == dataManager.GetSelectedCharacter()) {
                return;
            }

            if (character != null) {
                Destroy(character);
            }

            activeCharacter = dataManager.GetSelectedCharacter();
            character = GameObject.Instantiate(dataManager.GetCharacterPrefab(activeCharacter)) as GameObject;
            playerController = PlayerController.instance;
            playerController.Init();

            if (OnPlayerSpawn != null) {
                OnPlayerSpawn();
            }
        }

        private void SpawnChaseObject()
        {
            GameObject prefab;
            if ((prefab = dataManager.GetChaseObjectPrefab()) != null) {
                chaseController = (GameObject.Instantiate(prefab) as GameObject).GetComponent<ChaseController>();
            }
        }

        public void StartGame(bool fromRestart)
        {
            gameActive = true;
            inputController.StartGame();
            guiManager.ShowGUI(GUIState.InGame);
            audioManager.PlayBackgroundMusic(true);
            cameraController.StartGame(fromRestart);
            playerController.StartGame();
            if (OnStartGame != null) {
                OnStartGame();
            }
        }

        public bool IsGameActive()
        {
            return gameActive;
        }

        public void ToggleTutorial()
        {
            showTutorial = !showTutorial;
            infiniteObjectGenerator.ResetValues();
            if (showTutorial) {
                infiniteObjectGenerator.ShowStartupObjects(true);
            } else {
                // show the startup objects if there are any
                if (!infiniteObjectGenerator.ShowStartupObjects(false))
                    infiniteObjectGenerator.SpawnObjectRun(false);
            }
            infiniteObjectGenerator.ReadyFromReset();
        }

        public void ObstacleCollision(ObstacleObject obstacle, Vector3 position)
        {
            if (!powerUpManager.IsPowerUpActive(PowerUpTypes.Invincibility) && !powerUpManager.IsPowerUpActive(PowerUpTypes.SpeedIncrease) && !playerController.WithinReviveGracePeriod() &&
                !godMode && gameActive) {
                playerController.ObstacleCollision(obstacle.GetTransform(), position);
                dataManager.ObstacleCollision();
                if (dataManager.GetCollisionCount() == playerController.maxCollisions) {
                    GameOver(obstacle.isJump ? GameOverType.JumpObstacle : GameOverType.DuckObstacle, true);
                } else {
                    // the chase object will end the game
                    if (playerController.maxCollisions == 0 && chaseController != null) {
                        if (chaseController.IsVisible()) {
                            GameOver(obstacle.isJump ? GameOverType.JumpObstacle : GameOverType.DuckObstacle, true);
                        } else {
                            chaseController.Approach();
                            audioManager.PlaySoundEffect(SoundEffects.ObstacleCollisionSoundEffect);
                        }
                    } else {
                        // have the chase object approach the character when the collision count gets close
                        if (chaseController != null && dataManager.GetCollisionCount() == playerController.maxCollisions - 1) {
                            chaseController.Approach();
                        }
                        audioManager.PlaySoundEffect(SoundEffects.ObstacleCollisionSoundEffect);
                    }
                }
            }
        }

        // initial collection is true when the player first collects a coin. It will be false when the coin is done animating to the coin element on the GUI
        // returns the value of the coin with the double coin power up considered
        public int CoinCollected()
        {
            int coinValue = (powerUpManager.IsPowerUpActive(PowerUpTypes.DoubleCoin) ? 2 : 1);
            audioManager.PlaySoundEffect(SoundEffects.CoinSoundEffect);
            return coinValue;
        }

        public void CoinCollected(int coinValue, bool primaryCoin)
        {
            dataManager.AddToCoins(coinValue, primaryCoin);
        }

        public void ActivatePowerUp(PowerUpTypes powerUpType, bool activate)
        {
            if (activate) {
                // deactivate the current power up (if a power up is active) and activate the new one
                powerUpManager.DeactivatePowerUp();
                powerUpManager.ActivatePowerUp(powerUpType);
                audioManager.PlaySoundEffect(SoundEffects.PowerUpSoundEffect);
            }
            playerController.ActivatePowerUp(powerUpType, activate);
            guiManager.ActivatePowerUp(powerUpType, activate, dataManager.GetPowerUpLength(powerUpType));
        }

        public void GameOver(GameOverType gameOverType, bool waitForFrame)
        {
            if (!gameActive && waitForFrame)
                return;
            gameActive = false;

            if (waitForFrame) {
                // mecanim doesn't trigger the event if we wait until the frame is over
                playerController.GameOver(gameOverType);
                StartCoroutine(WaitForFrameGameOver(gameOverType));
            } else {
                inputController.GameOver();
                // Mission Manager's gameOver must be called before the Data Manager's gameOver so the Data Manager can grab the 
                // score multiplier from the Mission Manager to determine the final score
                missionManager.GameOver();
                coinGUICollection.GameOver();
                // Only call game over on the DataManager if there is no chance a revive is going to happen. We don't want to forget
                // the values stored in the data manager if a revive does happen
                if (!allowRevives || !dataManager.CanRevive() || !dataManager.CanPurchaseRevive())
                    dataManager.GameOver();
                if (playerController.enabled)
                    playerController.GameOver(gameOverType);
                if (chaseController != null)
                    chaseController.GameOver(gameOverType);
                audioManager.PlayBackgroundMusic(false);
                if (gameOverType != GameOverType.Quit)
                    audioManager.PlaySoundEffect(SoundEffects.GameOverSoundEffect);
                guiManager.GameOver();
                cameraController.GameOver(gameOverType);
            }
        }

        // Game over may be called from a trigger so wait for the physics loop to end
        private IEnumerator WaitForFrameGameOver(GameOverType gameOverType)
        {
            yield return new WaitForEndOfFrame();

            GameOver(gameOverType, false);

            // Wait a second for the ending animations to play
            yield return new WaitForSeconds(1.0f);

            // Show the revive panel if respawns are enabled
            if (allowRevives && dataManager.CanRevive() && dataManager.CanPurchaseRevive()) {
                guiManager.ShowGUI(GUIState.Revive);
            } else {
                guiManager.ShowGUI(GUIState.EndGame);
            }
        }

        public void TryRevive()
        {
            if (!dataManager.CanPurchaseRevive()) {
                return;
            }

            StartCoroutine(Revive());
        }

        private IEnumerator Revive()
        {
            dataManager.Revive();
            infiniteObjectGenerator.PrepareForRevive(false);
            cameraController.enabled = false;
            playerController.ResetValues(true);
            Transform playerTransform = playerController.transform;

            // Fire a raycast to determine where the paltform is. If a platform isn't directly under the character then move the platforms forward a small amount
            RaycastHit hit;
            int retryCount = 0;
            if (playerTransform.forward != infiniteObjectGenerator.GetMoveDirection()) {
                // reposition the player to be in the center of the platform
                playerController.UpdateForwardVector(infiniteObjectGenerator.GetMoveDirection());
            }
            Vector3 playerPosition = playerTransform.position;
            while (retryCount < 5) { // give up after 5 tries to prevent an infinite loop
                yield return new WaitForEndOfFrame();
                if (Physics.Raycast(playerPosition + 10000 * Vector3.up, Vector3.down, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Platform"))) {
                    playerPosition.y = hit.point.y + playerController.GetComponent<CapsuleCollider>().center.y + 0.5f;
                    break;
                }
                infiniteObjectGenerator.PrepareForRevive(true);
                retryCount++;
            }

            // the position of the player has been determined. Reset the managers/controllers for a continued game
            powerUpManager.ResetValues();
            cameraController.ResetValues();
            if (chaseController != null)
                chaseController.Reset(true);
            cameraController.enabled = true;

            // Face the same direction as the path
            playerTransform.position = playerPosition;
            if (chaseController != null)
                chaseController.transform.forward = playerTransform.forward;
            StartGame(true);
        }

        public void RestartGame(bool start)
        {
            if (gamePaused) {
                if (OnPauseGame != null)
                    OnPauseGame(false);
                GameOver(GameOverType.Quit, false);
            }

            dataManager.ResetValues();
            infiniteObjectGenerator.ResetValues();
            powerUpManager.ResetValues();
            playerController.ResetValues(false);
            cameraController.ResetValues();
            if (chaseController != null)
                chaseController.Reset(false);
            if (showTutorial) {
                infiniteObjectGenerator.ShowStartupObjects(true);
            } else {
                // show the startup objects if there are any
                if (!infiniteObjectGenerator.ShowStartupObjects(false))
                    infiniteObjectGenerator.SpawnObjectRun(false);
            }
            infiniteObjectGenerator.ReadyFromReset();

            if (start)
                StartGame(true);
        }

        public void BackToMainMenu(bool restart)
        {
            if (gamePaused) {
                if (OnPauseGame != null)
                    OnPauseGame(false);
                gamePaused = false;
                GameOver(GameOverType.Quit, false);
            }

            if (restart)
                RestartGame(false);
            guiManager.ShowGUI(GUIState.MainMenu);
        }

        // activate/deactivate the character when going into the store. The GUIManager will manage the preview
        public void ShowStore(bool show)
        {
            // ensure the correct character is used
            if (!show) {
                SpawnCharacter();
            }
            InfiniteRunnerStarterPackUtility.ActiveRecursively(character.transform, !show);
        }

        public void PauseGame(bool pause)
        {
            guiManager.ShowGUI(pause ? GUIState.Pause : GUIState.InGame);
            audioManager.PlayBackgroundMusic(!pause);
            if (OnPauseGame != null)
                OnPauseGame(pause);
            inputController.enabled = !pause;
            gamePaused = pause;
        }

        public void UpgradePowerUp(PowerUpTypes powerUpType)
        {
            // Can't upgrade if the player can't afford the power up
            int cost = dataManager.GetPowerUpCost(powerUpType);
            if (dataManager.GetTotalCoins(true) < cost) {
                return;
            }
            dataManager.UpgradePowerUp(powerUpType);
            dataManager.AdjustTotalCoins(-cost, true);
        }

        public void SelectCharacter(int character)
        {
            int characterCost = dataManager.GetCharacterCost(character);
            if (characterCost == -1) { // can only select a character if it has been purchased
                if (dataManager.GetSelectedCharacter() != character) {
                    dataManager.SetSelectedCharacter(character);
                }
            }
        }

        public void PurchaseCharacter(int character)
        {
            int cost = dataManager.GetCharacterCost(character);
            if (dataManager.GetTotalCoins(true) < cost) {
                return;
            }
            dataManager.PurchaseCharacter(character);
            dataManager.SetSelectedCharacter(character);
            dataManager.AdjustTotalCoins(-cost, true);
        }

        public void OnApplicationPause(bool pause)
        {
            if (gamePaused)
                return;

            if (gameActive) {
                PauseGame(pause);
            }
        }

        public void OnApplicationFocus(bool focus)
        {
            if (gamePaused)
                return;

            if (gameActive) {
                PauseGame(!focus);
            }
        }
    }
}
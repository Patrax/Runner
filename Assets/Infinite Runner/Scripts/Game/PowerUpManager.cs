using UnityEngine;
using System.Collections;

namespace InfiniteRunner.Game
{
    /*
     * The power up manager is a singleton which manages the state of the power ups. 
     */
    public enum PowerUpTypes { DoubleCoin, CoinMagnet, Invincibility, SpeedIncrease, None }
    public class PowerUpManager : MonoBehaviour
    {

        static public PowerUpManager instance;

        private PowerUpTypes activePowerUp;
        private CoroutineData activePowerUpData;

        private GameManager gameManager;
        private DataManager dataManager;

        public void Awake()
        {
            instance = this;
        }

        public void Start()
        {
            gameManager = GameManager.instance;
            dataManager = DataManager.instance;
            gameManager.OnPauseGame += GamePaused;

            activePowerUp = PowerUpTypes.None;
            activePowerUpData = new CoroutineData();
        }

        public void ResetValues()
        {
            if (activePowerUp != PowerUpTypes.None) {
                StopCoroutine("RunPowerUp");
                DeactivatePowerUp();
            }
        }

        public bool IsPowerUpActive(PowerUpTypes powerUpType)
        {
            return activePowerUp == powerUpType;
        }

        public PowerUpTypes GetActivePowerUp()
        {
            return activePowerUp;
        }

        public void ActivatePowerUp(PowerUpTypes powerUpType)
        {
            activePowerUp = powerUpType;
            activePowerUpData.duration = dataManager.GetPowerUpLength(powerUpType);
            StartCoroutine("RunPowerUp");
        }

        private IEnumerator RunPowerUp()
        {
            activePowerUpData.startTime = Time.time;
            yield return new WaitForSeconds(activePowerUpData.duration);

            DeactivatePowerUp();
        }

        public void DeactivatePowerUp()
        {
            if (activePowerUp == PowerUpTypes.None)
                return;

            // Be sure the coroutine is stopped, deactivate may be called before runPowerUp is finished
            StopCoroutine("RunPowerUp");
            gameManager.ActivatePowerUp(activePowerUp, false);
            activePowerUp = PowerUpTypes.None;
            activePowerUpData.duration = 0;
        }

        private void GamePaused(bool paused)
        {
            if (activePowerUp != PowerUpTypes.None) {
                if (paused) {
                    StopCoroutine("RunPowerUp");
                    activePowerUpData.CalcuateNewDuration();
                } else {
                    StartCoroutine("RunPowerUp");
                }
            }
        }
    }
}
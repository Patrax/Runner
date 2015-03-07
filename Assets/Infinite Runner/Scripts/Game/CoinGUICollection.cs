using UnityEngine;
using System.Collections.Generic;
using InfiniteRunner.Player;

namespace InfiniteRunner.Game
{
    // class that animates the coin moving from the player to the coin indicator on the GUI
    public class CoinGUICollection : MonoBehaviour
    {
        // CoinValueObject is a small class that will make it easy to get the value of the coin value and the game object / transform
        class CoinValueObject
        {
            public CoinValueObject(GameObject go) { coinGameObject = go; coinTransform = go.transform; }
            public GameObject coinGameObject;
            public Transform coinTransform;
            public int coinValue;
        }

        static public CoinGUICollection instance;

        public GameObject guiCoin;
        public Transform collectionPoint;
        public Vector3 startPoint;
        public float collectionSpeed;

        private List<CoinValueObject> pool;
        private List<CoinValueObject> activeCoins;
        private int poolIndex;

        private Transform thisTransform;
        private GameManager gameManager;
        private CameraController cameraController;

        public void Awake()
        {
            instance = this;
        }

        public void Start()
        {
            thisTransform = transform;
            gameManager = GameManager.instance;
            cameraController = CameraController.instance;

            pool = new List<CoinValueObject>();
            poolIndex = 0;
            activeCoins = new List<CoinValueObject>();

            gameManager.OnPauseGame += GamePaused;
        }

        // start the animation from the coin to the gui coin location
        public void CoinCollected(int coinValue, bool primaryCoin)
        {
            // don't show the animation if the camera isn't in its normal position
            if (!cameraController.IsInGameTransform() || !primaryCoin) {
                gameManager.CoinCollected(coinValue, primaryCoin);
                return;
            }

            CoinValueObject coin = CoinFromPool();
            coin.coinValue = coinValue;
            coin.coinTransform.position = startPoint;
            activeCoins.Add(coin);
        }

        public void Update()
        {
            for (int i = activeCoins.Count - 1; i >= 0; --i) {
                activeCoins[i].coinTransform.position = Vector3.MoveTowards(activeCoins[i].coinTransform.position, collectionPoint.position, collectionSpeed * Time.deltaTime);
                if (Vector3.SqrMagnitude(activeCoins[i].coinTransform.position - collectionPoint.position) < 0.001f) {
                    DisableCoin(i);
                }
            }
        }

        private CoinValueObject CoinFromPool()
        {
            CoinValueObject obj;

            // keep a start index to prevent the constant pushing and popping from the list
            if (pool.Count > 0 && pool[poolIndex].coinGameObject.activeSelf == false) {
                obj = pool[poolIndex];
                obj.coinGameObject.SetActive(true);
                poolIndex = (poolIndex + 1) % pool.Count;
                return obj;
            }

            // No inactive objects, need to instantiate a new one
            obj = new CoinValueObject(GameObject.Instantiate(guiCoin) as GameObject);
            obj.coinTransform.parent = thisTransform;

            pool.Insert(poolIndex, obj);
            poolIndex = (poolIndex + 1) % pool.Count;

            return obj;
        }

        public int GetAnimatingCoins()
        {
            return activeCoins.Count;
        }

        private void DisableCoin(int activeIndex)
        {
            activeCoins[activeIndex].coinGameObject.SetActive(false);
            gameManager.CoinCollected(activeCoins[activeIndex].coinValue, true);
            activeCoins.RemoveAt(activeIndex);
        }

        public void GameOver()
        {
            // add any coins animating to the coin gui element before the game is over
            for (int i = activeCoins.Count - 1; i >= 0; --i) {
                DisableCoin(i);
            }
        }

        private void GamePaused(bool paused)
        {
            enabled = !paused;
        }
    }
}
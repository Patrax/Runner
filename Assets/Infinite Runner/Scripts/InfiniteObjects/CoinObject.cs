using UnityEngine;
using System.Collections;
using InfiniteRunner.Game;
using InfiniteRunner.InfiniteGenerator;
using InfiniteRunner.Player;

namespace InfiniteRunner.InfiniteObjects
{
    // The primary coin allows you to purchase characters and power ups, the secondary coin allows you to revive after death
    public enum CoinType { Primary, Secondary }

    /*
     * The player collects coins to be able to purchase power ups
     */
    [RequireComponent(typeof(AppearanceProbability))]
    [RequireComponent(typeof(CollidableAppearanceRules))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class CoinObject : CollidableObject
    {
        public CoinType coinType = CoinType.Primary;
        public float collectSpeed = 0.75f;
        public float rotationSpeed = 2f;
        public float rotationDelay = 0;

        private int coinValue; // coin value with the double coin power up considered
        private int playerLayer;
        private int coinMagnetLayer;
        private bool collect;
        private Vector3 collectPoint;
        private Vector3 startLocalPosition;
        private bool canRotate;

        public override void Init()
        {
            base.Init();
            objectType = ObjectType.Coin;
        }

        public override void Awake()
        {
            base.Awake();

            playerLayer = LayerMask.NameToLayer("Player");
            coinMagnetLayer = LayerMask.NameToLayer("CoinMagnet");
            collectPoint = new Vector3(0, 1, 0);
            startLocalPosition = thisTransform.localPosition;
            collect = canRotate = false;
            enabled = rotationSpeed != 0;

            GameManager.instance.OnPauseGame += GamePaused;

            if (rotationSpeed > 0) {
                StartCoroutine("Rotate", rotationDelay);
            }
        }

        public void Update()
        {
            if (canRotate) {
                thisTransform.Rotate(0, rotationSpeed, 0);
            }

            if (!collect)
                return;

            if (thisTransform.localPosition != collectPoint) {
                thisTransform.localPosition = Vector3.MoveTowards(thisTransform.localPosition, collectPoint, collectSpeed);
            } else {
                PlayerController.instance.CoinCollected(coinType == CoinType.Primary);
                CoinGUICollection.instance.CoinCollected(coinValue, coinType == CoinType.Primary);
                collect = false;
                enabled = rotationSpeed != 0;
                CollidableDeactivation();
                thisTransform.localPosition = startLocalPosition;
            }
        }

        private IEnumerator Rotate(float delay)
        {
            if (delay > 0) {
                yield return new WaitForSeconds(delay);
            }

            canRotate = true;
        }

        public void OnTriggerEnter(Collider other)
        {
            if ((other.gameObject.layer == playerLayer || other.gameObject.layer == coinMagnetLayer) && !collect) {
                CollectCoin();
            }
        }

        public void CollectCoin()
        {
            coinValue = GameManager.instance.CoinCollected();

            // the coin may have been collected from far away with the coin magnet. Fly towards the player when collected
            thisTransform.parent = PlayerController.instance.transform;
            collect = true;
            enabled = true;
        }

        public void OnDisable()
        {
            GameManager.instance.OnPauseGame -= GamePaused;
        }

        private void GamePaused(bool paused)
        {
            if (rotationSpeed > 0) {
                if (paused) {
                    StopCoroutine("Rotate");
                } else {
                    StartCoroutine("Rotate", 0);
                }
            }
        }
    }
}
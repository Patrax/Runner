using UnityEngine;
using InfiniteRunner.Game;
using InfiniteRunner.InfiniteGenerator;
using InfiniteRunner.InfiniteObjects;

namespace InfiniteRunner.InfiniteObjects
{
    /*
     * Power ups are used to give the player an extra super ability
     */
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(AppearanceProbability))]
    [RequireComponent(typeof(PowerUpAppearanceRules))]
    public class PowerUpObject : CollidableObject
    {
        public PowerUpTypes powerUpType;

        private GameManager gameManager;
        private int playerLayer;

        public override void Init()
        {
            base.Init();
            objectType = ObjectType.PowerUp;
        }

        public override void Awake()
        {
            base.Awake();
            playerLayer = LayerMask.NameToLayer("Player");
        }

        public void Start()
        {
            gameManager = GameManager.instance;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == playerLayer) {
                gameManager.ActivatePowerUp(powerUpType, true);
                Deactivate();
            }
        }
    }
}
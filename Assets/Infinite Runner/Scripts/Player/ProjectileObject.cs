using UnityEngine;
using InfiniteRunner.Game;
using InfiniteRunner.InfiniteObjects;

namespace InfiniteRunner.Player
{
    // The projectile object is a relatively simply class which moves a projectile on each Update call after it has been fired
    public class ProjectileObject : MonoBehaviour
    {
        public float speed;
        public float destroyDistance; // the distance at which the projectile gets destroyed without it hitting anything

        private Transform startParent;
        private float distanceTravelled;
        private Vector3 forwardDirection;
        private int playerLayer;

        private GameObject thisGameObject;
        private Transform thisTransform;

        public void Init()
        {
            thisGameObject = gameObject;
            thisTransform = transform;

            playerLayer = LayerMask.NameToLayer("Player");
        }

        public bool IsActive()
        {
            return thisGameObject.activeSelf;
        }

        public void SetStartParent(Transform parent)
        {
            startParent = parent;
        }

        public void Fire(Vector3 position, Quaternion rotation, Vector3 forward)
        {
            distanceTravelled = 0;
            forwardDirection = forward;
            thisTransform.position = position;
            thisTransform.rotation = rotation;
            thisTransform.parent = null;

            thisGameObject.SetActive(true);
            GameManager.instance.OnPauseGame += GamePaused;
        }

        public void Update()
        {
            float deltaDistance = speed * Time.deltaTime;
            thisTransform.position = Vector3.MoveTowards(thisTransform.position, thisTransform.position + forwardDirection * deltaDistance, deltaDistance);

            distanceTravelled += deltaDistance;
            if (distanceTravelled > destroyDistance) {
                Deactivate();
            }
        }

        public void TransitionHeight(float amount)
        {
            Vector3 position = thisTransform.position;
            position.y -= amount;
            thisTransform.position = position;
        }

        public void OnTriggerEnter(Collider other)
        {
            // ignore player collisions
            if (other.gameObject.layer == playerLayer) {
                return;
            }

            // ignore tutorial triggers
            if (other.gameObject.GetComponent<TutorialTrigger>() != null) {
                return;
            }

            ObstacleObject obstacle;
            if ((obstacle = other.GetComponent<ObstacleObject>()) != null) {
                if (obstacle.isDestructible) {
                    obstacle.ObstacleAttacked();
                }
            }
            Deactivate();
        }

        private void Deactivate()
        {
            thisTransform.parent = startParent;
            thisGameObject.SetActive(false);
            GameManager.instance.OnPauseGame -= GamePaused;
        }

        private void GamePaused(bool paused)
        {
            enabled = !paused;
        }
    }
}
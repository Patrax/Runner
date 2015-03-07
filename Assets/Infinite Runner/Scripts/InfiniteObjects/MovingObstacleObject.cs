using UnityEngine;
using InfiniteRunner.Game;
using InfiniteRunner.Player;

namespace InfiniteRunner.InfiniteObjects
{
    public class MovingObstacleObject : ObstacleObject
    {
        public float startMoveSquaredDistance; // 0 to indicate that the obstacle should start moving right after it is spawned
        public float forwardSpeed;
        public float horizontalSpeed; // used if moveTowardsPlayer is enabled
        public bool moveTowardsPlayer;
        public bool lookAtPlayer;
        public Animation obstacleAnimation;

        private bool moving;
        private float yAngle;
        private float lastSquareMagnitude;
        private Rigidbody thisRigidbody;
        private Transform playerTransform;

        public override void Start()
        {
            base.Start();

            thisRigidbody = GetComponent<Rigidbody>();
            enabled = false;

            if (PlayerController.instance != null) {
                UpdatePlayerTransform();
            }
            GameManager.instance.OnPlayerSpawn += UpdatePlayerTransform;
            GameManager.instance.OnPauseGame += GamePaused;
        }

        private void UpdatePlayerTransform()
        {
            playerTransform = PlayerController.instance.transform;
            enabled = true;
        }

        public override void Orient(PlatformObject parent, Vector3 position, Quaternion rotation)
        {
            base.Orient(parent, position, rotation);

            collideWithPlayer = true;
            moving = (startMoveSquaredDistance == 0);
            lastSquareMagnitude = Mathf.Infinity;
            yAngle = thisTransform.eulerAngles.y;
        }

        public void Update()
        {
            if (moving) {
                if (moveTowardsPlayer) {
                    // forward position will automatically move with the rigidbody, only need to worry about the horizontal position
                    Vector3 position = thisTransform.position;
                    if (yAngle % 180 < 0.01f) {
                        position.x = Mathf.MoveTowards(thisTransform.position.x, playerTransform.position.x, horizontalSpeed);
                    } else {
                        position.z = Mathf.MoveTowards(thisTransform.position.z, playerTransform.position.z, horizontalSpeed);
                    }

                    thisTransform.position = position;
                }

                if (lookAtPlayer) {
                    // disable if the obstacle has passed the player
                    float squareMagnitude = Vector3.SqrMagnitude(thisTransform.position - playerTransform.position);
                    if (squareMagnitude > lastSquareMagnitude) {
                        moving = false;
                        collideWithPlayer = false;
                    }
                    lastSquareMagnitude = squareMagnitude;
                    thisTransform.LookAt(playerTransform);
                }
            } else {
                // take the square because square roots are expensive
                if (collideWithPlayer && Mathf.Abs(Vector3.Dot(thisTransform.forward, playerTransform.forward)) > 0.99f && Vector3.SqrMagnitude(thisTransform.position - playerTransform.position) < startMoveSquaredDistance) {
                    moving = true;
                }
            }
        }

        public void FixedUpdate()
        {
            if (!moving) {
                return;
            }

            Vector3 localVelocity = thisTransform.InverseTransformDirection(thisRigidbody.velocity);
            Vector3 deltaForce = Vector3.zero;
            deltaForce.y = -3;
            deltaForce.z = forwardSpeed - localVelocity.z;
            thisRigidbody.AddRelativeForce(deltaForce, ForceMode.VelocityChange);
        }

        public override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);

            // stop moving if we hit the player
            if (other.gameObject.layer == playerLayer && moveTowardsPlayer) {
                moving = false;
                collideWithPlayer = false;
                thisRigidbody.velocity = Vector3.zero;
            }
        }

        public override void CollidableDeactivation()
        {
            base.CollidableDeactivation();
            moving = false;
            thisRigidbody.velocity = Vector3.zero;
            GameManager.instance.OnPauseGame -= GamePaused;
        }

        private void GamePaused(bool paused)
        {
            enabled = !paused;
            thisRigidbody.isKinematic = paused;
            if (obstacleAnimation) {
                if (paused) {
                    obstacleAnimation.Stop();
                } else {
                    obstacleAnimation.Play();
                }
            }
        }
    }
}
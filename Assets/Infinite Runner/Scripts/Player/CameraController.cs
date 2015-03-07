using UnityEngine;
using InfiniteRunner.Game;

namespace InfiniteRunner.Player
{
    /*
     * Basic camera script which doesn't really do anything besides hold variables so it knows where to reset
     */
    public class CameraController : MonoBehaviour
    {
        static public CameraController instance;

        // the target position and rotation of the camera
        public Vector3 inGamePosition;
        public Vector3 inGameRotation;

        public float smoothMoveTime;
        public float moveSpeed;
        public float rotationSpeed;
        public bool steadyHeight;

        // How long should the camera shake for if the player collided with an obstacle
        public float shakeDuration;
        public float shakeIntensity;
        private bool isShaking;
        private float shakeStartTime;
        private float shakeDelta;
        private float currentShakeIntensity;

        private Vector3 targetPosition;
        private Quaternion targetRotation;
        private bool transitioning;
        private bool gameActive;
        private float verticalOffset;

        private Vector3 startPosition;
        private Quaternion startRotation;

        private Transform thisTransform;
        private PlayerController playerController;
        private Transform playerTransform;

        // for pausing:
        private float pauseTime;

        public void Awake()
        {
            instance = this;

            startPosition = transform.position;
            startRotation = transform.rotation;
        }

        public void Start()
        {
            thisTransform = transform;
            targetPosition = thisTransform.position;
            targetRotation = thisTransform.rotation;
        }

        public void StartGame(bool fromRestart)
        {
            playerController = PlayerController.instance;
            playerTransform = playerController.transform;
            if (fromRestart) {
                targetPosition = inGamePosition;
                targetRotation.eulerAngles = inGameRotation;

                thisTransform.position = playerTransform.TransformPoint(inGamePosition);

                Vector3 relativeTargetRotationVec = inGameRotation;
                relativeTargetRotationVec.y += playerTransform.eulerAngles.y;
                Quaternion relativeTargetRotation = thisTransform.rotation;
                relativeTargetRotation.eulerAngles = relativeTargetRotationVec;
                thisTransform.rotation = relativeTargetRotation;

                transitioning = false;
            } else {
                targetPosition = inGamePosition;
                targetRotation.eulerAngles = inGameRotation;
                transitioning = true;
            }
            gameActive = true;

            GameManager.instance.OnPauseGame += GamePaused;
            isShaking = false;
        }

        public bool IsInGameTransform()
        {
            return targetPosition == inGamePosition && targetRotation.eulerAngles == inGameRotation;
        }

        public void GameOver(GameOverType gameOverType)
        {
            gameActive = false;
            GameManager.instance.OnPauseGame -= GamePaused;
        }

        public void LateUpdate()
        {
            if (!gameActive && !isShaking) {
                return;
            }

            Vector3 relativeTargetPosition = playerTransform.TransformPoint(targetPosition);
            if (steadyHeight && verticalOffset > 0) {
                // set the vertical offset to 0 if the player isn't in the air anymore. This can happen if the player lands on an upward slope
                if (!playerController.InAir()) {
                    verticalOffset = 0;
                }
                relativeTargetPosition.y -= verticalOffset;
            }

            Vector3 relativeTargetRotationVec = targetRotation.eulerAngles;
            relativeTargetRotationVec.y += playerTransform.eulerAngles.y;
            Quaternion relativeTargetRotation = thisTransform.rotation;
            relativeTargetRotation.eulerAngles = relativeTargetRotationVec;

            // if the camera is transitioning from one position to another then use the regular move towards / rotate towards.
            if (transitioning) {
                bool transitioned = true;
                if ((thisTransform.position - relativeTargetPosition).sqrMagnitude > 0.01f) {
                    transitioned = false;
                    thisTransform.position = Vector3.MoveTowards(thisTransform.position, relativeTargetPosition, moveSpeed);
                }

                if (Quaternion.Angle(thisTransform.rotation, relativeTargetRotation) > 0.01f) {
                    transitioned = false;
                    thisTransform.rotation = Quaternion.RotateTowards(thisTransform.rotation, relativeTargetRotation, rotationSpeed);
                }
                transitioning = !transitioned;
            } else {
                Vector3 currentVelocity = Vector3.zero;
                thisTransform.position = Vector3.SmoothDamp(thisTransform.position, relativeTargetPosition, ref currentVelocity, smoothMoveTime);
                // if the camera should have a steady height and the player is in the air then keep the y position steady regardless of the smooth move time.
                if (steadyHeight && playerController.InAir()) {
                    Vector3 position = thisTransform.position;
                    position.y = relativeTargetPosition.y;
                    thisTransform.position = position;
                }
                thisTransform.rotation = Quaternion.RotateTowards(thisTransform.rotation, relativeTargetRotation, rotationSpeed);
            }

            if (isShaking) {
                Vector3 rotation = relativeTargetRotation.eulerAngles;
                rotation.Set(rotation.x + Random.Range(-currentShakeIntensity, currentShakeIntensity),
                    rotation.y + Random.Range(-currentShakeIntensity, currentShakeIntensity),
                    rotation.z + Random.Range(-currentShakeIntensity, currentShakeIntensity));
                currentShakeIntensity -= shakeDelta * Time.deltaTime; // slowly wind down
                thisTransform.eulerAngles = rotation;
                if (Time.time - shakeStartTime > shakeDuration) {
                    isShaking = false; // will end on the next update loop
                }
            }
        }

        public void AdjustVerticalOffset(float amount)
        {
            verticalOffset += amount;
            if (verticalOffset < 0) {
                verticalOffset = 0;
            }
        }

        public void Shake()
        {
            if (isShaking || shakeIntensity == 0 || shakeDuration == 0)
                return;

            isShaking = true;
            shakeStartTime = Time.time;
            currentShakeIntensity = shakeIntensity;
            shakeDelta = shakeIntensity / shakeDuration;
        }

        public void ResetValues()
        {
            thisTransform.position = startPosition;
            thisTransform.rotation = startRotation;
        }

        public void GamePaused(bool paused)
        {
            if (paused) {
                pauseTime = Time.time;
            } else {
                if (isShaking) {
                    shakeStartTime += (Time.time - pauseTime);
                }
            }
            enabled = !paused;
        }
    }
}
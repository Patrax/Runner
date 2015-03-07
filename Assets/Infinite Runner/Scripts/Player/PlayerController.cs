using UnityEngine;
using System.Collections;
using InfiniteRunner.Game;
using InfiniteRunner.InfiniteGenerator;
using InfiniteRunner.InfiniteObjects;

namespace InfiniteRunner.Player
{
    /*
     * Calculate the target position/rotation of the player every frame, as well as move the objects around the player.
     * This class also manages when the player is sliding/jumping, and calls any animations.
     * The player has a collider which only collides with the platforms/walls. All obstacles/coins/power ups have their
     * own trigger system and will call the player controller if they need to.
     */
    public enum SlotPosition { Left = -1, Center, Right }
    public enum AttackType { None, Fixed, Projectile }
    [RequireComponent(typeof(PlayerAnimation))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController instance;

        public int maxCollisions = 0; // set to 0 to allow infinite collisions. In this case the game will end with the chase character attacks the player
        [HideInInspector]
        public DistanceValueList forwardSpeeds;
        public float horizontalSpeed = 15;
        public float slowRotationSpeed = 1;
        public float fastRotationSpeed = 20;
        public float stumbleSpeedDecrement = 3; // amount to decrease the speed when the player hits an obstacle
        public float stumbleDuration = 1;
        public float jumpHeight = 5;
        public float repeatedJumpDelay = 0;
        public float gravity = -15;
        public float slideDuration = 0.75f;
        public AttackType attackType = AttackType.None;
        public float closeAttackDistance = 3; // the minimum distance that allows the attack to hit the target
        public float farAttackDistance = 6; // the maximum distance that allows the attack to hit the target
        public bool restrictTurns = true; // if true, can only turn on turn platforms
        public bool restrictTurnsToTurnTrigger = false; // if true, the player will only turn when the player hits a turn trigger. restrictTurns must also be enabled
        public bool autoTurn = false; // automatically turn on platforms
        public bool autoJump = false; // automatically jump on jump platforms
        public float turnGracePeriod = 0.5f; // if restrictTurns is on, if the player swipes within the grace period before a turn then the character will turn
        public float simultaneousTurnPreventionTime = 2; // the amount of time that must elapse in between two different turns
        public float powerUpSpeedIncrease = 10;
        public Vector3 pivotOffset; // Assume the meshes pivot point is at the bottom. If it isn't use this offset.
        public float reviveGracePeriod = 3; // The amount of time that the player is invincible after a revive

        // Deprecated variables:
        // jumpForce is deprecated. Use jumpHeight instead
        public float jumpForce;
        // jumpDownwardForce is deprecated. Use gravity instead
        public float jumpDownwardForce;

        public ParticleSystem coinCollectionParticleSystem;
        public ParticleSystem secondaryCoinCollectionParticleSystem;
        public ParticleSystem collisionParticleSystem;
        // particles must be in the same order as the PowerUpTypes
        public ParticleSystem[] powerUpParticleSystem;
        public ParticleSystem groundCollisionParticleSystem;
        public GameObject coinMagnetTrigger;

        private float totalMoveDistance;
        private SlotPosition currentSlotPosition;
        private Quaternion targetRotation;
        private Vector3 targetPosition;
        private float targetHorizontalPosition;
        private bool canUpdatePosition;

        private float minForwardSpeed;
        private float maxForwardSpeed;
        private float forwardSpeedDelta;

        private float jumpSpeed;
        private bool isJumping;
        // isJumping gets set to false when the player lands on a platform within OnControllerColliderHit. OnControllerColliderHit may get called even before the player does
        // the jump though (such as switching from one platform to another) so isJumpPending will be set to true when the jump is initiated and to false
        // when the player actually leaves the platform for a jump
        private bool isJumpPending;
        private bool isSliding;
        private bool isStumbling;
        private float turnRequestTime;
        private bool turnRightRequest;
        private float turnTime;
        private float jumpLandTime;
        private bool onGround;
        private bool skipFrame;
        private Transform prevHitTransform;
        private float reviveTime;

        private int platformLayer;
        private int floorLayer;
        private int wallLayer;
        private int obstacleLayer;

        private Vector3 startPosition;
        private Quaternion startRotation;
        private Vector3 turnOffset;
        private Vector3 curveOffset;
        private Vector3 prevTurnOffset;

        private PlatformObject platformObject;
        private float curveMoveDistance;
        private float curveTime;
        private int curveDistanceMapIndex;
        private bool followCurve; // may not follow the curve on a turn

        // for pausing:
        private CoroutineData slideData;
        private CoroutineData stumbleData;
        private bool pauseCollisionParticlePlaying;
        private bool pauseCoinParticlePlaying;
        private bool pauseSecondaryCoinParticlePlaying;
        private bool pauseGroundParticlePlaying;

        private Transform thisTransform;
        private CapsuleCollider capsuleCollider;
        private PlayerAnimation playerAnimation;
        private ProjectileManager projectileManager;
        private CameraController cameraController;
        private InfiniteObjectGenerator infiniteObjectGenerator;
        private PowerUpManager powerUpManager;
        private GameManager gameManager;

        public void Awake()
        {
            instance = this;
        }

        public void Init()
        {
            // deprecated variables warnings:
            if (jumpForce != 0 && jumpHeight == 0) {
                Debug.LogError("PlayerController.jumpForce is deprecated. Use jumpHeight instead.");
                jumpHeight = jumpForce;
            }
            if (jumpDownwardForce != 0 && gravity == 0) {
                Debug.LogError("PlayerController.jumpDownwardForce is deprecated. Use gravity instead.");
                gravity = jumpDownwardForce;
            }
            // rigidbody should no longer use gravity, be kinematic, and freeze all constraints
            Rigidbody playerRigibody = GetComponent<Rigidbody>();
            if (playerRigibody != null) {
                if (playerRigibody.useGravity) {
                    Debug.LogError("The rigidbody no longer needs to use gravity. Disabling.");
                    playerRigibody.useGravity = false;
                }
                if (!playerRigibody.isKinematic) {
                    Debug.LogError("The rigidbody should be kinematic. Enabling.");
                    playerRigibody.isKinematic = true;
                }
                if (playerRigibody.constraints != RigidbodyConstraints.FreezeAll) {
                    Debug.LogError("The rigidbody should freeze all constraints. The PlayerController will take care of the physics.");
                    playerRigibody.constraints = RigidbodyConstraints.FreezeAll;
                }
            }

            cameraController = CameraController.instance;
            infiniteObjectGenerator = InfiniteObjectGenerator.instance;
            powerUpManager = PowerUpManager.instance;
            gameManager = GameManager.instance;
            if (attackType == AttackType.Projectile) {
                projectileManager = GetComponent<ProjectileManager>();
            }

            platformLayer = 1 << LayerMask.NameToLayer("Platform");
            floorLayer = 1 << LayerMask.NameToLayer("Floor");
            wallLayer = 1 << LayerMask.NameToLayer("Wall");
            obstacleLayer = 1 << LayerMask.NameToLayer("Obstacle");

            thisTransform = transform;
            capsuleCollider = GetComponent<CapsuleCollider>();
            playerAnimation = GetComponent<PlayerAnimation>();
            playerAnimation.Init();

            startPosition = thisTransform.position;
            startRotation = thisTransform.rotation;

            slideData = new CoroutineData();
            stumbleData = new CoroutineData();
            forwardSpeeds.Init();
            // determine the fastest and the slowest forward speeds
            forwardSpeeds.GetMinMaxValue(out minForwardSpeed, out maxForwardSpeed);
            forwardSpeedDelta = maxForwardSpeed - minForwardSpeed;
            if (forwardSpeedDelta == 0) {
                playerAnimation.SetRunSpeed(1, 1);
            }

            ResetValues(false);
            enabled = false;
        }

        public void ResetValues(bool fromRevive)
        {
            slideData.duration = 0;
            stumbleData.duration = 0;

            jumpSpeed = 0;
            isJumping = false;
            isJumpPending = false;
            isSliding = false;
            isStumbling = false;
            onGround = true;
            prevHitTransform = null;
            canUpdatePosition = true;
            playerAnimation.ResetValues();
            if (projectileManager)
                projectileManager.ResetValues();
            pauseCollisionParticlePlaying = false;
            turnTime = -simultaneousTurnPreventionTime;
            jumpLandTime = Time.time;

            platformObject = null;
            curveTime = -1;
            curveMoveDistance = 0;
            curveDistanceMapIndex = 0;
            followCurve = false;

            if (!fromRevive) {
                currentSlotPosition = SlotPosition.Center;
                targetHorizontalPosition = (int)currentSlotPosition * infiniteObjectGenerator.slotDistance;
                totalMoveDistance = 0;
                turnOffset = prevTurnOffset = Vector3.zero;
                curveOffset = Vector3.zero;
                forwardSpeeds.ResetValues();

                thisTransform.position = startPosition;
                thisTransform.rotation = startRotation;
                targetRotation = startRotation;
                UpdateTargetPosition(targetRotation.eulerAngles.y);
                reviveTime = -1;
            } else {
                reviveTime = Time.time;
            }
        }

        public void StartGame()
        {
            // make sure the coin magnet trigger is deactivated
            ActivatePowerUp(PowerUpTypes.CoinMagnet, false);

            playerAnimation.Run();
            enabled = true;
            gameManager.OnPauseGame += GamePaused;
        }

        // There character doesn't move, all of the objects around it do. Make sure the character is in the correct position
        public void Update()
        {
            Vector3 moveDirection = Vector3.zero;
            float hitDistance = 0;
            RaycastHit hit;
            // cast a ray to see if we are over any platforms
            if (Physics.Raycast(thisTransform.position + capsuleCollider.center, -thisTransform.up, out hit, Mathf.Infinity, platformLayer)) {
                hitDistance = hit.distance;
                PlatformObject platform = null;
                if (!hit.transform.Equals(prevHitTransform)) {
                    prevHitTransform = hit.transform;
                    // update the platform object
                    if (((platform = hit.transform.GetComponent<PlatformObject>()) != null) || ((platform = hit.transform.parent.GetComponent<PlatformObject>()) != null)) {
                        if (platform != platformObject) {
                            platformObject = platform;
                            CheckForCurvedPlatform();
                        }
                    }
                }

                // we are over a platform, determine if we are on the ground of that platform
                if (hit.distance <= capsuleCollider.height / 2 + 0.0001f + pivotOffset.y) {
                    onGround = true;
                    // we are on the ground. Get the platform object and check to see if we are on a curve
                    // if we are sliding and the platform has a slope then stop sliding
                    if (isSliding) {
                        if (platformObject != null && platformObject.slope != PlatformSlope.None) {
                            StopCoroutine("DoSlide");
                            StopSlide(true);
                        }
                    }

                    // if we are jumping we either want to start jumping or land
                    if (isJumping) {
                        if (isJumpPending) {
                            moveDirection.y += jumpSpeed;
                            cameraController.AdjustVerticalOffset(jumpSpeed * Time.deltaTime);
                            jumpSpeed += gravity * Time.deltaTime;
                            onGround = false;
                        } else {
                            isJumping = false;
                            jumpLandTime = Time.time;
                            if (gameManager.IsGameActive())
                                playerAnimation.Run();
                            groundCollisionParticleSystem.Play();
                        }
                    } else {
                        // we are not jumping so our position should be the same as the hit point
                        Vector3 position = thisTransform.position;
                        position.y = hit.point.y;
                        thisTransform.position = position + pivotOffset;
                    }
                    skipFrame = true;
                    // a hit distance of -1 means that the platform is within distance
                    hitDistance = -1;
                }
                // if we didn't hit a platform we may hit a floor
            } else if (Physics.Raycast(thisTransform.position + capsuleCollider.center, -thisTransform.up, out hit, Mathf.Infinity, floorLayer)) {
                hitDistance = hit.distance;
            }

            if (hitDistance != -1) {
                // a platform is beneith us but it is far away. If we are jumping apply the jump speed and gravity
                if (isJumping) {
                    moveDirection.y += jumpSpeed;
                    cameraController.AdjustVerticalOffset(jumpSpeed * Time.deltaTime);
                    jumpSpeed += gravity * Time.deltaTime;

                    // the jump is no longer pending if we are in the air
                    if (isJumpPending) {
                        isJumpPending = false;
                    }
                } else if (!skipFrame) {
                    // apply gravity if we are not jumping
                    moveDirection.y = gravity * (powerUpManager.IsPowerUpActive(PowerUpTypes.SpeedIncrease) ? 2 : 1); // the speed power up needs a little extra push
                }

                if (!skipFrame && hitDistance == 0) {
                    platformObject = null;
                }
                if (skipFrame) {
                    skipFrame = false;
                } else if (hitDistance != 0 && thisTransform.position.y + (moveDirection.y * Time.deltaTime) < hit.point.y) {
                    // this transition should be instant so ignore Time.deltaTime
                    moveDirection.y = (hit.point.y - thisTransform.position.y) / Time.deltaTime;
                }
                onGround = false;
            }

            float xStrafe = (targetPosition.x - thisTransform.position.x) * Mathf.Abs(Mathf.Cos(targetRotation.eulerAngles.y * Mathf.Deg2Rad)) / Time.deltaTime;
            float zStrafe = (targetPosition.z - thisTransform.position.z) * Mathf.Abs(Mathf.Sin(targetRotation.eulerAngles.y * Mathf.Deg2Rad)) / Time.deltaTime;
            moveDirection.x += Mathf.Clamp(xStrafe, -horizontalSpeed, horizontalSpeed);
            moveDirection.z += Mathf.Clamp(zStrafe, -horizontalSpeed, horizontalSpeed);
            thisTransform.position += moveDirection * Time.deltaTime;

            // Make sure we don't run into a wall
            if (Physics.Raycast(thisTransform.position + Vector3.up, thisTransform.forward, capsuleCollider.radius, wallLayer)) {
                gameManager.GameOver(GameOverType.Wall, true);
            }

            if (!gameManager.IsGameActive()) {
                enabled = InAir(); // keep the character active for as long as they are in the air so gravity can keep pushing them down.
            }
        }

        // Move all of the objects within the LateObject to prevent jittering when the height transitions
        public void LateUpdate()
        {
            // don't move any objects if the game isn't active. The game may not be active if the character is in the air when they died
            if (!gameManager.IsGameActive()) {
                return;
            }

            float forwardSpeed = forwardSpeeds.GetValue(totalMoveDistance);
            if (isStumbling) {
                forwardSpeed -= stumbleSpeedDecrement;
            }
            if (powerUpManager.IsPowerUpActive(PowerUpTypes.SpeedIncrease)) {
                forwardSpeed += powerUpSpeedIncrease;
            }

            // continue along the curve if over a curved platform
            if (curveTime != -1 && platformObject != null) {
                curveTime = Mathf.Clamp01(curveMoveDistance / platformObject.curveLength);
                if (curveTime < 1 && followCurve) {
                    UpdateTargetPosition(thisTransform.eulerAngles.y);

                    // compute a future curve time to determine which direction the player will be heading
                    Vector3 curvePoint = GetCurvePoint(curveMoveDistance, true);
                    float futureMoveDistance = (curveMoveDistance + 2 * forwardSpeed * Time.deltaTime);
                    Vector3 futureCurvePoint = GetCurvePoint(futureMoveDistance, false);
                    futureCurvePoint.y = curvePoint.y = targetPosition.y;
                    Vector3 forwardDir = (futureCurvePoint - curvePoint).normalized;
                    targetRotation = Quaternion.LookRotation(forwardDir);
                    infiniteObjectGenerator.SetMoveDirection(forwardDir);
                }
                curveMoveDistance += forwardSpeed * Time.deltaTime;
            }

            if (thisTransform.rotation != targetRotation) {
                thisTransform.rotation = Quaternion.RotateTowards(thisTransform.rotation, targetRotation,
                                            Mathf.Lerp(slowRotationSpeed, fastRotationSpeed, Mathf.Clamp01(Quaternion.Angle(thisTransform.rotation, targetRotation) / 45)));
            }

            playerAnimation.SetRunSpeed(forwardSpeed, forwardSpeedDelta != 0 ? (forwardSpeed - minForwardSpeed) / (forwardSpeedDelta) : 1);
            forwardSpeed *= Time.deltaTime;
            totalMoveDistance += forwardSpeed;
            infiniteObjectGenerator.MoveObjects(forwardSpeed);
        }

        public bool AbovePlatform(bool aboveTurn /* if false, returns if above slope */)
        {
            if (platformObject != null) {
                if (aboveTurn) {
                    return platformObject.rightTurn || platformObject.leftTurn;
                } else { // slope
                    return platformObject.slope != PlatformSlope.None;
                }
            }

            return false;
        }

        // Turn left or right
        public bool Turn(bool rightTurn, bool fromInputManager)
        {
            // prevent two turns from occurring really close to each other (for example, to prevent a 180 degree turn)
            if (Time.time - turnTime < simultaneousTurnPreventionTime) {
                return false;
            }

            RaycastHit hit;
            // ensure we are over the correct platform
            if (Physics.Raycast(thisTransform.position + capsuleCollider.center, -thisTransform.up, out hit, Mathf.Infinity, platformLayer)) {
                PlatformObject platform = null;
                // update the platform object
                if (((platform = hit.transform.GetComponent<PlatformObject>()) != null) || ((platform = hit.transform.parent.GetComponent<PlatformObject>()) != null)) {
                    if (platform != platformObject) {
                        platformObject = platform;
                        CheckForCurvedPlatform();
                    }
                }
            }
            bool isAboveTurn = AbovePlatform(true);

            // if we are restricting a turn, don't turn unless we are above a turn platform
            if (restrictTurns && (!isAboveTurn || restrictTurnsToTurnTrigger)) {
                if (fromInputManager) {
                    turnRequestTime = Time.time;
                    turnRightRequest = rightTurn;
                    return false;
                }

                if (!powerUpManager.IsPowerUpActive(PowerUpTypes.Invincibility) && !powerUpManager.IsPowerUpActive(PowerUpTypes.SpeedIncrease) && !autoTurn && !WithinReviveGracePeriod()) {
                    // turn in the direction that the player swiped
                    rightTurn = turnRightRequest;

                    // don't turn if restrict turns is on and the player hasn't swipped within the grace period time or if the player isn't above a turn platform
                    if (!gameManager.godMode && (Time.time - turnRequestTime > turnGracePeriod || !isAboveTurn)) {
                        return false;
                    }
                }
            } else if (!fromInputManager && !autoTurn && !gameManager.godMode && (!restrictTurns || Time.time - turnRequestTime > turnGracePeriod) &&
                        !powerUpManager.IsPowerUpActive(PowerUpTypes.Invincibility) && !powerUpManager.IsPowerUpActive(PowerUpTypes.SpeedIncrease) &&
                        !WithinReviveGracePeriod()) {
                return false;
            }

            turnTime = Time.time;
            Vector3 direction = platformObject.GetTransform().right * (rightTurn ? 1 : -1);
            prevTurnOffset = turnOffset;
            canUpdatePosition = infiniteObjectGenerator.UpdateSpawnDirection(direction, platformObject.curveLength == 0, rightTurn, isAboveTurn, out turnOffset);
            if (canUpdatePosition && platformObject.curveLength > 0) {
                followCurve = true;
            } else {
                targetRotation = Quaternion.LookRotation(direction);
                curveOffset.x = (thisTransform.position.x - (startPosition.x + turnOffset.x)) * Mathf.Abs(Mathf.Sin(targetRotation.eulerAngles.y * Mathf.Deg2Rad));
                curveOffset.z = (thisTransform.position.z - (startPosition.z + turnOffset.z)) * Mathf.Abs(Mathf.Cos(targetRotation.eulerAngles.y * Mathf.Deg2Rad));
                if (isAboveTurn) {
                    UpdateTargetPosition(targetRotation.eulerAngles.y);
                }
            }
            return true;
        }

        // There are three slots on a track. Move left or right if there is a slot available
        public void ChangeSlots(bool right)
        {
            SlotPosition targetSlot = (SlotPosition)Mathf.Clamp((int)currentSlotPosition + (right ? 1 : -1), (int)SlotPosition.Left, (int)SlotPosition.Right);

            ChangeSlots(targetSlot);
        }

        // There are three slots on a track. The accelorometer/swipes determine the slot position
        public void ChangeSlots(SlotPosition targetSlot)
        {
            if (targetSlot == currentSlotPosition)
                return;

            if (!InAir())
                playerAnimation.Strafe((int)currentSlotPosition < (int)targetSlot);
            currentSlotPosition = targetSlot;
            targetHorizontalPosition = (int)currentSlotPosition * infiniteObjectGenerator.slotDistance;

            UpdateTargetPosition(targetRotation.eulerAngles.y);
        }

        public SlotPosition GetCurrentSlotPosition()
        {
            return currentSlotPosition;
        }

        public void MoveHorizontally(float amount)
        {
            // clamp the position between the min and max slot distance
            targetHorizontalPosition = Mathf.Clamp(targetHorizontalPosition + amount, -infiniteObjectGenerator.slotDistance, infiniteObjectGenerator.slotDistance);
            UpdateTargetPosition(targetRotation.eulerAngles.y);
        }

        // attack the object in front of the player if it can be destroyed
        public void Attack()
        {
            if (attackType == AttackType.None)
                return;

            if (!InAir() && !isSliding) {
                if (attackType == AttackType.Fixed) {
                    playerAnimation.Attack();

                    RaycastHit hit;
                    if (Physics.Raycast(thisTransform.position + Vector3.up / 2, thisTransform.forward, out hit, farAttackDistance, obstacleLayer)) {
                        // the player will collide with the obstacle if they are too close
                        if (hit.distance > closeAttackDistance) {
                            ObstacleObject obstacle = hit.collider.GetComponent<ObstacleObject>();
                            if (obstacle.isDestructible) {
                                obstacle.ObstacleAttacked();
                            }
                        }
                    }
                } else if (projectileManager.CanFire()) { // projectile
                    playerAnimation.Attack();
                    projectileManager.Fire();
                }
            }
        }

        private void UpdateTargetPosition(float yAngle)
        {
            // don't update the position when the player will be moving in the wrong direction from a turn
            if (!canUpdatePosition) {
                return;
            }
            if (curveTime != -1 && curveTime <= 1 && platformObject != null) {
                Vector3 curvePoint = GetCurvePoint(curveMoveDistance, true);
                targetPosition.x = curvePoint.x;
                targetPosition.z = curvePoint.z;
            } else {
                targetPosition.x = startPosition.x * Mathf.Abs(Mathf.Sin(yAngle * Mathf.Deg2Rad));
                targetPosition.z = startPosition.z * Mathf.Abs(Mathf.Cos(yAngle * Mathf.Deg2Rad));
                targetPosition += (turnOffset + curveOffset);
            }
            targetPosition.x += targetHorizontalPosition * Mathf.Cos(yAngle * Mathf.Deg2Rad);
            targetPosition.z += targetHorizontalPosition * -Mathf.Sin(yAngle * Mathf.Deg2Rad);
        }

        public void UpdateForwardVector(Vector3 forward)
        {
            thisTransform.forward = forward;
            targetRotation = thisTransform.rotation;
            turnOffset = prevTurnOffset;

            UpdateTargetPosition(targetRotation.eulerAngles.y);
            thisTransform.position.Set(targetPosition.x, thisTransform.position.y, targetPosition.z);
        }

        private Vector3 GetCurvePoint(float distance, bool updateMapIndex)
        {
            int index = curveDistanceMapIndex;
            float segmentDistance = platformObject.curveIndexDistanceMap[index];
            if (distance > segmentDistance && index < platformObject.curveIndexDistanceMap.Count - 1) {
                index++;
                if (updateMapIndex) {
                    curveDistanceMapIndex = index;
                }
            }
            float time = 0;
            if (index > 0) {
                float prevDistance = platformObject.curveIndexDistanceMap[index - 1];
                time = (distance - prevDistance) / (platformObject.curveIndexDistanceMap[index] - prevDistance);
            } else {
                time = distance / platformObject.curveIndexDistanceMap[index];
            }
            time = Mathf.Clamp01(time);

            Vector3 p0, p1, p2;
            if (index == 0) {
                p0 = platformObject.controlPoints[index];
            } else {
                p0 = (platformObject.controlPoints[index] + platformObject.controlPoints[index + 1]) / 2;
            }
            p1 = platformObject.controlPoints[index + 1];
            if (index + 2 == platformObject.controlPoints.Count - 1) {
                p2 = platformObject.controlPoints[index + 2];
            } else {
                p2 = (platformObject.controlPoints[index + 1] + platformObject.controlPoints[index + 2]) / 2;
            }

            return platformObject.GetTransform().TransformPoint(InfiniteRunnerStarterPackUtility.CalculateBezierPoint(p0, p1, p2, time));
        }

        public void Jump(bool fromTrigger)
        {
            if (jumpHeight > 0 && !InAir() && !isSliding && !AbovePlatform(false) && (fromTrigger || Time.time - jumpLandTime > repeatedJumpDelay)) { // can't jump on a sloped platform
                // don't jump if coming from a trigger and auto jump, invincibility/speed increase and God mode are not activated
                if (fromTrigger && !autoJump && !powerUpManager.IsPowerUpActive(PowerUpTypes.Invincibility) && !powerUpManager.IsPowerUpActive(PowerUpTypes.SpeedIncrease) && !gameManager.godMode) {
                    return;
                }

                jumpSpeed = jumpHeight;
                isJumping = isJumpPending = true;
                playerAnimation.Jump();
            }
        }

        public bool InAir()
        {
            return !onGround;
        }

        public void Slide()
        {
            if (slideDuration > 0 && !InAir() && !isSliding && !AbovePlatform(false)) { // can't slide above a sloped platform
                isSliding = true;
                playerAnimation.Slide();

                // adjust the collider bounds
                float height = capsuleCollider.height;
                height /= 2;
                Vector3 center = capsuleCollider.center;
                center.y = center.y - (capsuleCollider.height - height) / 2;
                capsuleCollider.height = height;
                capsuleCollider.center = center;

                slideData.duration = slideDuration;
                StartCoroutine("DoSlide");
            }
        }

        // stay in the slide postion for a certain amount of time
        private IEnumerator DoSlide()
        {
            slideData.startTime = Time.time;
            yield return new WaitForSeconds(slideData.duration);

            // only play the run animation if the game is still active
            if (gameManager.IsGameActive()) {
                playerAnimation.Run();
                // let the run animation start
                yield return new WaitForSeconds(playerAnimation.runTransitionTime);
            }

            StopSlide(false);
        }

        private void StopSlide(bool force)
        {
            if (force && gameManager.IsGameActive())
                playerAnimation.Run();

            isSliding = false;

            // adjust the collider bounds
            float height = capsuleCollider.height;
            height *= 2;
            capsuleCollider.height = height;
            Vector3 center = capsuleCollider.center;
            center.y = capsuleCollider.height / 2;
            capsuleCollider.center = center;
        }

        public bool WithinReviveGracePeriod()
        {
            return reviveTime != -1 && Time.time < reviveTime + reviveGracePeriod;
        }

        // the player collided with an obstacle, play some particle effects
        public void ObstacleCollision(Transform obstacle, Vector3 position)
        {
            if (!enabled)
                return;

            // Make sure the particle system is active
            if (!collisionParticleSystem.gameObject.activeSelf) {
                collisionParticleSystem.gameObject.SetActive(true);
            }
            collisionParticleSystem.transform.position = position;
            collisionParticleSystem.transform.parent = obstacle;
            collisionParticleSystem.Clear();
            collisionParticleSystem.Play();

            // stumble
            if (stumbleDuration > 0) {
                isStumbling = true;
                stumbleData.duration = stumbleDuration;
                StartCoroutine("Stumble");
            }

            // camera shake
            cameraController.Shake();
        }

        private IEnumerator Stumble()
        {
            stumbleData.startTime = Time.time;
            yield return new WaitForSeconds(stumbleData.duration);
            isStumbling = false;
        }

        private void CheckForCurvedPlatform()
        {
            // find the closest curve point
            if (platformObject.curveLength > 0) {
                curveMoveDistance = curveTime = curveDistanceMapIndex = 0;

                // don't follow the curve initally, wait until the player has turned or hit a turn trigger.
                followCurve = !platformObject.rightTurn && !platformObject.leftTurn;
            } else if (curveTime > 0) { // done with a curved platform
                curveTime = -1;
                followCurve = false;
                prevTurnOffset = turnOffset;
                turnOffset = infiniteObjectGenerator.GetTurnOffset();
                Vector3 forward = platformObject.GetTransform().forward;
                targetRotation = Quaternion.LookRotation(forward);
                float yAngle = targetRotation.eulerAngles.y;
                curveOffset.x = (thisTransform.position.x - (startPosition.x + turnOffset.x)) * Mathf.Abs(Mathf.Sin(yAngle * Mathf.Deg2Rad));
                curveOffset.z = (thisTransform.position.z - (startPosition.z + turnOffset.z)) * Mathf.Abs(Mathf.Cos(yAngle * Mathf.Deg2Rad));

                infiniteObjectGenerator.SetMoveDirection(forward);
                UpdateTargetPosition(platformObject.GetTransform().eulerAngles.y);
            }
        }

        public void TransitionHeight(float amount)
        {
            Vector3 position = thisTransform.position;
            position.y -= amount;
            thisTransform.position = position;

            position = cameraController.transform.position;
            position.y -= amount;
            cameraController.transform.position = position;

            // adjust all of the projectiles
            if (projectileManager) {
                projectileManager.TransitionHeight(amount);
            }
        }

        public void CoinCollected(bool primaryCoin)
        {
            if (primaryCoin) {
                coinCollectionParticleSystem.Play();
            } else {
                secondaryCoinCollectionParticleSystem.Play();
            }
        }

        public void ActivatePowerUp(PowerUpTypes powerUpType, bool activate)
        {
            if (powerUpType != PowerUpTypes.None) {
                ParticleSystem particleSystem = powerUpParticleSystem[(int)powerUpType];
                if (activate) {
                    particleSystem.Play();
                } else {
                    particleSystem.Stop();
                }
                if (powerUpType == PowerUpTypes.CoinMagnet) {
                    coinMagnetTrigger.SetActive(activate);
                }
            }
        }

        public void GameOver(GameOverType gameOverType)
        {
            if (!isSliding && gameOverType != GameOverType.Pit)
                playerAnimation.GameOver(gameOverType);
            // ensure the player returns to their original color
            ActivatePowerUp(powerUpManager.GetActivePowerUp(), false);
            collisionParticleSystem.transform.parent = null;
            gameManager.OnPauseGame -= GamePaused;
            // let the character fall if they are still in the air
            jumpSpeed = 0;
            enabled = InAir();
        }

        // disable the script if paused to stop the objects from moving
        private void GamePaused(bool paused)
        {
            ParticleSystem particleSystem = null;
            PowerUpTypes activePowerUp = powerUpManager.GetActivePowerUp();
            if (activePowerUp != PowerUpTypes.None) {
                particleSystem = powerUpParticleSystem[(int)activePowerUp];
            }

            if (paused) {
                if (coinCollectionParticleSystem.isPlaying) {
                    pauseCoinParticlePlaying = true;
                    coinCollectionParticleSystem.Pause();
                }
                if (secondaryCoinCollectionParticleSystem != null && secondaryCoinCollectionParticleSystem.isPlaying) {
                    pauseSecondaryCoinParticlePlaying = true;
                    secondaryCoinCollectionParticleSystem.Pause();
                }
                if (collisionParticleSystem.isPlaying) {
                    pauseCollisionParticlePlaying = true;
                    collisionParticleSystem.Pause();
                }
                if (groundCollisionParticleSystem.isPlaying) {
                    pauseGroundParticlePlaying = true;
                    groundCollisionParticleSystem.Pause();
                }
                if (particleSystem != null)
                    particleSystem.Pause();
            } else {
                if (pauseCoinParticlePlaying) {
                    coinCollectionParticleSystem.Play();
                    pauseCoinParticlePlaying = false;
                }
                if (pauseSecondaryCoinParticlePlaying) {
                    secondaryCoinCollectionParticleSystem.Play();
                    pauseSecondaryCoinParticlePlaying = false;
                }
                if (pauseCollisionParticlePlaying) {
                    collisionParticleSystem.Play();
                    pauseCollisionParticlePlaying = false;
                }
                if (pauseGroundParticlePlaying) {
                    groundCollisionParticleSystem.Play();
                    pauseGroundParticlePlaying = false;
                }
                if (particleSystem != null)
                    particleSystem.Play();
            }
            if (isSliding) {
                if (paused) {
                    StopCoroutine("DoSlide");
                    slideData.CalcuateNewDuration();
                } else {
                    StartCoroutine("DoSlide");
                }
            }
            if (isStumbling) {
                if (paused) {
                    StopCoroutine("Stumble");
                    stumbleData.CalcuateNewDuration();
                } else {
                    StartCoroutine("Stumble");
                }
            }
            enabled = !paused;
        }
    }
}
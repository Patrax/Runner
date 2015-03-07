using UnityEngine;
using InfiniteRunner.Player;

/*
 * The input controller is a singleton class which interperates the input (from a keyboard or touch) and passes
 * it to the player controller
 */
public class InputController : MonoBehaviour
{

    static public InputController instance;

    // if true the player is not bound to slot positions
    public bool freeHorizontalMovement = false;
#if UNITY_IPHONE || UNITY_ANDROID || UNITY_BLACKBERRY || UNITY_WP8
    // move horizontally with a swipe (true) or the accelerometer (false)
    public bool swipeToMoveHorizontally = false;
    // The number of pixels you must swipe in order to register a horizontal or vertical swipe
    public Vector2 swipeDistance = new Vector2(40, 40);
    // How sensitive the horizontal and vertical swipe are. The higher the value the more it takes to activate a swipe
    public float swipeSensitivty = 2;
    // More than this value and the player will move into the rightmost slot.
    // Less than the negative of this value and the player will move into the leftmost slot.
    // The accelerometer value in between these two values equals the middle slot.
    public float accelerometerRightSlotValue = 0.25f;
    // the higher the value the less likely the player will switch slots
    public float accelerometerSensitivity = 0.1f;
    private Vector2 touchStartPosition;
    private bool acceptInput; // ensure that only one action is performed per touch gesture
#endif
#if UNITY_EDITOR || !(UNITY_IPHONE || UNITY_ANDROID || UNITY_BLACKBERRY || UNITY_WP8)
    public bool sameKeyForTurnHorizontalMovement = false;
    public bool useMouseToMoveHorizontally = true;
    // if freeHorizontalMovement is enabled, this value will specify how much movement to apply when a key is pressed
    public float horizontalMovementDelta = 0.2f;
    // how sensitive the horizontal movement is with the mouse. The higher the value the more it takes to move
    public float horizontalMovementSensitivity = 100;
    // Allow slot changes by moving the mouse left or right
    public float mouseXDeltaValue = 100f;
    private float mouseStartPosition;
    private float mouseStartTime;
#endif

    private PlayerController playerController;

    public void Awake()
    {
        instance = this;
    }

    public void StartGame()
    {
        playerController = PlayerController.instance;

#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID || UNITY_BLACKBERRY || UNITY_WP8)
        touchStartPosition = Vector2.zero;
#else
        mouseStartPosition = Input.mousePosition.x;
        mouseStartTime = Time.time;
#endif

        enabled = true;
#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID || UNITY_BLACKBERRY || UNITY_WP8)
        acceptInput = true;
#endif
    }

    public void GameOver()
    {
        enabled = false;
    }

    public void Update()
    {
#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID || UNITY_BLACKBERRY || UNITY_WP8)
        if (Input.touchCount == 1) {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began) {
                touchStartPosition = touch.position;
            } else if (touch.phase == TouchPhase.Moved && acceptInput) {
                Vector2 diff = touch.position - touchStartPosition;
                if (diff.x == 0f)
                    diff.x = 1f; // avoid divide by zero
                float verticalPercent = Mathf.Abs(diff.y / diff.x);

                if (verticalPercent > swipeSensitivty && Mathf.Abs(diff.y) > swipeDistance.y) {
                    if (diff.y > 0) {
                        playerController.Jump(false);
                        acceptInput = false;
                    } else if (diff.y < 0) {
                        playerController.Slide();
                        acceptInput = false;
                    }
                    touchStartPosition = touch.position;
                } else if (verticalPercent < (1 / swipeSensitivty) && Mathf.Abs(diff.x) > swipeDistance.x) {
                    // turn if above a turn, otherwise move horizontally
                    if (swipeToMoveHorizontally) {
                        if (playerController.AbovePlatform(true)) {
                            playerController.Turn(diff.x > 0 ? true : false, true);
                        } else if (freeHorizontalMovement) {
                            playerController.MoveHorizontally(diff.x);
                        } else {
                            playerController.ChangeSlots(diff.x > 0 ? true : false);
                        }
                    } else {
                        playerController.Turn(diff.x > 0 ? true : false, true);
                    }
                    acceptInput = false;
                }
            } else if (touch.phase == TouchPhase.Stationary) {
                acceptInput = true;
            } else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) {
                if ((touch.position - touchStartPosition).sqrMagnitude < 100 && acceptInput) {
                    playerController.Attack();
                }
                acceptInput = true;
            }
        }

        if (!swipeToMoveHorizontally)
            CheckHorizontalPosition(Input.acceleration.x);
#else
        if (sameKeyForTurnHorizontalMovement) {
            bool hasTurned = false;
            if (Input.GetButtonDown("LeftTurn")) {
                hasTurned = playerController.Turn(false, true);
            } else if (Input.GetButtonDown("RightTurn")) {
                hasTurned = playerController.Turn(true, true);
            }

            // can move horizontally if the player hasn't turned
            if (!hasTurned) {
                if (freeHorizontalMovement) {
                    if (Input.GetButtonDown("LeftSlot")) {
                        playerController.MoveHorizontally(-horizontalMovementDelta);
                    } else if (Input.GetButtonDown("RightSlot")) {
                        playerController.MoveHorizontally(horizontalMovementDelta);
                    }
                } else {
                    if (Input.GetButtonDown("LeftSlot")) {
                        playerController.ChangeSlots(false);
                    } else if (Input.GetButtonDown("RightSlot")) {
                        playerController.ChangeSlots(true);
                    }
                }
            }
        } else {
            if (freeHorizontalMovement) {
                if (Input.GetButtonDown("LeftSlot")) {
                    playerController.MoveHorizontally(-horizontalMovementDelta);
                } else if (Input.GetButtonDown("RightSlot")) {
                    playerController.MoveHorizontally(horizontalMovementDelta);
                }
            } else {
                if (Input.GetButtonDown("LeftSlot")) {
                    playerController.ChangeSlots(false);
                } else if (Input.GetButtonDown("RightSlot")) {
                    playerController.ChangeSlots(true);
                }
            }

            if (Input.GetButtonDown("LeftTurn")) {
                playerController.Turn(false, true);
            } else if (Input.GetButtonDown("RightTurn")) {
                playerController.Turn(true, true);
            }
        }

        if (Input.GetButtonDown("Jump")) {
            playerController.Jump(false);
        } else if (Input.GetButtonDown("Slide")) {
            playerController.Slide();
        } else if (Input.GetButtonDown("Attack")) {
            playerController.Attack();
        }
        
        // Move horizontally if the player moves their mouse more than mouseXDeltaValue within a specified amount of time
        if (useMouseToMoveHorizontally) {
            if (Input.mousePosition.x != mouseStartPosition) {
                if (Time.time - mouseStartTime < 0.5f) {
                    float delta = Input.mousePosition.x - mouseStartPosition;
                    bool reset = false;
                    if (freeHorizontalMovement) {
                        playerController.MoveHorizontally(delta / horizontalMovementSensitivity);
                        reset = true;
                    } else {
                        if (delta > mouseXDeltaValue) {
                            playerController.ChangeSlots(true);
                            reset = true;
                        } else if (delta < -mouseXDeltaValue) {
                            playerController.ChangeSlots(false);
                            reset = true;
                        }
                    }
                    if (reset) {
                        mouseStartTime = Time.time;
                        mouseStartPosition = Input.mousePosition.x;
                    }
                } else {
                    mouseStartTime = Time.time;
                    mouseStartPosition = Input.mousePosition.x;
                }
            }
        }
#endif
    }

#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID || UNITY_BLACKBERRY || UNITY_WP8)
    private void CheckHorizontalPosition(float tiltValue)
    {
        if (freeHorizontalMovement) {
            playerController.MoveHorizontally(tiltValue);
        } else {
            SlotPosition currentSlot = playerController.GetCurrentSlotPosition();
            switch (currentSlot) {
                case SlotPosition.Center:
                    if (tiltValue < -accelerometerRightSlotValue) {
                        playerController.ChangeSlots(SlotPosition.Left);
                    } else if (tiltValue > accelerometerRightSlotValue) {
                        playerController.ChangeSlots(SlotPosition.Right);
                    }
                    break;
                case SlotPosition.Left:
                    if (tiltValue > -accelerometerRightSlotValue + accelerometerSensitivity) {
                        playerController.ChangeSlots(SlotPosition.Center);
                    }
                    break;
                case SlotPosition.Right:
                    if (tiltValue < accelerometerRightSlotValue - accelerometerSensitivity) {
                        playerController.ChangeSlots(SlotPosition.Center);
                    }
                    break;
            }

        }
    }
#endif
}


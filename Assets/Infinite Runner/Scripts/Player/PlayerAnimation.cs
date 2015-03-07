using UnityEngine;
using System.Collections;
using InfiniteRunner.Game;

namespace InfiniteRunner.Player
{
    /*
     * Basic class to manage the different animation states
     */
    public class PlayerAnimation : MonoBehaviour
    {
        public bool useMecanim = false;

        // the amount of time it takes to transition from the previous animation to a run
        public float runTransitionTime;

        // animation names
        public string runAnimationName = "Run";
        public string runJumpAnimationName = "RunJump";
        public string runSlideAnimationName = "RunSlide";
        public string runRightStrafeAnimationName = "RunRightStrafe";
        public string runLeftStrafeAnimationName = "RunLeftStrafe";
        public string attackAnimationName = "Attack";
        public string backwardDeathAnimationName = "BackwardDeath";
        public string forwardDeathAnimationName = "ForwardDeath";
        public string idleAnimationName = "Idle";

        // the speed of the run animation when the player is running
        public float slowRunSpeed = 1.0f;
        public float fastRunSpeed = 1.0f;

        private Animation thisAnimation;
        private Animator thisAnimator;
        private int jumpHash;
        private int slideHash;
        private int attackHash;
        private int leftStrafeHash;
        private int rightStrafeHash;
        private int forwardDeathHash;
        private int backwardDeathHash;

        public void Init()
        {
            if (useMecanim) {
                thisAnimator = GetComponent<Animator>();

                jumpHash = Animator.StringToHash("Base Layer.RunJump");
                slideHash = Animator.StringToHash("Base Layer.RunSlide");
                attackHash = Animator.StringToHash("Base Layer.Attack");
                rightStrafeHash = Animator.StringToHash("Base Layer.RunRightStrafe");
                leftStrafeHash = Animator.StringToHash("Base Layer.RunLeftStrafe");
                forwardDeathHash = Animator.StringToHash("Base Layer.ForwardDeath");
                backwardDeathHash = Animator.StringToHash("Base Layer.BackwardDeath");
            } else {
                thisAnimation = GetComponent<Animation>();

                thisAnimation[runAnimationName].wrapMode = WrapMode.Loop;
                thisAnimation[runAnimationName].layer = 0;
                if (!string.IsNullOrEmpty(runRightStrafeAnimationName)) {
                    thisAnimation[runRightStrafeAnimationName].wrapMode = WrapMode.Once;
                    thisAnimation[runRightStrafeAnimationName].layer = 1;
                }
                if (!string.IsNullOrEmpty(runLeftStrafeAnimationName)) {
                    thisAnimation[runLeftStrafeAnimationName].wrapMode = WrapMode.Once;
                    thisAnimation[runLeftStrafeAnimationName].layer = 1;
                }
                if (!string.IsNullOrEmpty(runJumpAnimationName)) {
                    thisAnimation[runJumpAnimationName].wrapMode = WrapMode.ClampForever;
                    thisAnimation[runJumpAnimationName].layer = 1;
                }
                if (!string.IsNullOrEmpty(runSlideAnimationName)) {
                    thisAnimation[runSlideAnimationName].wrapMode = WrapMode.ClampForever;
                    thisAnimation[runSlideAnimationName].layer = 1;
                }
                if (!string.IsNullOrEmpty(attackAnimationName)) {
                    thisAnimation[attackAnimationName].wrapMode = WrapMode.Once;
                    thisAnimation[attackAnimationName].layer = 1;
                }
                if (!string.IsNullOrEmpty(backwardDeathAnimationName)) {
                    thisAnimation[backwardDeathAnimationName].wrapMode = WrapMode.ClampForever;
                    thisAnimation[backwardDeathAnimationName].layer = 2;
                }
                if (!string.IsNullOrEmpty(forwardDeathAnimationName)) {
                    thisAnimation[forwardDeathAnimationName].wrapMode = WrapMode.ClampForever;
                    thisAnimation[forwardDeathAnimationName].layer = 2;
                }
                thisAnimation[idleAnimationName].wrapMode = WrapMode.Loop;
                thisAnimation[idleAnimationName].layer = 3;
            }
        }

        public void OnEnable()
        {
            GameManager.instance.OnPauseGame += OnPauseGame;
        }

        public void OnDisable()
        {
            GameManager.instance.OnPauseGame -= OnPauseGame;
        }

        public void Update()
        {
            // In some cases mecanim doesn't transition to the new state correctly. Instead of relying on mecanim to do the transition, we can do it ourselves.
            // Reset the bool parameter as soon as Mecanim actually arrives at the desired state.
            if (thisAnimator != null) {
#if (UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6)
                int stateHash = thisAnimator.GetNextAnimatorStateInfo(0).nameHash;
#else
                int stateHash = thisAnimator.GetNextAnimatorStateInfo(0).fullPathHash;
#endif
                if (stateHash == jumpHash) {
                    thisAnimator.SetBool(runJumpAnimationName, false);
                } else if (stateHash == slideHash) {
                    thisAnimator.SetBool(runSlideAnimationName, false);
                } else if (stateHash == attackHash) {
                    thisAnimator.SetBool(attackAnimationName, false);
                } else if (stateHash == leftStrafeHash) {
                    thisAnimator.SetBool(runLeftStrafeAnimationName, false);
                } else if (stateHash == rightStrafeHash) {
                    thisAnimator.SetBool(runRightStrafeAnimationName, false);
                } else if (stateHash == forwardDeathHash) {
                    thisAnimator.SetBool(forwardDeathAnimationName, false);
                } else if (stateHash == backwardDeathHash) {
                    thisAnimator.SetBool(backwardDeathAnimationName, false);
                }
            }
        }

        public void Run()
        {
            if (!useMecanim) {
                thisAnimation.CrossFade(runAnimationName, runTransitionTime, PlayMode.StopAll);
            }
        }

        public void SetRunSpeed(float speed, float t)
        {
            if (useMecanim) {
                thisAnimator.SetFloat("Speed", speed);
            } else {
                thisAnimation[runAnimationName].speed = Mathf.Lerp(slowRunSpeed, fastRunSpeed, t);
            }
        }

        public void Strafe(bool right)
        {
            if (useMecanim) {
                if (right) {
                    thisAnimator.SetBool(runRightStrafeAnimationName, true);
                } else {
                    thisAnimator.SetBool(runLeftStrafeAnimationName, true);
                }
            } else {
                if (right) {
                    thisAnimation.CrossFade(runRightStrafeAnimationName, 0.05f);
                } else {
                    thisAnimation.CrossFade(runLeftStrafeAnimationName, 0.05f);
                }
            }
        }

        public void Jump()
        {
            if (useMecanim) {
                thisAnimator.SetBool(runJumpAnimationName, true);
            } else {
                thisAnimation.CrossFade(runJumpAnimationName, 0.1f);
            }
        }

        public void Slide()
        {
            if (useMecanim) {
                thisAnimator.SetBool(runSlideAnimationName, true);
            } else {
                thisAnimation.CrossFade(runSlideAnimationName);
            }
        }

        public void Attack()
        {
            if (useMecanim) {
                thisAnimator.SetBool(attackAnimationName, true);
            } else {
                thisAnimation.CrossFade(attackAnimationName, 0.1f);
            }
        }

        public void Idle()
        {
            if (!useMecanim) {
                if (thisAnimation == null) {
                    thisAnimation = GetComponent<Animation>();
                    thisAnimation[idleAnimationName].wrapMode = WrapMode.Loop;
                }
                thisAnimation.Play(idleAnimationName);
            }
        }

        public void GameOver(GameOverType gameOverType)
        {
            if (!useMecanim) {
                thisAnimation.Stop(runAnimationName);
            }

            if (gameOverType != GameOverType.Quit) {
                if (gameOverType == GameOverType.JumpObstacle) {
                    if (useMecanim) {
                        thisAnimator.SetBool(forwardDeathAnimationName, true);
                    } else {
                        thisAnimation.Play(forwardDeathAnimationName);
                    }
                } else {
                    if (useMecanim) {
                        thisAnimator.SetBool(backwardDeathAnimationName, true);
                    } else {
                        thisAnimation.Play(backwardDeathAnimationName);
                    }
                }
            }
        }

        public void ResetValues()
        {
            if (useMecanim) {
                thisAnimator.SetFloat("Speed", 0);
                thisAnimator.SetBool(runJumpAnimationName, false);
                thisAnimator.SetBool(runSlideAnimationName, false);
                thisAnimator.SetBool(attackAnimationName, false);
                thisAnimator.SetBool(runRightStrafeAnimationName, false);
                thisAnimator.SetBool(runLeftStrafeAnimationName, false);
            } else {
                thisAnimation.Play(idleAnimationName);
            }
        }

        public void OnPauseGame(bool paused)
        {
            float speed = (paused ? 0 : 1);
            if (useMecanim) {
                thisAnimator.speed = speed;
            } else {
                foreach (AnimationState state in thisAnimation) {
                    state.speed = speed;
                }
            }
        }
    }
}
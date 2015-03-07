using UnityEngine;
using InfiniteRunner.InfiniteObjects;

namespace InfiniteRunner.Player
{
    /*
     * Turn on any track turns when the invincibility power up is active
     */
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class TurnTrigger : MonoBehaviour
    {
        private PlatformObject platform;
        private bool hasTurned;

        public void Start()
        {
            platform = transform.parent.GetComponent<PlatformObject>();
        }

        public void OnEnable()
        {
            hasTurned = false;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player") && !hasTurned) {
                //randomly choose left or right if the turn is available
                bool rotateRight = Random.value > 0.5f;
                if ((rotateRight && !platform.rightTurn) || (!rotateRight && !platform.leftTurn))
                    rotateRight = !rotateRight;

                // let the player controller decide if the player should really turn
                PlayerController.instance.Turn(rotateRight, false);
                hasTurned = true;
            }
        }

        // OnEnable will not be called if optimize deactivation is enabled
        public void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player") && hasTurned) {
                hasTurned = false;
            }
        }
    }
}
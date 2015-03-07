using UnityEngine;

namespace InfiniteRunner.Player
{
    /*
     * Jump when the invincibility power up is active
     */
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class JumpTrigger : MonoBehaviour
    {
        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player")) {
                PlayerController.instance.Jump(true);
            }
        }
    }
}
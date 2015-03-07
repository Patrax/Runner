using UnityEngine;
using InfiniteRunner.Game;

namespace InfiniteRunner.Player
{
    // The player feel into a bad area. Die.
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class DeathTrigger : MonoBehaviour
    {
        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player")) {
                GameManager.instance.GameOver(GameOverType.Pit, true);
            }
        }
    }
}
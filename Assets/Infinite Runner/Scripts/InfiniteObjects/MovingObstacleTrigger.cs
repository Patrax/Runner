using UnityEngine;

namespace InfiniteRunner.InfiniteObjects
{
    public class MovingObstacleTrigger : MonoBehaviour
    {
        public MovingObstacleObject parent;

        public void OnTriggerEnter(Collider other)
        {
            parent.OnTriggerEnter(other);
        }
    }
}
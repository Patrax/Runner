using UnityEngine;
using System.Collections;
using InfiniteRunner.Game;

namespace InfiniteRunner.Player
{
    /*
     * Show text on how to control the game
     */
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class TutorialTrigger : MonoBehaviour
    {
        public TutorialType tutorialType;

        private int playerLayer;

        public void Awake()
        {
            playerLayer = LayerMask.NameToLayer("Player");
            enabled = false;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == playerLayer) {
                StartCoroutine(ShowTutorial(true));
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == playerLayer) {
                StartCoroutine(ShowTutorial(false));
            }
        }

        private IEnumerator ShowTutorial(bool show)
        {
            yield return new WaitForEndOfFrame();

            GUIManager.instance.ShowTutorial(show, tutorialType);
        }
    }
}
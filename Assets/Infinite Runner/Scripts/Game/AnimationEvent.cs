using UnityEngine;
using System.Collections;

namespace InfiniteRunner.Game
{
    /*
     * Callback when the in game mission notification panel animation ends.
     */
    public class AnimationEvent : MonoBehaviour
    {
        public void AnimationComplete()
        {
            StartCoroutine(GUIManager.instance.InGameMissionPanelHidden());
        }
    }
}
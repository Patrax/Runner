using UnityEngine;
using InfiniteRunner;

namespace InfiniteRunner
{
    /*
     * Used by uGUI - is triggered to play an animation after a click event
     */
    public class PlayAnimationEvent : MonoBehaviour
    {
        public Animation[] target;
        public string[] clipName;
        public bool[] playForward;
        public bool[] positionAtEnd;

        public void Play()
        {
            for (int i = 0; i < target.Length; ++i) {
                target[i].gameObject.SetActive(true);
                target[i].enabled = true;
                target[i].Stop();
                // Set the speed and time depending on if the animation is playing normally or in reverse. If the animation is playing in reverse then the time should be at the end
                // so the animation will move it to the start 
                if (playForward[i]) {
                    target[i][clipName[i]].speed = 1;
                    target[i][clipName[i]].time = (positionAtEnd[i] ? target[i][clipName[i]].length : 0);
                } else {
                    target[i][clipName[i]].speed = -1;
                    target[i][clipName[i]].time = (positionAtEnd[i] ? 0 : target[i][clipName[i]].length);
                }
                target[i].Play(clipName[i]);
            }
        }
    }
}
using UnityEngine;
using InfiniteRunner.InfiniteGenerator;

namespace InfiniteRunner.InfiniteObjects
{
    /*
     * The scene surrounds the platforms
     */
    [RequireComponent(typeof(AppearanceProbability))]
    [RequireComponent(typeof(SceneAppearanceRules))]
    public class SceneObject : InfiniteObject
    {

        // Override the size if the object manager can't get the size right (a value of Vector3.zero will let the object manager calculate the size)
        public Vector3 overrideSize;

        // True if this piece is used for section transitions
        [HideInInspector]
        public bool sectionTransition;

        // Set this offset if the scene object's center doesn't match up with the true center (such as with just left or right corners)
        public Vector3 centerOffset;

        public override void Init()
        {
            base.Init();
            objectType = ObjectType.Scene;
        }
    }
}
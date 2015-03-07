using UnityEngine;
using System.Collections.Generic;
using InfiniteRunner.InfiniteGenerator;

namespace InfiniteRunner.InfiniteObjects
{
    public enum PlatformSlope { None, Up, Down }
    /*
     * A platform is the base track layer. It can hold up to n collidable objects within the left, center, and right slots.
     */
    [RequireComponent(typeof(AppearanceProbability))]
    [RequireComponent(typeof(PlatformAppearanceRules))]
    public class PlatformObject : InfiniteObject
    {

        public delegate void PlatformDeactivationHandler();
        public event PlatformDeactivationHandler OnPlatformDeactivation;

        // Override the size if the object manager can't get the size right (a value of Vector3.zero will let the object manager calculate the size)
        public Vector3 overrideSize;

        // Set this offset if the platform object's center doesn't match up with the true center (such as with just left or right corners)
        public Vector3 centerOffset;

        // True if this piece is used for section transitions
        [HideInInspector]
        public bool sectionTransition;
        // If section transition is true, this list contains the sections that it can transition from (used with the toSection list)
        [HideInInspector]
        public List<int> fromSection;
        // If section transition is true, this list contains the sections that it can transition to (used with the fromSection list)
        [HideInInspector]
        public List<int> toSection;

        // Direction of platform. At least one option must be true. Straight is the most common so is the default
        public bool straight = true;
        public bool leftTurn;
        public bool rightTurn;

        // The platform has a slope if it changes heights. The object generator will then take into account the height of the platform and its associated
        // scene object. A downward slope is when the start of the platform is higher than the end of the platform
        public PlatformSlope slope;

        // force different collidable object types to spawn on top of the platform, such as obstacle and coin
        // (assuming the propabilities allow the object to spawn)
        public bool forceDifferentCollidableTypes;

        // the number of collidable objects that can fit on one platform. The objects are spaced along the local z position of the platform
        public int collidablePositions;

        // boolean to determine what horizontal location objects can spawn. If collidablePositions is greater than 0 then at least one
        // of these booleans must be true
        public bool collidableLeftSpawn;
        public bool collidableCenterSpawn;
        public bool collidableRightSpawn;

        // the list of control points if the platform is a curve
        [HideInInspector]
        public List<Vector3> controlPoints;
        // a mapping of the distance of the curve to the starting control point
        [HideInInspector]
        public List<float> curveIndexDistanceMap;
        // the length of the total curve
        [HideInInspector]
        public float curveLength;

        private int slotPositionsAvailable;
        private int collidableSpawnPosition;

        // true if a scene object has linked to this platform. No other scene objects will be able to spawn near this object.
        private bool requireLinkedSceneObject;

        public override void Init()
        {
            base.Init();
            objectType = ObjectType.Platform;

            // notify the user that curved platform turns currently don't support branching
            if (curveLength > 0 && leftTurn && rightTurn) {
                Debug.LogError(string.Format("Error: curved platform turns do not allow branching, select either left turn or right turn (affecting object {0}) ", name));
            }
        }

        public override void Awake()
        {
            base.Awake();
            collidableSpawnPosition = 0;
            requireLinkedSceneObject = false;

            slotPositionsAvailable = 0;
            if (collidableLeftSpawn) {
                slotPositionsAvailable |= 1;
            }
            if (collidableCenterSpawn) {
                slotPositionsAvailable |= 2;
            }
            if (collidableRightSpawn) {
                slotPositionsAvailable |= 4;
            }

            CollidableObject[] collidableObjects = GetComponentsInChildren<CollidableObject>();
            for (int i = 0; i < collidableObjects.Length; ++i) {
                collidableObjects[i].SetStartParent(collidableObjects[i].transform.parent);
            }

            // If this platfom doesn't have any direction enabled then it won't do anything
            if (!straight && !leftTurn && !rightTurn) {
                Debug.LogWarning(thisGameObject.name + " has no direction set.");
                straight = true;
            }
        }

        public void EnableLinkedSceneObjectRequired()
        {
            requireLinkedSceneObject = true;
        }

        public bool LinkedSceneObjectRequired()
        {
            return requireLinkedSceneObject;
        }

        public int GetSlotsAvailable()
        {
            return slotPositionsAvailable;
        }

        // Determine if an object is already spawned in the same position. Do this using bitwise and/or.
        // For example, the following situation might occur:
        // Spawn pos 3:
        // 0 |= (2 ^ 3), result of 0000 1000 (decimal 8)
        // Spawn pos 1:
        // 8 |= (2 ^ 1), result of 0000 1010 (decimal 10)
        // Check pos 3:
        // 10 & (2 ^ 3), result of 0000 1000 (decimal 8), position is not free
        // Check pos 2:
        // 10 & (2 ^ 2), result of 0000 0000 (decimal 0), space is free
        // Spawn pos 0:
        // 10 |= (2 ^ 0), result of 0000 1011 (decimal 11)
        public bool CanSpawnCollidable(int pos)
        {
            return (collidableSpawnPosition & (int)Mathf.Pow(2, pos)) == 0;
        }

        public bool CanSpawnCollidable()
        {
            return collidablePositions != 0 && collidableSpawnPosition != (int)Mathf.Pow(2, collidablePositions) - 1;
        }

        public void CollidableSpawned(int pos)
        {
            collidableSpawnPosition |= (int)Mathf.Pow(2, pos);
        }

        public override void Orient(Vector3 position, Quaternion rotation)
        {
            base.Orient(position, rotation);

            // reset the number of collidables that have been spawned on top of the platform
            collidableSpawnPosition = 0;
        }

        public override void Deactivate()
        {
            // platforms have collidable children. Make sure they get deactivated properly
            if (OnPlatformDeactivation != null) {
                OnPlatformDeactivation();
                OnPlatformDeactivation = null;
            }

            base.Deactivate();
        }
    }
}
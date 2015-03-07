using UnityEngine;
using System.Collections.Generic;
using InfiniteRunner.InfiniteObjects;

namespace InfiniteRunner.InfiniteGenerator
{
    [System.Serializable]
    public class ScoreObjectCountRule
    {
        // True if the minimum distance applies to any object of the same type as the object this script is attached to
        public bool minDistanceSameObjectType;

        // Minimum distance is the minimum distance that two objects can spawn next to each other.
        // For example:
        //	Object			Location
        //	PlatformA		50
        //	PlatformB		60
        //	PlatformC		65
        //	PlatformB		70
        //	PlatformA		75
        // If PlatformB had a minimum distance of 15 then it would not have been able to spawn the second time. In the case
        // of PlatformA, it could be considered to have a minimum distance of 25. minDistance doesn't have to apply to two of the
        // same objects. In this example, PlatformA could also have a minimum distance of 10 from PlatformC.
        public int minDistance;

        // The number of objects which must be in between two objects
        // Consider the same order of platforms as the last example:
        //	Object			Spawn Index
        //	PlatformA		5
        //	PlatformB		6
        //	PlatformC		7
        //	PlatformB		8
        //	PlatformA		9
        // In this example, PlatformA could have a minimum object count of 3 when compared to itself. Similarly, PlatformC may have had
        // a mimimum object count of 1 compared to PlatformA. 
        public int minObjectSeparation;

        // Used by probability adjustment rules, this will force a particular object to always spawn after the current object
        public bool forceOccurance;

        public DistanceValue probability;

        private InfiniteObjectHistory infiniteObjectHistory;

        public ScoreObjectCountRule(int md, bool mdsot, int mos, DistanceValue p, bool fo)
        {
            minDistance = md;
            minDistanceSameObjectType = mdsot;
            minObjectSeparation = mos;
            probability = p;
            forceOccurance = fo;
        }

        public void Init(InfiniteObjectHistory objectHistory)
        {
            infiniteObjectHistory = objectHistory;
        }

        public bool CanSpawnObject(float distance, ObjectType thisObjectType, int targetObjectIndex, ObjectType targetObjectType)
        {
            // return true if the parameters do not apply to the current distance
            if (!probability.WithinDistance(distance))
                return true;

            // The target object doesn't matter if we are using objects of the same object type
            float totalDistance = infiniteObjectHistory.GetTotalDistance(thisObjectType == ObjectType.Scene);
            if (minDistanceSameObjectType) {
                // lastSpawnDistance: the distance of the last object spawned of the inputted object type
                float lastSpawnDistance = infiniteObjectHistory.GetLastObjectTypeSpawnDistance(thisObjectType);
                if (totalDistance - lastSpawnDistance <= minDistance) {
                    return false;
                }
            }

            // The rest of the tests need the target object, so if there is no target object then we are done early
            if (targetObjectIndex == -1)
                return true;

            // objectSpawnIndex: spawn index of the last object of the same type (for example, the last duck obstacle spawn index)
            int objectSpawnIndex = infiniteObjectHistory.GetObjectSpawnIndex(targetObjectIndex);
            // can always spawn if the object hasn't been spawned before and it is within the probabilities
            if (objectSpawnIndex == -1)
                return true;

            // latestSpawnIndex: spawn index of the latest object type
            int latestSpawnIndex = infiniteObjectHistory.GetObjectTypeSpawnIndex(targetObjectType);
            // can't spawn if there isn't enough object separation
            if (latestSpawnIndex - objectSpawnIndex <= minObjectSeparation)
                return false;

            // objectLastDistance: distance of the last spawned object of the same type
            float objectLastDistance = infiniteObjectHistory.GetSpawnDistance(targetObjectIndex);
            // can't spawn if we are too close to another object
            if (totalDistance - objectLastDistance <= minDistance)
                return false;

            // looks like we can spawn
            return true;
        }

        // probability adjustment is the opposite of can spawn object. Only return the adjusted probability of the object in question is within the object
        // count / distance specified. Otherwise, if it is outside of that range, return -1 meaning no adjustment.
        public float ProbabilityAdjustment(float distance, int targetObjectIndex, ObjectType targetObjectType)
        {
            // objectSpawnIndex: spawn index of the last object of the same type (for example, the last duck obstacle spawn index)
            int objectSpawnIndex = infiniteObjectHistory.GetObjectSpawnIndex(targetObjectIndex);
            // No probability adjustment if the target object hasn't even spawned yet
            if (objectSpawnIndex == -1)
                return -1;

            // latestSpawnIndex: spawn index of the latest object type
            int latestSpawnIndex = infiniteObjectHistory.GetObjectTypeSpawnIndex(targetObjectType);
            // No probability adjustment if we are outside the range of the minimum object separation
            if (minObjectSeparation != 0 && latestSpawnIndex - objectSpawnIndex > minObjectSeparation)
                return -1;

            float totalDistance = infiniteObjectHistory.GetTotalDistance(targetObjectType == ObjectType.Scene);
            // objectLastDistance: distance of the last spawned object of the same type
            float objectLastDistance = infiniteObjectHistory.GetSpawnDistance(targetObjectIndex);
            // No probability adjustment if we are outside the range of the minimum distance
            if (minDistance != 0 && totalDistance - objectLastDistance > minDistance)
                return -1;

            // Return the maximum value if we are forcing the occurance. The Infinite Object Manager will always spawn objects with a probability equal to the max float value
            if (forceOccurance) {
                return float.MaxValue;
            }

            return probability.GetValue(distance);
        }
    }

    /**
     * The object rule map links and object to its corresponding rules. The rules are described more in the class above (ScoreObjectCountRule), 
     * but the rules may prevent the object from spawning or change the probability that an object spawns based on another object occurring before it.
     */
    [System.Serializable]
    public class ObjectRuleMap
    {
        public InfiniteObject targetObject;
        public List<ScoreObjectCountRule> rules;

        private int targetObjectIndex; // the object index of the infinite object that we are interested in
        private bool targetObjectIsScene; // is the target object a scene object
        private ObjectType thisObjectType;

        public ObjectRuleMap(InfiniteObject io, ScoreObjectCountRule r)
        {
            targetObject = io;
            rules = new List<ScoreObjectCountRule>();
            rules.Add(r);
        }

        public void Init(InfiniteObjectHistory objectHistory, ObjectType objectType)
        {
            targetObjectIndex = -1;
            thisObjectType = objectType;
            for (int i = 0; i < rules.Count; ++i) {
                rules[i].Init(objectHistory);
            }
        }

        public bool AssignIndexToObject(InfiniteObject obj, int index)
        {
            if (targetObject == null) {
                return false;
            }

            if (obj == targetObject) {
                targetObjectIndex = index;
                targetObjectIsScene = targetObject.GetObjectType() == ObjectType.Scene;
                return true;
            }

            return false;
        }

        // Objects may not be able to be spawned if they are too close to another object, for example
        public bool CanSpawnObject(float distance)
        {
            for (int i = 0; i < rules.Count; ++i) {
                if (!rules[i].CanSpawnObject(distance, thisObjectType, targetObjectIndex, (targetObject != null ? targetObject.GetObjectType() : ObjectType.Last))) {
                    return false;
                }
            }
            return true;
        }

        // The probability of this object occuring can be based on the previous objects spawned.
        public bool ProbabilityAdjustment(InfiniteObjectHistory infiniteObjectHistory, float distance, ref float localDistance, ref float probability)
        {
            if (targetObjectIndex == -1) {
                Debug.LogError(string.Format("ObjectRuleMap:ProbabilityAdjustment error: target object {0} doesn't exist. Ensure the target object has been added to the Infinite Object Manager.", targetObject));
                return false;
            }
            for (int i = 0; i < rules.Count; ++i) {
                if ((probability = rules[i].ProbabilityAdjustment(distance, targetObjectIndex, targetObject.GetObjectType())) != -1) {
                    localDistance = infiniteObjectHistory.GetTotalDistance(targetObjectIsScene) - infiniteObjectHistory.GetSpawnDistance(targetObjectIndex);
                    return true;
                }
            }
            return false;
        }
    }

    /**
     * Each object can have multiple object rules map. This class will keep track of them all and use the correct one when CanSpawnObject or
     * probabilityAdjustment is called.
     */
    public class AppearanceRules : MonoBehaviour
    {

        // Don't spawn an object if it is within a predefined distance of another object
        public List<ObjectRuleMap> avoidObjectRuleMaps;
        // Allows the probability of an object to be changed based on previous objects
        public List<ObjectRuleMap> probabilityAdjustmentMaps;

        protected InfiniteObjectHistory infiniteObjectHistory;

        protected InfiniteObject thisInfiniteObject;

        public virtual void Init()
        {
            infiniteObjectHistory = InfiniteObjectHistory.instance;
            thisInfiniteObject = GetComponent<InfiniteObject>();

            ObjectType objectType = thisInfiniteObject.GetObjectType();
            for (int i = 0; i < avoidObjectRuleMaps.Count; ++i) {
                avoidObjectRuleMaps[i].Init(infiniteObjectHistory, objectType);
            }

            for (int i = 0; i < probabilityAdjustmentMaps.Count; ++i) {
                probabilityAdjustmentMaps[i].Init(infiniteObjectHistory, objectType);
            }
        }

        public virtual void AssignIndexToObject(InfiniteObject infiniteObject, int index)
        {
            for (int i = 0; i < avoidObjectRuleMaps.Count; ++i) {
                avoidObjectRuleMaps[i].AssignIndexToObject(infiniteObject, index);
            }

            for (int i = 0; i < probabilityAdjustmentMaps.Count; ++i) {
                probabilityAdjustmentMaps[i].AssignIndexToObject(infiniteObject, index);
            }
        }

        // Objects may not be able to be spawned if they are too close to another object, for example
        public virtual bool CanSpawnObject(float distance, ObjectSpawnData spawnData)
        {
            // can't spawn if the sections don't match up
            if (!thisInfiniteObject.CanSpawnInSection(spawnData.section)) {
                return false;
            }

            for (int i = 0; i < avoidObjectRuleMaps.Count; ++i) {
                if (!avoidObjectRuleMaps[i].CanSpawnObject(distance)) {
                    return false; // all it takes is one
                }
            }
            return true;
        }

        // The probability of this object occuring can be based on the previous objects spawned.
        public float ProbabilityAdjustment(float distance)
        {
            float closestObjectDistance = float.MaxValue;
            float closestProbabilityAdjustment = 1;
            float localDistance = 0;
            float probability = 0f;
            // Find the closest object within the probability adjustment map
            for (int i = 0; i < probabilityAdjustmentMaps.Count; ++i) {
                if (probabilityAdjustmentMaps[i].ProbabilityAdjustment(infiniteObjectHistory, distance, ref localDistance, ref probability)) {
                    // If the probability is equal to the maximum float value then this object must spawn
                    if (probability == float.MaxValue) {
                        return probability;
                    }
                    if (localDistance < closestObjectDistance) {
                        closestObjectDistance = localDistance;
                        closestProbabilityAdjustment = probability;
                    }
                }
            }
            return closestProbabilityAdjustment;
        }
    }

    /**
     * An infinite object may or may not be able to spawn with the last platform spawned. For example, it doesn't make sense to have a duck obstacle spawn on top of the
     * jump platform.
     */
    [System.Serializable]
    public class PlatformPlacementRule
    {
        public InfiniteObject platform;
        public bool canSpawn;

        private int platformIndex;

        public PlatformPlacementRule(InfiniteObject p, bool c)
        {
            platform = p;
            canSpawn = c;
        }

        public bool AssignIndexToObject(InfiniteObject infiniteObject, int index)
        {
            if (infiniteObject == platform) {
                platformIndex = index;
                return true;
            }
            return false;
        }

        public bool CanSpawnObject(int index)
        {
            if (index == platformIndex) {
                return canSpawn;
            }
            return !canSpawn;
        }
    }
}
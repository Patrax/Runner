using UnityEngine;
using System.Collections.Generic;
using InfiniteRunner.InfiniteObjects;

namespace InfiniteRunner.InfiniteGenerator
{
    /*
     * Infinite Object History keeps a record of the objects spawned which is mostly used by the appearance rules
     */
    public class InfiniteObjectHistory : MonoBehaviour
    {

        static public InfiniteObjectHistory instance;

        // local index is the index of the object within its own array
        // object index is the index of the object unique to all of the other objects (array independent)
        // spawn index is the index of the object spawned within its own object type.
        // 
        // For example, the following objects would have the corresponding local, object and spawn indexes:
        //
        // Name			Local Index		Object Index	Spawn Index     Notes
        // PlatformA	0				0 				0               
        // ObstacleA	0				3				0               ObstacleA has an object index of 3 because it is the third object in the complete object array:
        // PlatformB	1				1				1                   PlatformA, PlatformB, PlatformC, ObstacleA, ...
        // ObstacleA	0				3				1
        // PlatformC	2				2				2
        // ObstacleB	1				4				2
        // ObstacleC	2				5				3
        // PlatformA	0				0				3
        // ObstacleA	0				3				4
        // ObstacleC	2				5				5
        // PlatformC	2				2				4

        // The relative location of the objects being spawned: Center, Right, Left
        private ObjectLocation activeObjectLocation;

        // Spawn index for the given object index
        private List<int>[] objectSpawnIndex;
        // Spawn index for the given object type
        private int[][] objectTypeSpawnIndex;

        // local index for the given object type
        private int[][] lastLocalIndex;
        // spawn location (distance) for the given object index
        private List<float>[] lastObjectSpawnDistance;
        // distance of the last spawned object for the given object type
        private float[][] latestObjectTypeSpawnDistance;

        // angle for each object location
        private float[] objectLocationAngle;

        // The total distance spawned for both platforms and scenes
        private float[] totalDistance;
        private float[] totalSceneDistance;

        // distance at which the platform spawned. Indexes will be removed from this list when a scene object has spawned over it.
        private PlatformDistanceDataMap[] platformDistanceDataMap;

        // Keep track of the top-most and bottom-most objects in the scene hierarchy. When a new object is spawned, it is placed as the parent of the respective previous
        // objects. When the generator moves the platforms and scenes, it will only need to move the top-most object. It will also only need to check the bottom-most object
        // to see if it needs to be removed
        private InfiniteObject[] topPlatformObjectSpawned;
        private InfiniteObject[] bottomPlatformObjectSpawned;
        private InfiniteObject[] topSceneObjectSpawned;
        private InfiniteObject[] bottomSceneObjectSpawned;
        private InfiniteObject topTurnPlatformObjectSpawned;
        private InfiniteObject topTurnSceneObjectSpawned;
        private InfiniteObject bottomTurnPlatformObjectSpawned;
        private InfiniteObject bottomTurnSceneObjectSpawned;

        private InfiniteObject savedInfiniteObjects;

        // the previous section that occurred
        private int[] previousPlatformSection;
        private int[] previousSceneSection;
        private bool[] spawnedPlatformSectionTransition;
        private bool[] spawnedSceneSectionTransition;

        private InfiniteObjectManager infiniteObjectManager;

        public void Awake()
        {
            instance = this;
        }

        public void Init(int objectCount)
        {
            activeObjectLocation = ObjectLocation.Center;
            objectSpawnIndex = new List<int>[(int)ObjectLocation.Last];
            objectTypeSpawnIndex = new int[(int)ObjectLocation.Last][];
            lastLocalIndex = new int[(int)ObjectLocation.Last][];
            latestObjectTypeSpawnDistance = new float[(int)ObjectLocation.Last][];
            lastObjectSpawnDistance = new List<float>[(int)ObjectLocation.Last];

            objectLocationAngle = new float[(int)ObjectLocation.Last];

            totalDistance = new float[(int)ObjectLocation.Last];
            totalSceneDistance = new float[(int)ObjectLocation.Last];

            platformDistanceDataMap = new PlatformDistanceDataMap[(int)ObjectLocation.Last];

            topPlatformObjectSpawned = new InfiniteObject[(int)ObjectLocation.Last];
            bottomPlatformObjectSpawned = new InfiniteObject[(int)ObjectLocation.Last];
            topSceneObjectSpawned = new InfiniteObject[(int)ObjectLocation.Last];
            bottomSceneObjectSpawned = new InfiniteObject[(int)ObjectLocation.Last];

            previousPlatformSection = new int[(int)ObjectLocation.Last];
            previousSceneSection = new int[(int)ObjectLocation.Last];
            spawnedPlatformSectionTransition = new bool[(int)ObjectLocation.Last];
            spawnedSceneSectionTransition = new bool[(int)ObjectLocation.Last];

            for (int i = 0; i < (int)ObjectLocation.Last; ++i) {
                objectSpawnIndex[i] = new List<int>();
                objectTypeSpawnIndex[i] = new int[(int)ObjectType.Last];
                lastLocalIndex[i] = new int[(int)ObjectType.Last];
                latestObjectTypeSpawnDistance[i] = new float[(int)ObjectType.Last];

                lastObjectSpawnDistance[i] = new List<float>();

                platformDistanceDataMap[i] = new PlatformDistanceDataMap();

                for (int j = 0; j < objectCount; ++j) {
                    objectSpawnIndex[i].Add(-1);
                    lastObjectSpawnDistance[i].Add(0);
                }
                for (int j = 0; j < (int)ObjectType.Last; ++j) {
                    objectTypeSpawnIndex[i][j] = -1;
                    lastLocalIndex[i][j] = -1;
                    latestObjectTypeSpawnDistance[i][j] = -1;
                }
            }

            infiniteObjectManager = InfiniteObjectManager.instance;
        }

        // get the object history prepped for a new turn
        public void ResetTurnCount()
        {
            for (int i = 0; i < objectSpawnIndex[(int)ObjectLocation.Center].Count; ++i) {
                objectSpawnIndex[(int)ObjectLocation.Left][i] = objectSpawnIndex[(int)ObjectLocation.Right][i] = objectSpawnIndex[(int)ObjectLocation.Center][i];
                lastObjectSpawnDistance[(int)ObjectLocation.Left][i] = lastObjectSpawnDistance[(int)ObjectLocation.Right][i] = lastObjectSpawnDistance[(int)ObjectLocation.Center][i];
            }

            for (int i = 0; i < (int)ObjectLocation.Last; ++i) {
                objectTypeSpawnIndex[(int)ObjectLocation.Left][i] = objectTypeSpawnIndex[(int)ObjectLocation.Right][i] = objectTypeSpawnIndex[(int)ObjectLocation.Center][i];
                lastLocalIndex[(int)ObjectLocation.Left][i] = lastLocalIndex[(int)ObjectLocation.Right][i] = lastLocalIndex[(int)ObjectLocation.Center][i];
                latestObjectTypeSpawnDistance[(int)ObjectLocation.Left][i] = latestObjectTypeSpawnDistance[(int)ObjectLocation.Right][i] = latestObjectTypeSpawnDistance[(int)ObjectLocation.Center][i];
            }

            totalDistance[(int)ObjectLocation.Left] = totalDistance[(int)ObjectLocation.Right] = totalDistance[(int)ObjectLocation.Center];
            // on a turn, the scene catches up to the platforms, so the total scene distance equals the total distance
            totalSceneDistance[(int)ObjectLocation.Left] = totalSceneDistance[(int)ObjectLocation.Right] = totalDistance[(int)ObjectLocation.Center];
            objectLocationAngle[(int)ObjectLocation.Left] = objectLocationAngle[(int)ObjectLocation.Right] = objectLocationAngle[(int)ObjectLocation.Center];

            platformDistanceDataMap[(int)ObjectLocation.Left].ResetValues();
            platformDistanceDataMap[(int)ObjectLocation.Right].ResetValues();

            previousPlatformSection[(int)ObjectLocation.Left] = previousPlatformSection[(int)ObjectLocation.Right] = previousPlatformSection[(int)ObjectLocation.Center];
            previousSceneSection[(int)ObjectLocation.Left] = previousSceneSection[(int)ObjectLocation.Right] = previousSceneSection[(int)ObjectLocation.Center];
            spawnedPlatformSectionTransition[(int)ObjectLocation.Left] = spawnedPlatformSectionTransition[(int)ObjectLocation.Right] = spawnedPlatformSectionTransition[(int)ObjectLocation.Center];
            spawnedSceneSectionTransition[(int)ObjectLocation.Left] = spawnedSceneSectionTransition[(int)ObjectLocation.Right] = spawnedSceneSectionTransition[(int)ObjectLocation.Center];
        }

        // set the new active location
        public void SetActiveLocation(ObjectLocation location)
        {
            activeObjectLocation = location;
        }

        // the player has turned. Replace the center values with the corresponding turn values if they aren't -1
        public void Turn(ObjectLocation location)
        {
            for (int i = 0; i < objectSpawnIndex[(int)ObjectLocation.Center].Count; ++i) {
                lastObjectSpawnDistance[(int)ObjectLocation.Center][i] = lastObjectSpawnDistance[(int)location][i];
                if (objectSpawnIndex[(int)location][i] != -1) {
                    objectSpawnIndex[(int)ObjectLocation.Center][i] = objectSpawnIndex[(int)location][i];
                }
            }

            for (int i = 0; i < (int)ObjectLocation.Last; ++i) {
                if (objectTypeSpawnIndex[(int)location][i] != -1) {
                    objectTypeSpawnIndex[(int)ObjectLocation.Center][i] = objectTypeSpawnIndex[(int)location][i];
                }

                lastLocalIndex[(int)ObjectLocation.Center][i] = lastLocalIndex[(int)location][i];
                latestObjectTypeSpawnDistance[(int)ObjectLocation.Center][i] = latestObjectTypeSpawnDistance[(int)location][i];
            }

            totalDistance[(int)ObjectLocation.Center] = totalDistance[(int)location];
            totalSceneDistance[(int)ObjectLocation.Center] = totalSceneDistance[(int)location];
            objectLocationAngle[(int)ObjectLocation.Center] = objectLocationAngle[(int)location];

            platformDistanceDataMap[(int)ObjectLocation.Center].CopyFrom(platformDistanceDataMap[(int)location]);

            previousPlatformSection[(int)ObjectLocation.Center] = previousPlatformSection[(int)location];
            previousSceneSection[(int)ObjectLocation.Center] = previousSceneSection[(int)location];
            spawnedPlatformSectionTransition[(int)ObjectLocation.Center] = spawnedPlatformSectionTransition[(int)location];
            spawnedSceneSectionTransition[(int)ObjectLocation.Center] = spawnedSceneSectionTransition[(int)location];

            // use the center location if there aren't any objects in the location across from the turn location
            ObjectLocation acrossLocation = (location == ObjectLocation.Right ? ObjectLocation.Left : ObjectLocation.Right);
            if (bottomPlatformObjectSpawned[(int)acrossLocation] == null) {
                acrossLocation = ObjectLocation.Center;
            }

            if (topTurnPlatformObjectSpawned != null) {
                topTurnPlatformObjectSpawned.SetInfiniteObjectParent(topPlatformObjectSpawned[(int)ObjectLocation.Center]);
            } else {
                bottomTurnPlatformObjectSpawned = bottomPlatformObjectSpawned[(int)acrossLocation];
            }
            topTurnPlatformObjectSpawned = topPlatformObjectSpawned[(int)ObjectLocation.Center];
            if (topTurnSceneObjectSpawned != null) {
                topTurnSceneObjectSpawned.SetInfiniteObjectParent(topSceneObjectSpawned[(int)ObjectLocation.Center]);
            } else {
                bottomTurnSceneObjectSpawned = bottomSceneObjectSpawned[(int)acrossLocation];
            }
            topTurnSceneObjectSpawned = topSceneObjectSpawned[(int)ObjectLocation.Center];

            topPlatformObjectSpawned[(int)ObjectLocation.Center] = topPlatformObjectSpawned[(int)location];
            bottomPlatformObjectSpawned[(int)ObjectLocation.Center] = bottomPlatformObjectSpawned[(int)location];
            topSceneObjectSpawned[(int)ObjectLocation.Center] = topSceneObjectSpawned[(int)location];
            bottomSceneObjectSpawned[(int)ObjectLocation.Center] = bottomSceneObjectSpawned[(int)location];

            for (int i = (int)ObjectLocation.Left; i < (int)ObjectLocation.Last; ++i) {
                topPlatformObjectSpawned[i] = null;
                bottomPlatformObjectSpawned[i] = null;
                topSceneObjectSpawned[i] = null;
                bottomSceneObjectSpawned[i] = null;
            }
        }

        public InfiniteObject ObjectSpawned(int index, float locationOffset, ObjectLocation location, float angle, ObjectType objectType)
        {
            return ObjectSpawned(index, locationOffset, location, angle, objectType, null);
        }

        // Keep track of the object spawned. Returns the previous object at the top position
        public InfiniteObject ObjectSpawned(int index, float locationOffset, ObjectLocation location, float angle, ObjectType objectType, InfiniteObject infiniteObject)
        {
            lastObjectSpawnDistance[(int)location][index] = (objectType == ObjectType.Scene ? totalSceneDistance[(int)location] : totalDistance[(int)location]) + locationOffset;
            objectTypeSpawnIndex[(int)location][(int)objectType] += 1;
            objectSpawnIndex[(int)location][index] = objectTypeSpawnIndex[(int)location][(int)objectType];
            latestObjectTypeSpawnDistance[(int)location][(int)objectType] = lastObjectSpawnDistance[(int)location][index];
            lastLocalIndex[(int)location][(int)objectType] = infiniteObjectManager.ObjectIndexToLocalIndex(index, objectType);

            InfiniteObject prevTopObject = null;
            if (objectType == ObjectType.Platform) {
                prevTopObject = topPlatformObjectSpawned[(int)location];
                topPlatformObjectSpawned[(int)location] = infiniteObject;
                objectLocationAngle[(int)location] = angle;
            } else if (objectType == ObjectType.Scene) {
                prevTopObject = topSceneObjectSpawned[(int)location];
                topSceneObjectSpawned[(int)location] = infiniteObject;
            }

            return prevTopObject;
        }

        // the bottom infinite object only needs to be set for the very first object at the object location.. objectRemoved will otherwise take care of making sure the
        // bottom object is correct
        public void SetBottomInfiniteObject(ObjectLocation location, bool isSceneObject, InfiniteObject infiniteObject)
        {
            if (isSceneObject) {
                bottomSceneObjectSpawned[(int)location] = infiniteObject;
            } else {
                bottomPlatformObjectSpawned[(int)location] = infiniteObject;
            }
        }

        public void ObjectRemoved(ObjectLocation location, bool isSceneObject)
        {
            if (isSceneObject) {
                bottomSceneObjectSpawned[(int)location] = bottomSceneObjectSpawned[(int)location].GetInfiniteObjectParent();
                if (bottomSceneObjectSpawned[(int)location] == null) {
                    topSceneObjectSpawned[(int)location] = null;
                }
            } else {
                bottomPlatformObjectSpawned[(int)location] = bottomPlatformObjectSpawned[(int)location].GetInfiniteObjectParent();
                if (bottomPlatformObjectSpawned[(int)location] == null) {
                    topPlatformObjectSpawned[(int)location] = null;
                }
            }
        }

        public void TurnObjectRemoved(bool isSceneObject)
        {
            if (isSceneObject) {
                bottomTurnSceneObjectSpawned = bottomTurnSceneObjectSpawned.GetInfiniteObjectParent();
                if (bottomTurnSceneObjectSpawned == null) {
                    topTurnSceneObjectSpawned = null;
                }
            } else {
                bottomTurnPlatformObjectSpawned = bottomTurnPlatformObjectSpawned.GetInfiniteObjectParent();
                if (bottomTurnPlatformObjectSpawned == null) {
                    topTurnPlatformObjectSpawned = null;
                }
            }
        }

        // Increase the distance travelled by the specified amount
        public void AddTotalDistance(float amount, ObjectLocation location, bool isSceneObject, int section)
        {
            if (isSceneObject) {
                totalSceneDistance[(int)location] += amount;
                // truncate to prevent precision errors
                totalSceneDistance[(int)location] = ((int)(totalSceneDistance[(int)location] * 1000f)) / 1000f;
                // as time goes on totalDistance and totalSceneDistance become more separated because of the minor differences in sizes of the platforms/scenes.
                // prevent the two distances from becoming too out of sync by setting the platform distance to the scene distance if the distance between
                // the two is small
                if (Mathf.Abs(totalSceneDistance[(int)location] - totalDistance[(int)location]) < 0.1f) {
                    totalSceneDistance[(int)location] = totalDistance[(int)location];
                }
                platformDistanceDataMap[(int)location].CheckForRemoval(totalSceneDistance[(int)location]);
            } else {
                totalDistance[(int)location] += amount;
                // truncate to prevent precision errors
                totalDistance[(int)location] = ((int)(totalDistance[(int)location] * 1000f)) / 1000f;
                platformDistanceDataMap[(int)location].AddIndex(totalDistance[(int)location], lastLocalIndex[(int)location][(int)ObjectType.Platform], section);
            }
        }

        // returns the spawn index for the given object type
        public int GetObjectTypeSpawnIndex(ObjectType objectType)
        {
            return objectTypeSpawnIndex[(int)activeObjectLocation][(int)objectType];
        }

        // returns the spawn index for the given object index
        public int GetObjectSpawnIndex(int index)
        {
            return objectSpawnIndex[(int)activeObjectLocation][index];
        }

        // returns the local index for the given object type
        public int GetLastLocalIndex(ObjectType objectType)
        {
            return GetLastLocalIndex(activeObjectLocation, objectType);
        }

        // returns the local index for the given object type at the object location
        public int GetLastLocalIndex(ObjectLocation location, ObjectType objectType)
        {
            return lastLocalIndex[(int)location][(int)objectType];
        }

        // returns the spawn location (distance) for the given object index
        public float GetSpawnDistance(int index)
        {
            return lastObjectSpawnDistance[(int)activeObjectLocation][index];
        }

        // returns the distance of the last spawned object for the given object type
        public float GetLastObjectTypeSpawnDistance(ObjectType objectType)
        {
            return latestObjectTypeSpawnDistance[(int)activeObjectLocation][(int)objectType];
        }

        // returns the angle of location for a scene object or platform object
        public float GetObjectLocationAngle(ObjectLocation location)
        {
            return objectLocationAngle[(int)location];
        }

        // returns the total distance for a scene object or platform object
        public float GetTotalDistance(bool isSceneObject)
        {
            return (isSceneObject ? totalSceneDistance[(int)activeObjectLocation] : totalDistance[(int)activeObjectLocation]);
        }

        // returns the local index of the first platform
        public int GetFirstPlatformIndex()
        {
            return platformDistanceDataMap[(int)activeObjectLocation].FirstIndex();
        }

        // returns the section of the first platform
        public int GetFirstPlatformSection()
        {
            return platformDistanceDataMap[(int)activeObjectLocation].FirstSection();
        }

        // returns the top-most platform or scene object
        public InfiniteObject GetTopInfiniteObject(ObjectLocation location, bool isSceneObject)
        {
            return (isSceneObject ? topSceneObjectSpawned[(int)location] : topPlatformObjectSpawned[(int)location]);
        }

        // returns the bottom-most platform or scene object
        public InfiniteObject GetBottomInfiniteObject(ObjectLocation location, bool isSceneObject)
        {
            return (isSceneObject ? bottomSceneObjectSpawned[(int)location] : bottomPlatformObjectSpawned[(int)location]);
        }

        // returns the top-most turn platform or scene object
        public InfiniteObject GetTopTurnInfiniteObject(bool isSceneObject)
        {
            return (isSceneObject ? topTurnSceneObjectSpawned : topTurnPlatformObjectSpawned);
        }

        // returns the bottom-most turn platform or scene object
        public InfiniteObject GetBottomTurnInfiniteObject(bool isSceneObject)
        {
            return (isSceneObject ? bottomTurnSceneObjectSpawned : bottomTurnPlatformObjectSpawned);
        }

        // set everything back to 0 for a new game
        public void SaveObjectsReset()
        {
            // save off the current objects. They will be deactivated after new objects have been sapwned
            if (topPlatformObjectSpawned[(int)ObjectLocation.Center] != null) {
                savedInfiniteObjects = topPlatformObjectSpawned[(int)ObjectLocation.Center];

                for (int i = 0; i < (int)ObjectLocation.Last; ++i) {
                    if (i != (int)ObjectLocation.Center && topPlatformObjectSpawned[i])
                        topPlatformObjectSpawned[i].SetInfiniteObjectParent(savedInfiniteObjects);
                    if (bottomPlatformObjectSpawned[i] != null)
                        bottomPlatformObjectSpawned[i].SetInfiniteObjectParent(savedInfiniteObjects);
                    if (topSceneObjectSpawned[i] != null)
                        topSceneObjectSpawned[i].SetInfiniteObjectParent(savedInfiniteObjects);
                    if (bottomSceneObjectSpawned[i] != null)
                        bottomSceneObjectSpawned[i].SetInfiniteObjectParent(savedInfiniteObjects);

                    if (topTurnPlatformObjectSpawned != null)
                        topTurnPlatformObjectSpawned.SetInfiniteObjectParent(savedInfiniteObjects);
                }
            } else {
                // topPlatformObjectSpawned is null when the player turns the wrong way off of a turn
                savedInfiniteObjects = topTurnPlatformObjectSpawned;
            }

            if (bottomTurnPlatformObjectSpawned != null)
                bottomTurnPlatformObjectSpawned.SetInfiniteObjectParent(savedInfiniteObjects);
            if (topTurnSceneObjectSpawned != null)
                topTurnSceneObjectSpawned.SetInfiniteObjectParent(savedInfiniteObjects);
            if (bottomTurnSceneObjectSpawned != null)
                bottomTurnSceneObjectSpawned.SetInfiniteObjectParent(savedInfiniteObjects);

            activeObjectLocation = ObjectLocation.Center;
            for (int i = 0; i < (int)ObjectLocation.Last; ++i) {
                totalDistance[i] = 0;
                totalSceneDistance[i] = 0;
                platformDistanceDataMap[i].ResetValues();
                objectLocationAngle[i] = 0;

                topPlatformObjectSpawned[i] = bottomPlatformObjectSpawned[i] = null;
                topSceneObjectSpawned[i] = bottomSceneObjectSpawned[i] = null;

                previousPlatformSection[i] = 0;
                previousSceneSection[i] = 0;
                spawnedSceneSectionTransition[i] = false;
                spawnedPlatformSectionTransition[i] = false;

                for (int j = 0; j < objectSpawnIndex[i].Count; ++j) {
                    objectSpawnIndex[i][j] = -1;
                    lastObjectSpawnDistance[i][j] = 0;
                }
                for (int j = 0; j < (int)ObjectType.Last; ++j) {
                    objectTypeSpawnIndex[i][j] = -1;
                    lastLocalIndex[i][j] = -1;
                    latestObjectTypeSpawnDistance[i][j] = -1;
                }
            }

            topTurnPlatformObjectSpawned = bottomTurnPlatformObjectSpawned = null;
            topTurnSceneObjectSpawned = bottomTurnSceneObjectSpawned = null;
        }

        public InfiniteObject GetSavedInfiniteObjects()
        {
            return savedInfiniteObjects;
        }

        public void SetPreviousSection(ObjectLocation location, bool isSceneObject, int section)
        {
            if (isSceneObject) {
                previousSceneSection[(int)location] = section;
                spawnedSceneSectionTransition[(int)location] = false;
            } else {
                previousPlatformSection[(int)location] = section;
                spawnedPlatformSectionTransition[(int)location] = false;
            }
        }

        public int GetPreviousSection(ObjectLocation location, bool isSceneObject)
        {
            return (isSceneObject ? previousSceneSection[(int)location] : previousPlatformSection[(int)location]);
        }

        public void DidSpawnSectionTranition(ObjectLocation location, bool isSceneObject)
        {
            if (isSceneObject) {
                spawnedSceneSectionTransition[(int)location] = true;
            } else {
                spawnedPlatformSectionTransition[(int)location] = true;
            }
        }

        public bool HasSpawnedSectionTransition(ObjectLocation location, bool isSceneObject)
        {
            return (isSceneObject ? spawnedSceneSectionTransition[(int)location] : spawnedPlatformSectionTransition[(int)location]);
        }

        // For persisting the data:
        public void SaveInfiniteObjectPersistence(ref InfiniteObjectPersistence persistence)
        {
            persistence.totalDistance = totalDistance;
            persistence.totalSceneDistance = totalSceneDistance;
            persistence.objectLocationAngle = objectLocationAngle;
            persistence.topPlatformObjectSpawned = topPlatformObjectSpawned;
            persistence.bottomPlatformObjectSpawned = bottomPlatformObjectSpawned;
            persistence.topSceneObjectSpawned = topSceneObjectSpawned;
            persistence.bottomSceneObjectSpawned = bottomSceneObjectSpawned;
            persistence.previousPlatformSection = previousPlatformSection;
            persistence.previousSceneSection = previousSceneSection;
            persistence.spawnedPlatformSectionTransition = spawnedPlatformSectionTransition;
            persistence.spawnedSceneSectionTransition = spawnedSceneSectionTransition;

            int objectCount = objectSpawnIndex[0].Count;
            persistence.objectSpawnIndex = new int[(int)ObjectLocation.Last * objectCount];
            persistence.lastObjectSpawnDistance = new float[(int)ObjectLocation.Last * objectCount];
            persistence.objectTypeSpawnIndex = new int[(int)ObjectLocation.Last * (int)ObjectType.Last];
            persistence.lastLocalIndex = new int[(int)ObjectLocation.Last * (int)ObjectType.Last];
            persistence.latestObjectTypeSpawnDistance = new float[(int)ObjectLocation.Last * (int)ObjectType.Last];

            int width = (int)ObjectLocation.Last;
            for (int i = 0; i < (int)ObjectLocation.Last; ++i) {
                for (int j = 0; j < objectCount; ++j) {
                    persistence.objectSpawnIndex[i * width + j] = objectSpawnIndex[i][j];
                    persistence.lastObjectSpawnDistance[i * width + j] = lastObjectSpawnDistance[i][j];
                }
                for (int j = 0; j < (int)ObjectType.Last; ++j) {
                    persistence.objectTypeSpawnIndex[i * width + j] = objectTypeSpawnIndex[i][j];
                    persistence.lastLocalIndex[i * width + j] = lastLocalIndex[i][j];
                    persistence.latestObjectTypeSpawnDistance[i * width + j] = latestObjectTypeSpawnDistance[i][j];
                }
            }
        }

        public void LoadInfiniteObjectPersistence(InfiniteObjectPersistence persistence)
        {
            totalDistance = persistence.totalDistance;
            totalSceneDistance = persistence.totalSceneDistance;
            objectLocationAngle = persistence.objectLocationAngle;
            topPlatformObjectSpawned = persistence.topPlatformObjectSpawned;
            bottomPlatformObjectSpawned = persistence.bottomPlatformObjectSpawned;
            topSceneObjectSpawned = persistence.topSceneObjectSpawned;
            bottomSceneObjectSpawned = persistence.bottomSceneObjectSpawned;
            previousPlatformSection = persistence.previousPlatformSection;
            previousSceneSection = persistence.previousSceneSection;
            spawnedPlatformSectionTransition = persistence.spawnedPlatformSectionTransition;
            spawnedSceneSectionTransition = persistence.spawnedSceneSectionTransition;

            int objectCount = objectSpawnIndex[0].Count;
            int width = (int)ObjectLocation.Last;
            for (int i = 0; i < (int)ObjectLocation.Last; ++i) {
                for (int j = 0; j < objectCount; ++j) {
                    objectSpawnIndex[i][j] = persistence.objectSpawnIndex[i * width + j];
                    lastObjectSpawnDistance[i][j] = persistence.lastObjectSpawnDistance[i * width + j];
                }
                for (int j = 0; j < (int)ObjectType.Last; ++j) {
                    objectTypeSpawnIndex[i][j] = persistence.objectTypeSpawnIndex[i * width + j];
                    lastLocalIndex[i][j] = persistence.lastLocalIndex[i * width + j];
                    latestObjectTypeSpawnDistance[i][j] = persistence.latestObjectTypeSpawnDistance[i * width + j];
                }
            }
        }
    }

    /**
     * Maps the platform distance/section to a local platform index. Used by the scenes and sections to be able to determine which platform they are spawning near
     */
    [System.Serializable]
    public class PlatformDistanceDataMap
    {
        public List<float> distances;
        public List<int> localIndexes;
        public List<int> sections;

        public PlatformDistanceDataMap()
        {
            distances = new List<float>();
            localIndexes = new List<int>();
            sections = new List<int>();
        }

        // a new platform has been spawned, add the distance and section
        public void AddIndex(float distance, int index, int section)
        {
            distances.Add(distance);
            localIndexes.Add(index);
            sections.Add(section);
        }

        // remove the reference if the scene distance is greater than the earliest platform distance
        public void CheckForRemoval(float distance)
        {
            if (distances.Count > 0) {
                // add 0.1f to prevent rounding errors
                if (distances[0] <= distance + 0.1f) {
                    distances.RemoveAt(0);
                    localIndexes.RemoveAt(0);
                    sections.RemoveAt(0);
                }
            }
        }

        // returns the first platform index who doesnt have a scene spawned near it
        public int FirstIndex()
        {
            if (localIndexes.Count > 0) {
                return localIndexes[0];
            }
            return -1;
        }

        public int FirstSection()
        {
            if (sections.Count > 0) {
                return sections[0];
            }
            return -1;
        }

        public void ResetValues()
        {
            distances.Clear();
            localIndexes.Clear();
            sections.Clear();
        }

        public void CopyFrom(PlatformDistanceDataMap other)
        {
            distances = other.distances.GetRange(0, other.distances.Count);
            localIndexes = other.localIndexes.GetRange(0, other.localIndexes.Count);
            sections = other.sections.GetRange(0, other.sections.Count);
        }
    }
}
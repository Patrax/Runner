using UnityEngine;
using InfiniteRunner.InfiniteObjects;

/**
 * Will persist the state of the infintie object history.
 * This class is used when you want to perserve the starting state of a game, for example showing a tutorial
 * Do not directly add this class to any game object. The Infinite Object Persistence Editor will do that for you.
 */
public class InfiniteObjectPersistence : MonoBehaviour
{
    // From Infinite Object History:
    public int[] objectSpawnIndex;
    public int[] objectTypeSpawnIndex;
    public int[] lastLocalIndex;
    public float[] lastObjectSpawnDistance;
    public float[] latestObjectTypeSpawnDistance;
    public float[] totalDistance;
    public float[] totalSceneDistance;
    public float[] objectLocationAngle;
    public InfiniteObject[] topPlatformObjectSpawned;
    public InfiniteObject[] bottomPlatformObjectSpawned;
    public InfiniteObject[] topSceneObjectSpawned;
    public InfiniteObject[] bottomSceneObjectSpawned;
    public int[] previousPlatformSection;
    public int[] previousSceneSection;
    public bool[] spawnedPlatformSectionTransition;
    public bool[] spawnedSceneSectionTransition;
}

using UnityEngine;
using System.Collections.Generic;
using InfiniteRunner.InfiniteObjects;

namespace InfiniteRunner.InfiniteGenerator
{
    /**
     * SceneAppearanceRules extends AppearanceRules by checking to see if a scene can fit within the space provided. A scene may not be able to fit if for example
     * there is a turn 10m away and the scene object is 20m in length. This rule also checks for section transitions
     */
    public class SceneAppearanceRules : AppearanceRules
    {

        // A list of platforms that the scene object must spawn near. A size of 0 means it can spawn near any platform
        public List<PlatformPlacementRule> linkedPlatforms;

        private InfiniteObjectManager infiniteObjectManager;
        private float platformSceneWidthBuffer;
        private float zSize;

        public override void Init()
        {
            base.Init();

            infiniteObjectManager = InfiniteObjectManager.instance;
        }

        public void SetSizes(float buffer, float size)
        {
            platformSceneWidthBuffer = buffer;
            zSize = size;
        }

        public override void AssignIndexToObject(InfiniteObject infiniteObject, int index)
        {
            base.AssignIndexToObject(infiniteObject, index);

            for (int i = 0; i < linkedPlatforms.Count; ++i) {
                if (linkedPlatforms[i].AssignIndexToObject(infiniteObject, index)) {
                    PlatformObject platform = infiniteObject as PlatformObject;
                    platform.EnableLinkedSceneObjectRequired();
                    (thisInfiniteObject as SceneObject).sectionTransition = platform.sectionTransition;
                    break;
                }
            }
        }

        // distance is the scene distance
        public override bool CanSpawnObject(float distance, ObjectSpawnData spawnData)
        {
            if (!base.CanSpawnObject(distance, spawnData))
                return false;

            int platformLocalIndex = infiniteObjectHistory.GetFirstPlatformIndex();
            if (platformLocalIndex == -1)
                return false;

            // Get the platform that this scene is going to spawn next to. See if the platform requires linked scenes and if it does, if this scene fulfills that requirement.
            PlatformObject platform = infiniteObjectManager.LocalIndexToInfiniteObject(platformLocalIndex, ObjectType.Platform) as PlatformObject;

            if (platform.LinkedSceneObjectRequired()) {
                for (int i = 0; i < linkedPlatforms.Count; ++i) {
                    if (linkedPlatforms[i].CanSpawnObject(platformLocalIndex)) {
                        return true;
                    }
                }
                return false;
            } else if (linkedPlatforms.Count > 0) { // return false if this scene is linked to a platform but the platform doesn't have any linked scenes
                return false;
            }

            // if the platform can't fit, then don't spawn it
            float totalDistance = infiniteObjectHistory.GetTotalDistance(false);
            float largestScene = spawnData.largestScene;
            float sceneBuffer = (spawnData.useWidthBuffer ? platformSceneWidthBuffer : 0); // useWidthBuffer contains the information if we should spawn up to totalDistance

            if (totalDistance - distance - sceneBuffer - largestScene >= 0) {
                // largest scene of 0 means we are approaching a turn and it doesn't matter what size object is spawned as long as it fits
                if (largestScene == 0) {
                    return totalDistance - distance - sceneBuffer >= zSize;
                } else {
                    return largestScene >= zSize;
                }
            }

            return false;
        }
    }
}
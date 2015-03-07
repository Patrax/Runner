using UnityEngine;
using System.Collections.Generic;
using InfiniteRunner.InfiniteObjects;

namespace InfiniteRunner.InfiniteGenerator
{
    /**
     * The CollidableAppearanceRules extends AppearanceRules by checking to see if the collidable object can spawn on top of a platform.
     */
    public class CollidableAppearanceRules : AppearanceRules
    {

        // platforms in which the object cannot spawn over
        public List<PlatformPlacementRule> avoidPlatforms;

        public override void AssignIndexToObject(InfiniteObject infiniteObject, int index)
        {
            base.AssignIndexToObject(infiniteObject, index);

            for (int i = 0; i < avoidPlatforms.Count; ++i) {
                if (avoidPlatforms[i].AssignIndexToObject(infiniteObject, index))
                    break;
            }
        }

        public override bool CanSpawnObject(float distance, ObjectSpawnData spawnData)
        {
            if (!base.CanSpawnObject(distance, spawnData))
                return false;

            for (int i = 0; i < avoidPlatforms.Count; ++i) {
                if (!avoidPlatforms[i].CanSpawnObject(infiniteObjectHistory.GetLastLocalIndex(ObjectType.Platform)))
                    return false;
            }

            // may not be able to spawn if the slots don't line up
            return (spawnData.slotPositions & ((thisInfiniteObject as CollidableObject).GetSlotPositionsMask())) != 0;
        }
    }
}
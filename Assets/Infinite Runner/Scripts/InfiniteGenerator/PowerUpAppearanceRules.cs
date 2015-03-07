using UnityEngine;
using InfiniteRunner.Game;
using InfiniteRunner.InfiniteObjects;

namespace InfiniteRunner.InfiniteGenerator
{
    /**
     * If a power up hasn't been purchased yet then it cannot spawn.
     */
    public class PowerUpAppearanceRules : CollidableAppearanceRules
    {
        private PowerUpTypes powerUpType;
        private DataManager dataManager;

        public override void Init()
        {
            base.Init();

            dataManager = DataManager.instance;

            powerUpType = GetComponent<PowerUpObject>().powerUpType;
        }

        public override bool CanSpawnObject(float distance, ObjectSpawnData spawnData)
        {
            if (dataManager.GetPowerUpLevel(powerUpType) == 0)
                return false;

            if (!base.CanSpawnObject(distance, spawnData))
                return false;

            return true;
        }
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InfiniteRunner.Game;
using InfiniteRunner.Player;

namespace InfiniteRunner.Player
{
    // The projectile manager acts as an object pool for the player projectiles, as well as determining if the player can actually fire a projectile.
    public class ProjectileManager : MonoBehaviour
    {
        public ProjectileObject projectilePrefab; // the prefab of the projectile
        public Vector3 spawnPositionOffset; // the offset from the spawnTransform that the projectile should spawn
        public float reloadTime; // the amount of time that it takes to reload
        public float fireDelay; // the delay before the projectile is actually spawned

        private float fireTime;
        private Transform playerTransform;
        private List<ProjectileObject> pool;
        private int poolIndex;
        private CoroutineData fireData;

        public void Start()
        {
            playerTransform = PlayerController.instance.transform;
            pool = new List<ProjectileObject>();
            poolIndex = 0;
            fireTime = -reloadTime;
            fireData = new CoroutineData();
        }

        public bool CanFire()
        {
            return fireTime + reloadTime < Time.time;
        }

        public void Fire()
        {
            if (CanFire()) {
                fireTime = Time.time;

                GameManager.instance.OnPauseGame += GamePaused;
                fireData.duration = fireDelay;
                StartCoroutine("DoFire");
            }
        }

        private IEnumerator DoFire()
        {
            fireData.startTime = Time.time;
            yield return new WaitForSeconds(fireData.duration);

            ProjectileObject projectile = ProjectileFromPool();

            // rotate to the correct direction
            Vector3 eulerAngles = projectilePrefab.transform.eulerAngles;
            eulerAngles.y += playerTransform.eulerAngles.y;
            Quaternion rotation = Quaternion.identity;
            rotation.eulerAngles = eulerAngles;
            projectile.Fire(playerTransform.position + playerTransform.rotation * spawnPositionOffset, rotation, playerTransform.forward);
            GameManager.instance.OnPauseGame -= GamePaused;
        }

        private ProjectileObject ProjectileFromPool()
        {
            ProjectileObject obj = null;

            // keep a start index to prevent the constant pushing and popping from the list
            if (pool.Count > 0 && pool[poolIndex].IsActive() == false) {
                obj = pool[poolIndex];
                poolIndex = (poolIndex + 1) % pool.Count;
                return obj;
            }

            // No inactive objects, need to instantiate a new one
            obj = (GameObject.Instantiate(projectilePrefab.gameObject) as GameObject).GetComponent<ProjectileObject>();

            obj.Init();
            obj.SetStartParent(playerTransform);

            pool.Insert(poolIndex, obj);
            poolIndex = (poolIndex + 1) % pool.Count;

            return obj;
        }

        public void TransitionHeight(float amount)
        {
            for (int i = 0; i < pool.Count; ++i) {
                if (pool[i].IsActive()) {
                    pool[i].TransitionHeight(amount);
                }
            }
        }

        public void ResetValues()
        {
            if (fireData != null)
                fireData.duration = 0;
        }

        private void GamePaused(bool paused)
        {
            if (paused) {
                StopCoroutine("DoFire");
                fireData.CalcuateNewDuration();
            } else {
                StartCoroutine("DoFire");
            }
        }
    }
}
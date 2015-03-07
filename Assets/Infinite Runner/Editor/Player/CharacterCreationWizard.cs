using UnityEngine;
using UnityEditor;
using InfiniteRunner.Game;
using InfiniteRunner.InfiniteGenerator;

namespace InfiniteRunner.Player
{
    public class CharacterCreationWizard : EditorWindow
    {
        private Vector2 scrollPosition;

        private GameObject model;

        // player
        private float horizontalSpeed = 15;
        private float jumpHeight = 5;
        private float slideDuration = 0.75f;
        private AttackType attackType = AttackType.None;
        private float attackCloseDistance = 3;
        private float attackFarDistance = 6;
        private ProjectileObject projectileObject;
        private bool autoTurn = false;
        private bool autoJump = false;
        private DistanceValueList forwardSpeedList = null;

        // particle source
        private PlayerController particleSource;

        // animations
        private bool useMecanim = false;
        private string runAnimationName = "Run";
        private string runJumpAnimationName = "RunJump";
        private string runSlideAnimationName = "RunSlide";
        private string runRightStrafeAnimationName = "RunRightStrafe";
        private string runLeftStrafeAnimationName = "RunLeftStrafe";
        private string attackAnimationName = "Attack";
        private string backwardDeathAnimationName = "BackwardDeath";
        private string forwardDeathAnimationName = "ForwardDeath";
        private string idleAnimationName = "Idle";
        private string runStateName = "Run";
        private string jumpStateName = "Jump";
        private string slideStateName = "Slide";
        private string rightStrafeStateName = "RightStrafe";
        private string leftStrafeStateName = "LeftStrafe";
        private string attackStateName = "Attack";
        private string backwardDeathStateName = "BackwardDeath";
        private string forwardDeathStateName = "ForwardDeath";
        private string idleStateName = "Idle";

        [MenuItem("Window/Character Creation Wizard")]
        public static void ShowWindow()
        {
            CharacterCreationWizard window = EditorWindow.GetWindow<CharacterCreationWizard>("Character Wizard");
            window.minSize = new Vector2(370, 200);
        }

        public void OnGUI()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("This wizard will assist in the creation of a new character.\nThe more frequently changed fields are listed and the\nunlisted fields will remain with their default value.\n" +
                            "All of these values can be changed at a later point in time.");
            GUILayout.Space(5);
            GUILayout.Label("Model", "BoldLabel");
            if (model == null) {
                GUI.color = Color.red;
            }
            model = EditorGUILayout.ObjectField("Character Model", model, typeof(GameObject), true) as GameObject;
            GUI.color = Color.white;

            GUILayout.Label("Player", "BoldLabel");
            horizontalSpeed = EditorGUILayout.FloatField("Horizontal Speed", horizontalSpeed);
            jumpHeight = EditorGUILayout.FloatField("Jump Height", jumpHeight);
            slideDuration = EditorGUILayout.FloatField("Slide Duration", slideDuration);
            attackType = (AttackType)EditorGUILayout.EnumPopup("Attack Type", attackType);
            if (attackType == AttackType.Fixed) {
                attackCloseDistance = EditorGUILayout.FloatField("Close Attack Distance", attackCloseDistance);
                attackFarDistance = EditorGUILayout.FloatField("Far Attack Distance", attackFarDistance);
            } else if (attackType == AttackType.Projectile) {
                projectileObject = EditorGUILayout.ObjectField("Projectile", projectileObject, typeof(ProjectileObject), true) as ProjectileObject;
            }
            autoTurn = EditorGUILayout.Toggle("Auto Turn?", autoTurn);
            autoJump = EditorGUILayout.Toggle("Auto Jump?", autoJump);

            DistanceValueListInspector.ShowLoopToggle(ref forwardSpeedList, DistanceValueType.Speed);
            if (forwardSpeedList == null || forwardSpeedList.Count() == 0) {
                GUI.color = Color.red;
            }
            DistanceValueListInspector.ShowDistanceValues(ref forwardSpeedList, DistanceValueType.Speed);
            GUI.color = Color.white;
            DistanceValueListInspector.ShowAddNewValue(ref forwardSpeedList, DistanceValueType.Speed);

            GUILayout.Space(5);
            GUILayout.Label("Particles", "BoldLabel");
            GUILayout.Label("Optionally specify a character to copy the particles from");
            particleSource = EditorGUILayout.ObjectField("Particles Source", particleSource, typeof(PlayerController), true) as PlayerController;

            GUILayout.Space(5);
            GUILayout.Label("Animations", "BoldLabel");
            useMecanim = EditorGUILayout.Toggle("Use Mecanim?", useMecanim);
            if (useMecanim) {
                runStateName = EditorGUILayout.TextField("Run State Name", runStateName);
                jumpStateName = EditorGUILayout.TextField("Jump State Name", jumpStateName);
                slideStateName = EditorGUILayout.TextField("Slide State Name", slideStateName);
                rightStrafeStateName = EditorGUILayout.TextField("Right Strafe State Name", rightStrafeStateName);
                leftStrafeStateName = EditorGUILayout.TextField("Left Strafe State Name", leftStrafeStateName);
                attackStateName = EditorGUILayout.TextField("Attack State Name", attackStateName);
                backwardDeathStateName = EditorGUILayout.TextField("Backward Death State Name", backwardDeathStateName);
                forwardDeathStateName = EditorGUILayout.TextField("Forward Death State Name", forwardDeathStateName);
                idleStateName = EditorGUILayout.TextField("Idle State Name", idleStateName);
            } else {
                runAnimationName = EditorGUILayout.TextField("Run Animation Name", runAnimationName);
                runJumpAnimationName = EditorGUILayout.TextField("Jump Animation Name", runJumpAnimationName);
                runSlideAnimationName = EditorGUILayout.TextField("Slide Animation Name", runSlideAnimationName);
                runRightStrafeAnimationName = EditorGUILayout.TextField("Right Strafe Animation Name", runRightStrafeAnimationName);
                runLeftStrafeAnimationName = EditorGUILayout.TextField("Left Strafe Animation Name", runLeftStrafeAnimationName);
                attackAnimationName = EditorGUILayout.TextField("Attack Animation Name", attackAnimationName);
                backwardDeathAnimationName = EditorGUILayout.TextField("Backward Death Animation Name", backwardDeathAnimationName);
                forwardDeathAnimationName = EditorGUILayout.TextField("Forward Death Animation Name", forwardDeathAnimationName);
                idleAnimationName = EditorGUILayout.TextField("Idle Animation Name", idleAnimationName);
            }

            if (model == null || forwardSpeedList == null || forwardSpeedList.Count() == 0) {
                GUI.enabled = false;
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Create") && GUI.enabled) {
                CreateCharacter();
            }
            GUI.enabled = true;

            GUILayout.EndScrollView();
        }

        private void CreateCharacter()
        {
            string path = EditorUtility.SaveFilePanel("Save Character", "Assets/Infinite Runner/Prefabs/Characters", "Character.prefab", "prefab");
            if (path.Length != 0 && Application.dataPath.Length < path.Length) {
                var character = GameObject.Instantiate(model) as GameObject;

                // Add a PlayerController, which will add the required components
                PlayerController playerController = character.AddComponent<PlayerController>();
                playerController.horizontalSpeed = horizontalSpeed;
                playerController.jumpHeight = jumpHeight;
                playerController.slideDuration = slideDuration;
                playerController.attackType = attackType;
                playerController.closeAttackDistance = attackCloseDistance;
                playerController.farAttackDistance = attackFarDistance;
                playerController.autoTurn = autoTurn;
                playerController.autoJump = autoJump;
                playerController.forwardSpeeds = forwardSpeedList;

                // PlayerAnimation will automatically be added
                PlayerAnimation playerAnimation = character.GetComponent<PlayerAnimation>();
                playerAnimation.useMecanim = useMecanim;
                if (useMecanim) {
                    playerAnimation.runAnimationName = runStateName;
                    playerAnimation.runJumpAnimationName = jumpStateName;
                    playerAnimation.runSlideAnimationName = slideStateName;
                    playerAnimation.runRightStrafeAnimationName = rightStrafeStateName;
                    playerAnimation.runLeftStrafeAnimationName = leftStrafeStateName;
                    playerAnimation.attackAnimationName = attackStateName;
                    playerAnimation.backwardDeathAnimationName = backwardDeathStateName;
                    playerAnimation.forwardDeathAnimationName = forwardDeathStateName;
                    playerAnimation.idleAnimationName = idleStateName;
                } else {
                    playerAnimation.runAnimationName = runAnimationName;
                    playerAnimation.runJumpAnimationName = runJumpAnimationName;
                    playerAnimation.runSlideAnimationName = runSlideAnimationName;
                    playerAnimation.runRightStrafeAnimationName = runRightStrafeAnimationName;
                    playerAnimation.runLeftStrafeAnimationName = runLeftStrafeAnimationName;
                    playerAnimation.attackAnimationName = attackAnimationName;
                    playerAnimation.backwardDeathAnimationName = backwardDeathAnimationName;
                    playerAnimation.forwardDeathAnimationName = forwardDeathAnimationName;
                    playerAnimation.idleAnimationName = idleAnimationName;
                }

                // The Rigidbody will automatically be added
                Rigidbody rigidbody = character.GetComponent<Rigidbody>();
                rigidbody.useGravity = false;
                rigidbody.isKinematic = true;
                rigidbody.constraints = RigidbodyConstraints.FreezeAll;

                // The Capsule Collider will automatically be added
                CapsuleCollider capsuleCollider = character.GetComponent<CapsuleCollider>();
                capsuleCollider.height = 2;
                capsuleCollider.center = new Vector3(0, 1, 0);

                if (attackType == AttackType.Projectile) {
                    ProjectileManager projectileManager = character.AddComponent<ProjectileManager>();
                    projectileManager.projectilePrefab = projectileObject;
                }

                // Set the correct layers/tags/starting position
                character.layer = LayerMask.NameToLayer("Player");
                character.tag = "Player";
                character.transform.position = new Vector3(0, -1.2f, 0);

                // Add the coin magnet trigger
                GameObject coinMagnet = new GameObject("Coin Magnet Trigger");
                coinMagnet.layer = LayerMask.NameToLayer("CoinMagnet");
                coinMagnet.AddComponent<CoinMagnetTrigger>();
                SphereCollider coinMagnetCollider = coinMagnet.AddComponent<SphereCollider>();
                coinMagnetCollider.isTrigger = true;
                coinMagnet.transform.parent = character.transform;
                playerController.coinMagnetTrigger = coinMagnet;

                // Add the particles
                GameObject particles = new GameObject("Particles");
                particles.transform.parent = character.transform;

                playerController.coinCollectionParticleSystem = CreateParticleSystem("Coin Collection", particles.transform);
                playerController.secondaryCoinCollectionParticleSystem = CreateParticleSystem("Secondary Coin Collection", particles.transform);
                playerController.collisionParticleSystem = CreateParticleSystem("Collision", particles.transform);
                playerController.groundCollisionParticleSystem = CreateParticleSystem("Ground Collision", particles.transform);
                playerController.powerUpParticleSystem = new ParticleSystem[(int)PowerUpTypes.None];
                playerController.powerUpParticleSystem[(int)PowerUpTypes.DoubleCoin] = CreateParticleSystem("Double coin", particles.transform);
                playerController.powerUpParticleSystem[(int)PowerUpTypes.CoinMagnet] = CreateParticleSystem("Coin Magnet", particles.transform);
                playerController.powerUpParticleSystem[(int)PowerUpTypes.Invincibility] = CreateParticleSystem("Invincibility", particles.transform);
                playerController.powerUpParticleSystem[(int)PowerUpTypes.SpeedIncrease] = CreateParticleSystem("Speed Increase", particles.transform);

                // Copy the particles if there is a source
                if (particleSource != null) {
                    // And copy the particles
                    CopyParticleSystem(particleSource.coinCollectionParticleSystem, playerController.coinCollectionParticleSystem);
                    CopyParticleSystem(particleSource.secondaryCoinCollectionParticleSystem, playerController.secondaryCoinCollectionParticleSystem);
                    CopyParticleSystem(particleSource.collisionParticleSystem, playerController.collisionParticleSystem);
                    CopyParticleSystem(particleSource.groundCollisionParticleSystem, playerController.groundCollisionParticleSystem);
                    CopyParticleSystem(particleSource.powerUpParticleSystem[(int)PowerUpTypes.DoubleCoin], playerController.powerUpParticleSystem[(int)PowerUpTypes.DoubleCoin]);
                    CopyParticleSystem(particleSource.powerUpParticleSystem[(int)PowerUpTypes.CoinMagnet], playerController.powerUpParticleSystem[(int)PowerUpTypes.CoinMagnet]);
                    CopyParticleSystem(particleSource.powerUpParticleSystem[(int)PowerUpTypes.Invincibility], playerController.powerUpParticleSystem[(int)PowerUpTypes.Invincibility]);
                    CopyParticleSystem(particleSource.powerUpParticleSystem[(int)PowerUpTypes.SpeedIncrease], playerController.powerUpParticleSystem[(int)PowerUpTypes.SpeedIncrease]);
                }

                // All done, create the prefab
                path = string.Format("Assets/{0}", path.Substring(Application.dataPath.Length + 1));
                AssetDatabase.DeleteAsset(path);
                var prefab = PrefabUtility.CreatePrefab(path, character);
                DestroyImmediate(character, true);
                AssetDatabase.ImportAsset(path);
                Selection.activeObject = prefab;
            }
        }

        private ParticleSystem CreateParticleSystem(string name, Transform parent)
        {
            GameObject particle = new GameObject(name);
            particle.transform.parent = parent;
            ParticleSystem particleSystem = particle.AddComponent<ParticleSystem>();
            particleSystem.playOnAwake = false;
            return particleSystem;
        }

        private void CopyParticleSystem(ParticleSystem source, ParticleSystem destination)
        {
            if (source != null) {
                EditorUtility.CopySerialized(source, destination);
            }
        }
    }
}
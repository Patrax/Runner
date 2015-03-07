using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace InfiniteRunner.InfiniteObjects
{
    /*
     * A collidable object attaches itself to the platform when activated, and moves back to the original parent on deactivation
     */
    public class CollidableObject : InfiniteObject
    {
        public bool canSpawnInLeftSlot;
        public bool canSpawnInCenterSlot;
        public bool canSpawnInRightSlot;

        private PlatformObject platformParent;

        // a helper class which maps the slot position to the bitwise power
        private class SlotPositionPower
        {
            public int position;
            public int power;
            public SlotPositionPower(int pos, int pow)
            {
                position = pos;
                power = pow;
            }
        }

        private List<SlotPositionPower> slotPositions;
        private int slotPositionsMask;

        public override void Init()
        {
            base.Init();

            DetermineSlotPositions();
        }

        public override void Awake()
        {
            base.Awake();

            // need to determine the slot positions again because the cloned object doesn't get inited
            DetermineSlotPositions();
        }

        private void DetermineSlotPositions()
        {
            slotPositions = new List<SlotPositionPower>();
            slotPositionsMask = 0;
            if (canSpawnInLeftSlot) {
                slotPositions.Add(new SlotPositionPower(-1, 0));
                slotPositionsMask |= 1;
            }
            if (canSpawnInCenterSlot) {
                slotPositions.Add(new SlotPositionPower(0, 1));
                slotPositionsMask |= 2;
            }
            if (canSpawnInRightSlot) {
                slotPositions.Add(new SlotPositionPower(1, 2));
                slotPositionsMask |= 4;
            }
        }

        public override void SetParent(Transform parent)
        {
            base.SetParent(parent);

            CollidableObject[] childCollidableObjects = null;
            for (int i = 0; i < thisTransform.childCount; ++i) {
                childCollidableObjects = thisTransform.GetChild(i).GetComponentsInChildren<CollidableObject>();
                for (int j = 0; j < childCollidableObjects.Length; ++j) {
                    childCollidableObjects[j].SetStartParent(thisTransform.GetChild(i));
                }
            }
        }

        public int GetSlotPositionsMask()
        {
            return slotPositionsMask;
        }

        public Vector3 GetSpawnSlot(Vector3 platformPosition, int platformSlots)
        {
            if (slotPositions.Count > 0 && platformSlots != 0) {
                List<SlotPositionPower> positions = slotPositions;
                while (positions.Count > 0) {
                    int index = Random.Range(0, positions.Count);
                    int mask = (int)Mathf.Pow(2, positions[index].power);
                    // return the position if the platform can spawn with the given slot
                    if ((platformSlots & mask) == mask) {
                        return platformPosition * positions[index].position;
                    }
                    // can't spawn yet
                    positions.RemoveAt(index);
                }
            }
            return platformPosition;
        }

        public override void Orient(PlatformObject parent, Vector3 position, Quaternion rotation)
        {
            base.Orient(parent, position, rotation);

            platformParent = parent;
            platformParent.OnPlatformDeactivation += CollidableDeactivation;
        }

        public virtual void CollidableDeactivation()
        {
            if (platformParent)
                platformParent.OnPlatformDeactivation -= CollidableDeactivation;

            base.Deactivate();
        }
    }
}
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using InfiniteRunner.InfiniteGenerator;

namespace InfiniteRunner.InfiniteObjects
{
    /*
     * Adds a custom inspector to the section transitions and control points
     */
    [CustomEditor(typeof(PlatformObject))]
    public class PlatformObjectInspector : InfiniteObjectInspector
    {
        private const int StepCount = 200;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            PlatformObject platformObject = (PlatformObject)target;

            bool sectionTransition = EditorGUILayout.Toggle("Is Section Transition", platformObject.sectionTransition);
            if (sectionTransition != platformObject.sectionTransition) {
                platformObject.sectionTransition = sectionTransition;
                EditorUtility.SetDirty(target);
            }

            if (sectionTransition) {
                List<int> fromSection = platformObject.fromSection;
                List<int> toSection = platformObject.toSection;
                if (SectionSelectionInspector.ShowSectionTransitions(ref fromSection, ref toSection)) {
                    platformObject.fromSection = fromSection;
                    platformObject.toSection = toSection;
                    EditorUtility.SetDirty(target);
                }
            }

            List<Vector3> controlPoints = platformObject.controlPoints;
            if (controlPoints != null && controlPoints.Count > 0) {
                bool updated = false;
                GUILayout.Label("Control Points:");
                for (int i = 0; i < controlPoints.Count; ++i) {
                    GUILayout.BeginHorizontal(GUILayout.Width(100));
                    GUILayout.Label(string.Format("{0} - {1}", i + 1, controlPoints[i]));
                    if (GUILayout.Button("X", GUILayout.Width(30))) {
                        controlPoints.RemoveAt(i);
                        updated = true;
                        break;
                    }
                    GUILayout.EndHorizontal();
                }
                if (updated) {
                    platformObject.controlPoints = controlPoints;
                    ComputeBezierCurve(false);
                    EditorUtility.SetDirty(target);
                }
            }
            if (GUILayout.Button("Add Control Point")) {
                if (controlPoints == null) {
                    controlPoints = new List<Vector3>();
                }
                controlPoints.Add(Vector3.up);
                platformObject.controlPoints = controlPoints;
                ComputeBezierCurve(false);
                EditorUtility.SetDirty(target);
            }
        }

        public void OnSceneGUI()
        {
            PlatformObject platformObject = (PlatformObject)target;

            if (!Application.isPlaying) {
                Quaternion cameraRotation = SceneView.currentDrawingSceneView.camera.transform.rotation;
                Handles.color = Color.white;
                Vector3 platformPosition = platformObject.transform.position;
                if (platformObject.controlPoints != null) {
                    for (int i = 0; i < platformObject.controlPoints.Count; ++i) {
                        Handles.DotCap(0, platformPosition + platformObject.controlPoints[i], cameraRotation, 0.1f);
                        Vector3 prevPosition = platformPosition + platformObject.controlPoints[i];
                        Vector3 position = Handles.PositionHandle(platformPosition + platformObject.controlPoints[i], Quaternion.identity);
                        if (prevPosition != position) {
                            platformObject.controlPoints[i] = platformObject.transform.InverseTransformPoint(position);
                            ComputeBezierCurve(false);
                        }
                    }
                }
            }

            ComputeBezierCurve(true);
        }

        private void ComputeBezierCurve(bool draw /*if false, will compute length*/)
        {
            PlatformObject platformObject = (PlatformObject)target;
            if (platformObject.controlPoints == null || platformObject.controlPoints.Count < 3) {
                platformObject.curveLength = 0;
                EditorUtility.SetDirty(target);
                return;
            }

            if (!draw) {
                platformObject.curveIndexDistanceMap = new List<float>();
            }

            Vector3 p0, p1, p2;
            Vector3 q0, q1;
            float t;
            float length = 0;
            Handles.color = Color.white;
            Vector3 platformPosition = platformObject.transform.position;
            for (int i = 0; i < platformObject.controlPoints.Count - 2; ++i) {
                if (i == 0) {
                    p0 = platformObject.controlPoints[i];
                } else {
                    p0 = (platformObject.controlPoints[i] + platformObject.controlPoints[i + 1]) / 2;
                }
                p1 = platformObject.controlPoints[i + 1];
                if (i + 2 == platformObject.controlPoints.Count - 1) {
                    p2 = platformObject.controlPoints[i + 2];
                } else {
                    p2 = (platformObject.controlPoints[i + 1] + platformObject.controlPoints[i + 2]) / 2;
                }

                t = 0;
                q0 = InfiniteRunnerStarterPackUtility.CalculateBezierPoint(p0, p1, p2, t);
                for (int j = 1; j <= StepCount; j++) {
                    t = j / (float)StepCount;
                    q1 = InfiniteRunnerStarterPackUtility.CalculateBezierPoint(p0, p1, p2, t);
                    if (draw) {
                        Handles.DrawLine(platformPosition + q0, platformPosition + q1);
                    } else {
                        length += Vector3.Distance(q0, q1);
                    }
                    q0 = q1;
                }
                if (!draw) {
                    platformObject.curveIndexDistanceMap.Add(length);
                }
            }

            if (!draw) {
                platformObject.curveLength = length;
                EditorUtility.SetDirty(target);
            }
        }
    }
}
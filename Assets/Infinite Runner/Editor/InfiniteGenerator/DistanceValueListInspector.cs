using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace InfiniteRunner.InfiniteGenerator
{
    public enum DistanceValueType { Probability, Speed, Section }
    /*
     * A static class which will show the editor insector for the Distance Probability List
     */
    public class DistanceValueListInspector : Editor
    {
        private static bool addNewValue = false;
        private static bool noOccurrenceProbability = false;
        private static bool useEndDistance = false;
        private static int startDistance = 0;
        private static float startSliderValue = 1;
        private static float startFloatValue = 1;
        private static float startIntValue = 1;
        private static int endDistance = 0;
        private static float endValue = 1;
        private static string addError = "";
        private static string[] probTypeStrings = new string[] { "Occur", "No Occur" };

        public static bool ShowAddNewValue(ref DistanceValueList distanceValueList, DistanceValueType distanceValueType)
        {
            if (addNewValue) {
                if (ShowAddValueOptions(ref distanceValueList, distanceValueType))
                    return true;
            }

            if (!addNewValue && GUILayout.Button(string.Format("New {0}", Enum.GetName(typeof(DistanceValueType), (int)distanceValueType)))) {
                addError = "";
                addNewValue = true;
            }

            return false;
        }

        public static bool ShowAddNewValue(ref DistanceValueList distanceProbabilityList, ref DistanceValueList noOccurDistanceProbabilityList)
        {
            if (addNewValue) {
                noOccurrenceProbability = GUILayout.SelectionGrid((noOccurrenceProbability ? 1 : 0), probTypeStrings, 2) == 1;

                DistanceValueList list = (noOccurrenceProbability ? noOccurDistanceProbabilityList : distanceProbabilityList);
                if (ShowAddValueOptions(ref list, DistanceValueType.Probability))
                    return true;
            }

            if (!addNewValue && GUILayout.Button("New Probability")) {
                noOccurrenceProbability = false;
                useEndDistance = false;
                startDistance = 0;
                startSliderValue = startFloatValue = startIntValue = 1;
                endDistance = 0;
                endValue = 1;
                addError = "";
                addNewValue = true;
            }

            return false;
        }

        private static bool ShowAddValueOptions(ref DistanceValueList distanceValueList, DistanceValueType distanceValueType)
        {
            useEndDistance = GUILayout.Toggle(useEndDistance, "Use End Distance");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Start Distance", GUILayout.Width(100));
            startDistance = EditorGUILayout.IntField(startDistance);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("{0}{1}", distanceValueType != DistanceValueType.Section ? "Start " : "", Enum.GetName(typeof(DistanceValueType), (int)distanceValueType)), GUILayout.Width(100));
            if (distanceValueType == DistanceValueType.Probability) {
                startSliderValue = GUILayout.HorizontalSlider(startSliderValue, 0f, 1f, GUILayout.Width(100));
                GUILayout.Label(Math.Round(startSliderValue, 2).ToString());
            } else if (distanceValueType == DistanceValueType.Speed) {
                startFloatValue = EditorGUILayout.FloatField(startFloatValue);
            } else { // section
                startIntValue = EditorGUILayout.IntField((int)startIntValue);
            }
            GUILayout.EndHorizontal();

            if (useEndDistance) {
                GUILayout.BeginHorizontal();
                GUILayout.Label("End Distance", GUILayout.Width(100));
                endDistance = EditorGUILayout.IntField(endDistance);
                GUILayout.EndHorizontal();

                if (distanceValueType != DistanceValueType.Section) {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(string.Format("End {0}", Enum.GetName(typeof(DistanceValueType), (int)distanceValueType)), GUILayout.Width(100));
                    if (distanceValueType == DistanceValueType.Probability) {
                        endValue = GUILayout.HorizontalSlider(endValue, 0, 1, GUILayout.Width(100));
                        GUILayout.Label(Math.Round(endValue, 2).ToString());
                    } else if (distanceValueType == DistanceValueType.Speed) {
                        endValue = EditorGUILayout.FloatField(endValue);
                    }
                    GUILayout.EndHorizontal();
                } else {
                    endValue = startIntValue;
                }
            }

            if (addError.Length > 0) {
                GUI.contentColor = Color.red;
                GUILayout.Label(addError);
                GUI.contentColor = Color.white;
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add")) {
                int error;
                if ((error = AddValue(ref distanceValueList, distanceValueType)) == 0) {
                    addNewValue = false;
                    return true;
                } else {
                    switch (error) {
                        case 1:
                            addError = "Error: Start distance must be\ngreater than end distance";
                            break;
                        case 2:
                            addError = string.Format("Error: The {0} distances overlap\na different set of {0} distances", Enum.GetName(typeof(DistanceValueType), (int)distanceValueType).ToLower());
                            break;
                        case 3:
                            addError = string.Format("Error: Another {0} already exists\nwhich does not have the end distance set", Enum.GetName(typeof(DistanceValueType), (int)distanceValueType).ToLower());
                            break;
                        default:
                            addError = "Unknown Error";
                            break;
                    }
                }
            }

            if (GUILayout.Button("Cancel")) {
                addNewValue = false;
            }
            GUILayout.EndHorizontal();
            return false;
        }

        public static bool ShowLoopToggle(ref DistanceValueList distanceValueList, DistanceValueType distanceValueType)
        {
            if (distanceValueList == null)
                return false;

            List<DistanceValue> distanceValue = distanceValueList.values;

            // show the loop option if the last distance has an end
            if (distanceValue != null && distanceValue.Count > 0 && distanceValue[distanceValue.Count - 1].useEndDistance) {
                bool loop = GUILayout.Toggle(distanceValueList.loop, string.Format("Loop {0}", GetPluralName(distanceValueType)));
                if (distanceValueList.loop != loop) {
                    distanceValueList.loop = loop;
                    return true;
                }
                if (loop) {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(string.Format("Loop Back To {0}", Enum.GetName(typeof(DistanceValueType), (int)distanceValueType)), GUILayout.Width(130));
                    int loopBackToIndex = EditorGUILayout.IntField(distanceValueList.loopBackToIndex);
                    GUILayout.EndHorizontal();

                    if (distanceValueList.loopBackToIndex != loopBackToIndex) {
                        distanceValueList.loopBackToIndex = loopBackToIndex;
                        return true;
                    }
                }
            } else if (distanceValueList.loop) {
                distanceValueList.loop = false;
                return true;
            }
            return false;
        }

        public static void ShowDistanceValues(ref DistanceValueList distanceValueList, DistanceValueType distanceValueType)
        {
            List<DistanceValue> distanceValue;

            if (distanceValueList == null || (distanceValue = distanceValueList.values) == null || distanceValue.Count == 0) {
                GUILayout.Label(string.Format("No {0}", GetPluralName(distanceValueType)));
                return;
            }

            for (int i = 0; i < distanceValue.Count; ++i) {
                GUILayout.BeginHorizontal();
                if (distanceValue[i].useEndDistance && distanceValueType != DistanceValueType.Section) {
                    GUILayout.Label(string.Format("{0} {1} - {2}", Enum.GetName(typeof(DistanceValueType), (int)distanceValueType), Math.Round(distanceValue[i].startValue, 2), Math.Round(distanceValue[i].endValue, 2)));
                } else {
                    GUILayout.Label(string.Format("{0} {1}", Enum.GetName(typeof(DistanceValueType), (int)distanceValueType), Math.Round(distanceValue[i].startValue, 2)));
                }
                if (GUILayout.Button("Remove")) {
                    distanceValue.RemoveAt(i);
                    break;
                }
                GUILayout.EndHorizontal();
                if (distanceValue[i].useEndDistance) {
                    GUILayout.Label(string.Format("  Distance {0} - {1}", distanceValue[i].startDistance, distanceValue[i].endDistance));
                } else {
                    GUILayout.Label(string.Format("  Distance {0} - End", distanceValue[i].startDistance));
                }
            }
        }

        private static int AddValue(ref DistanceValueList distanceValueList, DistanceValueType distanceValueType)
        {
            if (startDistance >= endDistance && useEndDistance) {
                return 1;
            }

            List<DistanceValue> distanceValue = null;
            if (distanceValueList != null) {
                distanceValue = distanceValueList.values;
            }
            if (distanceValue == null) {
                distanceValue = new List<DistanceValue>();
            }

            // add the value to the correct spot
            int insertIndex = 0;
            for (; insertIndex < distanceValue.Count; ++insertIndex) {

                // error if the current probability is overlapping an existing probability or within it
                if (useEndDistance && ((startDistance < distanceValue[insertIndex].startDistance && endDistance > distanceValue[insertIndex].startDistance) ||
                    (startDistance > distanceValue[insertIndex].startDistance && endDistance < distanceValue[insertIndex].endDistance) ||
                    (startDistance < distanceValue[insertIndex].endDistance && endDistance > distanceValue[insertIndex].endDistance) ||
                    (!distanceValue[insertIndex].useEndDistance && endDistance > distanceValue[insertIndex].startDistance)) ||
                    (!distanceValue[insertIndex].useEndDistance && startDistance > distanceValue[insertIndex].startDistance) ||
                    (!useEndDistance && startDistance < distanceValue[insertIndex].endDistance)) {
                    return 2;
                }

                // two probabilities can't ignore the end distance
                if (!useEndDistance && !distanceValue[insertIndex].useEndDistance) {
                    return 3;
                }

                // found our place
                if (useEndDistance && endDistance <= distanceValue[insertIndex].startDistance) {
                    break;
                }
            }
            float startValue = 0;
            switch (distanceValueType) {
                case DistanceValueType.Probability:
                    startValue = startSliderValue;
                    break;
                case DistanceValueType.Speed:
                    startValue = startFloatValue;
                    break;
                case DistanceValueType.Section:
                    startValue = startIntValue;
                    break;
            }

            if (distanceValueList == null) {
                distanceValueList = new DistanceValueList(new DistanceValue(startDistance, startValue, endDistance, endValue, useEndDistance));
            } else {
                distanceValue.Insert(insertIndex, new DistanceValue(startDistance, startValue, endDistance, endValue, useEndDistance));
                distanceValueList.values = distanceValue;
            }

            return 0;
        }

        private static string GetPluralName(DistanceValueType distanceValueType)
        {
            switch (distanceValueType) {
                case DistanceValueType.Probability:
                    return "probabilities";
                case DistanceValueType.Speed:
                    return "speeds";
                case DistanceValueType.Section:
                    return "sections";
            }
            return "";
        }
    }
}
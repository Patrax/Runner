using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using InfiniteRunner.InfiniteGenerator;

namespace InfiniteRunner.InfiniteObjects
{
    /*
     * This editor script will allow you to add sections to different infinite objects
     */
    [CustomEditor(typeof(InfiniteObject))]
    public class InfiniteObjectInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Label("Sections:");
            InfiniteObject infiniteObject = (InfiniteObject)target;
            List<int> sections = infiniteObject.sections;
            if (SectionSelectionInspector.ShowSections(ref sections, false)) {
                infiniteObject.sections = sections;
                EditorUtility.SetDirty(target);
            }
        }
    }
}
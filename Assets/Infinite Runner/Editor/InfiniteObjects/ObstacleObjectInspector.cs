using UnityEngine;
using UnityEditor;

namespace InfiniteRunner.InfiniteObjects
{
    /*
     * Custom editor insepectors don't support inheritance.. get around that by subclassing
     */
    [CustomEditor(typeof(ObstacleObject))]
    public class ObstacleObjectInspector : CollidableObjectInspector
    {
        // Intentionally left blank, use the parent class
    }
}
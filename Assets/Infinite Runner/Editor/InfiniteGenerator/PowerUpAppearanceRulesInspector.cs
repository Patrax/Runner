using UnityEngine;
using UnityEditor;

namespace InfiniteRunner.InfiniteGenerator
{
    /*
     * Custom editor insepectors don't support inheritance.. get around that by subclassing
     */
    [CustomEditor(typeof(PowerUpAppearanceRules))]
    public class PowerUpAppearanceRulesInspector : CollidableAppearanceRulesInspector
    {
        // Intentionally left blank, use the parent class
    }
}
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WorldOfSpirits.Combat;

namespace WorldOfSpirits.Editor
{
    [CustomPropertyDrawer(typeof(ProjectileTrigger))]
    public sealed class ProjectileTriggerDrawer : PropertyDrawer
    {
        private static IEnumerable<string> VisibleFields(SerializedProperty property)
        {
            yield return "when";
            yield return "action";
            yield return "unlock";
            var unlock = (ProjectileTriggerUnlock)property.FindPropertyRelative("unlock").enumValueIndex;
            if (unlock == ProjectileTriggerUnlock.WeaponLevel) yield return "requiredWeaponLevel";
            if (unlock == ProjectileTriggerUnlock.UpgradeCard)
            {
                yield return "requiredUpgrade";
                yield return "requiredCardLevel";
            }
            yield return "oncePerShot";
            yield return "damageMultiplier";
            switch ((ProjectileTriggerAction)property.FindPropertyRelative("action").enumValueIndex)
            {
                case ProjectileTriggerAction.AreaDamage:
                    yield return "areaRadius";
                    break;
                case ProjectileTriggerAction.SpawnEffect:
                    yield return "effectPrefab";
                    yield return "effectDuration";
                    break;
                case ProjectileTriggerAction.SpawnProjectiles:
                    yield return "projectilePrefab";
                    yield return "projectileCount";
                    yield return "spreadAngle";
                    yield return "projectileSpeed";
                    break;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            if (property.isExpanded)
                foreach (string name in VisibleFields(property))
                    height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative(name), true) + EditorGUIUtility.standardVerticalSpacing;
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var row = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            string when = property.FindPropertyRelative("when").enumDisplayNames[property.FindPropertyRelative("when").enumValueIndex];
            string action = property.FindPropertyRelative("action").enumDisplayNames[property.FindPropertyRelative("action").enumValueIndex];
            property.isExpanded = EditorGUI.Foldout(row, property.isExpanded, when + " → " + action, true);
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                foreach (string name in VisibleFields(property))
                {
                    var field = property.FindPropertyRelative(name);
                    row.y += row.height + EditorGUIUtility.standardVerticalSpacing;
                    row.height = EditorGUI.GetPropertyHeight(field, true);
                    EditorGUI.PropertyField(row, field, true);
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.EndProperty();
        }
    }
}

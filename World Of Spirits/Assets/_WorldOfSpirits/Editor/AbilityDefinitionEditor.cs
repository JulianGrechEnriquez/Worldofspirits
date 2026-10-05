using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using WorldOfSpirits.Combat;
using WorldOfSpirits.Spirits;

namespace WorldOfSpirits.Editor
{
    [CustomEditor(typeof(AbilityDefinition))]
    public sealed class AbilityDefinitionEditor : UnityEditor.Editor
    {
        private VisualElement root;
        private bool rebuildQueued;

        public override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();
            Build();
            return root;
        }

        private void Build()
        {
            serializedObject.Update();
            root.Unbind();
            root.Clear();
            root.style.paddingTop = 8;
            var title = new Label("Ability Settings");
            title.style.fontSize = 18;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            root.Add(title);
            root.Add(new HelpBox("Choose an ability type to see its settings. Expand a level to edit its upgrades.", HelpBoxMessageType.Info));
            var identity = Section(root, "Details");
            Fields(identity, serializedObject, "abilityName", "description", "icon");
            var execution = serializedObject.FindProperty("executionType");
            var typeField = Field(identity, execution, "Ability Type");
            var type = (AbilityExecutionType)execution.enumValueIndex;
            typeField.RegisterValueChangeCallback(evt =>
            {
                // Binding a field also sends an initial change notification.
                // Rebuilding for that notification creates a bind/rebuild loop.
                if (evt.changedProperty.propertyPath == execution.propertyPath &&
                    evt.changedProperty.enumValueIndex != (int)type)
                    QueueRebuild();
            });
            if (type == AbilityExecutionType.Projectile || type == AbilityExecutionType.SpawnEffect)
            {
                var targetingField = Field(identity, serializedObject.FindProperty("targetingMode"));
                int targetingAtBuild = serializedObject.FindProperty("targetingMode").enumValueIndex;
                if (type == AbilityExecutionType.SpawnEffect)
                    targetingField.RegisterValueChangeCallback(evt =>
                    {
                        if (evt.changedProperty.propertyPath == "targetingMode" &&
                            evt.changedProperty.enumValueIndex != targetingAtBuild)
                            QueueRebuild();
                    });
            }

            var levels = serializedObject.FindProperty("levels");
            if (levels.arraySize == 0)
                root.Add(new HelpBox("Add a level before this ability can be used.", HelpBoxMessageType.Warning));
            for (int i = 0; i < levels.arraySize; i++)
            {
                int index = i;
                var level = levels.GetArrayElementAtIndex(i);
                var foldout = new Foldout { text = "Level " + (i + 1), value = i == 0,
                    viewDataKey = target.GetInstanceID() + ":level:" + i };
                foldout.style.marginTop = 8;
                root.Add(foldout);
                bool populated = false;
                System.Action populate = () =>
                {
                    if (populated) return;
                    populated = true;
                    serializedObject.Update();
                    var currentLevel = serializedObject.FindProperty("levels").GetArrayElementAtIndex(index);
                    var basic = Section(foldout, "Timing & Upgrade");
                    RelativeFields(basic, currentLevel, "level", "upgradeDescription", "cooldown");
                    DrawLevel(foldout, currentLevel, type);
                    var remove = new Button(() =>
                    {
                        serializedObject.Update();
                        var list = serializedObject.FindProperty("levels");
                        list.DeleteArrayElementAtIndex(index);
                        serializedObject.ApplyModifiedProperties();
                        QueueRebuild();
                    }) { text = "Remove this level" };
                    foldout.Add(remove);
                    foldout.contentContainer.Bind(serializedObject);
                };
                foldout.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue && foldout.value) populate();
                });
                if (foldout.value) populate();
            }
            root.Add(new Button(AddLevel) { text = levels.arraySize > 0 ? "+ Add level (copy last)" : "+ Add first level" });
            root.Bind(serializedObject);
        }

        private void QueueRebuild()
        {
            if (rebuildQueued) return;
            rebuildQueued = true;
            root.schedule.Execute(() =>
            {
                rebuildQueued = false;
                if (target != null) Build();
            });
        }

        private void DrawLevel(VisualElement parent, SerializedProperty level, AbilityExecutionType type)
        {
            var settings = Section(parent, ObjectNames.NicifyVariableName(type.ToString()));
            switch (type)
            {
                case AbilityExecutionType.Projectile:
                    RelativeFields(settings, level, "targetingRange");
                    DrawProjectile(settings, level.FindPropertyRelative("projectile"));
                    var effects = level.FindPropertyRelative("effects");
                    Field(settings, effects, "On-cast Effects");
                    settings.Add(new HelpBox("Projectile abilities use Grant Revive from this list. Hit damage and status are configured above.", HelpBoxMessageType.Info));
                    break;
                case AbilityExecutionType.Area:
                    RelativeFields(settings, level, "areaRadius");
                    Field(settings, level.FindPropertyRelative("spawnedEffectPrefab"), "Area Visual (optional)");
                    RelativeFields(settings, level, "effects");
                    break;
                case AbilityExecutionType.SpawnEffect:
                    RelativeFields(settings, level, "spawnedEffectPrefab", "spawnCount", "targetingRange", "areaRadius", "activeDuration");
                    var targeting = serializedObject.FindProperty("targetingMode");
                    if ((AbilityTargetingMode)targeting.enumValueIndex == AbilityTargetingMode.RandomPositionNearPlayer)
                        RelativeFields(settings, level, "minimumSpawnDistance", "maximumSpawnDistance");
                    // These spawned effects consume fields from the shared projectile data,
                    // even though they do not launch projectiles.
                    var prefab = level.FindPropertyRelative("spawnedEffectPrefab");
                    var extra = new VisualElement();
                    settings.Add(extra);
                    System.Action updateExtra = () =>
                    {
                        extra.Unbind();
                        extra.Clear();
                        var go = prefab.objectReferenceValue as GameObject;
                        var data = level.FindPropertyRelative("projectile");
                        if (go != null && (go.GetComponent<IceCrystalEffect>() != null ||
                            go.GetComponent<WaterAreaEffect>() != null || go.GetComponent<StoneSpikeEffect>() != null))
                        {
                            Field(extra, data.FindPropertyRelative("damage"));
                            if (go.GetComponent<WaterAreaEffect>() != null)
                            {
                                Field(extra, data.FindPropertyRelative("speed"), "Movement Speed");
                                Field(extra, data.FindPropertyRelative("homingStrength"), "Pull Force");
                            }
                            else DrawStatus(extra, data, go.GetComponent<IceCrystalEffect>() != null, false);
                        }
                        if (go != null && go.GetComponent<PersistentDamageZone>() != null)
                            Field(extra, data.FindPropertyRelative("homingStrength"), "Zone Strength (scaled by 1/6)");
                        extra.Bind(serializedObject);
                    };
                    updateExtra();
                    var displayedPrefab = prefab.objectReferenceValue;
                    extra.TrackPropertyValue(prefab, p =>
                    {
                        if (p.objectReferenceValue == displayedPrefab) return;
                        displayedPrefab = p.objectReferenceValue;
                        updateExtra();
                    });
                    break;
                case AbilityExecutionType.Orbiting:
                    RelativeFields(settings, level, "spawnedEffectPrefab", "spawnCount", "orbitRadius", "orbitSpeed");
                    break;
                case AbilityExecutionType.Chain:
                    RelativeFields(settings, level, "chainCount", "chainRange", "effects");
                    break;
                case AbilityExecutionType.Self:
                    RelativeFields(settings, level, "effects");
                    break;
                case AbilityExecutionType.FollowingArea:
                    RelativeFields(settings, level, "spawnedEffectPrefab", "activeDuration");
                    break;
            }
        }

        private static void DrawProjectile(VisualElement parent, SerializedProperty data)
        {
            RelativeFields(parent, data, "projectilePrefab", "damage", "speed", "count", "directionPattern");
            parent.Add(new Button(() =>
            {
                if (data.FindPropertyRelative("projectilePrefab").objectReferenceValue is ProjectileBase projectile)
                {
                    Selection.activeObject = projectile.gameObject;
                    EditorGUIUtility.PingObject(projectile.gameObject);
                }
            }) { text = "Edit projectile triggers on prefab" });
            var spread = new VisualElement();
            parent.Add(spread);
            RelativeFields(spread, data, "spreadAngle", "spreadMode");
            ShowWhen(spread, data.FindPropertyRelative("directionPattern"), p => p.enumValueIndex == (int)ProjectileDirectionPattern.AimedArc);
            RelativeFields(parent, data, "sizeMultiplier", "lifetimeMultiplier", "homeOnEnemies");
            var homing = new VisualElement();
            parent.Add(homing);
            RelativeFields(homing, data, "homingStrength", "homingRange");
            ShowWhen(homing, data.FindPropertyRelative("homeOnEnemies"), p => p.boolValue);
            RelativeFields(parent, data, "pierceCount", "bounceCount");
            var bounce = Field(parent, data.FindPropertyRelative("bounceRange"));
            ShowWhen(bounce, data.FindPropertyRelative("bounceCount"), p => p.intValue > 0);
            RelativeFields(parent, data, "explosionRadius", "growthPerSecond");
            DrawStatus(parent, data, false);
            var prefab = data.FindPropertyRelative("projectilePrefab");
            var warning = new HelpBox("Pierce, bounce, explosion, growth and hit status require a ConfigurableProjectile component on this prefab.", HelpBoxMessageType.Warning);
            parent.Add(warning);
            ShowWhen(warning, prefab, p => p.objectReferenceValue != null && !(p.objectReferenceValue is ConfigurableProjectile));
        }

        private static void DrawStatus(VisualElement parent, SerializedProperty data, bool chance, bool chooseType = true)
        {
            Field(parent, data.FindPropertyRelative("appliesStatus"));
            var status = new VisualElement();
            parent.Add(status);
            if (chooseType) Field(status, data.FindPropertyRelative("status"));
            RelativeFields(status, data, "statusDuration", "statusStrength");
            if (chance) Field(status, data.FindPropertyRelative("statusChance"));
            ShowWhen(status, data.FindPropertyRelative("appliesStatus"), p => p.boolValue);
        }

        private void AddLevel()
        {
            serializedObject.Update();
            var levels = serializedObject.FindProperty("levels");
            if (levels.arraySize > 0) levels.InsertArrayElementAtIndex(levels.arraySize - 1);
            else levels.arraySize = 1;
            var added = levels.GetArrayElementAtIndex(levels.arraySize - 1);
            added.FindPropertyRelative("level").intValue = levels.arraySize;
            if (levels.arraySize == 1)
            {
                // Unity's serialized list insertion does not run data constructors.
                var defaults = new AbilityLevelData();
                var holder = ScriptableObject.CreateInstance<AbilityDefinition>();
                holder.Configure("", "", AbilityExecutionType.Projectile, AbilityTargetingMode.ClosestEnemy, defaults);
                var source = new SerializedObject(holder);
                levels.GetArrayElementAtIndex(0).serializedObject.CopyFromSerializedProperty(source.FindProperty("levels").GetArrayElementAtIndex(0));
                DestroyImmediate(holder);
            }
            serializedObject.ApplyModifiedProperties();
            Build();
        }

        private static VisualElement Section(VisualElement parent, string title)
        {
            var section = new VisualElement();
            section.style.marginBottom = 8;
            section.style.paddingLeft = 8;
            section.style.paddingRight = 8;
            section.style.paddingTop = 6;
            section.style.paddingBottom = 6;
            section.style.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.08f);
            var label = new Label(title);
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginBottom = 4;
            section.Add(label);
            parent.Add(section);
            return section;
        }

        private static PropertyField Field(VisualElement parent, SerializedProperty property, string label = null)
        {
            var field = new PropertyField(property, label) { name = property.name };
            parent.Add(field);
            return field;
        }

        private static void Fields(VisualElement parent, SerializedObject data, params string[] names)
        {
            foreach (string name in names) Field(parent, data.FindProperty(name));
        }

        private static void RelativeFields(VisualElement parent, SerializedProperty data, params string[] names)
        {
            foreach (string name in names) Field(parent, data.FindPropertyRelative(name));
        }

        private static void ShowWhen(VisualElement element, SerializedProperty property, System.Func<SerializedProperty, bool> condition)
        {
            System.Action<SerializedProperty> update = p => element.style.display = condition(p) ? DisplayStyle.Flex : DisplayStyle.None;
            update(property);
            element.TrackPropertyValue(property, p => update(p));
        }
    }
}

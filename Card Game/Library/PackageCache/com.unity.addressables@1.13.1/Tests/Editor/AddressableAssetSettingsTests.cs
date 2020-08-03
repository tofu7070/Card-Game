﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using NUnit.Framework;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.TestTools;

namespace UnityEditor.AddressableAssets.Tests
{
    public class AddressableAssetSettingsTests : AddressableAssetTestBase
    {
        internal class InitializeScriptable : ScriptableObject, IObjectInitializationDataProvider
        {
            public string Name { get; }
            public ObjectInitializationData CreateObjectInitializationData()
            {
                return new ObjectInitializationData();
            }
        }

        internal class GroupTemplateTestObj : IGroupTemplate
        {
            public string Name { get; }
            public string Description { get; }
        }

        internal class InitializationObejctTest : IObjectInitializationDataProvider
        {
            public string Name { get; }
            public ObjectInitializationData CreateObjectInitializationData()
            {
                return new ObjectInitializationData();
            }
        }

        internal class DataBuilderTest : IDataBuilder
        {
            public string Name { get; set; }
            public bool CacheCleared = false;
            public bool CanBuildData<T>() where T : IDataBuilderResult
            {
                return true;
            }

            public TResult BuildData<TResult>(AddressablesDataBuilderInput builderInput) where TResult : IDataBuilderResult
            {
                return default(TResult);
            }

            public void ClearCachedData()
            {
                CacheCleared = true;
            }

            public string Description { get; }
        }

        [Test]
        public void HasDefaultInitialGroups()
        {
            Assert.IsNotNull(Settings.FindGroup(AddressableAssetSettings.PlayerDataGroupName));
            Assert.IsNotNull(Settings.FindGroup(AddressableAssetSettings.DefaultLocalGroupName));
        }

        [Test]
        public void AddRemovelabel()
        {
            const string labelName = "Newlabel";
            Settings.AddLabel(labelName);
            Assert.Contains(labelName, Settings.labelTable.labelNames);
            Settings.RemoveLabel(labelName);
            Assert.False(Settings.labelTable.labelNames.Contains(labelName));
        }

        [Test]
        public void GetLabels_ShouldReturnCopy()
        {
            const string labelName = "Newlabel";
            Settings.AddLabel("label_1");
            Settings.AddLabel("label_2");

            var labels = Settings.GetLabels();
            labels.Add(labelName);

            Assert.AreEqual(3, labels.Count);
            Assert.AreEqual(2, Settings.labelTable.labelNames.Count);
            Assert.IsFalse(Settings.labelTable.labelNames.Contains(labelName));
        }

        [Test]
        public void WhenLabelNameHasSquareBrackets_AddingNewLabel_ThrowsError()
        {
            string name = "[label]";
            Settings.AddLabel(name);
            LogAssert.Expect(LogType.Error, $"Label name '{name}' cannot contain '[ ]'.");
        }

        [Test]
        public void AddRemoveGroup()
        {
            const string groupName = "NewGroup";
            var group = Settings.CreateGroup(groupName, false, false, false, null);
            Assert.IsNotNull(group);
            Settings.RemoveGroup(group);
            Assert.IsNull(Settings.FindGroup(groupName));
        }

        [Test]
        public void RemoveMissingGroupsReferences_CheckGroupCount()
        {
            var size = Settings.groups.Count;
            var x = Settings.groups[size - 1];
            Settings.groups[size - 1] = null;
            bool b = Settings.RemoveMissingGroupReferences();
            Assert.AreEqual(Settings.groups.Count + 1, size);
            Settings.groups.Add(x);
            LogAssert.Expect(LogType.Log, "Addressable settings contains 1 group reference(s) that are no longer there. Removing reference(s).");
        }

        [Test]
        public void CanCreateAssetReference()
        {
            AssetReference testReference = Settings.CreateAssetReference(m_AssetGUID);
            Assert.NotNull(testReference);
            var entry = Settings.FindGroup(AddressableAssetSettings.DefaultLocalGroupName).GetAssetEntry(m_AssetGUID);
            Assert.AreSame(testReference.AssetGUID, entry.guid);
        }

        [Test]
        public void CreateUpdateNewEntry()
        {
            var group = Settings.CreateGroup("NewGroupForCreateOrMoveEntryTest", false, false, false, null);
            Assert.IsNotNull(group);
            var entry = Settings.CreateOrMoveEntry(m_AssetGUID, group);
            Assert.IsNotNull(entry);
            Assert.AreSame(group, entry.parentGroup);
            var localDataGroup = Settings.FindGroup(AddressableAssetSettings.DefaultLocalGroupName);
            Assert.IsNotNull(localDataGroup);
            entry = Settings.CreateOrMoveEntry(m_AssetGUID, localDataGroup);
            Assert.IsNotNull(entry);
            Assert.AreNotSame(group, entry.parentGroup);
            Assert.AreSame(localDataGroup, entry.parentGroup);
            Settings.RemoveGroup(group);
            localDataGroup.RemoveAssetEntry(entry);
            var tmp = Settings.FindAssetEntry(entry.guid);
            Assert.IsNull(Settings.FindAssetEntry(entry.guid));
        }

        [Test]
        public void CannotCreateOrMoveWithoutGuid()
        {
            Assert.IsNull(Settings.CreateOrMoveEntry(null, Settings.DefaultGroup));
            Assert.IsNull(Settings.CreateSubEntryIfUnique(null, "", null));
        }

        [Test]
        public void FindAssetEntry()
        {
            var localDataGroup = Settings.FindGroup(AddressableAssetSettings.DefaultLocalGroupName);
            Assert.IsNotNull(localDataGroup);
            var entry = Settings.CreateOrMoveEntry(m_AssetGUID, localDataGroup);
            var foundEntry = Settings.FindAssetEntry(m_AssetGUID);
            Assert.AreSame(entry, foundEntry);
        }

        [Test]
        public void AddressablesClearCachedData_DoesNotThrowError()
        {
            //individual clean paths
            foreach (ScriptableObject so in Settings.DataBuilders)
            {
                BuildScriptBase db = so as BuildScriptBase;
                Assert.DoesNotThrow(() => Settings.CleanPlayerContentImpl(db));
            }

            //Clean all path
            Assert.DoesNotThrow(() => Settings.CleanPlayerContentImpl());

            //Cleanup
            Settings.BuildPlayerContentImpl();
        }

        [Test]
        public void AddressablesCleanCachedData_ClearsData()
        {
            //Setup
            Settings.BuildPlayerContentImpl();

            //Check after each clean that the data is not built
            foreach (ScriptableObject so in Settings.DataBuilders)
            {
                BuildScriptBase db = so as BuildScriptBase;
                Settings.CleanPlayerContentImpl(db);
                Assert.IsFalse(db.IsDataBuilt());
            }
        }

        [Test]
        public void AddressablesCleanAllCachedData_ClearsAllData()
        {
            //Setup
            Settings.BuildPlayerContentImpl();

            //Clean ALL data builders
            Settings.CleanPlayerContentImpl();

            //Check none have data built
            foreach (ScriptableObject so in Settings.DataBuilders)
            {
                BuildScriptBase db = so as BuildScriptBase;
                Assert.IsFalse(db.IsDataBuilt());
            }
        }

        [Test]
        public void DeletingAsset_DoesNotDeleteGroupWithSimilarName()
        {
            //Setup
            const string groupName = "NewAsset";
            string assetPath = GetAssetPath(groupName);


            var mat = new Material(Shader.Find("Unlit/Color"));
            AssetDatabase.CreateAsset(mat, assetPath);

            var group = Settings.CreateGroup(groupName, false, false, false, null);
            Assert.IsNotNull(group);

            //Test
            AssetDatabase.DeleteAsset(assetPath);

            //Assert
            Settings.CheckForGroupDataDeletion(groupName);
            Assert.IsNotNull(Settings.FindGroup(groupName));

            //Clean up
            Settings.RemoveGroup(group);
            Assert.IsNull(Settings.FindGroup(groupName));
        }

#if UNITY_2019_2_OR_NEWER
        [Test]
        public void Settings_WhenActivePlayerDataBuilderIndexSetWithSameValue_DoesNotDirtyAsset()
        {
            var prevDC = EditorUtility.GetDirtyCount(Settings);
            Settings.ActivePlayerDataBuilderIndex = Settings.ActivePlayerDataBuilderIndex;
            var dc = EditorUtility.GetDirtyCount(Settings);
            Assert.AreEqual(prevDC, dc);
        }

        [Test]
        public void Settings_WhenActivePlayerDataBuilderIndexSetWithDifferentValue_DoesDirtyAsset()
        {
            var prevDC = EditorUtility.GetDirtyCount(Settings);
            Settings.ActivePlayerDataBuilderIndex = Settings.ActivePlayerDataBuilderIndex + 1;
            var dc = EditorUtility.GetDirtyCount(Settings);
            Assert.AreEqual(prevDC + 1, dc);
        }

        [Test]
        public void AddressableAssetSettings_OnPostprocessAllAssets_AddDeleteGroupTriggersSettingsSave()
        {
            // Setup
            var importedAssets = new string[1];
            var deletedAssets = new string[0];
            var movedAssets = new string[0];
            var movedFromAssetPaths = new string[0];
            var newGroup = ScriptableObject.CreateInstance<AddressableAssetGroup>();
            newGroup.Name = "testGroup";
            var groupPath = ConfigFolder + "/AssetGroups/" + newGroup.Name + ".asset";
            AssetDatabase.CreateAsset(newGroup, groupPath);
            newGroup.Initialize(ScriptableObject.CreateInstance<AddressableAssetSettings>(), "testGroup", AssetDatabase.AssetPathToGUID(groupPath), false);
            importedAssets[0] = groupPath;
            EditorUtility.ClearDirty(Settings);
            var prevDC = EditorUtility.GetDirtyCount(Settings);

            // Test
            Settings.OnPostprocessAllAssets(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
            Assert.AreEqual(prevDC + 1, EditorUtility.GetDirtyCount(Settings));
            Assert.IsTrue(EditorUtility.IsDirty(Settings));

            deletedAssets = new string[1];
            importedAssets = new string[0];
            deletedAssets[0] = groupPath;
            EditorUtility.ClearDirty(Settings);
            prevDC = EditorUtility.GetDirtyCount(Settings);
            Settings.OnPostprocessAllAssets(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
            Assert.AreEqual(prevDC + 2, EditorUtility.GetDirtyCount(Settings));
            Assert.IsTrue(EditorUtility.IsDirty(Settings));
        }

        [Test]
        public void AddressableAssetSettings_OnPostprocessAllAssets_AddAssetEntriesCollectionNotTriggerSettingsSave()
        {
            // Setup
            var importedAssets = new string[1];
            var deletedAssets = new string[0];
            var movedAssets = new string[0];
            var movedFromAssetPaths = new string[0];
            var collectionPath = Path.Combine(ConfigFolder, "collection.asset").Replace('\\', '/');
            var collection = ScriptableObject.CreateInstance<AddressableAssetEntryCollection>();
            var entry = new AddressableAssetEntry("12345698655", "TestAssetEntry", null, false);
            entry.m_cachedAssetPath = "TestPath";
            collection.Entries.Add(entry);
            AssetDatabase.CreateAsset(collection, collectionPath);
            importedAssets[0] = collectionPath;
            EditorUtility.ClearDirty(Settings);
            var prevDC = EditorUtility.GetDirtyCount(Settings);

            // Test
            Settings.OnPostprocessAllAssets(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
            Assert.AreEqual(prevDC, EditorUtility.GetDirtyCount(Settings));
            Assert.IsFalse(EditorUtility.IsDirty(Settings));
        }

        [Test]
        public void AddressableAssetSettings_OnPostprocessAllAssets_DeleteAssetToNullNotTriggerSettingsSave()
        {
            // Setup
            var importedAssets = new string[0];
            var deletedAssets = new string[1];
            var movedAssets = new string[0];
            var movedFromAssetPaths = new string[0];
            Settings.groups.Add(null);
            Settings.DataBuilders.Add(null);
            Settings.GroupTemplateObjects.Add(null);
            Settings.InitializationObjects.Add(null);
            deletedAssets[0] = "";
            EditorUtility.ClearDirty(Settings);
            var prevDC = EditorUtility.GetDirtyCount(Settings);

            // Test
            Settings.OnPostprocessAllAssets(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
            Settings.OnPostprocessAllAssets(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
            Settings.OnPostprocessAllAssets(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
            Settings.OnPostprocessAllAssets(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
            Assert.AreEqual(prevDC, EditorUtility.GetDirtyCount(Settings));
            Assert.IsFalse(EditorUtility.IsDirty(Settings));
        }

        [Test]
        public void AddressableAssetSettings_OnPostprocessAllAssets_ChangeImportedAssetsDoesNotTriggerSettingsSave()
        {
            var importedAssets = new string[1];
            var deletedAssets = new string[0];
            var movedAssets = new string[0];
            var movedFromAssetPaths = new string[0];
            var entry = Settings.CreateOrMoveEntry(m_AssetGUID, Settings.groups[0]);
            var prevTestObjName = entry.MainAsset.name;
            entry.MainAsset.name = "test";
            importedAssets[0] = ConfigFolder + "/test.prefab";
            EditorUtility.ClearDirty(Settings);
            var prevDC = EditorUtility.GetDirtyCount(Settings);
            Settings.OnPostprocessAllAssets(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
            Assert.AreEqual(prevDC, EditorUtility.GetDirtyCount(Settings));
            Assert.IsFalse(EditorUtility.IsDirty(Settings));
            entry.MainAsset.name = prevTestObjName;
        }

        [Test]
        public void AddressableAssetSettings_OnPostprocessAllAssets_MovedGroupNotTriggerSettingsSave()
        {
            // Setup
            var importedAssets = new string[0];
            var deletedAssets = new string[0];
            var movedAssets = new string[1];
            var movedFromAssetPaths = new string[1];
            var newGroup = ScriptableObject.CreateInstance<AddressableAssetGroup>();
            newGroup.Name = "testGroup";
            var groupPath = ConfigFolder + "/AssetGroups/" + newGroup.Name + ".asset";
            AssetDatabase.CreateAsset(newGroup, groupPath);
            newGroup.Initialize(ScriptableObject.CreateInstance<AddressableAssetSettings>(), "testGroup", AssetDatabase.AssetPathToGUID(groupPath), false);
            EditorUtility.ClearDirty(Settings);
            var prevDC = EditorUtility.GetDirtyCount(Settings);
            Settings.groups.Add(newGroup);
            string newGroupPath = ConfigFolder + "/AssetGroups/changeGroup.asset";
            AssetDatabase.MoveAsset(groupPath, newGroupPath);
            movedAssets[0] = newGroupPath;
            movedFromAssetPaths[0] = groupPath;

            // Test
            Settings.OnPostprocessAllAssets(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
            Assert.AreEqual(prevDC, EditorUtility.GetDirtyCount(Settings));
            Assert.IsFalse(EditorUtility.IsDirty(Settings));

            //Cleanup
            Settings.RemoveGroup(newGroup);
        }

        [Test]
        public void AddressableAssetSettings_OnPostprocessAllAssets_MovedAssetToResourcesNotTriggerSettingsSave()
        {
            // Setup
            var importedAssets = new string[0];
            var deletedAssets = new string[0];
            var movedAssets = new string[1];
            var movedFromAssetPaths = new string[1];
            var assetPath = ConfigFolder + "/test.prefab";
            var newAssetPath = ConfigFolder + "/resources/test.prefab";
            if (!Directory.Exists(ConfigFolder + "/resources"))
            {
                Directory.CreateDirectory(ConfigFolder + "/resources");
                AssetDatabase.Refresh();
            }
            Settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(newAssetPath), Settings.groups[0]);
            movedAssets[0] = newAssetPath;
            movedFromAssetPaths[0] = assetPath;
            EditorUtility.ClearDirty(Settings);
            var prevDC = EditorUtility.GetDirtyCount(Settings);
            Settings.OnPostprocessAllAssets(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
            Assert.AreEqual(prevDC, EditorUtility.GetDirtyCount(Settings));
            Assert.IsFalse(EditorUtility.IsDirty(Settings));

            // Cleanup
            AssetDatabase.MoveAsset(newAssetPath, assetPath);
            Settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(assetPath), Settings.groups[0]);
            Directory.Delete(ConfigFolder + "/resources");
        }

        [Test]
        public void AddressableAssetSettings_ActivePlayerDataBuilderIndex_CanGetActivePlayModeDataBuilderIndex()
        {
            Assert.NotNull(Settings.ActivePlayerDataBuilderIndex);
        }

        [Test]
        public void AddressableAssetSettings_ActivePlayerDataBuilderIndex_CanSetActivePlayModeDataBuilderIndex()
        {
            var prevActivePlayModeDataBuilderIndex = Settings.ActivePlayerDataBuilderIndex;
            Settings.ActivePlayerDataBuilderIndex = 1;
            Assert.AreNotEqual(prevActivePlayModeDataBuilderIndex, Settings.ActivePlayerDataBuilderIndex);
            Settings.ActivePlayerDataBuilderIndex = prevActivePlayModeDataBuilderIndex;
        }

#pragma warning disable 618
        [Test]
        public void AddressableAssetSettings_RemoveSchemaTemplate_CannotRemoveGroupSchemaTemplate()
        {
            LogAssert.Expect(LogType.Error, "GroupSchemaTemplates are deprecated, use GroupTemplateObjects");
            Settings.RemoveSchemaTemplate(0);
        }

#pragma warning restore 618

        [Test]
        public void AddressableAssetSettings_GetGroupTemplateObject_CanGetGroupTemplateObject()
        {
            var groupTemplate = Settings.GetGroupTemplateObject(Settings.GroupTemplateObjects.Count - 1);
            Assert.NotNull(groupTemplate);
        }

        [Test]
        public void AddressableAssetSettings_AddGroupTemplateObject_CanAddGroupTemplateObject()
        {
            var template = ScriptableObject.CreateInstance<AddressableAssetGroupTemplate>();
            template.name = "testGroup";
            Settings.AddGroupTemplateObject(template);
            var groupTemplate = Settings.GetGroupTemplateObject(Settings.GroupTemplateObjects.Count - 1);
            Assert.NotNull(groupTemplate);
            Assert.Greater(Settings.GroupTemplateObjects.Count, 1);
            Assert.AreEqual(groupTemplate.Name, template.name);
            Assert.AreSame(groupTemplate, template);

            Assert.IsTrue(Settings.CreateAndAddGroupTemplate("testCreatAndAdd", "test template function", typeof(BundledAssetGroupSchema)));
        }

        [Test]
        public void AddressableAssetSettings_SetGroupTemplateObjectAtIndex_CanSetGroupTemplateObject()
        {
            // Setup
            var testTemplate1 = ScriptableObject.CreateInstance<AddressableAssetGroupTemplate>();
            var testTemplate2 = ScriptableObject.CreateInstance<AddressableAssetGroupTemplate>();
            var template = ScriptableObject.CreateInstance<AddressableAssetGroupTemplate>();
            testTemplate1.name = "test1";
            testTemplate2.name = "test2";
            template.name = "testGroupIndex";
            var restoredObjects = new List<ScriptableObject>(Settings.GroupTemplateObjects);
            Settings.AddGroupTemplateObject(testTemplate1);
            Settings.AddGroupTemplateObject(testTemplate2);
            var saveTemplate = Settings.GetGroupTemplateObject(0);
            var checkUnchangedTemplate = Settings.GetGroupTemplateObject(1);

            // Test
            Settings.SetGroupTemplateObjectAtIndex((Settings.GroupTemplateObjects.Count - 1), template, true);
            Settings.SetGroupTemplateObjectAtIndex(0, template, true);
            var groupTemplate = Settings.GetGroupTemplateObject(Settings.GroupTemplateObjects.Count - 1);
            Assert.NotNull(groupTemplate);
            Assert.AreSame(template, groupTemplate);
            groupTemplate = Settings.GetGroupTemplateObject(0);
            Assert.NotNull(groupTemplate);
            Assert.AreSame(template, groupTemplate);
            groupTemplate = Settings.GetGroupTemplateObject(1);
            Assert.NotNull(groupTemplate);
            Assert.AreSame(checkUnchangedTemplate, groupTemplate);

            /* Cleanup
             * Restore GroupTemplateObjects
             */
            Settings.RemoveGroupTemplateObject(2, true);
            Settings.RemoveGroupTemplateObject(1, true);
            Settings.SetGroupTemplateObjectAtIndex(0, saveTemplate, true);
            Assert.AreEqual(restoredObjects.Count, Settings.GroupTemplateObjects.Count);
            Assert.AreSame(restoredObjects[0], Settings.GroupTemplateObjects[0]);
        }

        [Test]
        public void AddressableAssetSettings_AddGroupTemplateObject_CannotAddInvalidGroupTemplateObject()
        {
            int currentGroupTemplateCount = Settings.GroupTemplateObjects.Count;
            var unchangedGroupTemplate = Settings.GetGroupTemplateObject(Settings.GroupTemplateObjects.Count - 1);
            ScriptableObject groupTemplate = null;
            Assert.IsFalse(Settings.AddGroupTemplateObject(groupTemplate as IGroupTemplate));
            Assert.AreSame(unchangedGroupTemplate, Settings.GetGroupTemplateObject(Settings.GroupTemplateObjects.Count - 1));

            var groupTemplateTest = new GroupTemplateTestObj();
            Assert.IsFalse(Settings.AddGroupTemplateObject(groupTemplateTest as IGroupTemplate));
            Assert.AreEqual(currentGroupTemplateCount, Settings.GroupTemplateObjects.Count);
            Assert.AreSame(unchangedGroupTemplate, Settings.GetGroupTemplateObject(Settings.GroupTemplateObjects.Count - 1));
        }

        [Test]
        public void AddressableAssetSettings_CreateAndAddGroupTemplate_CannotAddInvalidGroupTemplateObject()
        {
            Assert.IsFalse(Settings.CreateAndAddGroupTemplate(null, "test template function", null));
            Assert.IsFalse(Settings.CreateAndAddGroupTemplate("testCreatAndAdd", "test template function", new Type[0]));
            var testParams = new Type[1];
            testParams[0] = null;
            Assert.IsFalse(Settings.CreateAndAddGroupTemplate("testCreatAndAdd", "test template function", testParams));
            testParams[0] = typeof(ScriptableObject);
            Assert.IsFalse(Settings.CreateAndAddGroupTemplate("testCreatAndAdd", "test template function", testParams));
        }

        [Test]
        public void AddressableAssetSettings_SetGroupTemplateObjectAtIndex_CannotSetInvalidGroupTemplateObject()
        {
            int currentGroupTemplateCount = Settings.GroupTemplateObjects.Count;
            var unchangedGroupTemplate = Settings.GetGroupTemplateObject(Settings.GroupTemplateObjects.Count - 1);
            ScriptableObject groupTemplate = null;
            Assert.IsFalse(Settings.SetGroupTemplateObjectAtIndex(Settings.GroupTemplateObjects.Count - 1, groupTemplate as IGroupTemplate));
            Assert.AreSame(unchangedGroupTemplate, Settings.GetGroupTemplateObject(Settings.GroupTemplateObjects.Count - 1));

            var groupTemplateTest = new GroupTemplateTestObj();
            Assert.IsFalse(Settings.SetGroupTemplateObjectAtIndex(Settings.GroupTemplateObjects.Count - 1, groupTemplateTest as IGroupTemplate));
            Assert.AreEqual(currentGroupTemplateCount, Settings.GroupTemplateObjects.Count);
            Assert.AreSame(unchangedGroupTemplate, Settings.GetGroupTemplateObject(Settings.GroupTemplateObjects.Count - 1));
        }

        [Test]
        public void AddressableAssetSettings_RemoveGroupTemplateObject_CannotRemoveNonExistentGroupTemplateObject()
        {
            Assert.IsFalse(Settings.RemoveGroupTemplateObject(Settings.GroupTemplateObjects.Count));
        }

        [Test]
        public void AddressableAssetSettings_GetGroupTemplateObject_CannotGetNonExistentGroupTemplateObject()
        {
            Assert.IsNull(Settings.GetGroupTemplateObject(Settings.GroupTemplateObjects.Count));
        }

        internal static bool IsNullOrEmpty<T>(ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }

        [Test]
        public void AddressableAssetSettings_GetGroupTemplateObject_CannotGetFromEmptyGroupTemplateObjectList()
        {
            var testSettings = new AddressableAssetSettings();
            while(!IsNullOrEmpty(testSettings.GroupTemplateObjects))
            {
                testSettings.RemoveGroupTemplateObject(0);
            }
            Assert.AreEqual(0, testSettings.GroupTemplateObjects.Count);
            Assert.IsNull(testSettings.GetGroupTemplateObject(1));
        }

        [Test]
        public void AddressableAssetSettings_AddDataBuilder_CanAddDataBuilder()
        {
            var testBuilder = Settings.GetDataBuilder(0);
            var testBuilderTwo = Settings.GetDataBuilder(1);
            var lastBuilder = Settings.GetDataBuilder(Settings.DataBuilders.Count - 1);
            int buildersCount = Settings.DataBuilders.Count;

            // Test
            Assert.IsTrue(Settings.AddDataBuilder(testBuilder as IDataBuilder));
            Assert.AreEqual(buildersCount + 1, Settings.DataBuilders.Count);
            Assert.AreEqual(Settings.DataBuilders[Settings.DataBuilders.Count - 1], testBuilder);
        }

        [Test]
        public void AddressableAssetSettings_RemoveDataBuilder_CanRemoveDataBuilder()
        {
            var testBuilder = Settings.GetDataBuilder(0);
            var testBuilderTwo = Settings.GetDataBuilder(1);
            var lastBuilder = Settings.GetDataBuilder(Settings.DataBuilders.Count - 1);
            int buildersCount = Settings.DataBuilders.Count;
            Settings.AddDataBuilder(testBuilder as IDataBuilder);

            // Test
            Assert.IsTrue(Settings.RemoveDataBuilder(Settings.DataBuilders.Count - 1));
            Assert.AreEqual(buildersCount, Settings.DataBuilders.Count);
            Assert.AreEqual(lastBuilder, Settings.GetDataBuilder(Settings.DataBuilders.Count - 1));
        }

        [Test]
        public void AddressableAssetSettings_SetDataBuilder_CanSetDataBuilder()
        {
            var testBuilder = Settings.GetDataBuilder(0);
            var testBuilderTwo = Settings.GetDataBuilder(1);
            var lastBuilder = Settings.GetDataBuilder(Settings.DataBuilders.Count - 1);
            int buildersCount = Settings.DataBuilders.Count;
            Settings.AddDataBuilder(testBuilder as IDataBuilder);

            // Test
            Assert.IsTrue(Settings.SetDataBuilderAtIndex(Settings.DataBuilders.Count - 1, testBuilderTwo));
            Assert.AreEqual(Settings.GetDataBuilder(Settings.DataBuilders.Count - 1), testBuilderTwo);

            //Cleanup
            Assert.IsTrue(Settings.RemoveDataBuilder(Settings.DataBuilders.Count - 1));
            Assert.AreEqual(buildersCount, Settings.DataBuilders.Count);
            Assert.AreEqual(lastBuilder, Settings.GetDataBuilder(Settings.DataBuilders.Count - 1));
        }

        [Test]
        public void AddressableAssetSettings_AddDataBuilder_CannotAddInvalidDataBuilders()
        {
            int currentDataBuildersCount = Settings.DataBuilders.Count;
            var unchangedDataBuilder = Settings.GetDataBuilder(Settings.DataBuilders.Count - 1);
            ScriptableObject testBuilder = null;
            Assert.IsFalse(Settings.AddDataBuilder(testBuilder as IDataBuilder));
            Assert.AreSame(unchangedDataBuilder, Settings.GetDataBuilder(Settings.DataBuilders.Count - 1));

            var testDataBuilder = new DataBuilderTest();
            Assert.IsFalse(Settings.AddDataBuilder(testDataBuilder as IDataBuilder));
            Assert.AreEqual(currentDataBuildersCount, Settings.DataBuilders.Count);
            Assert.AreSame(unchangedDataBuilder, Settings.GetDataBuilder(Settings.DataBuilders.Count - 1));
        }

        [Test]
        public void AddressableAssetSettings_SetDataBuilderAtIndex_CannotSetInvalidDataBuilders()
        {
            int currentDataBuildersCount = Settings.DataBuilders.Count;
            var unchangedDataBuilder = Settings.GetDataBuilder(Settings.DataBuilders.Count - 1);
            ScriptableObject testBuilder = null;
            Assert.IsFalse(Settings.SetDataBuilderAtIndex(Settings.DataBuilders.Count - 1, testBuilder as IDataBuilder));
            Assert.AreSame(unchangedDataBuilder, Settings.GetDataBuilder(Settings.DataBuilders.Count - 1));

            var testDataBuilder = new DataBuilderTest();
            Assert.IsFalse(Settings.SetDataBuilderAtIndex(Settings.DataBuilders.Count - 1, testDataBuilder as IDataBuilder));
            Assert.AreEqual(currentDataBuildersCount, Settings.DataBuilders.Count);
            Assert.AreSame(unchangedDataBuilder, Settings.GetDataBuilder(Settings.DataBuilders.Count - 1));
            Assert.IsFalse(Settings.SetDataBuilderAtIndex(5, testDataBuilder));
        }

        [Test]
        public void AddressableAssetSettings_GetDataBuilder_CannotGetNonExistentDataBuilder()
        {
            Assert.IsNull(Settings.GetDataBuilder(Settings.DataBuilders.Count));
        }

        [Test]
        public void AddressableAssetSettings_RemoveDataBuilder_CannotRemoveNonExistentDataBuilder()
        {
            Assert.IsFalse(Settings.RemoveDataBuilder(Settings.DataBuilders.Count));
        }

        [Test]
        public void AddressableAssetSettings_GetDataBuilder_CannotGetDataBuilderFromEmpty()
        {
            var testSettings = new AddressableAssetSettings();
            while(!IsNullOrEmpty(testSettings.DataBuilders))
            {
                testSettings.RemoveDataBuilder(0);
            }
            Assert.AreEqual(0, testSettings.DataBuilders.Count);
            Assert.IsNull(testSettings.GetDataBuilder(1));
        }

        [Test]
        public void AddressableAssetSettings_AddInitializationObject_CanAddInitializationObject()
        {
            var testInitObject = ScriptableObject.CreateInstance<InitializeScriptable>();
            testInitObject.name = "testObj";
            var testInitObjectTwo = ScriptableObject.CreateInstance<InitializeScriptable>();
            testInitObjectTwo.name = "testObjTwo";
            var lastInitObject = Settings.GetInitializationObject(Settings.InitializationObjects.Count - 1);
            int initObjectsCount = Settings.InitializationObjects.Count;

            // Test
            Assert.IsTrue(Settings.AddInitializationObject(testInitObject as IObjectInitializationDataProvider));
            Assert.AreEqual(initObjectsCount + 1, Settings.InitializationObjects.Count);
            Assert.AreEqual(Settings.InitializationObjects[Settings.InitializationObjects.Count - 1], testInitObject);

            // Cleanup
            Settings.RemoveInitializationObject(Settings.InitializationObjects.Count - 1);
        }

        [Test]
        public void AddressableAssetSettings_SetInitializationObject_CanSetInitializationObject()
        {
            var testInitObject = ScriptableObject.CreateInstance<InitializeScriptable>();
            testInitObject.name = "testObj";
            var testInitObjectTwo = ScriptableObject.CreateInstance<InitializeScriptable>();
            testInitObjectTwo.name = "testObjTwo";
            var lastInitObject = Settings.GetInitializationObject(Settings.InitializationObjects.Count - 1);
            int initObjectsCount = Settings.InitializationObjects.Count;
            Settings.AddInitializationObject(testInitObject as IObjectInitializationDataProvider);

            // Test
            Assert.IsTrue(Settings.SetInitializationObjectAtIndex(Settings.InitializationObjects.Count - 1, testInitObjectTwo as IObjectInitializationDataProvider));
            Assert.AreEqual(Settings.GetInitializationObject(Settings.InitializationObjects.Count - 1), testInitObjectTwo);

            // Cleanup
            Settings.RemoveInitializationObject(Settings.InitializationObjects.Count - 1);
        }

        [Test]
        public void AddressableAssetSettings_RemoveInitializationObject_CanRemoveInitializationObject()
        {
            var testInitObject = ScriptableObject.CreateInstance<InitializeScriptable>();
            testInitObject.name = "testObj";
            var testInitObjectTwo = ScriptableObject.CreateInstance<InitializeScriptable>();
            testInitObjectTwo.name = "testObjTwo";
            var lastInitObject = Settings.GetInitializationObject(Settings.InitializationObjects.Count - 1);
            int initObjectsCount = Settings.InitializationObjects.Count;
            Settings.AddInitializationObject(testInitObject as IObjectInitializationDataProvider);

            /* Cleanup */
            Assert.IsTrue(Settings.RemoveInitializationObject(Settings.InitializationObjects.Count - 1));
            Assert.AreEqual(initObjectsCount, Settings.InitializationObjects.Count);
            Assert.AreEqual(lastInitObject, Settings.GetInitializationObject(Settings.InitializationObjects.Count - 1));
        }

        [Test]
        public void AddressableAssetSettings_AddInitializationObject_CannotAddInvalidInitializationObject()
        {
            int currentInitObjectsCount = Settings.InitializationObjects.Count;
            ScriptableObject initObject = null;
            Assert.IsFalse(Settings.AddInitializationObject(initObject as IObjectInitializationDataProvider));
            Assert.IsNull(Settings.GetInitializationObject(Settings.InitializationObjects.Count - 1));

            var initTestObject = new InitializationObejctTest();
            Assert.IsFalse(Settings.AddInitializationObject(initTestObject as IObjectInitializationDataProvider));
            Assert.AreEqual(currentInitObjectsCount , Settings.InitializationObjects.Count);
            Assert.IsNull(Settings.GetDataBuilder(Settings.InitializationObjects.Count - 1));
        }

        [Test]
        public void AddressableAssetSettings_SetInitializationObjectAtIndex_CannotSetInvalidInitializationObject()
        {
            int currentInitObjectsCount = Settings.InitializationObjects.Count;
            ScriptableObject initObject = null;
            Assert.IsFalse(Settings.SetInitializationObjectAtIndex(Settings.InitializationObjects.Count - 1, initObject as IObjectInitializationDataProvider));
            Assert.IsNull(Settings.GetInitializationObject(Settings.InitializationObjects.Count - 1));

            var initTestObject = new InitializationObejctTest();
            Assert.IsFalse(Settings.SetInitializationObjectAtIndex(Settings.InitializationObjects.Count - 1, initTestObject as IObjectInitializationDataProvider));

            var testInitObject = ScriptableObject.CreateInstance<InitializeScriptable>();
            testInitObject.name = "testObj";
            Settings.AddInitializationObject(testInitObject as IObjectInitializationDataProvider);
            Assert.IsFalse(Settings.SetInitializationObjectAtIndex(2, testInitObject));
        }

        [Test]
        public void AddressableAssetSettings_GetInitializationObject_CannotGetInvalidInitializationObject()
        {
            int currentInitObjectsCount = Settings.InitializationObjects.Count;
            Assert.IsNull(Settings.GetInitializationObject(Settings.InitializationObjects.Count - 1));
            Assert.IsNull(Settings.GetInitializationObject(-1));
        }

        [Test]
        public void AddressableAssetSettings_RemoveInitializationObject_CannotRemoveNonExistentInitializationObject()
        {
            Assert.IsFalse(Settings.RemoveInitializationObject(Settings.InitializationObjects.Count));
        }

        [Test]
        public void AddressableAssetSettings_GetInitializationObject_CannotGetNonExistentInitializationObject()
        {
            Assert.IsNull(Settings.GetInitializationObject(Settings.InitializationObjects.Count));
        }

        [Test]
        public void AddressableAssetSettings_GetInitializationObject_CannotGetInitializationObjectFromEmptyList()
        {
            var testSettings = new AddressableAssetSettings();
            while(!IsNullOrEmpty(testSettings.InitializationObjects))
            {
                testSettings.RemoveInitializationObject(0);
            }
            Assert.AreEqual(0, testSettings.InitializationObjects.Count);
            Assert.IsNull(testSettings.GetInitializationObject(1));
        }

        [Test]
        public void AddressableAssetSettings_MoveAssetsFromResources_CanMoveAssetsFromResources()
        {
            // Setup
            var testGuidsToPaths = new Dictionary<string, string>();
            var testObject = new GameObject("TestObjectMoveAssets");
#if UNITY_2018_3_OR_NEWER
                PrefabUtility.SaveAsPrefabAsset(testObject, ConfigFolder + "/testasset.prefab");
#else
                PrefabUtility.CreatePrefab(k_TestConfigFolder + "/test.prefab", testObject);
#endif
            var testAssetGUID = AssetDatabase.AssetPathToGUID(ConfigFolder + "/testasset.prefab");
            Settings.CreateOrMoveEntry(testAssetGUID, Settings.FindGroup(AddressableAssetSettings.PlayerDataGroupName));
            var originalAssetEntry = Settings.CreateOrMoveEntry(m_AssetGUID, Settings.FindGroup(AddressableAssetSettings.PlayerDataGroupName));
            var prevAddress = originalAssetEntry.address;
            var prevDC = EditorUtility.GetDirtyCount(Settings);
            var prevGroup = originalAssetEntry.parentGroup;
            var prevPath = AssetDatabase.GUIDToAssetPath(m_AssetGUID);
            var prevPathTwo = AssetDatabase.GUIDToAssetPath(testAssetGUID);
            var testGroup = Settings.FindGroup(AddressableAssetSettings.DefaultLocalGroupName);
            var testAssetPath = ConfigFolder + "/testMoveAssets";
            testGuidsToPaths[m_AssetGUID] = testAssetPath + "/resources/test.prefab";
            testGuidsToPaths[testAssetGUID] = testAssetPath + "/resources/testasset.prefab";

            // Test
            Settings.MoveAssetsFromResources(testGuidsToPaths, testGroup);
            var dc = EditorUtility.GetDirtyCount(Settings);
            Assert.AreNotEqual(prevPath, AssetDatabase.GUIDToAssetPath(m_AssetGUID));
            Assert.AreNotEqual(prevPathTwo, AssetDatabase.GUIDToAssetPath(testAssetGUID));
            Assert.AreNotEqual(prevGroup, Settings.FindAssetEntry(m_AssetGUID).parentGroup);
            Assert.AreNotEqual(prevGroup, Settings.FindAssetEntry(testAssetGUID).parentGroup);
            Assert.AreEqual(prevDC + 1, dc);

            testGuidsToPaths[m_AssetGUID] = ConfigFolder + "/test.prefab";
            testGuidsToPaths[testAssetGUID] = ConfigFolder + "/testasset.prefab";
            Settings.MoveAssetsFromResources(testGuidsToPaths, prevGroup);
            originalAssetEntry = Settings.FindAssetEntry(m_AssetGUID);
            Assert.AreEqual(originalAssetEntry.address, "test");

            //Cleanup
            originalAssetEntry.address = prevAddress;
            if (Directory.Exists(testAssetPath))
                AssetDatabase.DeleteAsset(testAssetPath);
            EditorBuildSettings.RemoveConfigObject(testAssetPath);
        }

        [Test]
        public void AddressableAssetSettings_MoveAssetsFromResources_CannotMoveNullOrInvalidAsset()
        {
            Settings.MoveAssetsFromResources(null, null);
            var testAssetEntry = Settings.CreateOrMoveEntry(m_AssetGUID, Settings.FindGroup(AddressableAssetSettings.PlayerDataGroupName));
            var currentGroup = testAssetEntry.parentGroup;
            var testGuidsToPaths = new Dictionary<string, string>();
            var currentPath = AssetDatabase.GUIDToAssetPath(m_AssetGUID);
            var newAssetPath = ConfigFolder + "/testMoveAssets";
            testGuidsToPaths[m_AssetGUID] = newAssetPath + "/test.prefab";
            Settings.MoveAssetsFromResources(testGuidsToPaths, null);
            Settings.MoveEntry(testAssetEntry, null);
            Assert.AreEqual(currentPath, AssetDatabase.GUIDToAssetPath(m_AssetGUID));
            Assert.AreEqual(currentGroup, Settings.FindAssetEntry(m_AssetGUID).parentGroup);
        }

        [Test]
        public void AddressableAssetSettings_SetLabelValueForEntries_CanSet()
        {
            // Setup
            List<AddressableAssetEntry> entries = new List<AddressableAssetEntry>();
            var testObject = new GameObject("TestObjectSetLabel");
            var newLabel = "testSetLabelValueForEntries";
#if UNITY_2018_3_OR_NEWER
                PrefabUtility.SaveAsPrefabAsset(testObject, ConfigFolder + "/testasset.prefab");
#else
                PrefabUtility.CreatePrefab(k_TestConfigFolder + "/test.prefab", testObject);
#endif
            var testAssetGUID = AssetDatabase.AssetPathToGUID(ConfigFolder + "/testasset.prefab");
            entries.Add(Settings.CreateOrMoveEntry(m_AssetGUID, Settings.FindGroup(AddressableAssetSettings.PlayerDataGroupName)));
            entries.Add(Settings.CreateOrMoveEntry(testAssetGUID, Settings.FindGroup(AddressableAssetSettings.PlayerDataGroupName)));
            var prevDC = EditorUtility.GetDirtyCount(Settings);

            // Test
            Settings.SetLabelValueForEntries(entries, newLabel, true, true);
            Assert.IsTrue(entries[0].labels.Contains(newLabel));
            Assert.IsTrue(entries[1].labels.Contains(newLabel));
            Assert.AreEqual(prevDC + 2, EditorUtility.GetDirtyCount(Settings));

            // Cleanup
            Settings.RemoveLabel(newLabel);
        }

        [Test]
        public void AddressableAssetSettings_HashChanges_WhenGroupIsAdded()
        {
            var prevHash = Settings.currentHash;
            var testGroupObj = Settings.GetGroupTemplateObject(Settings.GroupTemplateObjects.Count - 1);
            var testGroup = Settings.groups[0];
            Assert.AreEqual(Settings.currentHash, prevHash);
            Settings.AddGroupTemplateObject(testGroupObj);
            Settings.groups.Add(testGroup);
            var newHash = Settings.currentHash;
            Assert.AreNotEqual(newHash, prevHash);
            Settings.RemoveGroupTemplateObject(Settings.GroupTemplateObjects.Count - 1);
            Settings.groups.RemoveAt(Settings.groups.Count - 1);
        }

#endif
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using PSB_Lib.ObjectPool.RunTime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PSB_Lib.ObjectPool.Editor
{
    public class PoolManagerEditor : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset visualTreeAsset;

        [SerializeField] private PoolManagerSO poolManager;
        [SerializeField] private VisualTreeAsset itemAsset;

        private string _rootFolderPath;
        private Button _createBtn;
        private ScrollView _itemView;
        private List<PoolItemUI> _itemList;
        private PoolItemUI _selectedItem;

        private UnityEditor.Editor _cashedEditor;
        private VisualElement _inspectorView;
    
        [MenuItem("Tools/PoolManager")]
        public static void ShowWindow()
        {
            PoolManagerEditor wnd = GetWindow<PoolManagerEditor>();
            wnd.titleContent = new GUIContent("PoolManagerEditor");
        }

        public void CreateGUI()
        {
            InitializeWindow();

            VisualElement root = rootVisualElement;

            visualTreeAsset.CloneTree(root);

            SetElements(root);
        }

        private void SetElements(VisualElement root)
        {
            _createBtn = root.Q<Button>("CreateBtn");
            _createBtn.clicked += HandleCreateBtn;
            _itemView = root.Q<ScrollView>("ItemView");
            _inspectorView = root.Q<VisualElement>("InspectorView");
        
            _itemList = new List<PoolItemUI>();

            GeneratePoolItems();
        }

        private void HandleCreateBtn()
        {
            if (EditorUtility.DisplayDialog("Create", "Do you want to create a new pool?", "Yes", "No") == false)
            {
                return;
            }
        
            PoolItemSO newItem = ScriptableObject.CreateInstance<PoolItemSO>();
            Guid itemGuid = Guid.NewGuid();
            newItem.poolingName = itemGuid.ToString();

            if (Directory.Exists($"{_rootFolderPath}/Items") == false)
            {
                Directory.CreateDirectory($"{_rootFolderPath}/Items");
            }
        
            AssetDatabase.CreateAsset(newItem, $"{_rootFolderPath}/Items/{newItem.poolingName}.asset");
        
            poolManager.itemList.Add(newItem);
            EditorUtility.SetDirty(poolManager);
            AssetDatabase.SaveAssets();
        
            GeneratePoolItems();
        }

        private void GeneratePoolItems()
        {
            _itemView.Clear();
            _itemList.Clear();
            _inspectorView.Clear();

            foreach (var item in poolManager.itemList)
            {
                TemplateContainer itemTemplate = itemAsset.Instantiate();
                PoolItemUI itemUI = new PoolItemUI(itemTemplate, item);
                _itemView.Add(itemTemplate);
                _itemList.Add(itemUI);

                itemUI.Name = item.poolingName;

                if (_selectedItem != null && _selectedItem.poolItem == item)
                {
                    HandleItemSelect(itemUI);
                    //인스펙터 뷰 보여주게 하는 것 처리 해야함.
                }
            
                itemUI.OnSelectEvent += HandleItemSelect;
                itemUI.OnDeleteEvent += HandleItemDelete;
            }
        }

        private void HandleItemSelect(PoolItemUI target)
        {
            _inspectorView.Clear();
            if (_selectedItem != null)
                _selectedItem.IsActive = false;
            _selectedItem = target;
            _selectedItem.IsActive = true;
        
            UnityEditor.Editor.CreateCachedEditor(_selectedItem.poolItem, null, ref _cashedEditor);
            VisualElement inspectorContent = _cashedEditor.CreateInspectorGUI();
        
            SerializedObject serializedObject = new SerializedObject(_selectedItem.poolItem);
            inspectorContent.Bind(serializedObject);
            inspectorContent.TrackSerializedObjectValue(serializedObject, so =>
            {
                _selectedItem.Name = so.FindProperty("poolingName").stringValue;
            });
            _inspectorView.Add(inspectorContent);
        }

        private void HandleItemDelete(PoolItemUI target)
        {
            if (EditorUtility.DisplayDialog("Delete", "Are you sure you want to delete?", "OK", "Cancel") == false)
            {
                return;
            }
        
            poolManager.itemList.Remove(target.poolItem);
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(target.poolItem));
            EditorUtility.SetDirty(poolManager);
            AssetDatabase.SaveAssets();

            if (_selectedItem == target)
            {
                _selectedItem = null;
            }
        
            GeneratePoolItems();
        }

        private void InitializeWindow()
        {
            MonoScript monoScript = MonoScript.FromScriptableObject(this);
            string scriptPath = AssetDatabase.GetAssetPath(monoScript);

            _rootFolderPath = Directory.GetParent(Path.GetDirectoryName(scriptPath)).FullName.Replace("\\", "/");
            _rootFolderPath = "Assets" + _rootFolderPath.Substring(Application.dataPath.Length);

            Debug.Log(_rootFolderPath);

            if (poolManager == null)
            {
                string filePath = $"{_rootFolderPath}/PoolManagerSO.asset";
                poolManager = AssetDatabase.LoadAssetAtPath<PoolManagerSO>(filePath);
            
                if (poolManager == null)  //로드를 하려고 하는데 없었다면
                {
                    Debug.LogWarning("PoolManagerSO not found. Creating new one");
                    poolManager = ScriptableObject.CreateInstance<PoolManagerSO>();
                    AssetDatabase.CreateAsset(poolManager, filePath);
                }
            }
        
            visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{_rootFolderPath}/Editor/PoolManagerEditor.uxml");
            itemAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{_rootFolderPath}/Editor/PoolItemUI.uxml");
        
        }
    
    
    }
}

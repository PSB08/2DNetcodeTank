using System;
using PSB_Lib.ObjectPool.RunTime;
using UnityEngine.UIElements;

namespace PSB_Lib.ObjectPool.Editor
{
    public class PoolItemUI
    {
        private Label _nameLabel;
        private Button _deleteBtn;
        private VisualElement _rootElement;
        
        public event Action<PoolItemUI> OnDeleteEvent;
        public event Action<PoolItemUI> OnSelectEvent;

        public string Name
        {
            get => _nameLabel.text;
            set => _nameLabel.text = value;
        }

        public PoolItemSO poolItem;

        public bool IsActive
        {
            get => _rootElement.ClassListContains("active");
            set => _rootElement.EnableInClassList("active", value);
        }

        public PoolItemUI(VisualElement root, PoolItemSO item)
        {
            poolItem = item;
            _rootElement = root.Q<VisualElement>("PoolItem");
            _nameLabel = root.Q<Label>("ItemName");
            _deleteBtn = root.Q<Button>("DeleteBtn");
            
            _deleteBtn.RegisterCallback<ClickEvent>(evt =>
            {
                OnDeleteEvent?.Invoke(this);
                evt.StopPropagation();   // StopPropagation은 이 이벤트는 여기서 더 실행되지 않는다는 거임
            });
            //RegisterCallback 이벤트가 발생하면 람다 안에 실행
            _rootElement.RegisterCallback<ClickEvent>(evt =>
            {
                OnSelectEvent?.Invoke(this);
                evt.StopPropagation();
            });
            
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PSB_Lib.Dependencies
{
    [DefaultExecutionOrder(-10)]  //가장 빨리 실행되도록 한다 (기본 실행 순서) 기본적인 MonoBehaviour는 0
    public class Injector : MonoBehaviour
    {
        private const BindingFlags _bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private readonly Dictionary<Type, object> _registry = new Dictionary<Type, object>();

        private void Awake()
        {
            //인터페이스를 구현한 모든 녀석들을 가져와서 Provide 어트리뷰트가 있을 경우 딕셔너리에 넣는다.
            IEnumerable<IDependencyProvider> providers = GetMonoBehaviours().OfType<IDependencyProvider>();

            foreach (var provider in providers)
            {
                RegisterProvider(provider);    
            }

            IEnumerable<MonoBehaviour> inhectables = GetMonoBehaviours().Where(IsInjectable);

            foreach (var inhectable in inhectables)
            {
                InJect(inhectable);
            }
            
        }

        private void InJect(MonoBehaviour inhectableMono)
        {
            Type type = inhectableMono.GetType();
            IEnumerable<FieldInfo> injectableFields = type.GetFields(_bindingFlags).Where
                (field => Attribute.IsDefined(field, typeof(InjectAttribute)));

            foreach (FieldInfo field in injectableFields)
            {
                Type fieldType = field.FieldType;
                object instance = Resolve(fieldType);
                Debug.Assert(instance != null, $"Inject instance not found for {fieldType.Name}");
                field.SetValue(inhectableMono, instance);
            }
            
            IEnumerable<MethodInfo> injectableMethods = type.GetMethods(_bindingFlags).Where
                (field => Attribute.IsDefined(field, typeof(InjectAttribute)));

            foreach (var method in injectableMethods)
            {
                Type[] requiredParams = method.GetParameters().Select(p => p.ParameterType).ToArray();
                object[] paramValues = requiredParams.Select(Resolve).ToArray();
                method.Invoke(inhectableMono, paramValues);
            }
            
        }

        private object Resolve(Type fieldType)
        {
            _registry.TryGetValue(fieldType, out object instance);
            return instance;
        }

        private bool IsInjectable(MonoBehaviour mono)
        {
            MemberInfo[] members = mono.GetType().GetMembers(_bindingFlags);
            return members.Any(member => Attribute.IsDefined(member, typeof(InjectAttribute)));
        }

        private void RegisterProvider(IDependencyProvider provider)
        {
            //클래스 그 자체에 Provide 어트리뷰트가 붙어있는 경우 별도로 찾을 필요 없이 해당 클래스를 그냥 넣는다.
            if (Attribute.IsDefined(provider.GetType(), typeof(ProvideAttribute)))
            {
                _registry.Add(provider.GetType(), provider);
                return;
            }
            
            //해당 클래스에 모든 메서드를 가져온다.
            MethodInfo[] methods = provider.GetType().GetMethods(_bindingFlags);
            foreach (var method in methods)
            {
                //해당 메서드에 Provide 어트리뷰트가 없다면 무시해도 된다.
                if (!Attribute.IsDefined(method, typeof(ProvideAttribute)))
                    continue;
                Type returnType = method.ReturnType;
                object instance = method.Invoke(provider, null);
                Debug.Assert(instance != null, $"Provide method {method.Name} returned null.");
                
                _registry.Add(returnType, instance);
            }
            
        }

        private IEnumerable<MonoBehaviour> GetMonoBehaviours()
        {
            return FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        }
        
    }
}
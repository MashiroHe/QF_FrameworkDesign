/*
 * version: v1.0.1
 * author:  MashiroHe  
 * created: 2023-09-18 17:14
 * email:   1967407707@qq.com
 * purpose: QF��ܴ������ ����Ϊ v1.0.0�汾-����ӵ���������У�����������ٷ�v1.0���ֲ��䣬�����Ϲٷ�v1.0.2�汾���� �����汾ʹ�ý̳���QF�ٷ�v1.0.2һ�£�����ֻ������ܲ��֣�������QFToolKit��QFToolKitҲ��ʹ��v1.0.2�汾
 */

using System;
using System.Collections.Generic;
using UnityEngine;

/*��ܺ��Ĳ���*/
namespace QFramework
{
    #region Rule

    /*Architecture*/
    public interface IBelongToArchitecture { }
    public interface ICanGetArchitecture : IBelongToArchitecture  //Դ���Ϊ IBelongToArchitecture
    {
        IArchitecture GetArchitecture();
    }
    public interface ICanSetArchitecture
    {
        void SetArchitecture(IArchitecture architecture);
    }

    /*Model*/
    public interface ICanGetModel : ICanGetArchitecture { }
    public static class CanGetModelExtension
    {
        /// <summary>
        /// �̳нӿ�ICanGetModel�� GetModel��̬��չ����
        /// </summary>
        public static T GetModel<T>(this ICanGetModel self) where T : class, IModel
        {
            return self.GetArchitecture().GetModel<T>();
        }
    }

    /*System*/
    public interface ICanGetSystem : ICanGetArchitecture { }
    public static class CanGetSystemExtension
    {
        public static T GetSystem<T>(this ICanGetSystem self) where T : class, ISystem
        {
            return self.GetArchitecture().GetSystem<T>();
        }
    }

    /*Utility*/
    public interface ICanGetUtility : ICanGetArchitecture { }
    public static class CanGetUtilityExtension
    {
        /// <summary>
        /// �̳нӿ�ICanGetUtility ��GetUtility��̬��չ����
        /// </summary>
        public static T GetUtility<T>(this ICanGetUtility self) where T : class, IUtility
        {
            return self.GetArchitecture().GetUtility<T>();
        }
    }

    /*Command*/
    public interface ICanSendCommand : ICanGetArchitecture { }
    public static class CanSendCommandExtension
    {
        public static void SendCommand<T>(this ICanSendCommand self) where T : ICommand, new()
        {
            self.GetArchitecture().SendCommand<T>();
        }
        public static void SendCommand<T>(this ICanSendCommand self, T command) where T : ICommand
        {
            self.GetArchitecture().SendCommand<T>(command);
        }
    }

    /*Event*/
    public interface ICanRegisterEvent : ICanGetArchitecture { }
    public static class CanRegisterEventExtension
    {
        public static IUnRegister RegisterEvent<T>(this ICanRegisterEvent self, Action<T> onEvent)
        {
            return self.GetArchitecture().RegisterEvent<T>(onEvent);
        }
        public static void UnRegisterEvent<T>(this ICanRegisterEvent self, Action<T> onEvent)
        {
            self.GetArchitecture().UnRegisterEvent<T>(onEvent);
        }
    }

    public interface ICanSendEvent : ICanGetArchitecture { }
    public static class CanSendEventExtension
    {
        public static void SendEvent<T>(this ICanSendEvent self) where T : new()
        {
            self.GetArchitecture().SendEvent<T>();
        }
        public static void SendEvent<T>(this ICanSendEvent self, T e) where T : new()
        {
            self.GetArchitecture().SendEvent<T>(e);
        }
    }

    /*���ݹٷ�v1.0.2��� ȫ�־�̬��չ���������̳�IOnEvent�ӿ���ע��ע���¼�������ο�QFʹ���ֲ�*/
    public interface IOnEvent<T>
    {
        void OnEvent(T e);
    }

    public static class OnGlobalEventExtension
    {
        public static IUnRegister RegisterEvent<T>(this IOnEvent<T> self) where T : struct
        {
            return TypeEventSystem.Global.Register<T>(self.OnEvent);
        }

        public static void UnRegisterEvent<T>(this IOnEvent<T> self) where T : struct
        {
            TypeEventSystem.Global.UnRegister<T>(self.OnEvent);
        }
    }


    /*Query*/
    public interface ICanSendQuery : ICanGetArchitecture
    {
    }
    public static class CanSendQueryExtension
    {
        public static T SendQuery<T>(this ICanSendQuery self, IQuery<T> query)
        {
            return self.GetArchitecture().SendQuery(query);
        }
    }
    #endregion

    #region Architecture
    public interface IArchitecture
    {
        #region ע��ģ��ӿ�
        void RegisterSystem<T>(T instance) where T : ISystem;
        /// <summary>
        /// ע��Model
        /// </summary>
        void RegisterModel<T>(T instance) where T : IModel;
        /// <summary>
        /// ע��Utility
        /// </summary>
        void RegisterUtility<T>(T instance);

        /// <summary>
        /// ע���¼�
        /// </summary>
        IUnRegister RegisterEvent<T>(Action<T> onEvent);
        /// <summary>
        /// ע���¼�
        /// </summary>
        void UnRegisterEvent<T>(Action<T> onEvent);
        #endregion

        #region ��ȡģ��ӿ�
        /// <summary>
        /// ��ȡ����
        /// </summary>
        T GetUtility<T>() where T : class, IUtility;
        T GetModel<T>() where T : class, IModel;
        T GetSystem<T>() where T : class, ISystem;
        #endregion

        #region  ����ģ��ӿ�
        /// <summary>
        /// ��������
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void SendCommand<T>() where T : ICommand, new();
        void SendCommand<T>(T command) where T : ICommand;

        /// <summary>
        /// �����¼�
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void SendEvent<T>() where T : new();
        void SendEvent<T>(T e);

        T SendQuery<T>(IQuery<T> query);
        #endregion
    }
    /// <summary>
    /// IOC���� ����������
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Architecture<T> : IArchitecture where T : Architecture<T>, new()
    {
        #region Private Field 
        /// <summary>
        /// �Ƿ��Ѿ���ʼ�����
        /// </summary>
        private bool mInited = false;
        /// <summary>
        ///   ���ڳ�ʼ���� Models �Ļ���
        /// </summary>
        private List<IModel> mModels = new List<IModel>();
        /// <summary>
        ///   ���ڳ�ʼ���� Systems �Ļ���
        /// </summary>
        private List<ISystem> mSystems = new List<ISystem>();

        private IOCContainer mContainer = new IOCContainer();

        private ITypeEventSystem mTypeEventSystem = new TypeEventSystem();


        /// <summary>
        ///  �ڲ���������֤��Ψһ��
        /// </summary>
        private static T mArchitecture = null;

        public static Action<T> OnRegisterPatch = architecture => { };
        #endregion

        #region Private Static Method

        private static void MakeSureArchitecture()
        {
            if (mArchitecture == null)
            {
                mArchitecture = new T();
                //��ʼ��Architectureע��ģ�Ͳ㣬ϵͳ�㣬���߲�
                mArchitecture.Init();

                OnRegisterPatch?.Invoke(mArchitecture);
                //��ʼ��Model
                foreach (var architectureModel in mArchitecture.mModels)
                {
                    architectureModel.Init();
                }
                //���Model
                mArchitecture.mModels.Clear();

                //��ʼ��System 
                foreach (var architectureSystem in mArchitecture.mSystems)
                {
                    architectureSystem.Init();
                }
                mArchitecture.mSystems.Clear();

                mArchitecture.mInited = true;


            }
        }

        #endregion

        #region abstract Method

        /// <summary>
        /// ����������д����ע��
        /// </summary>
        protected abstract void Init();
        #endregion

        #region Public Static  Method
        public static IArchitecture Interface
        {
            get
            {
                if (mArchitecture == null)
                {
                    MakeSureArchitecture();
                }
                return mArchitecture;
            }
        }
        /// <summary>
        /// IOC����ע�ᵥ��,�����Model ���� System ע����ʹ��RegisterSystem or RegisterModel
        /// v1.0.2�Ѿ��Ƴ��ú�������
        /// </summary>
        public static void Register<TGeneric>(TGeneric instance)
        {
            MakeSureArchitecture();
            mArchitecture.mContainer.Register<TGeneric>(instance);
        }

        #endregion

        #region Public Method
        /// <summary>
        /// ����ע�� ϵͳ������IOC������������ ��
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        public void RegisterSystem<TSystem>(TSystem instance) where TSystem : ISystem
        {
            instance.SetArchitecture(this);
            mContainer.Register<TSystem>(instance);
            if (mInited)
            {
                instance.Init();
            }
            else
            {
                mSystems.Add(instance);
            }
        }
        /// <summary>
        /// ����ע�� Model������IOC������������ ��
        /// </summary>
        public void RegisterModel<TModel>(TModel instance) where TModel : IModel
        {
            instance.SetArchitecture(this);
            mContainer.Register<TModel>(instance);
            if (mInited)
            {
                instance.Init();
            }
            else
            {
                mModels.Add(instance);
            }
        }

        public void RegisterUtility<TUtility>(TUtility instance)
        {
            mContainer.Register<TUtility>(instance);
        }

        public TUtility GetUtility<TUtility>() where TUtility : class, IUtility
        {
            return mContainer.Get<TUtility>();
        }

        public TModel GetModel<TModel>() where TModel : class, IModel
        {
            return mContainer.Get<TModel>();
        }

        public TSystem GetSystem<TSystem>() where TSystem : class, ISystem
        {
            return mContainer.Get<TSystem>();
        }

        public void SendCommand<TCommand>() where TCommand : ICommand, new()
        {
            var command = new TCommand();
            ExecuteCommand(command);
        }

        public void SendCommand<TCommand>(TCommand command) where TCommand : ICommand
        {
            ExecuteCommand(command);
        }

        protected virtual void ExecuteCommand(ICommand command)
        {
            command.SetArchitecture(this);
            command.Execute();
        }

        public void SendEvent<TEvent>() where TEvent : new()
        {
            mTypeEventSystem.Send<TEvent>();
        }

        public void SendEvent<TEvent>(TEvent e)
        {
            mTypeEventSystem.Send<TEvent>(e);
        }

        public IUnRegister RegisterEvent<TEvent>(Action<TEvent> onEvent)
        {
            return mTypeEventSystem.Register<TEvent>(onEvent);
        }
        public void UnRegisterEvent<TEvent>(Action<TEvent> onEvent)
        {
            mTypeEventSystem.UnRegister<TEvent>(onEvent);
        }

        public TQuery SendQuery<TQuery>(IQuery<TQuery> query)
        {
            query.SetArchitecture(this);
            return query.DoQuery();
        }

        #endregion
    }

    #endregion

    /*���ֲ�*/
    #region Controller
    public interface IController : ICanGetArchitecture, ICanSendCommand, ICanGetSystem, ICanGetModel, ICanSendEvent, ICanRegisterEvent, ICanSendQuery { }

    #endregion

    /*ϵͳ��*/
    #region  System
    public interface ISystem : ICanGetArchitecture, ICanSetArchitecture, ICanGetModel, ICanGetUtility, ICanSendEvent, ICanSendCommand, ICanGetSystem
    {
        void Init();
    }
    public abstract class AbstractSystem : ISystem
    {
        private IArchitecture mArchitecture = null;
        IArchitecture ICanGetArchitecture.GetArchitecture()
        {
            return mArchitecture;
        }

        public void Init()
        {
            OnInit();
        }

        void ICanSetArchitecture.SetArchitecture(IArchitecture architecture)
        {
            mArchitecture = architecture;
        }
        protected abstract void OnInit();
    }
    #endregion

    /*ģ�Ͳ�*/
    #region Model
    public interface IModel : ICanGetArchitecture, ICanSetArchitecture, ICanGetUtility, ICanSendEvent
    {
        void Init();
    }
    public abstract class AbstractModel : IModel
    {
        private IArchitecture mArchitecture = null;
        IArchitecture ICanGetArchitecture.GetArchitecture()
        {
            return mArchitecture;
        }

        public void Init()
        {
            OnInit();
        }

        void ICanSetArchitecture.SetArchitecture(IArchitecture architecture)
        {
            mArchitecture = architecture;
        }
        protected abstract void OnInit();
    }
    #endregion

    /*���߲�*/
    #region Utility
    public interface IUtility { }
    #endregion

    /*����*/
    #region Command
    //�̳�ICanSetArchitecture  �ӿ�ʵ�� ���нṹ����
    public interface ICommand : ICanSetArchitecture, ICanGetArchitecture, ICanGetSystem, ICanGetModel, ICanGetUtility, ICanSendEvent, ICanSendCommand, ICanSendQuery
    {
        void Execute();
    }
    //���� Command �࣬���� Command ����չ
    public abstract class AbstractCommand : ICommand
    {
        private IArchitecture mArchitexture;
        public void Execute()
        {
            OnExecute();
        }

        IArchitecture ICanGetArchitecture.GetArchitecture()
        {
            return mArchitexture;
        }

        void ICanSetArchitecture.SetArchitecture(IArchitecture architecture)
        {
            mArchitexture = architecture;
        }
        protected abstract void OnExecute();
    }
    #endregion

    /*��ѯ*/
    #region Query
    public interface IQuery<T> : ICanGetArchitecture, ICanSetArchitecture, ICanGetModel, ICanGetSystem, ICanSendQuery
    {
        T DoQuery();
    }
    public abstract class AbstractQuery<T> : IQuery<T>
    {
        private IArchitecture m_Architecture;
        public T DoQuery()
        {
            return OnDoQuery();
        }
        protected abstract T OnDoQuery();
        public IArchitecture GetArchitecture()
        {
            return m_Architecture;
        }

        public void SetArchitecture(IArchitecture architecture)
        {
            m_Architecture = architecture;
        }
    }
    #endregion

}

/*��ܿɷ��빤�߲���*/
namespace QFramework
{
    /*�¼�*/
    #region Event

      #region Event UnRegister
        /*�����ǣ���MonoBehavier���¼�ע������*/
        public interface IUnRegister
        {
            void UnRegister();
        }
        /*����v1.0.2����IUnRegisterList*/
        public interface IUnRegisterList
        {
            List<IUnRegister> UnregisterList { get; }
        }
        public static class IUnRegisterListExtension
        {
            public static void AddToUnregisterList(this IUnRegister self, IUnRegisterList unRegisterList)
            {
                unRegisterList.UnregisterList.Add(self);
            }

            public static void UnRegisterAll(this IUnRegisterList self)
            {
                foreach (var unRegister in self.UnregisterList)
                {
                    unRegister.UnRegister();
                }

                self.UnregisterList.Clear();
            }
        }

        /*Ȼ���ǣ��Զ����ע������*/
        /*����v1.0.2����CustomUnRegister*/
        /// �Զ����ע������
        public struct CustomUnRegister : IUnRegister
        {
            /// ί�ж���
            private Action mOnUnRegister { get; set; }

            /// ���ι��캯����ÿ��new ʱע��ί�� ע������
            public CustomUnRegister(Action onUnRegsiter)
            {
                mOnUnRegister = onUnRegsiter;
            }

            /// ��Դ�ͷ�
            public void UnRegister()
            {
                //����ִ��ע���ע������
                mOnUnRegister.Invoke();
                //Ȼ�����
                mOnUnRegister = null;
            }
        }

        public class UnRegisterOnDestroyTrigger : MonoBehaviour
        {
            private readonly HashSet<IUnRegister> mUnRegisters = new HashSet<IUnRegister>();
            public void AddUnRegister(IUnRegister unRegister)
            {
                mUnRegisters.Add(unRegister);
            }
            public void RemoveUnRegister(IUnRegister unRegister)
            {
                mUnRegisters.Remove(unRegister);
            }
            private void OnDestroy()
            {
                foreach (var unRegister in mUnRegisters)
                {
                    unRegister.UnRegister();
                }
                mUnRegisters.Clear();
            }
        }

        public static class UnRegisterExtension
        {
            public static IUnRegister UnRegisterWhenGameObjectDestroy(this IUnRegister unRegister, GameObject gameObject)
            {
                var trigger = gameObject.GetComponent<UnRegisterOnDestroyTrigger>();
                if (!trigger)
                {
                    trigger = gameObject.AddComponent<UnRegisterOnDestroyTrigger>();
                }
                trigger.AddUnRegister(unRegister);
                return unRegister;
            }
        }

      #endregion

      #region TypeEventSystem
        public interface ITypeEventSystem
        {
            void Send<T>() where T : new();
            void Send<T>(T e);
            IUnRegister Register<T>(Action<T> onEvent);
            void UnRegister<T>(Action<T> onEvent);
        }

        /*����TypeEventSystemҲ��ΪV1.0.2�汾 TypeEventSystem*/
        public class TypeEventSystem : ITypeEventSystem
        {
            //v1.0.2�汾
            private readonly EasyEvents mEvents = new EasyEvents();
            //ȫ��Ψһ����������
            public static readonly TypeEventSystem Global = new TypeEventSystem();

            public void Send<T>() where T : new()
            {
                mEvents.GetEvent<EasyEvent<T>>()?.Trigger(new T());
            }

            public void Send<T>(T e)
            {
                mEvents.GetEvent<EasyEvent<T>>()?.Trigger(e);
            }

            public IUnRegister Register<T>(Action<T> onEvent)
            {
                var e = mEvents.GetOrAddEvent<EasyEvent<T>>();
                return e.Register(onEvent);
            }

            public void UnRegister<T>(Action<T> onEvent)
            {
                var e = mEvents.GetEvent<EasyEvent<T>>();
                if (e != null)
                {
                    e.UnRegister(onEvent);
                }
            }
        }
    #endregion

      #region EasyEvents
        /* ԭ��Ϊ��ܾ�����Event ����ΪEasyEvent*/
        public interface IEasyEvent { }
        //�޲��¼�
        public class EasyEvent : IEasyEvent
        {
            private Action mOnEvent = () => { };

            public IUnRegister Register(Action onEvent)
            {
                mOnEvent += onEvent;
                //���ﷵ�ص���CustomUnRegister�Զ���ж���࣬���Ե�����ж�غ����Ӷ�ִ�к��� UnRegister
                return new CustomUnRegister(() => { UnRegister(onEvent); });
            }

            public void UnRegister(Action onEvent)
            {
                mOnEvent -= onEvent;
            }

            public void Trigger()
            {
                mOnEvent?.Invoke();
            }
        }
        //�����¼�
        public class EasyEvent<T> : IEasyEvent
        {
            private Action<T> mOnEvent = e => { };

            public IUnRegister Register(Action<T> onEvent)
            {
                mOnEvent += onEvent;
                return new CustomUnRegister(() => { UnRegister(onEvent); });
            }

            public void UnRegister(Action<T> onEvent)
            {
                mOnEvent -= onEvent;
            }

            public void Trigger(T t)
            {
                mOnEvent?.Invoke(t);
            }
        }
        //˫���¼�
        public class EasyEvent<T, K> : IEasyEvent
        {
            private Action<T, K> mOnEvent = (t, k) => { };

            public IUnRegister Register(Action<T, K> onEvent)
            {
                mOnEvent += onEvent;
                return new CustomUnRegister(() => { UnRegister(onEvent); });
            }

            public void UnRegister(Action<T, K> onEvent)
            {
                mOnEvent -= onEvent;
            }

            public void Trigger(T t, K k)
            {
                mOnEvent?.Invoke(t, k);
            }
        }
        //�����¼�
        public class EasyEvent<T, K, S> : IEasyEvent
        {
            private Action<T, K, S> mOnEvent = (t, k, s) => { };

            public IUnRegister Register(Action<T, K, S> onEvent)
            {
                mOnEvent += onEvent;
                return new CustomUnRegister(() => { UnRegister(onEvent); });
            }

            public void UnRegister(Action<T, K, S> onEvent)
            {
                mOnEvent -= onEvent;
            }

            public void Trigger(T t, K k, S s)
            {
                mOnEvent?.Invoke(t, k, s);
            }
        }

        //�����EasyEvents��
        public class EasyEvents
        {
            //ȫ��Ψһ
            private static EasyEvents mGlobalEvents = new EasyEvents();
            private Dictionary<Type, IEasyEvent> mTypeEvents = new Dictionary<Type, IEasyEvent>();

            public static T Get<T>() where T : IEasyEvent
            {
                return mGlobalEvents.GetEvent<T>();
            }


            public static void Register<T>() where T : IEasyEvent, new()
            {
                mGlobalEvents.AddEvent<T>();
            }


            public void AddEvent<T>() where T : IEasyEvent, new()
            {
                mTypeEvents.Add(typeof(T), new T());
            }

            public T GetEvent<T>() where T : IEasyEvent
            {
                IEasyEvent e;

                if (mTypeEvents.TryGetValue(typeof(T), out e))
                {
                    return (T)e;
                }

                return default;
            }

            public T GetOrAddEvent<T>() where T : IEasyEvent, new()
            {
                var eType = typeof(T);
                if (mTypeEvents.TryGetValue(eType, out var e))
                {
                    return (T)e;
                }

                var t = new T();
                mTypeEvents.Add(eType, t);
                return t;
            }
        }
    #endregion

    #endregion

    /*IOC*/
    #region IOC
    /*�������������ֵ������*/
    public class IOCContainer
    {
        public Dictionary<Type, object> mInstances = new Dictionary<Type, object>();
        public void Register<T>(T instance)
        {
            var key = typeof(T);
            if (mInstances.ContainsKey(key))
            {
                mInstances[key] = instance;
            }
            else
            {
                mInstances.Add(key, instance);
            }
        }
        public T Get<T>() where T : class
        {
            var key = typeof(T);

            if (mInstances.TryGetValue(key, out var tempInstance))
            {
                return tempInstance as T;
            }
            return null;
        }
    }
    #endregion

    /*���԰�*/
    #region BindableProperty
    public interface IReadonlyBindableProperty<T>
    {
        T Value { get; }

        IUnRegister RegisterWithInitValue(Action<T> action);
        void UnRegister(Action<T> onValueChanged);
        IUnRegister Register(Action<T> onValueChanged);
    }
    public interface IBindableProperty<T> : IReadonlyBindableProperty<T>
    {
        new T Value { get; set; }
        void SetValueWithoutEvent(T newValue);
    }
    public class BindableProperty<T> : IBindableProperty<T>
    {
        public BindableProperty(T defaultValue = default) { mValue = defaultValue; }

        private T mValue;

        public T Value
        {
            get => GetValue();
            set
            {
                if (value == null && mValue == null) return;
                if (value != null && value.Equals(mValue)) return;
                if (!value.Equals(mValue))
                {
                    SetValue(value);
                    mOnValueChanged?.Invoke(mValue);
                }
            }
        }

        protected virtual void SetValue(T newValue)
        {
            mValue = newValue;
        }
        protected virtual T GetValue()
        {
            return mValue;
        }

        public void SetValueWithoutEvent(T newValue)
        {
            mValue = newValue;
        }


        public Action<T> mOnValueChanged = (v) => { };

        public IUnRegister Register(Action<T> onValueChanged)
        {
            mOnValueChanged += onValueChanged;
            return new BindablePropertyUnRegister<T>()
            {
                BindableProperty = this,
                onValueChanged = onValueChanged
            };
        }
        public IUnRegister RegisterWithInitValue(Action<T> onValueChanged)
        {
            onValueChanged(mValue);
            return Register(onValueChanged);
        }

        public void UnRegister(Action<T> onValueChanged)
        {
            mOnValueChanged -= onValueChanged;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
        public static implicit operator T(BindableProperty<T> property)
        {
            return property.Value;
        }
    }
    public class BindablePropertyUnRegister<T> : IUnRegister  
    {
        public BindableProperty<T> BindableProperty { get; set; }
        public Action<T> onValueChanged { get; set; }
        public void UnRegister()
        {
            BindableProperty.UnRegister(onValueChanged);
            BindableProperty = null;
            onValueChanged = null;
        }
    }
    #endregion

    /*����*/
    #region Singleton

        #region MonoSingleton
        /*��Mono�����������Ѿ����ű�������Ϸ�����ϣ��Ѿ����ڲ���Ҫ����*/
        public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
        {
            private static T _single;
            public static T Single
            {
                get
                {
                    if (_single == null)
                    {
                        _single = GameObject.FindObjectOfType<T>();
                    }

                    return _single;
                }
            }
        }
        #endregion

        #region NormalSingleton
        /*��Normal���������ڷ� MonoBehaviour�ű���*/
        public class NormalSingleton<T> where T : class, new()
        {
            protected static T _single;

            public static T Single
            {
                get
                {
                    if (_single == null)
                    {
                        var t = new T();
                        if (t is MonoBehaviour)
                        {
                            Debug.LogError("Mono����ʹ��MonoSingleton");
                            return null;
                        }

                        _single = t;
                    }

                    return _single;
                }
            }
        }
        #endregion

        #region Singleton
        /*��Singleton���������ڷ� MonoBehaviour�ű���*/
        public class Singleton<T> where T : class
        {
            private static T mInstance;
            public static T Instance
            {
                get
                {
                    if (mInstance == null)
                    {
                        //ͨ�������ȡʵ�����ǹ��������캯������Ϊ��� �࣬������T �������һ���޲�˽�й��캯��
                        var ctors = typeof(T).GetConstructors(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        var ctor = Array.Find(ctors, c => c.GetParameters().Length == 0);
                        if (ctor == null)
                        {
                            throw new Exception("This class can not find any private constructors,please check up now!:" + typeof(T));
                        }
                        mInstance = ctor.Invoke(null) as T;
                    }
                    return mInstance;
                }
            }

        }
        #endregion

    #endregion

}







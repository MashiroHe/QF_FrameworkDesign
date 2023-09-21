/*
 * version: v1.0.1
 * author:  MashiroHe  
 * created: 2023-09-18 17:14
 * email:   1967407707@qq.com
 * purpose: QF框架搭建决定版 整合为 v1.0.0版本-并添加单例到框架中，其它部分与官方v1.0保持不变，并整合官方v1.0.2版本内容 ，本版本使用教程与QF官方v1.0.2一致，下面只包含框架部分，不包含QFToolKit，QFToolKit也请使用v1.0.2版本
 */

using System;
using System.Collections.Generic;
using UnityEngine;

/*框架核心部分*/
namespace QFramework
{
    #region Rule

    /*Architecture*/
    public interface IBelongToArchitecture { }
    public interface ICanGetArchitecture : IBelongToArchitecture  //源框架为 IBelongToArchitecture
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
        /// 继承接口ICanGetModel， GetModel静态扩展函数
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
        /// 继承接口ICanGetUtility ，GetUtility静态扩展函数
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

    /*根据官方v1.0.2添加 全局静态扩展函数，供继承IOnEvent接口类注销注册事件，具体参考QF使用手册*/
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
        #region 注册模块接口
        void RegisterSystem<T>(T instance) where T : ISystem;
        /// <summary>
        /// 注册Model
        /// </summary>
        void RegisterModel<T>(T instance) where T : IModel;
        /// <summary>
        /// 注册Utility
        /// </summary>
        void RegisterUtility<T>(T instance);

        /// <summary>
        /// 注册事件
        /// </summary>
        IUnRegister RegisterEvent<T>(Action<T> onEvent);
        /// <summary>
        /// 注销事件
        /// </summary>
        void UnRegisterEvent<T>(Action<T> onEvent);
        #endregion

        #region 获取模块接口
        /// <summary>
        /// 获取工具
        /// </summary>
        T GetUtility<T>() where T : class, IUtility;
        T GetModel<T>() where T : class, IModel;
        T GetSystem<T>() where T : class, ISystem;
        #endregion

        #region  发送模块接口
        /// <summary>
        /// 发送命令
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void SendCommand<T>() where T : ICommand, new();
        void SendCommand<T>(T command) where T : ICommand;

        /// <summary>
        /// 发送事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void SendEvent<T>() where T : new();
        void SendEvent<T>(T e);

        T SendQuery<T>(IQuery<T> query);
        #endregion
    }
    /// <summary>
    /// IOC容器 单例管理类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Architecture<T> : IArchitecture where T : Architecture<T>, new()
    {
        #region Private Field 
        /// <summary>
        /// 是否已经初始化完成
        /// </summary>
        private bool mInited = false;
        /// <summary>
        ///   用于初始化的 Models 的缓存
        /// </summary>
        private List<IModel> mModels = new List<IModel>();
        /// <summary>
        ///   用于初始化的 Systems 的缓存
        /// </summary>
        private List<ISystem> mSystems = new List<ISystem>();

        private IOCContainer mContainer = new IOCContainer();

        private ITypeEventSystem mTypeEventSystem = new TypeEventSystem();


        /// <summary>
        ///  内部单例，保证其唯一性
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
                //初始化Architecture注册模型层，系统层，工具层
                mArchitecture.Init();

                OnRegisterPatch?.Invoke(mArchitecture);
                //初始化Model
                foreach (var architectureModel in mArchitecture.mModels)
                {
                    architectureModel.Init();
                }
                //清空Model
                mArchitecture.mModels.Clear();

                //初始化System 
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
        /// 留给子类重写单例注册
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
        /// IOC容器注册单例,如果是Model 或者 System 注册请使用RegisterSystem or RegisterModel
        /// v1.0.2已经移除该函数方法
        /// </summary>
        public static void Register<TGeneric>(TGeneric instance)
        {
            MakeSureArchitecture();
            mArchitecture.mContainer.Register<TGeneric>(instance);
        }

        #endregion

        #region Public Method
        /// <summary>
        /// 用于注册 系统单例到IOC单例管理容器 中
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
        /// 用于注册 Model单例到IOC单例管理容器 中
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

    /*表现层*/
    #region Controller
    public interface IController : ICanGetArchitecture, ICanSendCommand, ICanGetSystem, ICanGetModel, ICanSendEvent, ICanRegisterEvent, ICanSendQuery { }

    #endregion

    /*系统层*/
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

    /*模型层*/
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

    /*工具层*/
    #region Utility
    public interface IUtility { }
    #endregion

    /*命令*/
    #region Command
    //继承ICanSetArchitecture  接口实现 持有结构函数
    public interface ICommand : ICanSetArchitecture, ICanGetArchitecture, ICanGetSystem, ICanGetModel, ICanGetUtility, ICanSendEvent, ICanSendCommand, ICanSendQuery
    {
        void Execute();
    }
    //抽象 Command 类，来简化 Command 的扩展
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

    /*查询*/
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

/*框架可分离工具部分*/
namespace QFramework
{
    /*事件*/
    #region Event

      #region Event UnRegister
        /*首先是，非MonoBehavier类事件注销函数*/
        public interface IUnRegister
        {
            void UnRegister();
        }
        /*根据v1.0.2新增IUnRegisterList*/
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

        /*然后是，自定义可注销的类*/
        /*根据v1.0.2新增CustomUnRegister*/
        /// 自定义可注销的类
        public struct CustomUnRegister : IUnRegister
        {
            /// 委托对象
            private Action mOnUnRegister { get; set; }

            /// 带参构造函数，每次new 时注入委托 注销函数
            public CustomUnRegister(Action onUnRegsiter)
            {
                mOnUnRegister = onUnRegsiter;
            }

            /// 资源释放
            public void UnRegister()
            {
                //首先执行注入的注销函数
                mOnUnRegister.Invoke();
                //然后，清空
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

        /*这里TypeEventSystem也改为V1.0.2版本 TypeEventSystem*/
        public class TypeEventSystem : ITypeEventSystem
        {
            //v1.0.2版本
            private readonly EasyEvents mEvents = new EasyEvents();
            //全局唯一，供外界调用
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
        /* 原型为框架决定版Event 更改为EasyEvent*/
        public interface IEasyEvent { }
        //无参事件
        public class EasyEvent : IEasyEvent
        {
            private Action mOnEvent = () => { };

            public IUnRegister Register(Action onEvent)
            {
                mOnEvent += onEvent;
                //这里返回的是CustomUnRegister自定义卸载类，可以调用其卸载函数从而执行函数 UnRegister
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
        //单参事件
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
        //双参事件
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
        //三参事件
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

        //具体的EasyEvents类
        public class EasyEvents
        {
            //全局唯一
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
    /*简单理解就是数据字典管理类*/
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

    /*属性绑定*/
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

    /*单例*/
    #region Singleton

        #region MonoSingleton
        /*该Mono单例适用于已经将脚本挂在游戏物体上，已经存在不需要创建*/
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
        /*该Normal单例适用于非 MonoBehaviour脚本类*/
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
                            Debug.LogError("Mono类请使用MonoSingleton");
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
        /*该Singleton单例适用于非 MonoBehaviour脚本类*/
        public class Singleton<T> where T : class
        {
            private static T mInstance;
            public static T Instance
            {
                get
                {
                    if (mInstance == null)
                    {
                        //通过反射获取实例化非公开，构造函数参数为零的 类，所以类T 必须的有一个无参私有构造函数
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







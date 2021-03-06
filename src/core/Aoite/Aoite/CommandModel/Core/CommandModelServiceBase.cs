﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aoite.CommandModel
{
    /// <summary>
    /// 表示一个命令模型的基础服务。
    /// </summary>
    public abstract class CommandModelServiceBase : ObjectDisposableBase, ICommandModelService
    {
        /// <summary>
        /// 初始化一个 <see cref="Aoite.CommandModel.CommandModelServiceBase"/> 类的新实例。
        /// </summary>
        public CommandModelServiceBase() : this(ObjectFactory.Global) { }

        /// <summary>
        /// 指定服务容器，初始化一个 <see cref="Aoite.CommandModel.CommandModelServiceBase"/> 类的新实例。
        /// </summary>
        /// <param name="container">服务容器。</param>
        public CommandModelServiceBase(IIocContainer container)
        {
            if(container == null) throw new ArgumentNullException("container");
            this._Container = container;
        }

        private IIocContainer _Container;
        /// <summary>
        /// 设置或获取命令模型服务容器。
        /// </summary>
        public IIocContainer Container
        {
            get { return this._Container; }
            set
            {
                this._Container = value;
                if(this._Container != null) this._Container.AddService<ICommandModelService>(this);
            }
        }

        /// <summary>
        /// 获取执行命令模型的用户。该属性可能返回 null 值。
        /// </summary>
        public virtual dynamic User { get { return this._Container.GetUser(); } }

        /// <summary>
        /// 获取命令总线。
        /// </summary>
        protected ICommandBus Bus { get { return this._Container.GetService<ICommandBus>(); } }

        /// <summary>
        /// 执行一个命令模型。
        /// </summary>
        /// <typeparam name="TCommand">命令模型的数据类型。</typeparam>
        /// <param name="command">命令模型。</param>
        /// <param name="executing">命令模型执行前发生的方法。</param>
        /// <param name="executed">命令模型执行后发生的方法。</param>
        /// <returns>返回命令模型。</returns>
        protected virtual TCommand Execute<TCommand>(TCommand command
            , CommandExecutingHandler<TCommand> executing = null
            , CommandExecutedHandler<TCommand> executed = null) where TCommand : ICommand
        {
            return this.Bus.Execute(command, executing, executed);
        }

        /// <summary>
        /// 以异步的方式执行一个命令模型。
        /// </summary>
        /// <typeparam name="TCommand">命令模型的数据类型。</typeparam>
        /// <param name="command">命令模型。</param>
        /// <param name="executing">命令模型执行前发生的方法。</param>
        /// <param name="executed">命令模型执行后发生的方法。</param>
        /// <returns>返回一个异步操作。</returns>
        protected virtual Task<TCommand> ExecuteAsync<TCommand>(TCommand command
            , CommandExecutingHandler<TCommand> executing = null
            , CommandExecutedHandler<TCommand> executed = null) where TCommand : ICommand
        {
            return this.Bus.ExecuteAsync(command, executing, executed);
        }

        /// <summary>
        /// 获取一个全局锁的功能，如果获取锁超时将会抛出异常。
        /// </summary>
        /// <typeparam name="T">数据类型。</typeparam>
        /// <param name="timeout">获取锁的超时设定。</param>
        /// <returns>返回一个锁。</returns>
        protected IDisposable AcquireLock<T>(TimeSpan? timeout = null)
        {
            return this.AcquireLock(typeof(T).FullName, timeout);
        }

        /// <summary>
        /// 获取一个全局锁的功能，如果获取锁超时将会抛出异常。
        /// </summary>
        /// <param name="key">锁的键。</param>
        /// <param name="timeout">获取锁的超时设定。</param>
        /// <returns>返回一个锁。</returns>
        protected virtual IDisposable AcquireLock(string key, TimeSpan? timeout = null)
        {
            return this._Container.GetService<ILockProvider>().Lock(key, timeout);
        }

        /// <summary>
        /// 获取指定数据类型键的原子递增序列。
        /// </summary>
        /// <typeparam name="T">数据类型。</typeparam>
        /// <param name="increment">递增量。</param>
        /// <returns>返回递增的序列。</returns>
        protected long Increment<T>(long increment = 1)
        {
            return this.Increment(typeof(T).FullName, increment);
        }

        /// <summary>
        /// 获取指定键的原子递增序列。
        /// </summary>
        /// <param name="key">序列的键。</param>
        /// <param name="increment">递增量。</param>
        /// <returns>返回递增的序列。</returns>
        protected virtual long Increment(string key, long increment = 1)
        {
            return this._Container.GetService<ICounterProvider>().Increment(key, increment);
        }

        /// <summary>
        /// 开始事务模式。
        /// </summary>
        /// <returns>返回一个事务。</returns>
        protected virtual ITransaction BeginTransaction()
        {
            return new DefaultTransaction();
        }

        /// <summary>
        /// 获取指定实体数据类型的缓存键。
        /// </summary>
        /// <param name="type">实体的数据类型。</param>
        /// <param name="keyValue">实体的主键。</param>
        /// <returns>返回一个非 null 值的字符串。</returns>
        protected virtual string GenerateEntityCacheKey(Type type, object keyValue)
        {
            return type.Name + ":" + keyValue;
        }

        ///// <summary>
        ///// 获取指定实体数据类型的缓存键。
        ///// </summary>
        ///// <typeparam name="T">实体的数据类型。</typeparam>
        ///// <param name="keyValue">实体的主键。</param>
        ///// <returns>返回一个非 null 值的字符串。</returns>
        //protected virtual string GenerateEntityCacheKey<T>(T value)
        //{
        //    var mapper = TypeMapper.Instance<T>.Mapper;
        //    var kps = mapper.KeyProperties;
        //    if(kps.Length == 0) throw new ArgumentNullException("类型“{0}”不存在任何主键。".Fmt(mapper.Type.FullName));
        //    var key = mapper.Name + ":" + kps[0].GetValue(value);

        //    for(int i = 1; i < kps.Length; i++)
        //    {
        //        key = key + "+" + kps[i].GetValue(value);
        //    }
            
        //    return key;
        //}

        /// <summary>
        /// 设置缓存。
        /// </summary>
        /// <param name="key">缓存键。</param>
        /// <param name="value">缓存值。</param>
        protected virtual void Set(string key, object value)
        {
            this._Container.GetService<ICacheProvider>().Set(key, value);
        }
        /// <summary>
        /// 设置基于实体的缓存。
        /// </summary>
        /// <typeparam name="T">实体的数据类型。</typeparam>
        /// <param name="keyValue">实体的主键。</param>
        /// <param name="value">实体的值。</param>
        protected void SetEntity<T>(object keyValue, T value)
        {
            this.Set(this.GenerateEntityCacheKey(typeof(T), keyValue), value);
        }

        /// <summary>
        /// 获取缓存。
        /// </summary>
        /// <param name="key">缓存键。</param>
        /// <param name="valueFactory">若找不到缓存时的延迟设置回调方法。</param>
        /// <returns>返回缓存值，或一个 null 值。</returns>
        protected virtual object Get(string key, Func<object> valueFactory = null)
        {
            return this._Container.GetService<ICacheProvider>().Get(key, valueFactory);
        }

        /// <summary>
        /// 获取缓存。
        /// </summary>
        /// <typeparam name="T">缓存的数据类型。</typeparam>
        /// <param name="key">缓存键。</param>
        /// <param name="valueFactory">若找不到缓存时的延迟设置回调方法。</param>
        /// <returns>返回缓存值，或一个 <typeparamref name="T"/> 的默认值。</returns>
        protected T Get<T>(string key, Func<T> valueFactory = null)
        {
            var value = this.Get(key, new Func<object>(() => valueFactory()));
            if(value == null) return default(T);
            return (T)value;
        }

        /// <summary>
        /// 获取基于实体的缓存。
        /// </summary>
        /// <typeparam name="T">缓存的数据类型。</typeparam>
        /// <param name="keyValue">实体的主键。</param>
        /// <param name="valueFactory">若找不到缓存时的延迟设置回调方法。</param>
        /// <returns>返回缓存值，或一个 <typeparamref name="T"/> 的默认值。</returns>
        protected T GetEntity<T>(object keyValue, Func<T> valueFactory = null)
        {
            return Get<T>(this.GenerateEntityCacheKey(typeof(T), keyValue), valueFactory);
        }

        /// <summary>
        /// 检测指定的缓存键是否存在。
        /// </summary>
        /// <param name="key">缓存键。</param>
        /// <returns>存在返回 true，否则返回 false。</returns>
        protected virtual bool Exstis(string key)
        {
            return this._Container.GetService<ICacheProvider>().Exists(key);
        }

        /// <summary>
        /// 检测基于实体指定的缓存键是否存在。
        /// </summary>
        /// <typeparam name="T">缓存的数据类型。</typeparam>
        /// <param name="keyValue">实体的主键。</param>
        /// <returns>存在返回 true，否则返回 false。</returns>
        protected bool ExstisEntity<T>(object keyValue)
        {
            return this.Exstis(this.GenerateEntityCacheKey(typeof(T), keyValue));
        }
    }
}

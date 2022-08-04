namespace Qin.AutoInject
{
    /// <summary>
    /// 自动注入接口
    /// </summary>
    public interface IDependency
    {

    }
    /// <summary>
    /// 自动注入——单例
    /// </summary>
    public interface ISingletonDependency : IDependency
    {

    }
    /// <summary>
    /// 自动注入——域接口
    /// </summary>
    public interface IScopedDependency : IDependency
    {

    }
    /// <summary>
    /// 自动注入——暂时性
    /// </summary>
    public interface ITransientDependency : IDependency
    {

    }
    /// <summary>
    /// 阻止注入——当接口切换实现类时，在旧的实现类上实现这个接口
    /// </summary>
    public interface IRejectDependenct : IDependency
    {

    }
    /// <summary>
    /// 对象自动注入——单例
    /// </summary>
    public interface IObjectSingletonDependency : IDependency
    {

    }
    /// <summary>
    /// 对象自动注入——域接口
    /// </summary>
    public interface IObjectScopedDependency : IDependency
    {

    }
    /// <summary>
    /// 对象自动注入——暂时性
    /// </summary>
    public interface IObjectTransientDependency : IDependency
    {

    }


}

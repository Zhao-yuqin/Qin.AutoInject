using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Qin.AutoInject
{

    public static class AutoInject
    {
        /// <summary>
        /// 自动注入入口，传入需要注入的程序集AssemblyName
        /// </summary>
        /// <remarks>
        /// 可以通过 Assembly.GetEntryAssembly().GetReferencedAssemblies()获取当前项目引用的程序集
        /// </remarks>
        public static void Inject(this IServiceCollection services, IEnumerable<AssemblyName> assemblies)
        {
            var dType = new DependencyTypes();
            var iTypes = new List<Type>();
            var oTypes = new List<Type>();
            var ioTypes = new List<Type>();

            var allTypes = GetAllTypes(assemblies);
            GetInjectTypes(allTypes, out iTypes, out oTypes, out ioTypes);

            foreach (var iType in iTypes)
            {
                Type[] objtypes = oTypes.Where(d => iType.IsAssignableFrom(d) && !dType.RejectDI.IsAssignableFrom(d)).ToArray();
                if (objtypes?.Length > 0)
                {
                    foreach (var objtype in objtypes)
                    {
                        var injectTypeList = objtype.GetInterfaces()
                                                    .Where(i => i == dType.TransientDI
                                                             || i == dType.ScopedDI
                                                             || i == dType.SingletonDI)
                                                    .ToList();

                        DoInject(services, iType, objtype, injectTypeList);
                    }

                }
                else
                {
                    throw new Exception($"当前接口[{iType.Name}]没有找到对应的实现类");
                }
            }

            foreach (var ioType in ioTypes)
            {
                var injectTypeList = ioType.GetInterfaces()
                                            .Where(i => i == dType.ObjTransientDI
                                                     || i == dType.ObjScopedDI
                                                     || i == dType.ObjSingletonDI)
                                            .ToList();
                DoInject(services, ioType, injectTypeList);
            }

        }


        private static List<Type> GetAllTypes(IEnumerable<AssemblyName> assemblies)
        {
            var types = new List<Type>();
            var currentAssembly = Assembly.GetExecutingAssembly().GetName();
            types.AddRange(Assembly.Load(currentAssembly).GetTypes());

            foreach (var a in assemblies)
                types.AddRange(Assembly.Load(a).GetTypes());

            return types;
        }


        private static void GetInjectTypes(List<Type> allTypes, out List<Type> iTypes, out List<Type> oTypes, out List<Type> ioTypes)
        {
            var dType = new DependencyTypes();

            iTypes = allTypes.Where(d => d.IsInterface && dType.DefaultDI.IsAssignableFrom(d))
                             .Where(d => d != dType.DefaultDI)
                             .Where(d => d != dType.SingletonDI)
                             .Where(d => d != dType.ScopedDI)
                             .Where(d => d != dType.TransientDI)
                             .Where(d => d != dType.ObjSingletonDI)
                             .Where(d => d != dType.ObjScopedDI)
                             .Where(d => d != dType.ObjTransientDI)
                             .Where(d => d != dType.RejectDI)
                             .ToList();

            oTypes = allTypes.Where(d => d.IsClass)
                             .Where(d => dType.DefaultDI.IsAssignableFrom(d))
                             .Where(d => !dType.RejectDI.IsAssignableFrom(d))
                             .ToList();

            ioTypes = oTypes.Where(d => dType.ObjSingletonDI.IsAssignableFrom(d)
                                     || dType.ObjScopedDI.IsAssignableFrom(d)
                                     || dType.ObjTransientDI.IsAssignableFrom(d))
                            .ToList();


        }

        /// <summary>
        /// 对象注入
        /// </summary>
        /// <param name="services">ServiceCollection</param>
        /// <param name="objtype">对象类型</param>
        /// <param name="injectTypeList">IDependency类型</param>
        /// <exception cref="Exception"></exception>
        private static void DoInject(IServiceCollection services, Type objtype, List<Type> injectTypeList)
        {
            if (injectTypeList?.Count == 1)
            {
                string injecttypename = injectTypeList.Single().Name;

                switch (injecttypename)
                {
                    case "IObjectSingletonDependency":
                        //Console.WriteLine($"services.AddSingleton({objtype})");
                        services.AddSingleton(objtype);
                        break;
                    case "IObjectScopedDependency":
                        //Console.WriteLine($"services.AddScoped({objtype})");
                        services.AddScoped(objtype);
                        break;
                    case "IObjectTransientDependency":
                        //Console.WriteLine($"services.AddTransient({objtype})");
                        services.AddTransient(objtype);
                        break;
                    default:
                        throw new Exception($"当前接[{objtype.Name}]没有指定注入实例的生命周期");
                }
            }
            else
            {
                throw new Exception($"当前接口[{objtype.Name}]没有找到合适的生命周期类型");
            }
        }
        /// <summary>
        /// 接口注入
        /// </summary>
        /// <param name="services">ServiceCollection</param>
        /// <param name="iType">接口类型</param>
        /// <param name="objtype">对象类型</param>
        /// <param name="injectTypeList">IDependency类型</param>
        /// <exception cref="Exception"></exception>
        private static void DoInject(IServiceCollection services, Type iType, Type objtype, List<Type> injectTypeList)
        {
            if (injectTypeList?.Count == 1)
            {
                string injecttypename = injectTypeList.Single().Name;

                switch (injecttypename)
                {
                    case "ISingletonDependency":
                        //Console.WriteLine($"services.AddSingleton({iType}, {objtype})");
                        services.AddSingleton(iType, objtype);
                        break;
                    case "IScopedDependency":
                        //Console.WriteLine($"services.AddScoped({iType}, {objtype})");
                        services.AddScoped(iType, objtype);
                        break;
                    case "ITransientDependency":
                        //Console.WriteLine($"services.AddTransient({iType}, {objtype})");
                        services.AddTransient(iType, objtype);
                        break;
                    default:
                        throw new Exception($"当前接[{iType.Name}]没有指定注入实例的生命周期");
                }
            }
            else
            {
                throw new Exception($"当前接口[{iType.Name}]没有找到合适的生命周期类型");
            }
        }



        /// <summary>
        /// 自动注入类型集合
        /// </summary>
        private class DependencyTypes
        {
            public DependencyTypes()
            {
                DefaultDI = typeof(IDependency);
                SingletonDI = typeof(ISingletonDependency);
                ScopedDI = typeof(IScopedDependency);
                TransientDI = typeof(ITransientDependency);
                ObjSingletonDI = typeof(IObjectSingletonDependency);
                ObjScopedDI = typeof(IObjectScopedDependency);
                ObjTransientDI = typeof(IObjectTransientDependency);
                RejectDI = typeof(IRejectDependenct);
            }
            public Type DefaultDI { get; set; }
            public Type SingletonDI { get; set; }
            public Type ScopedDI { get; set; }
            public Type TransientDI { get; set; }
            public Type ObjSingletonDI { get; set; }
            public Type ObjScopedDI { get; set; }
            public Type ObjTransientDI { get; set; }
            public Type RejectDI { get; set; }

        }
    }
}

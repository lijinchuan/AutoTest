using Autofac;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.UI
{
    /// <summary>
    /// Autofac注入类
    /// </summary>
    public class AutofacBuilder
    {
        private static IContainer _container;
        public static void init()
        {
            ContainerBuilder builder = new ContainerBuilder();

            //注册当前程序集的所有类成员
            _ = builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AsImplementedInterfaces().AsSelf();

            var readAssemblies = ReadAssemblies(AppDomain.CurrentDomain.BaseDirectory, new[] { "AutoTest*.dll" });
            var assemblies = readAssemblies.Select(Assembly.Load).ToArray();
            foreach (var a in assemblies)
            {
                _ = builder.RegisterAssemblyTypes(a)
                .AsImplementedInterfaces().AsSelf();
            }

            //var configFile = System.Configuration.ConfigurationManager.AppSettings["configfile"];
            //var configText = File.ReadAllText(configFile, Encoding.UTF8);
            

            //_ = builder.Register<IConfigurationProvider>(ctx => new MapperConfiguration(cfg => cfg.AddMaps("Topuc22Top.Spiders.Position.Simulator.Domain"))).SingleInstance();
            _ = builder.Register<IMapper>(ctx => new Mapper(ctx.Resolve<IConfigurationProvider>(), ctx.Resolve)).InstancePerDependency();

            //_ = builder.RegisterModule<Util.LoggingModule>();

            _container = builder.Build();
            //只有在Build之后，才能调用GetFromFac
        }

        public static T GetFromFac<T>()
        {
            return _container.Resolve<T>();
        }

        private static IEnumerable<AssemblyName> ReadAssemblies(string basePath, string[] searchPatterns)
        {
            var files = searchPatterns.SelectMany(searchPattern => Directory.GetFiles(
                basePath,
                searchPattern,
                SearchOption.TopDirectoryOnly));

            foreach (var dllFile in files)
            {
                yield return AssemblyName.GetAssemblyName(dllFile);
            }
        }
    }
}

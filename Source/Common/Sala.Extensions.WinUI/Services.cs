// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Sala.Extensions.WinUI;

using Castle.MicroKernel.Context;
using Castle.MicroKernel.ModelBuilder.Inspectors;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.MicroKernel;
using Castle.Windsor;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Net.Http;
using MdcAi.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

public static class Services
{
    // Doc: https://github.com/castleproject/Windsor/blob/master/docs/registering-components-by-conventions.md

    public static IWindsorContainer Container { get; set; }

    public static void Install()
    {
        Container = new WindsorContainer();

        // So we can use collections of implementations of the same base type
        Container.Kernel.Resolver.AddSubResolver(new CollectionResolver(Container.Kernel));

        // We don't want to inject properties, only ctors
        // https://stackoverflow.com/questions/178611
        var propInjector = Container.Kernel.ComponentModelBuilder
                                    .Contributors
                                    .OfType<PropertiesDependenciesModelInspector>()
                                    .Single();

        Container.Kernel.ComponentModelBuilder.RemoveContributor(propInjector);

        Container.Install();
    }

    public static void RegisterViewModels(params Assembly[] assemblies)
    {
        foreach (var asm in assemblies)
            RegisterViewModels(asm);
    }

    public static void RegisterViewModels(Assembly asm) =>
        RegisterViewModels(Types.FromAssembly(asm));

    public static void RegisterViewModels(FromAssemblyDescriptor assemblyDescriptor) =>
        Container.Register(assemblyDescriptor
                           .BasedOn<ViewModel>()
                           .Unless(t => t.GetCustomAttributes(typeof(DoNotRegisterAttribute), false).Any() ||
                                        t.IsAbstract ||
                                        Container.Kernel.HasComponent(t))
                           .WithService.Self()
                           .WithService.DefaultInterfaces()
                           .Configure(c =>
                           {
                               var isSingleton = c.Implementation.GetCustomAttribute(typeof(SingletonAttribute), false) != null;
                               if (isSingleton)
                                   c.LifestyleSingleton();
                               else
                                   c.LifestyleTransient();
                           }));

    public static void RegisterViewModel<TVm>() where TVm : class =>
        RegisterTransient<TVm>();

    public static void RegisterViewModel<T, TVm>() where T : class
                                                   where TVm : T =>
        RegisterTransient<T, TVm>(true);

    public static void RegisterViews(params Assembly[] assemblies)
    {
        foreach (var asm in assemblies)
            RegisterViews(asm);
    }

    public static void RegisterViews(Assembly asm) =>
        RegisterViews(Types.FromAssembly(asm));

    public static void RegisterViews(FromAssemblyDescriptor assemblyDescriptor) =>
        Container.Register(assemblyDescriptor
                           .BasedOn(typeof(IViewFor<>))
                           .Unless(t => t.GetCustomAttributes(typeof(DoNotRegisterAttribute), false).Any())
                           .WithService.FromInterface()
                           .LifestyleTransient());

    public static void RegisterViewModelsAndViews(params Assembly[] assemblies)
    {
        foreach (var asm in assemblies)
            RegisterViewModelsAndViews(asm);
    }

    public static void RegisterViewModelsAndViews(params string[] asmNames)
    {
        foreach (var asm in asmNames)
            RegisterViewModelsAndViews(asm);
    }

    public static void RegisterViewModelsAndViews(Assembly asm) =>
        RegisterViewModelsAndViews(Types.FromAssembly(asm));

    public static void RegisterViewModelsAndViews(string asmName) =>
        RegisterViewModelsAndViews(Types.FromAssemblyNamed(asmName));

    public static void RegisterViewModelsAndViews(FromAssemblyDescriptor assemblyDescriptor)
    {
        RegisterViewModels(assemblyDescriptor);
        RegisterViews(assemblyDescriptor);
    }

    public static void RegisterLoggerFactory() =>
        Container.Register(Component.For<ILoggerFactory>()
                                    .Instance(LoggingExtensions.LoggerFactory)
                                    .LifeStyle.Singleton);

    public static void RegisterHttpClient(HttpClient client) => RegisterSingleton(client);

    public static void RegisterMainWinow<T>() where T : Window => RegisterSingleton<T>();

    public static void RegisterSingleton<T>(T instance = default) where T : class
    {
        if (instance == null)
            Container.Register(Component.For<T>().LifeStyle.Singleton);
        else
            Container.Register(Component.For<T>().Instance(instance).LifeStyle.Singleton);
    }

    public static void RegisterSingleton<T, TImpl>(bool implToo = false) where T : class
                                                                         where TImpl : T =>
        Container.Register(GetComponentWithImpl<T, TImpl>(implToo).LifeStyle.Singleton);

    public static void RegisterSingleton<T1, T2, TImpl>() where T1 : class
                                                          where T2 : class
                                                          where TImpl : T1, T2 =>
        Container.Register(Component.For<T1, T2>()
                                    .ImplementedBy<TImpl>()
                                    .LifeStyle.Singleton);

    public static void RegisterTransient<T>() where T : class =>
        Container.Register(Component.For<T>().LifeStyle.Transient);

    public static void RegisterTransient<T, TImpl>(bool implToo = false) where T : class
                                                                         where TImpl : T =>
        Container.Register(GetComponentWithImpl<T, TImpl>(implToo).LifeStyle.Transient);

    public static void RegisterTransient<T1, T2, TImpl>() where T1 : class
                                                          where T2 : class
                                                          where TImpl : T1, T2 =>
        Container.Register(Component.For<T1, T2>()
                                    .ImplementedBy<TImpl>()
                                    .LifeStyle.Transient);

    private static ComponentRegistration<T> GetComponentWithImpl<T, TImpl>(bool implToo) where T : class
                                                                                         where TImpl : T =>
        implToo ?
            Component.For<T, TImpl>()
                     .ImplementedBy<TImpl>() :
            Component.For<T>()
                     .ImplementedBy<TImpl>();

    public static T Get<T>(T def = default)
    {
        try
        {
            if (Container?.Kernel.HasComponent(typeof(T)) ?? false)
                return Container.Resolve<T>();
            return def;
        }
        catch (Exception ex)
        {
            throw new DependencyResolveException(ex);
        }
    }

    public static T Get<T>(IDictionary<string, object> arguments, T def = default)
    {
        try
        {
            if (Container?.Kernel.HasComponent(typeof(T)) ?? false)
                return Container.Resolve<T>(arguments);
            return def;
        }
        catch (Exception ex)
        {
            throw new DependencyResolveException(ex);
        }
    }

    public static bool HasComponent(Type type) => Container?.Kernel.HasComponent(type) ?? false;

    public static bool HasComponent<T>() => HasComponent(typeof(T));

    public static object GetRequired(Type type, string key = null)
    {
        try
        {
            return key == null ? Container.Resolve(type) : Container.Resolve(key, type);
        }
        catch (Exception ex)
        {
            throw new DependencyResolveException(ex);
        }
    }

    public static T GetRequired<T>(string typeName = null) => (T)GetRequired(typeof(T), typeName);

    public static T GetRequired<T>((string name, object value)[] arguments) =>
        GetRequired<T>(arguments.ToDictionary(k => k.name, v => v.value));

    public static T GetRequired<T>(IDictionary<string, object> arguments)
    {
        try
        {
            return Container.Resolve<T>(arguments);
        }
        catch (Exception ex)
        {
            throw new DependencyResolveException(ex);
        }
    }

    public static IViewFor GetView(Type viewModelType, string viewName = null, bool window = false)
    {
        IHandler[] GetAppropriateHandlers(Type vmt)
        {
            var viewHandlersAttempt =
                Container.Kernel
                         .GetAssignableHandlers(typeof(IViewFor<>).MakeGenericType(vmt))
                         .Where(h => typeof(Window).IsAssignableFrom(h.ComponentModel.Implementation) ? window : !window)
                         .ToArray();

            // If no handler is found, try base type - multiple vms may use the same view if it's set to their base type. It'd be
            // nice if this also supported interfaces.
            if (viewHandlersAttempt.Length == 0 && vmt.BaseType is { } baseType)
                return GetAppropriateHandlers(baseType);

            return viewHandlersAttempt;
        }

        var viewHandlers = GetAppropriateHandlers(viewModelType);

        if (viewHandlers.Length == 0)
            throw new ArgumentException("No view found for specified view model.");

        if (viewHandlers.Length > 1)
            throw new ArgumentException("Multiple views found for specified view model, " +
                                        "this is currently not supported.");

        if (viewHandlers.First().Resolve(CreationContext.CreateEmpty()) is IViewFor view)
            return view;

        throw new DependencyResolveException();
    }

    public static IViewFor GetView<T>(T viewModel, string viewName = null, bool window = false) =>
        GetView(typeof(T), viewName, window);

    public static IViewFor GetView(object viewModel, string viewName = null, bool window = false) =>
        GetView(viewModel.GetType(), viewName, window);

    public static UserControl GetUserControlView<TVm>(TVm viewModel, string viewName = null, bool setVm = true) =>
        GetUserControlViewInternal(viewModel, FixGenericVmTypeForViews(viewModel), viewName, setVm);

    public static UserControl GetUserControlView(object viewModel, string viewName = null, bool setVm = true) =>
        GetUserControlViewInternal(viewModel, viewModel.GetType(), viewName, setVm);

    private static UserControl GetUserControlViewInternal(object viewModel, Type viewModelType, string viewName = null, bool setVm = true)
    {
        if (GetView(viewModelType, viewName) is not (UserControl uc and IViewFor v))
            throw new DependencyResolveException();
        if (setVm)
            v.ViewModel = viewModel;
        return uc;
    }

    private static Type FixGenericVmTypeForViews<TVm>(TVm viewModel)
    {
        // This 'fixes' a problem where VM is often an interface but we really want to resolve the
        // view for an actual VM type, not its interface.
        var t = typeof(TVm);
        return t.IsInterface ? viewModel.GetType() : t;
    }
}

public class DependencyResolveException : Exception
{
    public DependencyResolveException(Exception innerEx)
        : base("Failed to resolve one or more dependencies.", innerEx) { }

    public DependencyResolveException()
        : base("Failed to resolve dependency.") { }
}

public class DoNotRegisterAttribute : Attribute { }

public class SingletonAttribute : Attribute { }
﻿using System.Linq;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using UnityConfiguration.OtherNamespace;

namespace UnityConfiguration
{
    [TestFixture]
    public class Configuration_tester
    {
        [Test]
        public void Can_initalize_container_with_one_registry()
        {
            var container = new UnityContainer();

            container.Initialize(x => x.AddRegistry<FooRegistry>());

            Assert.That(container.Resolve<IFooService>(), Is.InstanceOf<FooService>());
        }

        [Test]
        public void Can_initalize_container_with_two_registries()
        {
            var container = new UnityContainer();

            container.Initialize(x =>
                                     {
                                         x.AddRegistry<FooRegistry>();
                                         x.AddRegistry<BarRegistry>();
                                     });

            Assert.That(container.Resolve<IFooService>(), Is.InstanceOf<FooService>());
            Assert.That(container.Resolve<IBarService>(), Is.InstanceOf<BarService>());
        }

        [Test]
        public void Can_register_type()
        {
            var container = new UnityContainer();

            container.Initialize(x => x.Register<IBarService, BarService>().AsSingleton());

            Assert.That(container.Resolve<IBarService>(), Is.InstanceOf<BarService>());
        }

        [Test]
        public void Can_register_singletons()
        {
            var container = new UnityContainer();

            container.Initialize(x => x.Register<IBarService, BarService>().AsSingleton());

            Assert.That(container.Resolve<IBarService>(), Is.SameAs(container.Resolve<IBarService>()));
        }

        [Test]
        public void Can_register_named_instance()
        {
            var container = new UnityContainer();
            container.Initialize(x => x.Register<IBarService, BarService>().WithName("name"));

            Assert.That(container.Resolve<IBarService>("name"), Is.InstanceOf<BarService>());
        }

        [Test]
        public void Can_register_named_singleton_instance()
        {
            var container = new UnityContainer();
            container.Initialize(x => x.Register<IBarService, BarService>().WithName("name").AsSingleton());

            Assert.That(container.Resolve<IBarService>("name"), Is.SameAs(container.Resolve<IBarService>("name")));
        }

        [Test]
        public void Can_register_using_factory_delegate()
        {
            var container = new UnityContainer();

            var myService = new BarService();
            container.Initialize(x => x.Register<IBarService>(c => myService));

            Assert.That(container.Resolve<IBarService>(), Is.SameAs(myService));
        }

        [Test]
        public void Can_register_named_instance_using_factory_delegate()
        {
            var container = new UnityContainer();

            var myService = new BarService();
            container.Initialize(x => x.Register<IBarService>(c => myService).WithName("name"));

            Assert.That(container.Resolve<IBarService>("name"), Is.SameAs(myService));
        }

        [Test]
        public void Can_scan_using_the_first_interface_convention()
        {
            var container = new UnityContainer();

            container.Initialize(x => x.Scan(scan =>
                     {
                         scan.AssemblyContaining<FooRegistry>();
                         scan.With<FirstInterfaceConvention>();
                     }));

            Assert.That(container.Resolve<IFooService>(), Is.InstanceOf<FooService>());
            Assert.That(container.Resolve<IBarService>(), Is.InstanceOf<BarService>());
        }

        [Test]
        public void Can_scan_using_the_add_all_convention()
        {
            var container = new UnityContainer();

            container.Initialize(x => x.Scan(scan =>
                     {
                         scan.AssemblyContaining<FooRegistry>();
                         scan.With(new AddAllConvention(typeof(IHaveManyImplementations)));
                     }));

            Assert.That(container.ResolveAll<IHaveManyImplementations>().Count(), Is.EqualTo(2));
        }

        [Test]
        public void Can_scan_using_several_conventions()
        {
            var container = new UnityContainer();

            container.Initialize(x => x.Scan(scan =>
                     {
                         scan.AssemblyContaining<FooRegistry>();
                         scan.With<FirstInterfaceConvention>();
                         scan.With(new AddAllConvention(typeof(IHaveManyImplementations)));
                     }));

            Assert.That(container.Resolve<IFooService>(), Is.InstanceOf<FooService>());
            Assert.That(container.Resolve<IBarService>(), Is.InstanceOf<BarService>());
            Assert.That(container.ResolveAll<IHaveManyImplementations>().Count(), Is.EqualTo(2));
        }

        [Test]
        public void Can_configure_concrete_types_as_singletons()
        {
            var container = new UnityContainer();

            container.Initialize(x =>
                {
                    x.Register<IBarService, BarService>();
                    x.MakeSingleton<BarService>();
                });

            Assert.That(container.Resolve<IBarService>(), Is.SameAs(container.Resolve<IBarService>()));
        }

        [Test]
        public void Can_connect_implementations_to_open_generic_types()
        {
            var container = new UnityContainer();

            container.Initialize(x => x.Scan(scan =>
            {
                scan.AssemblyContaining<FooRegistry>();
                scan.With<FirstInterfaceConvention>();
            }));

            Assert.That(container.Resolve<IHandler<Message>>(), Is.InstanceOf<MessageHandler>());
            Assert.That(container.Resolve<IHandler<AnotherMessage>>(), Is.InstanceOf<AnotherMessageHandler>());
        }

        [Test]
        public void Can_connect_implementations_to_open_generic_types_2()
        {
            var container = new UnityContainer();

            container.Initialize(x => x.Scan(scan =>
            {
                scan.AssemblyContaining<FooRegistry>();
                scan.With<FirstInterfaceConvention>();
            }));

            Assert.That(container.Resolve<IMapper<Message, AnotherMessage>>(), Is.InstanceOf<MessageToAnotherMessageMapper>());
        }

        [Test]
        public void Can_exclude_type()
        {
            var container = new UnityContainer();

            container.Initialize(x => x.Scan(scan =>
            {
                scan.AssemblyContaining<FooRegistry>();
                scan.With<FirstInterfaceConvention>();
                scan.ExcludeType<BarService>();
            }));

            Assert.Throws<ResolutionFailedException>(() => container.Resolve<IBarService>());
        }

        [Test]
        public void Can_exclude_type_using_delegate()
        {
            var container = new UnityContainer();

            container.Initialize(x => x.Scan(scan =>
            {
                scan.AssemblyContaining<FooRegistry>();
                scan.With<FirstInterfaceConvention>();
                scan.Exclude(t => t == typeof(BarService));
            }));

            Assert.Throws<ResolutionFailedException>(() => container.Resolve<IBarService>());
        }

        [Test]
        public void Can_exclude_namespoace_containing_type()
        {
            var container = new UnityContainer();

            container.Initialize(x => x.Scan(scan =>
            {
                scan.AssemblyContaining<FooRegistry>();
                scan.With<FirstInterfaceConvention>();
                scan.ExcludeNamespaceContaining<ServiceInOtherNamespace>();
            }));

            Assert.Throws<ResolutionFailedException>(() => container.Resolve<IServiceInOtherNamespace>());
        }

        [Test]
        public void Can_include_namespoace_containing_type()
        {
            var container = new UnityContainer();

            container.Initialize(x => x.Scan(scan =>
            {
                scan.AssemblyContaining<FooRegistry>();
                scan.With<FirstInterfaceConvention>();
                scan.IncludeNamespaceContaining<ServiceInOtherNamespace>();
            }));

            Assert.Throws<ResolutionFailedException>(() => container.Resolve<IFooService>());
            Assert.That(container.Resolve<IServiceInOtherNamespace>(), Is.InstanceOf<ServiceInOtherNamespace>());
        }

        [Test]
        public void Can_include_namespoace()
        {
            var container = new UnityContainer();

            container.Initialize(x => x.Scan(scan =>
            {
                scan.AssemblyContaining<FooRegistry>();
                scan.With<FirstInterfaceConvention>();
                scan.IncludeNamespace("UnityConfiguration.OtherNamespace");
            }));

            Assert.Throws<ResolutionFailedException>(() => container.Resolve<IFooService>());
            Assert.That(container.Resolve<IServiceInOtherNamespace>(), Is.InstanceOf<ServiceInOtherNamespace>());
        }

        [Test]
        public void Can_include_using_delegate()
        {
            var container = new UnityContainer();

            container.Initialize(x => x.Scan(scan =>
            {
                scan.AssemblyContaining<FooRegistry>();
                scan.With<FirstInterfaceConvention>();
                scan.Include(t => t == typeof(ServiceInOtherNamespace));
            }));

            Assert.Throws<ResolutionFailedException>(() => container.Resolve<IFooService>());
            Assert.That(container.Resolve<IServiceInOtherNamespace>(), Is.InstanceOf<ServiceInOtherNamespace>());
        }

        [Test]
        public void Can_configure_ctor_arguments_for_type()
        {
            var container = new UnityContainer();

            container.Initialize(x =>
            {
                x.Register<IServiceWithCtorArgs, ServiceWithCtorArgs>();
                x.Register<IFooService, FooService>();
                x.ConfigureCtorArgsFor<ServiceWithCtorArgs>("some string", typeof(IFooService));
            });

            var serviceWithCtorArgs = container.Resolve<IServiceWithCtorArgs>();
            Assert.That(serviceWithCtorArgs.SomeString, Is.EqualTo("some string"));
            Assert.That(serviceWithCtorArgs.FooService, Is.InstanceOf<FooService>());
        }

        [Test]
        public void Can_select_constructor_to_use()
        {
            var container = new UnityContainer();

            container.Initialize(x =>
            {
                x.Register<IServiceWithCtorArgs, ServiceWithCtorArgs>();
                x.SelectConstructor<ServiceWithCtorArgs>();
            });

            var serviceWithCtorArgs = container.Resolve<IServiceWithCtorArgs>();
            Assert.That(serviceWithCtorArgs.SomeString, Is.Null);
            Assert.That(serviceWithCtorArgs.FooService, Is.Null);
        }

        [Test]
        public void Can_select_constructor_to_use_2()
        {
            var container = new UnityContainer();

            container.Initialize(x =>
            {
                x.Register<IServiceWithCtorArgs, ServiceWithCtorArgs>();
                x.Register<IFooService, FooService>();
                x.SelectConstructor<ServiceWithCtorArgs>(typeof(IFooService));
            });

            var serviceWithCtorArgs = container.Resolve<IServiceWithCtorArgs>();
            Assert.That(serviceWithCtorArgs.SomeString, Is.Null);
            Assert.That(serviceWithCtorArgs.FooService, Is.InstanceOf<FooService>());
        }

        [Test]
        public void Can_make_registered_transient_sevices_a_singleton_in_child_container()
        {
            var container = new UnityContainer();
            container.Initialize(x => x.Register<IFooService, FooService>());

            IUnityContainer childContainer = container.CreateChildContainer();
            childContainer.Initialize(x => x.MakeSingleton<FooService>());
            
            Assert.That(container.Resolve<IFooService>(), Is.Not.SameAs(container.Resolve<IFooService>()));
            Assert.That(container.Resolve<IFooService>(), Is.Not.SameAs(childContainer.Resolve<IFooService>()));
            Assert.That(childContainer.Resolve<IFooService>(), Is.SameAs(childContainer.Resolve<IFooService>()));
        }

        [Test]
        public void Can_configure_to_call_method_on_concrete_after_build_up()
        {
            var container = new UnityContainer();
            container.Initialize(x => x.AfterBuildUp<StartableService1>(s => s.Start()));

            Assert.That(container.Resolve<StartableService1>().StartWasCalled);
        }

        [Test]
        public void Can_configure_to_call_method_on_interface_after_build_up()
        {
            var container = new UnityContainer();
            container.Initialize(x =>
                                     {
                                         x.Register<IStartable, StartableService1>();
                                         x.AfterBuildUp<IStartable>(s => s.Start());
                                     });

            Assert.That(container.Resolve<IStartable>().StartWasCalled);
        }

        [Test]
        public void Can_configure_to_call_method_on_interface_after_build_up_2()
        {
            var container = new UnityContainer();
            container.Initialize(x =>
                                     {
                                         x.Register<IStartable, StartableService1>().WithName("1");
                                         x.Register<IStartable, StartableService2>().WithName("2");
                                         x.AfterBuildUp<IStartable>(s => s.Start());
                                     });

            Assert.That(container.Resolve<IStartable>("1").StartWasCalled);
            Assert.That(container.Resolve<IStartable>("2").StartWasCalled);
        }
    }

    public class FooRegistry : UnityRegistry
    {
        public FooRegistry()
        {
            Register<IFooService, FooService>();
        }
    }

    public class BarRegistry : UnityRegistry
    {
        public BarRegistry()
        {
            Register<IBarService, BarService>();
        }
    }

    public interface IFooService
    {
    }

    public class FooService : IFooService
    {
    }

    public interface IBarService
    {
    }

    public class BarService : IBarService, IFooService
    {
    }

    public interface IHaveManyImplementations
    {

    }

    public class Implementation1 : IHaveManyImplementations
    {
    }

    public class Implementation2 : IHaveManyImplementations
    {
    }

    public interface IHandler<T>
    {
    }

    public class MessageHandler : IHandler<Message>
    {
    }

    public class AnotherMessageHandler : IHandler<AnotherMessage>
    {
    }

    public class AnotherMessage
    {
    }

    public class Message
    {
    }

    public interface IMapper<TFrom, TTo>
    {
    }

    public class MessageToAnotherMessageMapper : IMapper<Message, AnotherMessage>
    {
    }

    public class ServiceWithCtorArgs : IServiceWithCtorArgs
    {
        public string SomeString { get; set; }
        public IFooService FooService { get; set; }

        public ServiceWithCtorArgs()
        {
        }

        public ServiceWithCtorArgs(IFooService fooService)
        {
            FooService = fooService;
        }

        public ServiceWithCtorArgs(string someString, IFooService fooService)
        {
            SomeString = someString;
            FooService = fooService;
        }
    }

    public interface IServiceWithCtorArgs
    {
        string SomeString { get; set; }
        IFooService FooService { get; set; }
    }

    public class StartableService1 : IStartable
    {
        public void Start()
        {
            StartWasCalled = true;
        }

        public bool StartWasCalled { get; set; }
    }

    public class StartableService2 : IStartable
    {
        public void Start()
        {
            StartWasCalled = true;
        }

        public bool StartWasCalled { get; set; }
    }

    public interface IStartable
    {
        void Start();
        bool StartWasCalled { get; set; }
    }
}

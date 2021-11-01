using System;
using NLog;
using Unity;
using Unity.Injection;

namespace Asgard
{
    public sealed class Resolver
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly Lazy<Resolver> singleton = new(() => new Resolver());

        private readonly UnityContainer container = new();

        public static Resolver Instance => singleton.Value;

        private Resolver()
        {
            this.container.RegisterType<IGridConnectProcessor, GridConnectProcessor>("SerialPortAdapter",
                new InjectionConstructor(new ResolvedParameter<ICommsAdapter>("SerialPort")));

            this.container.RegisterType<IGridConnectProcessor, GridConnectProcessor>("SocketClientAdapter",
                new InjectionConstructor(new ResolvedParameter<ICommsAdapter>("SocketClient")));

            this.container.RegisterType<ICommsAdapterFactory, CommsAdapterFactory>(
                new InjectionConstructor(
                    new ResolvedParameter<Func<ICommsAdapter>>("SerialPortAdapter"),
                    new ResolvedParameter<Func<ICommsAdapter>>("SocketClientAdapter")));
        }
    }
}

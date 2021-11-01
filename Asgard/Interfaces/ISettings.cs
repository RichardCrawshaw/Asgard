using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard
{
    /// <summary>
    /// An interface to describe the settings needed to configure the adapters and processors.
    /// </summary>
    public interface ISettings
    {
        int Count => this.SettingsNodes?.Count() ?? 0;

        IEnumerable<ISettingsNode> SettingsNodes { get; }

        //ISettingsNode<T> Get<T>()
        //    where T : class, IDisposable;

        TSettings Get<TClass, TSettings>()
            where TClass : IDisposable
            where TSettings : ISettingsNode<TClass>;

        public interface ISettingsNode
        {
            bool Enabled { get; }

            Type Type { get; }
        }

        public interface ISettingsNode<T> :
            ISettingsNode
            where T : IDisposable
        {
        }
    }

    /// <summary>
    /// An interface to describe the settings for an <see cref="ISocketClientAdapter"/> object.
    /// </summary>
    public interface IClientSettings :
        ISettings.ISettingsNode<ISocketClientAdapter>
    {
        string Address { get; }

        int Port { get; }
    }

    /// <summary>
    /// An interface to describe the settings for an <see cref="ISocketServerAdapter"/> object.
    /// </summary>
    public interface IServerSettings :
        ISettings.ISettingsNode<ISocketServerAdapter>
    {
        string Address { get; }

        int Port { get; }
    }

    /// <summary>
    /// An interface to describe the settings for an <see cref="ISerialPortAdapter"/> object.
    /// </summary>
    public interface ISerialSettings :
        ISettings.ISettingsNode<ISerialPortAdapter>
    {
        int BaudRate { get; }

        int DataBits { get; }

        string Parity { get; }

        string PortName { get; }

        string StopBits { get; }
    }

    /// <summary>
    /// An interface to describe the settings for an <see cref="IGridConnectProcessor"/> object.
    /// </summary>
    public interface IGridConnectSettings :
    ISettings.ISettingsNode<IGridConnectProcessor>
    {

    }

    /// <summary>
    /// An interface to describe the settings for an <see cref="ICbusProcessor"/> object.
    /// </summary>
    public interface ICbusSettings :
        ISettings.ISettingsNode<ICbusProcessor>
    {

    }
}

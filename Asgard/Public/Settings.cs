using System;
using System.Collections.Generic;
using System.Linq;

namespace Asgard
{
    public class Settings :
        ISettings
    {
        #region Fields

        private IList<ISettings.ISettingsNode> settingsNodes = new List<ISettings.ISettingsNode>();

        #endregion

        #region Properties

        public static string NodeName => "Settings";

        public IEnumerable<ISettings.ISettingsNode> SettingsNodes => this.settingsNodes;

        #endregion

        #region Methods

        protected internal void Add(ISettings.ISettingsNode settingsNode) => this.settingsNodes.Add(settingsNode);

        public TSettings Get<TClass, TSettings>()
            where TClass : IDisposable
            where TSettings : ISettings.ISettingsNode<TClass>
        {
            var type = typeof(TClass);
            var settingsType = typeof(TSettings);
            var results =
                this.settingsNodes
                    .Where(n => n.Type == typeof(TClass))
                    .ToList();
            var result = results.FirstOrDefault();
            return (TSettings)result;
        }

        public static ISettings Load(string source, SettingsOptionsEnum options)
        {
            var helper = new SettingsHelper();
            if (helper.Load(source, options))
                return helper.Settings;
            return null;
        }

        #endregion

        public abstract class SettingsNode :
            ISettings.ISettingsNode
        {
            [SettingsNodeProperty(Name = "Enabled", Path = "Enabled")]
            public bool Enabled { get; set; }

            public abstract Type Type { get; }
        }

        public abstract class SettingsNode<T> : SettingsNode,
            ISettings.ISettingsNode<T>
            where T : class, IDisposable
        {
            public override Type Type => typeof(T);
        }
    }

    public class ClientSettings : Settings.SettingsNode<AsyncSocketClient>,
        IClientSettings
    {
        public static string NodeName => "Client";

        [SettingsNodeProperty(Name = "Address", Path = "Address")]
        public string Address { get; set; }

        [SettingsNodeProperty(Name = "Port", Path = "Port")]
        public int Port { get; set; }
    }

    public class ServerSettings : Settings.SettingsNode<AsyncSocketServer>,
        IServerSettings
    {
        public static string NodeName => "Server";

        [SettingsNodeProperty(Name = "Address", Path = "Address")]
        public string Address { get; set; }

        [SettingsNodeProperty(Name = "Port", Path = "Port")]
        public int Port { get; set; }
    }

    public class SerialSettings : Settings.SettingsNode<SerialPortAdapter>,
        ISerialSettings
    {
        public static string NodeName => "Serial";

        [SettingsNodeProperty(Name = "BaudRate", Path = "BaudRate")]
        public int BaudRate { get; set; }

        [SettingsNodeProperty(Name = "DataBits", Path = "DataBits")]
        public int DataBits { get; set; }

        [SettingsNodeProperty(Name = "Parity", Path = "Parity")]
        public string Parity { get; set; }

        [SettingsNodeProperty(Name = "PortName", Path = "PortName")]
        public string PortName { get;  set; }

        [SettingsNodeProperty(Name = "StopBits", Path = "StopBits")]
        public string StopBits { get; set; }
    }

    public class GridConnectSettings : Settings.SettingsNode<GridConnectProcessor>,
        IGridConnectSettings
    {
        public static string NodeName => "GridConnect";


    }

    public class CbusSettings : Settings.SettingsNode<CbusProcessor>,
        ICbusSettings
    {
        public static string NodeName => "CBUS";


    }
}

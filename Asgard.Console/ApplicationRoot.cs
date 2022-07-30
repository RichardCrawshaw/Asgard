using System.Runtime.Versioning;
using Asgard.Communications;
using Terminal.Gui;

namespace Asgard.Console
{
    [SupportedOSPlatform("Linux")]
    [SupportedOSPlatform("macOS")]
    [SupportedOSPlatform("windows")]
    internal class ApplicationRoot
    {
        private readonly ICbusMessenger cbusMessenger;

        public ApplicationRoot(ICbusMessenger cbusMessenger)
        {
            this.cbusMessenger = cbusMessenger;
        }
        public MenuBar CreateMenu()
        {
            var menu = new MenuBar(new MenuBarItem[] {
                new MenuBarItem("CBUS", new MenuItem[] {
                    new MenuItem("Connect", "", ShowConnectionOptions),
                    new MenuItem("Query Nodes","", QueryNodes),
                    new MenuItem("Drive Loco", "", DriveLoco),
                    new MenuItem("Loco Service Mode", "", ServiceMode)
                })
            });
            return menu;       
        }

        private void ShowConnectionOptions()
        {
            var connectionOptions = new ConnectionOptions();
            connectionOptions.Initialise();
            Application.Run(connectionOptions);
            var port = connectionOptions.SelectedPort;
            if (!string.IsNullOrWhiteSpace(port))
            {
                cbusMessenger.OpenAsync(new Communications.ConnectionOptions
                {
                    ConnectionType = Communications.ConnectionOptions.ConnectionTypes.SerialPort,
                    SerialPort = new SerialPortTransportSettings { PortName = port }
                });
            }
        }
        private Window? activeWindow = null;
        private QueryNodes? queryNodes = null;
        private void QueryNodes()
        {
            if (queryNodes == null)
            {
                queryNodes = new QueryNodes(cbusMessenger);
            }
            SetActiveWindow(queryNodes);
        }

        private DriveLoco? driveLoco = null;
        private void DriveLoco()
        {
            if (driveLoco == null)
            {
                driveLoco = new DriveLoco(cbusMessenger);
            }
            SetActiveWindow(driveLoco);
        }

        private ServiceMode? serviceMode = null;
        private void ServiceMode()
        {
            if (serviceMode == null)
            {
                serviceMode = new ServiceMode(cbusMessenger);
            }
            SetActiveWindow(serviceMode);
        }
        private void SetActiveWindow(Window window)
        {
            if (activeWindow != null)
            {
                Application.Top.Remove(activeWindow);
            }
            activeWindow = window;
            activeWindow.X = 0;
            activeWindow.Y = 1;
            activeWindow.Width = Dim.Fill();
            activeWindow.Height = Dim.Height(Application.Top) - 3;
            Application.Top.Add(activeWindow);
        }

        public void Start()
        {
            Application.Init();
            var top = Application.Top;

            top.Add(CreateMenu());

            var history = new MessageHistory(cbusMessenger)
            {
                X = 0,
                Y = Pos.Bottom(top) - 2,
                Width = Dim.Fill(),
                Height = 2
            };

            top.Add(history);





            Application.Run();
        }
    }
}

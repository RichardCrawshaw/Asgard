namespace TestSocketListenerConsole
{
    class Settings : Asgard.Settings
    {
        public Settings()
        {
            Add(new ServerSettings());
        }

        class ServerSettings : Asgard.ServerSettings
        {
            public ServerSettings() : base()
            {
                this.Enabled = true;

                this.Address = ".";
                this.Port = 5550;
            }
        }
    }
}

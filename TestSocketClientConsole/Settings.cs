namespace TestSocketClientConsole
{
    class Settings : Asgard.Settings
    {
        public Settings()
        {
            Add(new ClientSettings());
        }

        class ClientSettings : Asgard.ClientSettings
        {
            public ClientSettings() : base()
            {
                this.Enabled = true;

                this.Address = ".";
                this.Port = 5550;
            }
        }
    }
}

using Asgard.Communications;
using System.Linq;
using Asgard.Data;
using Terminal.Gui;

namespace Asgard.Console
{
    internal class QueryNodes : Window
    {
        private readonly ICbusMessenger cbusMessenger;

        private ListView nodeList;
        public QueryNodes(ICbusMessenger cbusMessenger)
        {
            this.Title = "Nodes";
            var query = new Button("Query")
            {
                X = 0,
                Y = 0
            };
            query.Clicked += SendQuery;

            nodeList = new ListView()
            {
                X = 0,
                Y = 1,
                Width = 15,
                Height = Dim.Fill(),
                ColorScheme = Colors.TopLevel
            };

            this.Add(query);
            this.Add(nodeList);
            this.cbusMessenger = cbusMessenger;
        }

        private async void SendQuery()
        {
            var mm = new MessageManager(cbusMessenger);
            try
            {
                var replies = await mm.SendMessageWaitForReplies(new QueryNodeNumber());
                Application.MainLoop.Invoke(() =>
                {
                    nodeList.SetSource(replies.Select(r => $"Module ID: {(r as ResponseToQueryNode).ModuleId}").ToList());
                });
                
                
            }catch(Exception ex)
            {
                //TODO: handle errors, but don't let them leave async void
            }
        } 
    }
}
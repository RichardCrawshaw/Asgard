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
        private ListView paranList;
        private List<ResponseToQueryNode> nodes;
        public QueryNodes(ICbusMessenger cbusMessenger)
        {
            this.Title = "Nodes";
            var query = new Button("Refresh Node List")
            {
                X = 0,
                Y = Pos.Bottom(this) - 4 //TODO: figure out why we need -4 here, why isn't it offset just from this window (ie -1)
            };
            query.Clicked += SendQuery;
            
            nodeList = new ListView()
            {
                X = 0,
                Y = 0,
                Width = 21,
                Height = Dim.Fill() - 1,
                ColorScheme = Colors.TopLevel
            };
            nodeList.SelectedItemChanged += GetNodeInfo;

            paranList = new ListView()
            {
                X = 23,
                Y = 0,
                Width = 5,
                Height = 10,
                ColorScheme = Colors.TopLevel,
                

            };

            this.Add(nodeList);
            this.Add(query);
            this.Add(paranList);

            var sbv = new ScrollBarView(paranList, true);
            sbv.ChangedPosition += () =>
            {
                paranList.TopItem = sbv.Position;
                if (paranList.TopItem != sbv.Position)
                {
                    sbv.Position = paranList.TopItem;
                }
                paranList.SetNeedsDisplay();
            };

            paranList.DrawContent += (e) =>
            {
                if (paranList.Source == null)
                    return;
                sbv.Size = paranList.Source.Count - 1;
                sbv.Position = paranList.TopItem;
                sbv.Refresh();
            };

            this.cbusMessenger = cbusMessenger;
        }

        private async void GetNodeInfo(ListViewItemEventArgs obj)
        {
            if (nodes == null || nodes.Count == 0)
                return;

            var node = nodes[nodeList.SelectedItem];
            var mm = new MessageManager(this.cbusMessenger);
            var parans = new List<byte>();
            var r = await mm.SendMessageWaitForReply(new RequestReadOfANodeParameterByIndex()
            {
                NodeNumber = node.NodeNumber,
                ParamIndex = 0
                
            });
            
            if (r is ResponseToRequestForIndividualNodeParameter paran)
            {
                parans.Add(paran.Value);
                for (byte x = 1; x < paran.Value; x++)
                {
                    var p = await mm.SendMessageWaitForReply(new RequestReadOfANodeParameterByIndex()
                    {
                        NodeNumber = node.NodeNumber,
                        ParamIndex = x
                    });
                    if (p is ResponseToRequestForIndividualNodeParameter paran2)
                    {
                        parans.Add(paran2.Value);
                    }
                }
            }

            paranList.SetSource(parans);
        }

        private async void SendQuery()
        {
            var mm = new MessageManager(cbusMessenger);
            nodes = (await mm.SendMessageWaitForReplies(new QueryNodeNumber())).Select(r => (ResponseToQueryNode)r).ToList();
                
            Application.MainLoop.Invoke(() =>
            {
                nodeList.SetSource(nodes.Select(r => $"Module ID: {r.ModuleId}").ToList());
            });
        } 
    }
}
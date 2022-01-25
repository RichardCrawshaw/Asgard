using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Asgard.Console
{
    internal static class Extensions
    {
        public static void AddScrollbar(this ListView list)
        {
            var sbv = new ScrollBarView(list, true);
            sbv.ChangedPosition += () =>
            {
                list.TopItem = sbv.Position;
                if (list.TopItem != sbv.Position)
                {
                    sbv.Position = list.TopItem;
                }
                list.SetNeedsDisplay();
            };

            list.DrawContent += (e) =>
            {
                if (list.Source == null)
                    return;
                sbv.Size = list.Source.Count - 1;
                sbv.Position = list.TopItem;
                sbv.Refresh();
            };
        }
    }
}

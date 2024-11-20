using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedLaunch
{
    public interface Plugin
    {
        void InitAction(Log log, string dllPath);
        void CloseAction();

        ListItem FilterAction(string search);

        void AddItem(List<ListItem> items, string search);
        bool doItem(ListItem item, string search);


    }
}

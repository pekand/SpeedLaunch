using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpeedLaunch
{
    public class DiagramPlugin: Plugin
    {
        Log log = null;

        public void InitAction(Log log, string dllPath){
            this.log = log;
            log.write("Init DiagramPlugin");
        }

        public void CloseAction()
        {

        }

        public ListItem FilterAction(string search)
        {
            return null;
        }

        public void AddItem(List<ListItem> items, string search)
        {
            if (items.Count < 5)
            {
                ListItem item = new ListItem();
                item.text = "Create diagram";
                item.description = "";
                item.path = "";
                item.action = "CREATE_DIAGRAM";
                items.Add(item);
            }
        }

        public void doItem(ListItem item, string search)
        {
            if (item.action == "CREATE_DIAGRAM")
            {
                string diagramName = search;
                Diagram.CreateDiagram(diagramName);
                return;
            }
        }
    }
}

using NCalc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpeedLaunch
{
    public class CalcPlugin : Plugin
    {
        Log log = null;

        public void InitAction(Log log, string dllPath)
        {
            this.log = log;
            log.write("Init Calc");
        }

        public void CloseAction() { 
        }

        public ListItem FilterAction(string search)
        {
            Regex calcMatchExpression = new Regex(@"^=(.*)$", RegexOptions.IgnoreCase);

            Match matchExpression = calcMatchExpression.Match(search);

            if (matchExpression.Success)
            { // check if command start with = -> calc comand            

                string result = "";
                string exp = matchExpression.Groups[1].Captures[0].Value;
                try
                {
                    if (exp.Trim() != "")
                    {
                        Expression e = new Expression(exp);
                        result = e.Evaluate().ToString();
                    }
                }
                catch (Exception ex)
                {
                    result = ex.Message;
                }

                ListItem item = new ListItem();
                item.text = result;
                item.description = "ncalc";
                item.index = null;

                return item;
            }

            return null;
        }

        public void AddItem(List<ListItem> items, string search)
        { 
        }

        public void doItem(ListItem item, string search) { 
        }
    }
}


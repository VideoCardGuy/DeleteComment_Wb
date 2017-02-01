using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeleteWb
{
    public class EnvFunction
    {
        public static void Exit(string strText)
        {
            MessageBox.Show(strText);
            System.Environment.Exit(0);
        }
    }
}

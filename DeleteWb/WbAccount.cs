using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeleteWb
{
    public class WbAccount
    {
        public string strName { get; set; }

        public string strComment { get; set; }

        public string GetText()
        {
            return String.Format("Remove [{0}] Comment:{1}", strName, strComment);
        }
    }
}

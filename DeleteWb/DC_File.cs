using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeleteWb
{
    public class DC_File
    {
        public static List<string> VecDisharmonyWord = new List<string>();

        public static int nConfigActionFlag = 0;

        public static string strRoolupScriptContent = string.Empty;
        public static void SetDisharmonyWord()
        {
            lock (VecDisharmonyWord)
            {
                VecDisharmonyWord.Clear();
                using (StreamReader Sr = new StreamReader(System.Environment.CurrentDirectory + @"\DisharmonyWord.txt", Encoding.Unicode))
                {
                    string strText = string.Empty;
                    while ((strText = Sr.ReadLine()) != null)
                        VecDisharmonyWord.Add(strText);

                    Sr.Close();
                }
            }
        }

        public static void AddDisharmonyRecord(string strText)
        {
            using (StreamWriter Sw = new StreamWriter(System.Environment.CurrentDirectory + @"\Record.txt", true, Encoding.Unicode))
            {
                Sw.WriteLine(strText);
                Sw.Close();
            }
        }
    }
}

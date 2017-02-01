using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DeleteWb
{
    enum em_Comment_Action
    {
        em_Comment_Action_Fuck  = 0x2,
        em_Comment_Action_Save  = 0x4,
        em_Comment_Action_Show  = 0x8,
        em_Comment_Action_Confirm = 0x10,
    }
    public class API
    {
        [DllImport("kernel32")]
        public static extern long WritePrivateProfileString(string section, string key, string val, string filepath);

        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public static int GetCommentAction()
        {
            string strFilePath = System.Environment.CurrentDirectory + @"\Config.ini";
            if (!File.Exists(strFilePath))
                EnvFunction.Exit("UnExist 'Config.ini' in " + strFilePath);

            StringBuilder sbText = new StringBuilder(1024);
            GetPrivateProfileString("Config", "CommentAction", "0", sbText, 1024, System.Environment.CurrentDirectory + @"\Config.ini");
            if (sbText.ToString() == "0")
                EnvFunction.Exit("UnExist Section 'CommentAction' in " + strFilePath);

            int nFlag = 0;
            if (sbText.ToString().ToLower().IndexOf("fuck") != -1)
                nFlag |= (int)em_Comment_Action.em_Comment_Action_Fuck;
            if (sbText.ToString().ToLower().IndexOf("save") != -1)
                nFlag |= (int)em_Comment_Action.em_Comment_Action_Save;
            if (sbText.ToString().ToLower().IndexOf("show") != -1)
                nFlag |= (int)em_Comment_Action.em_Comment_Action_Show;
            if (sbText.ToString().ToLower().IndexOf("confirm") != -1)
                nFlag |= (int)em_Comment_Action.em_Comment_Action_Confirm;
            return nFlag;
        }
    }
}

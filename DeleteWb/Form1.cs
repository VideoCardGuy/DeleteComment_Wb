using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeleteWb
{
    
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            InitizationScriptContent();
            DC_File.SetDisharmonyWord();
            CWebBrowserResult.ShowLog(txtLog,"Init DisharmonyWord Done!");

            DC_File.nConfigActionFlag = API.GetCommentAction();
            CWebBrowserResult.ShowLog(txtLog, "CommentAction=" + DC_File.nConfigActionFlag.ToString("X"));

            webBrowser1.ScriptErrorsSuppressed = true;
            HarmoniouWeibo();
            
        }


        private void InitizationScriptContent()
        {
            DC_File.strRoolupScriptContent = "function RollupToButtom(){ var h = document.documentElement.scrollHeight - 1000;window.scrollTo(h,h); }";
        }

        private async void HarmoniouWeibo()
        {
            var WebBrowserResult = new CWebBrowserResult();

            // Load MainForm
            CWebBrowserResult.ShowLog(txtLog, "Loading MainPage……");
            await WebBrowserResult.Load(webBrowser1, "http://weibo.com/p/1005055986182091/home?profile_ftype=1&is_ori=1#_0");
            // WaitForLogin
            CWebBrowserResult.ShowLog(txtLog, "Wait to Login……");
            await WebBrowserResult.WaitForLogin(webBrowser1);
            await Task.Run(() => Thread.Sleep(5 * 1000));

            webBrowser1.Visible = false;
            txtLog.Visible = true;

            int nIndex = -1;
            while (true)
            {
                // Load MainForm
                CWebBrowserResult.ShowLog(txtLog, "Loading MainPage……");
                await WebBrowserResult.Load(webBrowser1, "http://weibo.com/p/1005055986182091/home?profile_ftype=1&is_ori=1#_0");
                // Roll up to Buttom
                WebBrowserResult.AddJsContent(webBrowser1, DC_File.strRoolupScriptContent);

                CWebBrowserResult.ShowLog(txtLog, "Loading More Page");
                await WebBrowserResult.RollupToButtom(webBrowser1, 3);

                int nCount = WebBrowserResult.GetWeiBoCountByPage(webBrowser1, 0);
                CWebBrowserResult.ShowLog(txtLog, "Weibo Count for single page = " + nCount.ToString());

                // Next Weibo
                nIndex = nIndex >= nCount ? 0 : nIndex + 1;
                CWebBrowserResult.ShowLog(txtLog, "Checking index for weibo=" + nIndex.ToString() + ",and Show it Comment.");
                if (!WebBrowserResult.ShowMoreComment(webBrowser1, nIndex))
                    continue;

                await Task.Run(() => Thread.Sleep(8 * 1000));
                string strLink = WebBrowserResult.GetMoreCommentLink(webBrowser1, 0);
                CWebBrowserResult.ShowLog(txtLog, "More Comment Link =" + strLink);
                if (strLink != string.Empty)
                {
                    // reload in new link
                    await WebBrowserResult.CheckHarmoniousWord(webBrowser1, strLink, (x) => 
                    { 
                        txtLog.Text += x + "\r\n"; 
                    });
                }
                else
                {
                    // check current page
                    //MessageBox.Show("UnExist More Comment Link!");
                }
            }
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            switch ((em_WebBowr_Event)webBrowser1.Tag)
            {
                case em_WebBowr_Event.em_WebBowr_Event_ClickMoreComment:
                    webBrowser1.Tag = em_WebBowr_Event.em_WebBowr_Event_Done;
                    break;
                case em_WebBowr_Event.em_WebBowr_Event_LoadMain:
                    webBrowser1.Tag = em_WebBowr_Event.em_WebBowr_Event_Done;
                    break;
                case em_WebBowr_Event.em_WebBowr_Event_None:
                    break;
            }
        }
    }
}

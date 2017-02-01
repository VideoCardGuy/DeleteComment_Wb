using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeleteWb
{
    public enum em_WebBowr_Event
    {
        // Load www.weibo.com mainform
        em_WebBowr_Event_LoadMain,
        // Click anyone MoreComment to Get More Comment
        em_WebBowr_Event_ClickMoreComment,
        // None
        em_WebBowr_Event_None,
        // Done
        em_WebBowr_Event_Done
    }

    public class CWebBrowserResult
    {
        private async Task WaitForCompleted(Func<bool> TaskPtr, int nEverySleepTime, int nTimeout)
        {
            DateTime NowTick = DateTime.Now;
            while ( (nTimeout == -1 || (int)(DateTime.Now - NowTick).TotalSeconds < nTimeout) && !TaskPtr())
            {
                await Task.Run(() => Thread.Sleep(nEverySleepTime));
            }
            await Task.Run(() => Thread.Sleep(3 * 1000));
        }

        /// <summary>
        /// Navigate URL
        /// </summary>
        /// <param name="WebBrowser_"></param>
        /// <param name="strURL"></param>
        /// <returns></returns>
        public async Task Load(WebBrowser WebBrowser_, string strURL)
        {
            WebBrowser_.Tag = em_WebBowr_Event.em_WebBowr_Event_LoadMain;
            WebBrowser_.Navigate(strURL);
            await WaitForCompleted(() => { return (em_WebBowr_Event)WebBrowser_.Tag == em_WebBowr_Event.em_WebBowr_Event_Done; }, 1000, 30 * 1000);
        }

        public List<HtmlElement> getElementsByClassName(WebBrowser WebBrowser_, string strTagName, string strClassName)
        {
            var Lst = new List<string>();
            var Vec = new List<HtmlElement>();
            var TagVec = WebBrowser_.Document.GetElementsByTagName(strTagName);
            for (int i = 0; i < TagVec.Count; i++)
            {
                if (TagVec[i].GetAttribute("className").Trim() == strClassName)
                    Vec.Add(TagVec[i]);
                Lst.Add(TagVec[i].GetAttribute("className"));
            }

            return Vec;
        }

        private bool IsLogin(WebBrowser WebBrowser_)
        {
            return getElementsByClassName(WebBrowser_, "ul", "gn_login_list").Count == 0;
        }

        public async Task WaitForLogin(WebBrowser WebBrowser_)
        {
            await WaitForCompleted(() => { return IsLogin(WebBrowser_); }, 3000, -1);
        }

        public void AddJsContent(WebBrowser WebBrowser_, string strScriptContent)
        {
            HtmlElement HtmlElement_ = WebBrowser_.Document.CreateElement("script");
            HtmlElement_.SetAttribute("type", "text/javascript");
            HtmlElement_.SetAttribute("text", strScriptContent);
            WebBrowser_.Document.Body.AppendChild(HtmlElement_);
        }

        public void RunJsByName(WebBrowser WebBrowser_, string strScriptName)
        {
            WebBrowser_.Document.InvokeScript(strScriptName);
        }

        public bool ShowMoreComment(WebBrowser WebBrowser_, int nIndex)
        {
            var Vec = getElementsByClassName(WebBrowser_, "em", "W_ficon ficon_repeat S_ficon");
            if (Vec.Count <= nIndex)
                return false;

            Vec[nIndex].InvokeMember("click");
            return true;
        }

        public int GetWeiBoCountByPage(WebBrowser WebBrowser_, int nPage)
        {
            // Only Current Page
            return getElementsByClassName(WebBrowser_, "div", "WB_cardwrap WB_feed_type S_bg2").Count;
        }

        public string GetMoreCommentLink(WebBrowser WebBrowser_, int nIndex)
        {
            var Vec = getElementsByClassName(WebBrowser_, "a", "WB_cardmore S_txt1 S_line1 clearfix");
            if (Vec.Count <= nIndex + 1)
                return string.Empty;

            return Vec[nIndex + 1].GetAttribute("href");
        }

        private async Task<bool> ClickDeleteCommentByIndex(WebBrowser WebBrowser_, int nIndex)
        {
            var VecHover = getElementsByClassName(WebBrowser_, "li", "hover");
            if (VecHover.Count <= nIndex)
                return false;

            HtmlElement Element_ = VecHover[nIndex];
            if (Element_.Children.Count == 0)
                return false;

            Element_ = Element_.Children[0];
            if (Element_.Children.Count == 0)
                return false;

            // click delete
            Element_.Children[0].InvokeMember("click");
            await Task.Run(() => Thread.Sleep(2 * 1000));
            return true;
        }

        private async Task<bool> ConfirmDeleteComment(WebBrowser WebBrowser_)
        {
            // document.getElementsByClassName("W_layer W_layer_pop")[1].children[0].children[2].children[0].click()
            var Vec = getElementsByClassName(WebBrowser_, "div", "W_layer W_layer_pop");
            if (Vec.Count == 0)
                return false;

            var VecChild = Vec[0].Children;
            if (VecChild.Count == 0)
                return false;

            HtmlElement Element_ = VecChild[0];;
            VecChild = Element_.Children;
            if (VecChild.Count <= 2)
                return false;

            Element_ = VecChild[2];
            VecChild = Element_.Children;
            if (VecChild.Count == 0)
                return false;

            Element_ = VecChild[0];
            Element_.InvokeMember("click");
            await Task.Run(() => Thread.Sleep(3 * 1000));
            return true;
        }

        public async Task RollupToButtom(WebBrowser WebBrowser_, int nCount)
        {
            for (int i = 0; i < nCount; i++)
            {
                RunJsByName(WebBrowser_, "RollupToButtom");
                await Task.Run(() => Thread.Sleep(3 * 1000));
            }
        }

        private bool ExistMoreCommentLink_NewPage(WebBrowser WebBrowser_)
        {
            return getElementsByClassName(WebBrowser_, "a", "WB_cardmore S_txt1 S_line1 clearfix").Count != 0;
        }

        private async void ClickMoreCommentLink_NewPage(WebBrowser WebBrowser_)
        {
            var Vec = getElementsByClassName(WebBrowser_, "a", "WB_cardmore S_txt1 S_line1 clearfix");
            Vec[0].InvokeMember("click");
            await Task.Run(() => Thread.Sleep(5 * 1000));
        }

        public async Task CheckHarmoniousWord(WebBrowser WebBrowser_, string strLink, Action<string> fnShowPtr)
        {
            Action<string> ShowLogPtr = (x) =>
            {
                if ((DC_File.nConfigActionFlag & (int)em_Comment_Action.em_Comment_Action_Show) != 0)
                {
                    fnShowPtr(x);
                }
            };

            bool bExistUnHarmoniousWord = false;
            while (!bExistUnHarmoniousWord)
            {
                ShowLogPtr("Loading Page:" + strLink);
                await Load(WebBrowser_, strLink);
                AddJsContent(WebBrowser_, DC_File.strRoolupScriptContent);

                ShowLogPtr("Rolling down to Bottom");
                await RollupToButtom(WebBrowser_, 5);
                // click more comment document.getElementsByClassName("WB_cardmore S_txt1 S_line1 clearfix")[0].click()
                while (ExistMoreCommentLink_NewPage(WebBrowser_))
                {
                    ShowLogPtr("Show More Comment!");
                    ClickMoreCommentLink_NewPage(WebBrowser_);

                    ShowLogPtr("Rolling down to Bottom");
                    await RollupToButtom(WebBrowser_, 2);
                }

                bExistUnHarmoniousWord = true;
                var Vec = getElementsByClassName(WebBrowser_, "div", "WB_text");
                for (int i = 0; i < Vec.Count; i++)
                {
                    string strText = Vec[i].InnerText;
                    int nContentIndex = strText.IndexOf("：");
                    if(nContentIndex == -1)
                        continue;

                    strText = strText.Substring(nContentIndex + 1);
                    string strResult = DC_File.VecDisharmonyWord.Find((x) => { return strText.IndexOf(x) != -1; });
                    if (strResult == string.Empty || strResult == null)
                        continue;

                    string strWord = new WbAccount() { strName = Vec[i].InnerText.Substring(0, nContentIndex), strComment = strText }.GetText();
                    ShowLogPtr("Exist Bad Word:" + strWord);
                    if ((DC_File.nConfigActionFlag & (int)em_Comment_Action.em_Comment_Action_Confirm) != 0 && MessageBox.Show("Exist UnHarmoniousWord:" + strWord, "Warning", MessageBoxButtons.YesNo) == DialogResult.No)
                        continue;

                    // delete comment
                    if ((DC_File.nConfigActionFlag & (int)em_Comment_Action.em_Comment_Action_Fuck) == 0)
                        continue;

                    // document.getElementsByClassName("hover")[i].children[0].children[0].click()
                     ShowLogPtr("Ready Remove Bad Word:" + strWord);
                    if (!await ClickDeleteCommentByIndex(WebBrowser_, i))
                        break;
                    // confirm delete
                    ShowLogPtr("Confirm Remove Bad Word:" + strWord);
                    if (!await ConfirmDeleteComment(WebBrowser_))
                        break;

                    // contiune loop!
                    bExistUnHarmoniousWord = false;

                    // record comment
                    if ((DC_File.nConfigActionFlag & (int)em_Comment_Action.em_Comment_Action_Save) != 0)
                    {
                        ShowLogPtr("Save Record!");
                        DC_File.AddDisharmonyRecord(strWord);
                    }
                   
                    Vec = getElementsByClassName(WebBrowser_, "div", "WB_text");
                    i = -1;
                }
            }
        }

        public int GetWeiboLength(WebBrowser WebBrowser_)
        {
            // document.getElementsByClassName("WB_cardwrap WB_feed_type S_bg2")
            return getElementsByClassName(WebBrowser_, "div", "WB_cardwrap WB_feed_type S_bg2").Count;
        }

        public static void ShowLog(TextBox txtLog, string strText)
        {
            if ((DC_File.nConfigActionFlag & (int)em_Comment_Action.em_Comment_Action_Show) != 0)
                txtLog.Text += strText + "\r\n";
        }
    }
}

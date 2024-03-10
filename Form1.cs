using CefSharp;
using CefSharp.DevTools.Emulation;
using CefSharp.DevTools.IO;
using CefSharp.Handler;
using CefSharp.WinForms;
using CefSharp.WinForms.Handler;
using CefSharp.WinForms.Internals;
using FMUtils.KeyboardHook;
using Focus_Browser.Properties;
using HtmlAgilityPack;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using VersOne.Epub;

namespace Focus_Browser
{
    public partial class Form1 : Form
    {
        public static Form1 Instance;
        public Form1()
        {

            Instance = this;
     
            InitializeComponent();
           
        }

        public List<String> blocked=new List<String>();
        private String _text;
        public String text
        {
            set
            {
                String tmp = "";
                Regex regex = new Regex("\\<[p|P](.*)\\<\\/[p|P]\\>");
                var v = regex.Match(value);
                if(v.Success)
                {
                    tmp += v.Value+"\r\n\r\n";

                    v = v.NextMatch();
                    while (v.Success)
                    {
                        String s = v.Value;
                        tmp += s + "\r\n\r\n";
                        v = v.NextMatch();
                    }
                }
                else
                {
                    tmp = value;
                }
                

                
                



                tmp = tmp.Replace("<p>", "").Replace("<P>", "").Replace("</p>", "").Replace("</P>", "").Replace("</a>","").Replace("</A>","");

                tmp=HtmlUtilities.ConvertToPlainText(tmp);

                if (_text != tmp)
                {
                    _text = tmp;
                    richTextBox1.Invoke(new Action(() => { richTextBox1.Text = _text; }));
                    offset = 0;
                    int start = 0;
                    int end = 0;
                    if (tmp != "")
                    {
                        richTextBox1.Invoke(new Action(() =>
                        {
                            richTextBox1.DeselectAll();
                            while (offset < richTextBox1.Text.Length && (richTextBox1.Text[offset] == ' ' || richTextBox1.Text[offset] == '\r' || richTextBox1.Text[offset] == '\n'))
                                offset++;

                            //richTextBox1.SelectionStart = offset;
                            start=offset;
                            int i = 0;
                            do
                            {
                                if (sentenceToolStripMenuItem.CheckState == CheckState.Checked)
                                {
                                    while (offset + i + 2 < richTextBox1.Text.Length && richTextBox1.Text[offset + i] != '\n' && !(richTextBox1.Text[offset + i] == '.' && (richTextBox1.Text[offset + i + 1] == ' ' || richTextBox1.Text[offset + i + 1] == '\r' || richTextBox1.Text[offset + i + 1] == '\n')) && !(richTextBox1.Text[offset + i] == ':' && richTextBox1.Text[offset + i + 1] == ' '))
                                        i++;
                                }
                                else
                                {
                                    while (offset + i + 2 < richTextBox1.Text.Length && richTextBox1.Text[offset + i] != '\n')
                                        i++;
                                }

                                richTextBox1.SelectionLength = i + 1;

                                offset += i + 1;
                                end=offset;
                            } while (richTextBox1.SelectionLength < 5 && offset < richTextBox1.TextLength);

                            if (highlightWholeParagraphToolStripMenuItem1.CheckState == CheckState.Checked)
                            {
                                richTextBox1.SelectionStart = getParagraphAroundOffset(end-2, richTextBox1.Text)[0];
                                richTextBox1.SelectionLength = getParagraphAroundOffset(end-2, richTextBox1.Text)[1] - richTextBox1.SelectionStart;
                                if (invertColorsToolStripMenuItem1.CheckState != CheckState.Checked)
                                {
                                    richTextBox1.SelectionBackColor = Color.LightYellow;
                                }
                                else
                                {
                                    richTextBox1.SelectionBackColor = Color.Honeydew;

                                }

                            }


                            richTextBox1.ScrollToCaret();

                            richTextBox1.SelectionStart = start;
                            richTextBox1.SelectionLength = end - start;

                            if (highlightWholeParagraphToolStripMenuItem1.CheckState == CheckState.Checked)
                            {
                                if (invertColorsToolStripMenuItem1.CheckState != CheckState.Checked)
                                {
                                    richTextBox1.SelectionBackColor = Color.LightGreen;

                                }
                                else
                                {
                                    richTextBox1.SelectionBackColor = Color.LightYellow;
                                }
                            }
                            else
                            {
                                richTextBox1.SelectionBackColor = Color.LightGreen;
                            }

                            richTextBox1.Select(0, 0);
                        }
                ));

                    }
                }
            }
        }

        int[] getParagraphAroundOffset( int offset, string wholeText)
        {
            offset++;
            if (offset > wholeText.Length-1 || offset<0)
                offset = 0;
            int start = offset;
            int end = offset;
            for (int i = offset;i>0;i--)
            {
                if (wholeText[i] != '\n')
                    start--;
                else
                    break;
            }

            for (int i = offset; i < wholeText.Length; i++)
            {
                if (wholeText[i] != '\n')
                    end++;
                else
                    break;
            }
            return new int[] { start, end };
        }

        int offset = 0;
        int length = 100;
        private void toolStripTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar==13)
            {
                chromiumWebBrowser1.Load(toolStripTextBox1.Text);
                chromiumWebBrowser1.Focus();
            }

        }

        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {
          
        }

        private void chromiumWebBrowser1_LoadingStateChanged(object sender, CefSharp.LoadingStateChangedEventArgs e)
        {

        }

        async void chromiumWebBrowser1_LoadingStateChanged_1(object sender, CefSharp.LoadingStateChangedEventArgs e)
        {

            if ((DateTime.Now - lastUpdate).TotalMilliseconds > 400)
                new System.Threading.Thread(updateThread).Start();

         if(e.IsLoading==false)
            {
                if (!e.IsLoading) // browser.CanExecuteJavascriptInMainFrame == TRUE !
                {
                    JavascriptResponse response =
                        await chromiumWebBrowser1.EvaluateScriptAsync(
                            // GET HEIGHT OF CONTENT
                            "(function() {                       " +
                            "    document.width="+this.Width+"; " +
                            "    window.width=" + this.Width + "; " +
                            "}                                   " +
                            ")();");


                    var currentDPI = Int32.Parse((string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ThemeManager", "LastLoadedDPI", "96"));
                    var scale = 96 / (float)currentDPI;
                    chromiumWebBrowser1.SetZoomLevel(-(1.0f + scale));

                    using (var client = chromiumWebBrowser1.GetDevToolsClient())
                    {

                        _ = client.Network.SetUserAgentOverrideAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36 - Testing 123");

                    }
                    String txt = await chromiumWebBrowser1.GetSourceAsync();
                    if (chromiumWebBrowser1.CanExecuteJavascriptInMainFrame)
                    {
                        JavascriptResponse ret = await chromiumWebBrowser1.EvaluateScriptAsync("window.getSelection().toString();");


                        if (ret != null)
                        {
                            if (ret.Result.ToString() != "")
                                txt = ret.Result.ToString();
                        }


                    }
                    text = txt;
                }
             
            }
        }

        async void chromiumWebBrowser1_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
         
        }

        async void chromiumWebBrowser1_LocationChanged(object sender, EventArgs e)
        {

         

        }

        async void toolStripButton3_Click(object sender, EventArgs e)
        {
            chromiumWebBrowser1.Back();
        }
        public class ExtensionHandler : CefSharp.IExtensionHandler
        {
            public bool CanAccessBrowser(IExtension extension, IBrowser browser, bool includeIncognito, IBrowser targetBrowser)
            {
                return true;
            }

            public void Dispose()
            {
                
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public IBrowser GetActiveBrowser(IExtension extension, IBrowser browser, bool includeIncognito)
            {
                return browser;
            }

            public bool GetExtensionResource(IExtension extension, IBrowser browser, string file, IGetExtensionResourceCallback callback)
            {
                //callback.Continue(new FileStream(file,FileMode.Open, FileAccess.Read, FileShare.Read));
                return true;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public bool OnBeforeBackgroundBrowser(IExtension extension, string url, IBrowserSettings settings)
            {
                return true;
            }

            public bool OnBeforeBrowser(IExtension extension, IBrowser browser, IBrowser activeBrowser, int index, string url, bool active, IWindowInfo windowInfo, IBrowserSettings settings)
            {
                
                return true;
            }

            public void OnExtensionLoaded(IExtension extension)
            {
           
            }

            public void OnExtensionLoadFailed(CefErrorCode errorCode)
            {
              
            }

            public void OnExtensionUnloaded(IExtension extension)
            {
            
            }

            public override string ToString()
            {
                return base.ToString();
            }
        }
        private DateTime lastUpdate = DateTime.Now;
        async void updateThread()
        {
            bool loaded=false;
            while (true)
            {
                lastUpdate = DateTime.Now;
                if (chromiumWebBrowser1 != null)
                  //  if (chromiumWebBrowser1.IsBrowserInitialized)
                {
                    try
                    {
                        var currentDPI = Int32.Parse((string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ThemeManager", "LastLoadedDPI", "96"));
                        var scale = 96 / (float)currentDPI;
                        chromiumWebBrowser1.SetZoomLevel(-(1.0f + scale));

                        using (var client = chromiumWebBrowser1.GetDevToolsClient())
                        {

                            _ = client.Network.SetUserAgentOverrideAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36 - Testing 123");

                        }
                        String txt = await chromiumWebBrowser1.GetSourceAsync();
                        if (chromiumWebBrowser1.CanExecuteJavascriptInMainFrame)
                        {
                            JavascriptResponse ret = await chromiumWebBrowser1.EvaluateScriptAsync("window.getSelection().toString();");


                            if (ret != null)
                            {
                                if (ret.Result.ToString() != "")
                                    txt = ret.Result.ToString();
                            }


                        }
                        text = txt;
                    }catch (Exception ex) { }
                    }
                    System.Threading.Thread.Sleep(100);
               
            }
        }

        private void chromiumWebBrowser1_KeyPress(object sender, KeyPressEventArgs e)
        {
          
        }

        private void Form1_Move(object sender, EventArgs e)
        {
        }

        async void chromiumWebBrowser1_MouseUp(object sender, MouseEventArgs e)
        {
         

        }
        public class CustomRequestHandler : CefSharp.Handler.RequestHandler
        {
            protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
            {
                
                if (Form1.Instance.sHosts.Contains(request.Url.Replace("http://","").Replace("https://","").Substring(0, request.Url.Replace("http://", "").Replace("https://", "").IndexOf("/"))))
                        return new CustomResourceRequestHandler();                
                return null;
            }
        }

        public class CustomResourceRequestHandler : CefSharp.Handler.ResourceRequestHandler
        {
            protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
            {

                request.Url = "about:blank";
                return CefReturnValue.Cancel;
            }
        }
        String[] sHosts;


        class LifeSpanHandler : ILifeSpanHandler
        {
            public bool DoClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
            {
                return false;
            }

            public void OnAfterCreated(IWebBrowser chromiumWebBrowser, IBrowser browser)
            {
                
            }

            public void OnBeforeClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
            {
                
            }

            public bool OnBeforePopup(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
            {
                if (!popupFeatures.IsPopup)
                    chromiumWebBrowser.Load(targetUrl);
                newBrowser = null;
                return true;
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            new System.Threading.Thread(updateThread).Start();
            sHosts = File.ReadAllLines(".\\hosts");
            

            CefSettings settings = new CefSettings();
            settings.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36 /CefSharp Browser" + Cef.CefSharpVersion;
            settings.RootCachePath = Path.GetDirectoryName(Application.ExecutablePath) + "\\cache";
            settings.CachePath = Path.GetDirectoryName(Application.ExecutablePath)+"\\cache";
            settings.LogFile = null;
            settings.PersistSessionCookies = true;
            settings.PersistUserPreferences = true;
            settings.LogSeverity = LogSeverity.Disable; 

            settings.CefCommandLineArgs.Add("window-size", this.Width+","+this.Height);
            settings.CefCommandLineArgs.Add("window-height", this.Height.ToString());
            settings.CefCommandLineArgs.Add("window-width", this.Width.ToString());
            //settings.CefCommandLineArgs.Add("off-screen-rendering-enabled");
            //settings.CefCommandLineArgs.Add("enable-gpu");
            //settings.CefCommandLineArgs.Add("disable-gpu-compositing");
            
            Cef.Initialize(settings);
            chromiumWebBrowser1 = new ChromiumWebBrowser();
        
            this.chromiumWebBrowser1.ActivateBrowserOnCreation = true;
            
            this.chromiumWebBrowser1.Location = new System.Drawing.Point(0, 0);
            
            
            this.chromiumWebBrowser1.Name = "chromiumWebBrowser1";
          
            this.chromiumWebBrowser1.TabIndex = 1;
            this.chromiumWebBrowser1.LoadingStateChanged += chromiumWebBrowser1_LoadingStateChanged_1;
            this.chromiumWebBrowser1.FrameLoadEnd += chromiumWebBrowser1_FrameLoadEnd;
            this.chromiumWebBrowser1.MouseUp += chromiumWebBrowser1_MouseUp;
            
            this.chromiumWebBrowser1.Parent = panel1;

            chromiumWebBrowser1.LifeSpanHandler = new LifeSpanHandler();
            
            chromiumWebBrowser1.Width = richTextBox1.Left;
            chromiumWebBrowser1.Height = this.Height - 55;
            chromiumWebBrowser1.RequestHandler = new CustomRequestHandler();
            hook = new Hook("test");
            hook.KeyUpEvent += keyPress;
            hook.isPaused = false;



            this.chromiumWebBrowser1.Show();
            toolStripTextBox1.Width = Width - toolStripButton1.Width - toolStripButton2.Width - toolStripButton3.Width - toolStripDropDownButton1.Width - 50 - toolStripButton4.Width;

            this.WindowState=FormWindowState.Minimized;
            this.WindowState = FormWindowState.Normal;
        }
        FMUtils.KeyboardHook.Hook hook;
        private void keyPress(KeyboardHookEventArgs e)
        {

            if(!this.toolStripTextBox1.Focused && this.ContainsFocus &&e.Key==Keys.Right)
            {
                keyPress();
            }
         /*   hook = new Hook("test");
            hook.KeyUpEvent += keyPress;
            hook.isPaused = false;*/
        }

        private void keyPress(object sender, KeyPressEventArgs e)
        {
            keyPress();
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            
        }
        bool activateBlocked = false;
        private void Form1_Activated(object sender, EventArgs e)
        {
            
                       
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {

         
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
           
        }

        private void chromiumWebBrowser1_FrameLoadStart(object sender, FrameLoadStartEventArgs e)
        {

        }

        private void chromiumWebBrowser1_IsBrowserInitializedChanged(object sender, EventArgs e)
        {
      
        }

        private void chromiumWebBrowser1_LoadingStateChanged_2(object sender, LoadingStateChangedEventArgs e)
        {

        }

        private void toolStripButton3_MouseUp(object sender, MouseEventArgs e)
        {
            
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            keyPress();
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }
        int getLineOffset(int iline)
        {
            int ioffset = 0;
            for (int i = 0; i < iline; i++)
            {
                ioffset += richTextBox1.Lines[iline].Length;
            }
            return ioffset / 2;
        }

        public void keyPress()
        {
            richTextBox1.Invoke(new Action(() => {
                if (richTextBox1.Text != "")
                {
                    if(ebookContent.Count > 0) { 
                        if(offset+5 > richTextBox1.Text.Length)
                        {
                            ebookNum++;
                            text = ebookContent[ebookNum];

                        }
                    }

                    int start = 0;
                    int end = 0;
                    richTextBox1.SelectAll();
                    richTextBox1.SelectionBackColor = Color.White;
                    richTextBox1.Select(0, 0);
                    if (offset >= richTextBox1.Text.Length - 2)
                    {
                        offset = 0;
                    }
                    richTextBox1.DeselectAll();
                    while (richTextBox1.Text[offset] == ' ' || richTextBox1.Text[offset] == '\r' || richTextBox1.Text[offset] == '\n')
                        offset++;

                    // richTextBox1.SelectionStart = offset;
                    start = offset;
                    int i = 0;
                    do
                    {
                        if (sentenceToolStripMenuItem.CheckState==CheckState.Checked)
                        {
                            while (offset + i + 2 < richTextBox1.Text.Length && richTextBox1.Text[offset + i] != '\n' && !(richTextBox1.Text[offset + i] == '.' && (richTextBox1.Text[offset + i + 1] == ' ' || richTextBox1.Text[offset + i + 1] == '\r' || richTextBox1.Text[offset + i + 1] == '\n')) && !(richTextBox1.Text[offset + i] == ':' && richTextBox1.Text[offset + i + 1] == ' '))
                                i++;
                        }
                        else
                        {
                            while (offset + i + 2 < richTextBox1.Text.Length && richTextBox1.Text[offset + i] != '\n')
                                i++;
                        }

                        richTextBox1.SelectionLength = i + 1;

                        offset += i + 1;
                        end = offset;
                    } while (richTextBox1.SelectionLength < 5 && offset < richTextBox1.TextLength);


                    if(highlightWholeParagraphToolStripMenuItem1.CheckState == CheckState.Checked)
                    {
                        richTextBox1.SelectionStart = getParagraphAroundOffset(end - 2, richTextBox1.Text)[0];
                        richTextBox1.SelectionLength = (getParagraphAroundOffset(end - 2, richTextBox1.Text)[1] - richTextBox1.SelectionStart);
                        if(invertColorsToolStripMenuItem1.CheckState!=CheckState.Checked)
                        {
                            richTextBox1.SelectionBackColor = Color.LightYellow;
                        }
                        else
                        {
                            richTextBox1.SelectionBackColor = Color.Honeydew;
                            
                        }
                        
                    }
                    

                    richTextBox1.ScrollToCaret();

                    richTextBox1.SelectionStart = start;
                    richTextBox1.SelectionLength = (end - start)+1;

                    if (highlightWholeParagraphToolStripMenuItem1.CheckState == CheckState.Checked)
                    {
                        if (invertColorsToolStripMenuItem1.CheckState != CheckState.Checked)
                        {
                            richTextBox1.SelectionBackColor = Color.LightGreen;
                            
                        }
                        else
                        {
                            richTextBox1.SelectionBackColor = Color.LightYellow;
                        }
                    }
                    else
                    {
                        richTextBox1.SelectionBackColor = Color.LightGreen;
                    }

                    
                    richTextBox1.Select(0, 0);
                }
            }));
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void panel2_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void Form1_Resize_1(object sender, EventArgs e)
        {
            if (chromiumWebBrowser1 != null)
            {
                toolStripTextBox1.Width = Width - toolStripButton1.Width - toolStripButton2.Width - toolStripButton3.Width - toolStripDropDownButton1.Width - 50 - toolStripButton4.Width;
                chromiumWebBrowser1.Width = richTextBox1.Left;
                chromiumWebBrowser1.Height = this.Height - 55;
            }
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            chromiumWebBrowser1.Back();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            chromiumWebBrowser1.Forward();
        }

        private void toolStripButton3_Click_1(object sender, EventArgs e)
        {
            chromiumWebBrowser1.Load(toolStripTextBox1.Text);
            chromiumWebBrowser1.Focus();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void sentenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sentenceToolStripMenuItem.Checked = true;
            paragraphToolStripMenuItem.Checked = false;
        }

        private void paragraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sentenceToolStripMenuItem.Checked = false;
            paragraphToolStripMenuItem.Checked = true;
        }

        private void highlightAlsoSentenceeToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void highlightWholeParagraphToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void invertColorsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        List<String> ebookContent=new List<string>();
        int ebookNum = 0;

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd= new OpenFileDialog();
            ofd.Filter = "Text Files (*.txt)|*.txt|eBook Files (*.epub)|*.epub";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (ofd.FileName.EndsWith(".txt"))
                {
                    text = File.ReadAllText(ofd.FileName);
                }
                else if(ofd.FileName.EndsWith(".epub"))
                {
                   // MessageBox.Show(this, "Feature not yet implemented.");
                                        
                    EpubBook book = EpubReader.ReadBook(ofd.FileName);
                    foreach (EpubLocalTextContentFile textContentFile in book.ReadingOrder)
                    {
                        HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
                        htmlDocument.LoadHtml(textContentFile.Content);
                        StringBuilder sb = new StringBuilder();
                        foreach (HtmlNode node in htmlDocument.DocumentNode.SelectNodes("//text()"))
                        {
                            if(node.InnerText.Trim().Length > 0)
                                ebookContent.Add(node.InnerText);                             
                        }                        
                    }
                    if (ebookContent.Count > 0)
                        text = ebookContent[0];
                }
            }
        }
    }
}

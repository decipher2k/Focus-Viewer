using CefSharp;
using CefSharp.DevTools.CSS;
using CefSharp.DevTools.Emulation;
using CefSharp.DevTools.IO;
using CefSharp.Handler;
using CefSharp.WinForms;
using CefSharp.WinForms.Handler;
using CefSharp.WinForms.Internals;
using DeviceId;
using Elskom.Generic.Libs;
using FMUtils.KeyboardHook;
using Focus_Browser.Properties;
using HtmlAgilityPack;
using IvanAkcheurov.NTextCat.Lib;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using RicherTextBoxDemo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Speech.Synthesis;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using VersOne.Epub;
using WMPLib;

namespace Focus_Browser
{
    public partial class Form1 : Form
    {
        RankedLanguageIdentifier identifier = null;
        string language = "de";
        public static Form1 Instance;
        private String _masterPass;
        public String  masterPass { 
            get
            {
               

                string deviceId = new DeviceIdBuilder()
                    .AddMachineName()
                    .AddMacAddress()
                    .AddOsVersion()
                    .AddUserName()
                    .ToString();

                var tmpSource = ASCIIEncoding.ASCII.GetBytes(Process.GetCurrentProcess().StartTime.ToString() + deviceId);
                var tmpHash = new MD5CryptoServiceProvider().ComputeHash(tmpSource).ToString();
                BlowFish b = new BlowFish(Encoding.UTF8.GetBytes(tmpHash));
                return b.DecryptECB(_masterPass);
            }
            set 
            {
                string deviceId = new DeviceIdBuilder()
                  .AddMachineName()
                  .AddMacAddress()
                  .AddOsVersion()
                  .AddUserName()
                  .ToString();
                var tmpSource = ASCIIEncoding.ASCII.GetBytes(Process.GetCurrentProcess().StartTime.ToString() + deviceId);
                var tmpHash = new MD5CryptoServiceProvider().ComputeHash(tmpSource).ToString();
                BlowFish b = new BlowFish(Encoding.UTF8.GetBytes(tmpHash));
                _masterPass=b.EncryptECB(value);
            }
        }

        public static string ConvertThreeLetterNameToTwoLetterName(string twoLetterCountryCode)
        {
            if (twoLetterCountryCode == null || twoLetterCountryCode.Length != 3)
            {
                throw new ArgumentException("name must be three letters.");
            }

            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);

            foreach (CultureInfo culture in cultures)
            {
                
                if (culture.ThreeLetterWindowsLanguageName.ToUpper()== twoLetterCountryCode.ToUpper())
                {
                    return culture.TwoLetterISOLanguageName;
                }
            }

            return "en";
        }
        int offset = 0;
        public Form1()
        {

            Instance = this;
     
            InitializeComponent();
            var factory = new RankedLanguageIdentifierFactory();
            identifier = factory.Load("Core14.profile.xml"); // can be an absolute or relative path. Beware of 260 chars limitation of the path length in Windows. Linux allows 4096 chars.
           
            
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
                if (v.Success)
                {
                    tmp += v.Value + "\r\n\r\n";

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







                tmp = tmp.Replace("<p>", "").Replace("<P>", "").Replace("</p>", "").Replace("</P>", "").Replace("</a>", "").Replace("</A>", "");

                tmp = HtmlUtilities.ConvertToPlainText(tmp);

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
                            start = offset;
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
                                end = offset;
                            } while (richTextBox1.SelectionLength < 5 && offset < richTextBox1.TextLength);

                            if (highlightWholeParagraphToolStripMenuItem1.CheckState == CheckState.Checked)
                            {
                                richTextBox1.SelectionStart = getParagraphAroundOffset(end - 2, richTextBox1.Text)[0];
                                richTextBox1.SelectionLength = getParagraphAroundOffset(end - 2, richTextBox1.Text)[1] - richTextBox1.SelectionStart;
                                if (invertColorsToolStripMenuItem1.CheckState != CheckState.Checked)
                                {
                                    richTextBox1.SelectionBackColor = Color.LightYellow;
                                }
                                else
                                {
                                    richTextBox1.SelectionBackColor = Color.Honeydew;

                                }

                            }


                            

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

                           var languages = identifier.Identify(richTextBox1.Text);
                            var mostCertainLanguage = languages.FirstOrDefault();
                            CultureInfoConverter converter = new CultureInfoConverter();

                            Cca3 cca3;
                            Enum.TryParse<Cca3>(mostCertainLanguage.Item1.Iso639_3, out cca3);

                            String twoLetter = Enum.GetName(typeof(Cca2), cca3);

                            language = ConvertThreeLetterNameToTwoLetterName(mostCertainLanguage.Item1.Iso639_3);
                            SpeakText(richTextBox1.SelectedText);
                            richTextBox1.ScrollToCaret();
                            richTextBox1.Select(0, 0);

                    }));
                    }
                    
                }

            }
            }


        String getNextSentence()
        {
            String ret = "";
            do { 
            
            richTextBox1.Invoke(new Action(() =>
            {

                int i = 0;

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
                ret = richTextBox1.Text.Substring(offset, i);
                if (ret == "")
                    offset++;
            }));
            } while (ret == "");
            
            return ret;
        }

        bool newDL = true;
        int mode = 0;
        String lastFile = "";
        SpeechSynthesizer synthesizer=new SpeechSynthesizer();
        WMPLib.WindowsMediaPlayer wplayer = new WMPLib.WindowsMediaPlayer();
        WMPLib.WindowsMediaPlayer wplayer1 = new WMPLib.WindowsMediaPlayer();


        void DownloadVoice(String text, String file)
        {
        

            // Construct HTTP request to get the file
            HttpWebRequest httpRequest = (HttpWebRequest)
                WebRequest.Create("https://api.elevenlabs.io/v1/text-to-speech/" + ElevenLabsSettings.Instance.VoiceID + "/stream?optimize_streaming_latency=3");
            httpRequest.Method = WebRequestMethods.Http.Post;
            string postData = "";
            // Include post data in the HTTP request
            
            postData = "{\n  \"model_id\": \"eleven_multilingual_v1\",\n  \"text\": \"" + text + "\",\n  \"voice_settings\": {\n    \"similarity_boost\": 0.5,\n    \"stability\": 0.5  }\n}";
            
            httpRequest.ContentLength = postData.Length;
            httpRequest.ContentType = "application/x-www-form-urlencoded";
            httpRequest.Headers = new WebHeaderCollection();
            httpRequest.Accept = "audio/mpeg";
            httpRequest.ContentType = "application/json";
            httpRequest.Headers.Add("xi-api-key", ElevenLabsSettings.Instance.getApiKey());


            // Write the post data to the HTTP request
            StreamWriter requestWriter = new StreamWriter(
                httpRequest.GetRequestStream(),
                System.Text.Encoding.ASCII);
            requestWriter.Write(postData);
            requestWriter.Close();

            HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            Stream httpResponseStream = httpResponse.GetResponseStream();

            // Define buffer and buffer size
            int bufferSize = 1024;
            byte[] buffer = new byte[bufferSize];
            int bytesRead = 0;

            // Read from response and write to file
            FileStream fileStream;
            fileStream = File.Create(file);
            

            while ((bytesRead = httpResponseStream.Read(buffer, 0, bufferSize)) != 0)
            {
                fileStream.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();
        }

        void SpeakText(string text)
        {
            if(useElevenlabsioToolStripMenuItem.Checked||useWindowsSpeechToolStripMenuItem.Checked)
            {
                if (!useElevenlabsioToolStripMenuItem.Checked)
                {


                    try
                    {
                        synthesizer.SpeakAsyncCancelAll();
                        synthesizer.Rate =1;

                        foreach (InstalledVoice vc in synthesizer.GetInstalledVoices())
                        {
                            var v = vc;
                        }

                        synthesizer.SelectVoice(synthesizer.GetInstalledVoices().Where(a => a.VoiceInfo.Culture.TwoLetterISOLanguageName.ToLower() == language.ToLower() && a.Enabled == true).First().VoiceInfo.Name);

                        synthesizer.SpeakAsync(text);
                    }
                    catch { }
                }
                else
                {
                    if (ElevenLabsSettings.Instance.getApiKey() != "" && ElevenLabsSettings.Instance.VoiceID != "")
                    {
                        try
                        {

                            if(!File.Exists("voice.mp3") && !File.Exists("voice_new.mp3"))
                            {
                                newDL=true;
                            }

                            if (newDL)
                            {
                                DownloadVoice(text.Replace("\"", "").Replace("ä", "&auml;").Replace("ö", "&ouml;").Replace("ü", "&uuml;"), "voice.mp3");
                            }
                            else
                            {
                                if (!File.Exists("voice.mp3"))
                                {
                                    DownloadVoice(getNextSentence().Replace("\"", "").Replace("ä", "&auml;").Replace("ö", "&ouml;").Replace("ü", "&uuml;"), "voice.mp3");
                                    
                                }
                                else
                                {
                                    DownloadVoice(getNextSentence().Replace("\"", "").Replace("ä", "&auml;").Replace("ö", "&ouml;").Replace("ü", "&uuml;"), "voice_new.mp3");
                                }
                            }
                        }
                        catch
                        {
                         //   Debugger.Break();
                        }

                        try
                        {
                        

                            try
                            {

                                new System.Threading.Thread(() =>
                                {

                                    if (File.Exists(lastFile))
                                        File.Delete(lastFile);
                                    wplayer = new WindowsMediaPlayer();
                                    /*wplayer.PositionChange += (a, b) =>
                                    {
                                        if (wplayer.playState == WMPPlayState.wmppsStopped)
                                        {
                                            
                                        }
                                    };*/

                                    if (lastFile == "voice.mp3")
                                        wplayer.URL = "voice_new.mp3";
                                    else
                                        wplayer.URL = "voice.mp3";
                                    lastFile = wplayer.URL;
                                    wplayer.controls.play();
                                }).Start();

                                    DownloadVoice(getNextSentence().Replace("\"", "").Replace("ä", "&auml;").Replace("ö", "&ouml;").Replace("ü", "&uuml;"), "voice_new.mp3");
                                    


                                newDL = false;

                            }
                            catch (Exception ex)
                            {
                                Debugger.Break();
                            }
                        }
                        catch (Exception ex)
                        {
                                Debugger.Break();
                        }

                       
                    }
                }
            }

        }

        int[] getParagraphAroundOffset( int offset, string wholeText)
        {
            
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
            handleKeyboard = true;

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

        static bool handleKeyboard = true;

        class keyboardHandler : IKeyboardHandler
        {
            public bool OnKeyEvent(IWebBrowser chromiumWebBrowser, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey)
            {
                return !handleKeyboard;
            }

            public bool OnPreKeyEvent(IWebBrowser chromiumWebBrowser, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey, ref bool isKeyboardShortcut)
            {
                return !handleKeyboard;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ///new Splash().ShowDialog();
            if (ElevenLabsSettings.loadData())
            {
                masterPass=Microsoft.VisualBasic.Interaction.InputBox("Decryption Password", "Password", " ").Trim();                
            }

            synthesizer.SetOutputToDefaultAudioDevice();
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

            this.chromiumWebBrowser1.KeyboardHandler = new keyboardHandler();
            this.chromiumWebBrowser1.MouseMove+= (a, b) => handleKeyboard = true;

            this.chromiumWebBrowser1.Click += (a,b) => handleKeyboard = true;
            

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
                handleKeyboard = false;
                keyPress();                
            }
            else if(e.Key != Keys.Right && handleKeyboard==false && this.chromiumWebBrowser1.Focused)
            {
                handleKeyboard= true;
                if(e.isShiftPressed)
                    SendKeys.Send(e.Key.ToString());
                else
                    SendKeys.Send(e.Key.ToString().ToLower());
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
                            while (offset + i + 2 < richTextBox1.Text.Length && richTextBox1.Text[offset + i] != '\n' && !(richTextBox1.Text[offset + i] == '.' && (richTextBox1.Text[offset + i + 1] == ' ') && richTextBox1.Text[offset + i + 1] != '\r' && richTextBox1.Text[offset + i + 1] != '\n') && !(richTextBox1.Text[offset + i] == ':' && richTextBox1.Text[offset + i + 1] == ' '))
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
                    richTextBox1.SelectionLength = (end - start)+ (ebookContent.Count()>0? 1:0);

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
                    SpeakText(richTextBox1.SelectedText);

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
                toolStripTextBox1.Width = (Width - toolStripButton1.Width - toolStripButton2.Width - toolStripButton3.Width - toolStripDropDownButton1.Width - 50 - toolStripButton4.Width)-50;
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

        private void elevenlabsioSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new frmElevenLabsSettings().ShowDialog();
        }

        private void useElevenlabsioToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (useElevenlabsioToolStripMenuItem.CheckState != CheckState.Checked)
            {
                if (ElevenLabsSettings.Instance.getApiKey() == "")
                {
                    if (new frmElevenLabsSettings().ShowDialog() == DialogResult.OK)
                    {
                        useElevenlabsioToolStripMenuItem.Checked = true;
                        useWindowsSpeechToolStripMenuItem.Checked = false;
                    }
                }
                else
                {
                    useElevenlabsioToolStripMenuItem.Checked = true;
                    useWindowsSpeechToolStripMenuItem.Checked = false;
                }
            }
            else
            {
                useElevenlabsioToolStripMenuItem.Checked = false;
            }

        }

        private void useWindowsSpeechToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (useWindowsSpeechToolStripMenuItem.CheckState != CheckState.Checked)
            { 
                useElevenlabsioToolStripMenuItem.Checked = false;
                useWindowsSpeechToolStripMenuItem.Checked = true;
            }
            else
            {
                useWindowsSpeechToolStripMenuItem.Checked= false;
            }
        }
    }
}

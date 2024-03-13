using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.ComTypes;

using Elskom.Generic.Libs;
using CefSharp.DevTools.CSS;

namespace Focus_Browser
{
    [Serializable]
    public class ElevenLabsSettings
    {
        public static ElevenLabsSettings Instance=new ElevenLabsSettings();

        public String _apiKey { get; set; } = "";
        public String getApiKey()
        {
            if (_apiKey == "")
                return "";

            BlowFish b = new BlowFish(Encoding.UTF8.GetBytes(Form1.Instance.masterPass));
            return b.DecryptECB(_apiKey);
        }

        public void setApiKey(String key)
        {
            BlowFish b = new BlowFish(Encoding.UTF8.GetBytes(Form1.Instance.masterPass));
            _apiKey = b.EncryptECB(key);
        }

        public String VoiceID {get;set;} = "";

        public static void saveData()
        {
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\FocusViewer"))
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\FocusViewer");

            String settingsFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\FocusViewer\\ElevenLabsSettings.dat", FileMode.Create, FileAccess.Write);

            formatter.Serialize(stream, Instance);
            stream.Close();
        }

        public static bool loadData()
        {
            try
            {
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\FocusViewer\\ElevenLabsSettings.dat"))
                {
                    IFormatter formatter = new BinaryFormatter();
                    FileStream stream = new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\FocusViewer\\ElevenLabsSettings.dat", FileMode.Open, FileAccess.Read);
                    Instance = (ElevenLabsSettings)formatter.Deserialize(stream);
                    return true;
                }
            }
            catch { }
            return false;
        }
    }
}

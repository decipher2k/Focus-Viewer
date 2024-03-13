using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Focus_Browser
{
    public partial class frmElevenLabsSettings : Form
    {
        ToolTip toolTip1 = new ToolTip();
        ToolTip toolTip2 = new ToolTip();
        public frmElevenLabsSettings()
        {
            InitializeComponent();
            toolTip1.IsBalloon = true;
            toolTip1.SetToolTip(linkLabel1, "Tooltip");
            
            toolTip2.IsBalloon = true;
            toolTip2.SetToolTip(linkLabel2, "Tooltip");
        }

        private void linkLabel1_MouseHover(object sender, EventArgs e)
        {
                  
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://elevenlabs.io/app/voice-library");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://elevenlabs.io/");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox1.Text!="" && textBox2.Text!="" && textBox3.Text!="")
            {
                Form1.Instance.masterPass = textBox3.Text;                
                ElevenLabsSettings.Instance.setApiKey(textBox1.Text);
                ElevenLabsSettings.Instance.VoiceID = textBox2.Text;
                ElevenLabsSettings.saveData();
                DialogResult= DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show(this,"Please fill all fields.","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        private void frmElevenLabsSettings_Load(object sender, EventArgs e)
        {

            textBox1.Text = ElevenLabsSettings.Instance.getApiKey();
            textBox2.Text = ElevenLabsSettings.Instance.VoiceID;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MCC_Communication
{
    public partial class Host : Form
    {
        private MCC_BMP_Host M_Host = new MCC_BMP_Host();
        private MCC_BMP M_Client = new MCC_BMP();
        public Host()
        {
            InitializeComponent();
            
        }

        private async void Host_Load(object sender, EventArgs e)
        {
            await M_Host.Start();
        }

        private void Host_FormClosing(object sender, FormClosingEventArgs e)
        {
            M_Host.Stop();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (M_Host.bRefresh)
            {
                richTextBox1.AppendText(M_Host.UpdateText());
                richTextBox1.ScrollToCaret();
                //richTextBox1.Text += BMP.UpdateText();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string _msg = string.Empty;
            //M_Client.Action_SDP("Unload", 3, 2, ref _msg);
            byte[] data = M_Client.Action_SDP("Unload", 3, 2, ref _msg);
            M_Host.SendData(data);
            if (!string.IsNullOrEmpty(_msg))
            {
                richTextBox1.AppendText("Error Message : " + _msg);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            string _msg = string.Empty;
            //M_Client.Action_SDP("Unload", 3, 2, ref _msg);
            //byte[] data = M_Client.Action_SDP("Unload", 3, 2, ref _msg);
            string map = "1,1,1,1,1,1";
            byte[] data = M_Client.Action_ETS("Unload", 4, 1, map, ref _msg);
            M_Host.SendData(data);
            if (!string.IsNullOrEmpty(_msg))
            {
                richTextBox1.AppendText("Error Message : " + _msg);
            }
        }
    }
}

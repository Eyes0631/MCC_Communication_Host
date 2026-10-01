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
    public partial class ClientForm : Form
    {
        private Host Host_Program = new Host();
        private MCC_BMP BMP = new MCC_BMP();
        public ClientForm()
        {
            InitializeComponent();
            Host_Program.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string _msg = string.Empty;
            BMP.Action_ISB("Insert", 1, 1, ref _msg);
        }
    }
}

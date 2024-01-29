using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ssch.tools
{
    public partial class EGivingsCopyToProd : Form
    {
        public EGivingsCopyToProd()
        {
            InitializeComponent();
        }

        private void EGivingsCopyToProd_Load(object sender, EventArgs e)
        {

        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            File.Copy(textBox1.Text, textBox2.Text, true);
            File.Copy(textBox1.Text, textBox3.Text, true);
            btnCopy.Text = "OK";
        }
    }
}

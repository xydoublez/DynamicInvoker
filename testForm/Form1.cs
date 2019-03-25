using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace testForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ComCtrl.ComObject comObject = new ComCtrl.ComObject("ComCtrl.ini","log");
            Object re = comObject.Call("Count", "_count@4", "true", "1432432432", "#");
            MessageBox.Show(re.ToString());
        }
    }
}

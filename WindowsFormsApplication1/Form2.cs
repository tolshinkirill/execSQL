using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form2 : Form
    {
        private Form1 _mainForm;
        public Form2(Form1 mainForm)
        {
            _mainForm = mainForm;
            InitializeComponent();
            button1.Enabled = false;
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            _mainForm.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _mainForm.executeSQL(tb_serverAddress.Text, tb_userName.Text, tb_userPassword.Text);
            this.Close();
        }

        private void validateFields(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.tb_serverAddress.Text) ||
                string.IsNullOrWhiteSpace(this.tb_userName.Text) ||
                string.IsNullOrWhiteSpace(this.tb_userPassword.Text))
            {
                button1.Enabled = false;
            }
            else
            {
                button1.Enabled = true;
            }
        }
    }
}

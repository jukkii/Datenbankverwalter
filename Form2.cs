using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Datenbankverwalter
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            var mainForm = Application.OpenForms.OfType<Form1>().Single();
            Close();
            mainForm.isConnected = false;
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            var mainForm = Application.OpenForms.OfType<Form1>().Single();

            mainForm.serverText = TxtServer.Text;
            mainForm.userText = TxtUser.Text;
            mainForm.passwordText = TxtPassword.Text;
            mainForm.databaseText = TxtDatabase.Text;

            Close();
            mainForm.isConnected = true;
            mainForm.BtnConnect.Text = "Disconnect";
            mainForm.connectToDatabase();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            var mainForm = Application.OpenForms.OfType<Form1>().Single();
            TxtServer.Text = mainForm.serverText;
            TxtUser.Text = mainForm.userText;
            TxtPassword.Text = mainForm.passwordText;
            TxtDatabase.Text = mainForm.databaseText;
        }
    }
}

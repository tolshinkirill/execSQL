using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading;
using System.Text;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private string _serverAddress;
        private string _userLogin;
        private string _userPassword;
        private int counter;

        public Form1()
        {
            InitializeComponent();
            tb_pathToScript.Text = @"C:\temp\script.sql";
            openFileDialog1.FileName = tb_pathToScript.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_serverAddress) ||
                string.IsNullOrWhiteSpace(_userLogin) ||
                string.IsNullOrWhiteSpace(_userPassword))
            {
                this.Enabled = false;
                Form2 authForm = new Form2(this);
                authForm.ShowDialog();
            }
            else
            {
                executeSQL(_serverAddress, _userLogin, _userPassword);
            }
        }

        public async void executeSQL(string serverAddress, string userLogin, string userPassword)
        {
            if (string.IsNullOrWhiteSpace(_serverAddress) ||
                string.IsNullOrWhiteSpace(_userLogin) ||
                string.IsNullOrWhiteSpace(_userPassword))
            {
                _serverAddress = serverAddress;
                _userLogin = userLogin;
                _userPassword = userPassword;
            }
            string connectionString = string.Format(@"Data Source={0};User ID={1};Password={2}", serverAddress, userLogin, userPassword);
            string pathToResultFile = Path.GetDirectoryName(tb_pathToScript.Text) + @"\result.csv";
            bool resultIsSuccess = false;

            // Run this procedure in an appropriate event.  
            counter = 0;
            l_timer.Text = "00:00:00";
            timer1.Start();

            b_openFileDialog.Enabled = false;
            b_executeScript.Enabled = false;
            dtp_from.Enabled = false;
            dtp_to.Enabled = false;
            await Task.Run(() =>
            {
                resultIsSuccess = runSqlScriptFile(tb_pathToScript.Text, connectionString);
            });
            b_openFileDialog.Enabled = true;
            b_executeScript.Enabled = true;
            dtp_from.Enabled = true;
            dtp_to.Enabled = true;

            timer1.Stop();
            if (resultIsSuccess)
            {
                MessageBox.Show("Результат запроса:\n" + pathToResultFile, "Выполнено");
            }
        }

        private bool runSqlScriptFile(string pathStoreProceduresFile, string connectionString)
        {
            //Thread.Sleep(5000);
            try
            {
                string script = File.ReadAllText(pathStoreProceduresFile, Encoding.GetEncoding(1251));
                string path = Path.GetDirectoryName(pathStoreProceduresFile);
                string delimiter = ";";

                // split script on GO command
                System.Collections.Generic.IEnumerable<string> commandStrings = Regex.Split(script, @"^\s*GO\s*$",
                                         RegexOptions.Multiline | RegexOptions.IgnoreCase);
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        foreach (string commandString in commandStrings)
                        {
                            if (commandString.Trim() != "")
                            {
                                string finalCommandString = string.Format(commandString, dtp_from.Value.ToString("yyyyMMdd"), dtp_to.Value.ToString("yyyyMMdd"));
                                using (var command = new SqlCommand(finalCommandString, connection))
                                {
                                    try
                                    {
                                        command.CommandTimeout = 3600; // 1 hour
                                        SqlDataReader dr = command.ExecuteReader();

                                        using (System.IO.StreamWriter fs = new System.IO.StreamWriter(path + @"\result.csv", false, Encoding.GetEncoding(1251)))
                                        {
                                            // Loop through the fields and add headers
                                            for (int i = 0; i < dr.FieldCount; i++)
                                            {
                                                string name = dr.GetName(i);
                                                name = name.Replace("\"", "'");
                                                fs.Write("\"" + name + "\"" + delimiter);
                                            }
                                            fs.WriteLine();

                                            // Loop through the rows and output the data
                                            while (dr.Read())
                                            {
                                                for (int i = 0; i < dr.FieldCount; i++)
                                                {
                                                    string value = dr[i].ToString();
                                                    value = value.Replace("\"", "'");
                                                    fs.Write("\"" + value + "\"" + delimiter);
                                                }
                                                fs.WriteLine();
                                            }

                                            fs.Close();
                                        }
                                    }
                                    catch (SqlException ex)
                                    {
                                        timer1.Enabled = false;
                                        string spError = commandString.Length > 100 ? commandString.Substring(0, 100) + " ...\n..." : commandString;
                                        MessageBox.Show(string.Format("Please check the SqlServer script.\nFile: {0} \nLine: {1} \nError: {2} \nSQL Command: \n{3}", pathStoreProceduresFile, ex.LineNumber, ex.Message, spError), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                        return false;
                                    }
                                }
                            }
                        }
                        connection.Close();
                    }
                    catch (SqlException ex)
                    {
                        clearCredentials();
                        timer1.Enabled = false;
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                clearCredentials();
                timer1.Enabled = false;
                MessageBox.Show(ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        private void clearCredentials() {
            _serverAddress = null;
            _userLogin = null;
            _userPassword = null;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            counter += 1;
            var timespan = TimeSpan.FromSeconds(counter);
            l_timer.Text = timespan.ToString(@"hh\:mm\:ss");
        }

        private void tb_pathToScript_TextChanged(object sender, EventArgs e)
        {
            b_executeScript.Enabled = !string.IsNullOrWhiteSpace(this.tb_pathToScript.Text);
        }

        private void b_openFileDialog_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "Script files (*.txt;*.sql)|*.txt;*.sql|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Get the path of specified file
                tb_pathToScript.Text = openFileDialog1.FileName;
            }
        }
    }
}

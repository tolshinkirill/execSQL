using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private int counter;
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            // Run this procedure in an appropriate event.  
            counter = 0;
            //timer1.Interval = 600;
            timer1.Enabled = true;

            button1.Enabled = false;
            dateTimePicker1.Enabled = false;
            dateTimePicker2.Enabled = false;
            await Task.Run(() =>
            {
                runSqlScriptFile(@"script.sql", @"Data Source=localhost;Initial Catalog=Demodb;User ID=sa;Password=demol23");
            });
            button1.Enabled = true;
            dateTimePicker1.Enabled = true;
            dateTimePicker2.Enabled = true;

            timer1.Enabled = false;
        }
        private bool runSqlScriptFile(string pathStoreProceduresFile, string connectionString)
        {
            try
            {
                string script = File.ReadAllText(pathStoreProceduresFile);

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
                                using (var command = new SqlCommand(commandString, connection))
                                {
                                    try
                                    {
                                        command.ExecuteNonQuery();
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
                        timer1.Enabled = false;
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                timer1.Enabled = false;
                MessageBox.Show(ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            counter = counter + 1;
            label4.Text = counter.ToString();

            /*if (counter >= 10)
            {
                // Exit loop code.  
                timer1.Enabled = false;
                counter = 0;
            }
            else
            {
                // Run your procedure here.  
                // Increment counter.  
                counter = counter + 1;
                label4.Text = counter.ToString();
            }*/
        }
    }
}

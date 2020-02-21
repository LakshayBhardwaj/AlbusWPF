using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using Npgsql;
using System.Management;

namespace WpfAlbus
{
    /// <summary>  
    /// Interaction logic for MainWindow.xaml  
    /// </summary>   
    public partial class Authentication : Window
    {
        private DataSet ds = new DataSet();
        private DataTable dt = new DataTable();
        Registration registration = new Registration();
        string connstring = String.Format("Server={0};Port={1};" +
                           "User Id={2};Password={3};Database={4};",
                           "localhost", 5432, "",
                           "", "");
        string unique_id;
        public Authentication()
        {
            try
            {
                ManagementObjectCollection mbsList = null;
                ManagementObjectSearcher mbs = new ManagementObjectSearcher("Select * From Win32_processor");
                mbsList = mbs.Get();
                string id = "";
                foreach (ManagementObject mo in mbsList)
                {
                    id = mo["ProcessorID"].ToString();
                }

                ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
                ManagementObjectCollection moc = mos.Get();
                string motherBoard = "";
                foreach (ManagementObject mo in moc)
                {
                    motherBoard = (string)mo["SerialNumber"];
                }

                unique_id = id + motherBoard;
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
            //Console.WriteLine(myUniqueID);

            NpgsqlConnection conn = new NpgsqlConnection(connstring);
            conn.Open();
            string sql = "Select * from login where unique_id='" + unique_id + "' and sign_out = false";
            // data adapter making request from our connection
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            // i always reset DataSet before i do
            // something with it.... i don't know why :-)
            ds.Reset();
            // filling DataSet with result from NpgsqlDataAdapter
            da.Fill(ds);
            // since it C# DataSet can handle multiple tables, we will select first
            dt = ds.Tables[0];
            if (dt.Rows.Count > 0)
            {
                int i = 0;
                while (i== (((int)dt.Rows.Count)-1))
                {
                    if ((bool)dt.Rows[i]["sign_out"] == false) {
                        
                        MainWindow welcome = new MainWindow(dt.Rows[i]["email"].ToString(), unique_id);
                        string sql1 = "Select * from registration where email='" + dt.Rows[i]["email"] + "'";
                        // data adapter making request from our connection
                        NpgsqlDataAdapter da1 = new NpgsqlDataAdapter(sql1, conn);
                        // i always reset DataSet before i do
                        // something with it.... i don't know why :-)
                        ds.Reset();
                        // filling DataSet with result from NpgsqlDataAdapter
                        da1.Fill(ds);
                        // since it C# DataSet can handle multiple tables, we will select first
                        dt = ds.Tables[0];
                        welcome.UserName.Content = dt.Rows[i]["firstname"]+" "+ dt.Rows[i]["lastname"];//Sending value from one form to another form.  
                        welcome.Show();
                        Close();
                        break;
                    }
                }
            }
            else {
                InitializeComponent();
            }
            conn.Close();  
        }

        
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (textBoxEmail.Text.Length == 0)
            {
                errormessage.Text = "Enter an email.";
                textBoxEmail.Focus();
            }
            else if (!Regex.IsMatch(textBoxEmail.Text, @"^[a-zA-Z][\w\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$"))
            {
                errormessage.Text = "Enter a valid email.";
                textBoxEmail.Select(0, textBoxEmail.Text.Length);
                textBoxEmail.Focus();
            }
            else
            {
                string email = textBoxEmail.Text;
                string password = passwordBox1.Password;
               

               
                // Making connection with Npgsql provider
                NpgsqlConnection conn = new NpgsqlConnection(connstring);
                conn.Open();
                string sql = "Select * from Registration where Email='" + email + "'  and password='" + password + "'";
                // data adapter making request from our connection
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
                // i always reset DataSet before i do
                // something with it.... i don't know why :-)
                ds.Reset();
                // filling DataSet with result from NpgsqlDataAdapter
                da.Fill(ds);
                // since it C# DataSet can handle multiple tables, we will select first
                dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    string username = dt.Rows[0]["FirstName"].ToString() + " " + dt.Rows[0]["LastName"].ToString();
                    MainWindow welcome = new MainWindow(dt.Rows[0]["email"].ToString(),unique_id);
                    welcome.UserName.Content = username;//Sending value from one form to another form.  
                    welcome.Show();

                    string sql1 = "Select * from login where email='" + email + "'";
                    // data adapter making request from our connection
                    NpgsqlDataAdapter da1 = new NpgsqlDataAdapter(sql1, conn);
                    // i always reset DataSet before i do
                    // something with it.... i don't know why :-)
                    ds.Reset();
                    // filling DataSet with result from NpgsqlDataAdapter
                    da1.Fill(ds);
                    // since it C# DataSet can handle multiple tables, we will select first
                    dt = ds.Tables[0];
                    if (dt.Rows.Count > 0)
                    {
                        NpgsqlCommand cmd1 = new NpgsqlCommand("UPDATE login SET sign_out = false WHERE email ='"+email+"' ", conn);
                        cmd1.ExecuteNonQuery();
                    }
                    else {
                        NpgsqlCommand cmd = new NpgsqlCommand("insert into login (email,sign_out,unique_id) values('" + email +"',false,'"+unique_id+"')", conn);
                        cmd.ExecuteNonQuery();
                    }

                    Close();
                }
                else
                {
                    errormessage.Text = "Sorry! Please enter existing emailid/password.";
                }
                conn.Close();
            }
        }
        private void buttonRegister_Click(object sender, RoutedEventArgs e)
        {
            registration.Show();
            Close();
        }
    }
}

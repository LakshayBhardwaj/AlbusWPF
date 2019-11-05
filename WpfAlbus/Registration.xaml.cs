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
using System.Windows.Shapes;  
using System.Data;  
using System.Data.SqlClient;  
using System.Text.RegularExpressions;
using Npgsql;

namespace WpfAlbus
{  
    /// <summary>  
    /// Interaction logic for Registration.xaml  
    /// </summary>  
    public partial class Registration : Window  
    {
        private DataSet ds = new DataSet();
        private DataTable dt = new DataTable();
        public Registration()  
        {  
            InitializeComponent();  
        }  
        private void Login_Click(object sender, RoutedEventArgs e)  
        {  
            Authentication login = new Authentication();  
            login.Show();  
            Close();  
        }  
        private void button2_Click(object sender, RoutedEventArgs e)  
        {  
            Reset();  
        }  
        public void Reset()  
        {  
            textBoxFirstName.Text = "";  
            textBoxLastName.Text = "";  
            textBoxEmail.Text = "";  
            textBoxAddress.Text = "";  
            passwordBox1.Password = "";  
            passwordBoxConfirm.Password = "";  
        }  
        private void button3_Click(object sender, RoutedEventArgs e)  
        {  
            Close();  
        }  
        private void Submit_Click(object sender, RoutedEventArgs e)  
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
                string firstname = textBoxFirstName.Text;  
                string lastname = textBoxLastName.Text;  
                string email = textBoxEmail.Text;  
                string password = passwordBox1.Password;  
                if (passwordBox1.Password.Length == 0)  
                {  
                    errormessage.Text = "Enter password.";  
                    passwordBox1.Focus();  
                }  
                else if (passwordBoxConfirm.Password.Length == 0)  
                {  
                    errormessage.Text = "Enter Confirm password.";  
                    passwordBoxConfirm.Focus();  
                }  
                else if (passwordBox1.Password != passwordBoxConfirm.Password)  
                {  
                    errormessage.Text = "Confirm password must be same as password.";  
                    passwordBoxConfirm.Focus();  
                }  
                else  
                {  
                    errormessage.Text = "";  
                    string address = textBoxAddress.Text;  
                    //SqlConnection con = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=WpfAlbus;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");  
                    //con.Open();  
                    //SqlCommand cmd = new SqlCommand("Insert into Registration (FirstName,LastName,Email,Password,Address) values('" + firstname + "','" + lastname + "','" + email + "','" + password + "','" + address + "')", con);  
                    //cmd.CommandType = CommandType.Text;  
                    //cmd.ExecuteNonQuery();  
                    //con.Close();

                    try
                    {
                        // PostgeSQL-style connection string
                        string connstring = String.Format("Server={0};Port={1};" +
                            "User Id={2};Password={3};Database={4};",
                            "localhost", 5432, "postgres",
                            "thinksys@123", "albuswpf");
                        // Making connection with Npgsql provider
                        NpgsqlConnection conn = new NpgsqlConnection(connstring);
                        conn.Open();
                        string sql = "Select * from Registration where Email='" + email + "'";
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
                            errormessage.Text = "Sorry! emailid already exist";
                        }
                        else
                        {
                            NpgsqlCommand cmd = new NpgsqlCommand("insert into Registration (firstname,lastname,email,password,address) values('" + firstname + "','" + lastname + "','" + email + "','" + password + "','" + address + "')", conn);
                            cmd.ExecuteNonQuery();
                            errormessage.Text = "You have Registered successfully.";
                        }
                        
                        conn.Close();
                    }
                    catch (Exception msg)
                    {
                        // something went wrong, and you wanna know why
                        MessageBox.Show(msg.ToString());
                        throw;
                    }

                    
                    Reset();  
                }  
            }  
        }  
    }  
}
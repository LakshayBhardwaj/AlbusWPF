using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfAlbus
{
    /// <summary>
    /// Interaction logic for CreateAlbumDialog.xaml
    /// </summary>
    public partial class CreateAlbumDialog : Window
    {
        string email;
        private DataSet ds = new DataSet();
        private DataTable dt = new DataTable();
        public CreateAlbumDialog(string userId)
        {
            InitializeComponent();
            email = userId;
        }

        public string ResponseText
        {
            get { return ResponseTextBox.Text; }
            set { ResponseTextBox.Text = value; }
        }

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string connstring = String.Format("Server={0};Port={1};" +
                            "User Id={2};Password={3};Database={4};",
                            "localhost", 5432, "",
                            "", "");
            // Making connection with Npgsql provider
            NpgsqlConnection conn = new NpgsqlConnection(connstring);
            conn.Open();
            string sql = "Select * from event where email='" + email + "'  and event_name ='" + ResponseText + "'";
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
                error_message.Text = "Event name Already Present !!";
            }
            else
            {
                string sql1 = "insert into event (email,event_name) values('" + email + "', '"+ ResponseText +"')";
                // data adapter making request from our connection
                NpgsqlCommand cmd = new NpgsqlCommand(sql1, conn);
                cmd.ExecuteNonQuery();
                DialogResult = true;
            }
                
        }
    }
}

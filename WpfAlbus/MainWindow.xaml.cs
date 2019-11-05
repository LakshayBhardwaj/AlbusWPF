using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.Forms.MessageBox;

namespace WpfAlbus
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public PhotoCollection Photos;
        static ManualResetEvent divisionsignal = new ManualResetEvent(false);
        public delegate void UpdateTextCallback(double ct1, double cnt);
        bool inLoop;
        bool deleteAllImages;
        public CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString("DefaultEndpointsProtocol=https;AccountName=albus;AccountKey=V6tfipu99EwHGMhaxSiYNAE3m1FE43jSKHtNpbr3Z76Pf7nsbRFfJOsleI2Po3UWObTX/7c9R6h5JYS1EfqlGQ==;EndpointSuffix=core.windows.net");
        CloudBlobClient blobClient;
        CloudBlobContainer container;
        CloudBlobDirectory eventurl = null;
        private DataSet ds = new DataSet();
        private DataTable dt = new DataTable();
        string userId;
        public bool signOut;
        string myunique_id;
        string connstring = String.Format("Server={0};Port={1};" +
                            "User Id={2};Password={3};Database={4};",
                            "localhost", 5432, "postgres",
                            "thinksys@123", "albuswpf");
        // Create a blob client for interacting with the blob service.

        public MainWindow()
        {
            userId = "lakshy";
            myunique_id = "xxxx";
           
            InitializeComponent();

            Photos = (PhotoCollection)(Application.Current.Resources["Photos"] as ObjectDataProvider)?.Data;

            string connstring = String.Format("Server={0};Port={1};" +
                            "User Id={2};Password={3};Database={4};",
                            "localhost", 5432, "postgres",
                            "thinksys@123", "albuswpf");
            // Making connection with Npgsql provider
            NpgsqlConnection conn = new NpgsqlConnection(connstring);
            conn.Open();
            string sql = "Select * from event where email='" + userId + "'";
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
                RenderAlbums(dt.Rows);
            }
            else
            {
                MessageBox.Show("No Album present");
            }

           
            //buttonList.Children.Add(new Button() { Content="1", Style= (Style)FindResource("MetadataButton") });

            //buttonList.Children.Add(new Button() { Content = "Create", Style = (Style)FindResource("Metacreatebutton") });
            blobClient = storageAccount.CreateCloudBlobClient();

            container = blobClient.GetContainerReference("containerservice");

        }

        public MainWindow(string email, string unique_id)
        {
            userId = email;
            myunique_id = unique_id;
            //var x = PhotosListBox.Items;
           
            InitializeComponent();
            
            Photos = (PhotoCollection)(Application.Current.Resources["Photos"] as ObjectDataProvider)?.Data;

            string connstring = String.Format("Server={0};Port={1};" +
                            "User Id={2};Password={3};Database={4};",
                            "localhost", 5432, "postgres",
                            "thinksys@123", "albuswpf");
            // Making connection with Npgsql provider
            NpgsqlConnection conn = new NpgsqlConnection(connstring);
            conn.Open();
            string sql = "Select * from event where email='" + userId + "'";
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
                RenderAlbums(dt.Rows);
            }
            else
            {
                MessageBox.Show("No Album present");
            }


            //buttonList.Children.Add(new Button() { Content="1", Style= (Style)FindResource("MetadataButton") });

            //buttonList.Children.Add(new Button() { Content = "Create", Style = (Style)FindResource("Metacreatebutton") });
            blobClient = storageAccount.CreateCloudBlobClient();

            container = blobClient.GetContainerReference("containerservice");

        }

        private void RenderAlbums(DataRowCollection row)
        {
            for (int i = 0; i < row.Count; i++)
            {
                StackPanel buttonList = ButtonList;
                var newExpander = new Expander { Name = row[i]["event_name"].ToString(), Header = row[i]["event_name"].ToString(), Style = (Style)FindResource("MetadataExpander") };
                var newstackPanel = new StackPanel { Name = "NewExpanderStackPanel" };

                Button addFolder = new Button() { Content = "Add Image Folder", Style = (Style)FindResource("Metacreatebutton") };
                addFolder.Click += Click_Button;

                Button addImages = new Button() { Content = "Add Images", Style = (Style)FindResource("Metacreatebutton") };
                addImages.Click += Image_Button;

                Button deleteAllImages = new Button() { Content = "Delete All Images", Style = (Style)FindResource("Metacreatebutton") };
                deleteAllImages.Click += Delete_Button;

                Button deleteAlbum = new Button() { Content = "Delete This Album", Style = (Style)FindResource("Metacreatebutton") };
                deleteAlbum.Click += Delete_Album;

                Button publish = new Button() { Content = "Publish This Album", Style = (Style)FindResource("Metacreatebutton") };
                publish.Click += Publish_Album;
                // Add above items as children.     
                newstackPanel.Children.Add(addFolder);

                newstackPanel.Children.Add(addImages);
                newstackPanel.Children.Add(deleteAllImages);
                newstackPanel.Children.Add(deleteAlbum);
                newstackPanel.Children.Add(publish);
                newExpander.Content = newstackPanel;

                buttonList.Children.Add(newExpander);
                newExpander.Expanded += Expander_Expanded;
            }
           
        }

        private void Sign_Out(object sender, RoutedEventArgs e)
        {
            string connstring = String.Format("Server={0};Port={1};" +
                            "User Id={2};Password={3};Database={4};",
                            "localhost", 5432, "postgres",
                            "thinksys@123", "albuswpf");
            // Making connection with Npgsql provider
            NpgsqlConnection conn = new NpgsqlConnection(connstring);
            conn.Open();

            NpgsqlCommand cmd1 = new NpgsqlCommand("UPDATE login SET sign_out = true WHERE email ='" + userId + "' and unique_id = '"+myunique_id+"'", conn);
            cmd1.ExecuteNonQuery();

            conn.Close();
            Authentication auth = new Authentication();
            auth.Show();
            signOut = true;
            Close();
            

        }

            private void Create_Album(object sender, RoutedEventArgs e)
        {
            var dialog = new CreateAlbumDialog(userId);
            if (dialog.ShowDialog() == true)
            {
                //MessageBox.Show("You said: " + dialog.ResponseText);
                StackPanel buttonList = ButtonList;
                var newExpander = new Expander { Name = dialog.ResponseText, Header = dialog.ResponseText, Style = (Style)FindResource("MetadataExpander") };
                var newstackPanel = new StackPanel { Name = "NewExpanderStackPanel" };

                Button addFolder = new Button() { Content = "Add Image Folder", Style = (Style)FindResource("Metacreatebutton") };
                addFolder.Click += Click_Button;

                Button addImages = new Button() { Content = "Add Images", Style = (Style)FindResource("Metacreatebutton") };
                addImages.Click += Image_Button;

                Button deleteAllImages = new Button() { Content = "Delete All Images", Style = (Style)FindResource("Metacreatebutton") };
                deleteAllImages.Click += Delete_Button;

                Button deleteAlbum = new Button() { Content = "Delete This Album", Style = (Style)FindResource("Metacreatebutton") };
                deleteAlbum.Click += Delete_Album;
                // Add above items as children.     
                newstackPanel.Children.Add(addFolder);

                newstackPanel.Children.Add(addImages);
                newstackPanel.Children.Add(deleteAllImages);
                newstackPanel.Children.Add(deleteAlbum);
                newExpander.Content = newstackPanel;

                buttonList.Children.Add(newExpander);
                newExpander.Expanded += Expander_Expanded;
            }

           
        }

            public void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            foreach (Expander expander in ButtonList.Children.OfType<Expander>()) {
                if (expander != sender) {
                    expander.IsExpanded = false;
                }
            }
            // System.Diagnostics.Debug.WriteLine("---"+(sender as Button).Content.ToString());
            Photos.Path(userId,(sender as Expander).Name.ToString());
            // PhotosListBox.Items.Cast<IEnumerable<Photo>>();
            Photo y = (Photo)PhotosListBox.Items[1];
            //var x = y.ActualHeight;
            //var z = y.Height;

        }

        public void Click_Button(object sender, RoutedEventArgs e)
        {
            deleteAllImages = false;
            Thread t = new Thread(()=>Click_ButtonAsync(sender, e));
            //t.IsBackground = true;
            t.Start();
            
            
        }

        public void Image_Button(object sender, RoutedEventArgs e)
        {
            deleteAllImages = false;
            Thread t = new Thread(() => Add_Images(sender, e));
            //t.IsBackground = true;
            t.Start();


        }

        public void Delete_Button(object sender, RoutedEventArgs e)
        {
            deleteAllImages = true;
            Thread t = new Thread(() => Delete_All_Images(sender, e));
            //t.IsBackground = true;
            t.Start();

        }

        public void Delete_Album(object sender, RoutedEventArgs e)
        {
            deleteAllImages = true;
            Thread t = new Thread(() => Delete_All_Images(sender, e));
            //t.IsBackground = true;
            t.Start();
            NpgsqlConnection conn = new NpgsqlConnection(connstring);
            conn.Open();
            string sql1 = "delete from event where email = '"+userId+"' and event_name = '"+ (((sender as Button).Parent as StackPanel).Parent as Expander).Name.ToString() + "'";
            // data adapter making request from our connection
            NpgsqlCommand cmd = new NpgsqlCommand(sql1, conn);
            cmd.ExecuteNonQuery();
            conn.Close();
            ButtonList.Children.Remove(((sender as Button).Parent as StackPanel).Parent as Expander);
        }

        public void Publish_Album(object sender, RoutedEventArgs e)
        {

        }

            public void Delete_Image(object sender, RoutedEventArgs e)
        {
            var photo = (Photo) PhotosListBox.SelectedItem;
            //MessageBox.Show(System.IO.Path.GetFileName(photo.Source));

            this.Dispatcher.Invoke(new Action(delegate () { eventurl = container.GetDirectoryReference(userId + "/" + Photos.getEventName + "/EventImages"); }));

            CloudBlockBlob blockBlob = eventurl.GetBlockBlobReference(System.IO.Path.GetFileName(photo.Source));
            blockBlob.DeleteAsync().Wait();
           
            this.Dispatcher.Invoke(new Action(delegate () { Photos.Path(userId, Photos.getEventName); }));

        }


        private void  Click_ButtonAsync(object sender, RoutedEventArgs e)
        {
            //this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate ()
            //{
           
            progress.Dispatcher.Invoke(new Action(delegate () { progress.Value = 0; }));
                    
                    //System.Diagnostics.Debug.WriteLine("---" + (((sender as Button).Parent as StackPanel).Parent as Expander).Name.ToString());

                    System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            var result = STAShowDialog(dlg);

                    
            if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        
                        string img;
                        double ct = 0;
                        var dir = dlg.SelectedPath;
                        var imageList =
                            new ConcurrentBag<string>(
                                Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories)
                                    .Where(s => s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".jpeg") || s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".bmp") || s.ToLower().EndsWith(".gif")));
                        double count = (double)imageList.Count();
                        while (imageList.TryTake(out img))
                        {
                    inLoop = true;
                    var breaker = false;
                    this.Dispatcher.Invoke(new Action(delegate () { if ((!stopButton.IsEnabled)||deleteAllImages) { breaker = true; progress.Value = 0; stopButton.IsEnabled = true; } }));
                    if (breaker) { break; }
                        
                            System.Diagnostics.Debug.WriteLine(img);
                            

                            this.Dispatcher.Invoke(new Action(delegate () { eventurl = container.GetDirectoryReference(userId+"/" + (((sender as Button).Parent as StackPanel).Parent as Expander).Name.ToString() + "/EventImages"); }));
                   

                            //CloudBlockBlob blockBlob = container.GetBlockBlobReference(ImageToUpload);
                            string imageName = System.IO.Path.GetFileName(img);
                            CloudBlockBlob blockBlob = eventurl.GetBlockBlobReference(imageName);
                            Task t = blockBlob.UploadFromFileAsync(img, FileMode.Open);
                            t.Wait();
                            ct++;
                           
                            progress.Dispatcher.Invoke(
                                  new UpdateTextCallback(this.UpdateText),
                                  new object[] { ct, count }
                                );
                                                 
                            Thread.Sleep(2000);
                            this.Dispatcher.Invoke(new Action(delegate () { Photos.Path(userId, (((sender as Button).Parent as StackPanel).Parent as Expander).Name.ToString()); }));
                }
                        inLoop=false;
                    }
            //}));

            this.Dispatcher.Invoke(new Action(delegate () { Photos.Path(userId, (((sender as Button).Parent as StackPanel).Parent as Expander).Name.ToString());  progress.Value = 0; }));
                

        }

        private void UpdateText(double ct, double count)
        {
            progress.Value = (ct / count) * 100; // Do all the ui thread updates here
            //this.Hide();
            //this.ShowDialog();
        }

        private void Add_Images(object sender, RoutedEventArgs e)
        {
            
            progress.Dispatcher.Invoke(new Action(delegate () { progress.Value = 0; }));
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Images (*.BMP;*.JPG;*.JPEG;*.GIF,*.PNG,*.TIFF)|*.BMP;*.JPG;*.GIF;*.PNG;*.TIFF|" +
"All files (*.*)|*.*";

            openFileDialog1.Multiselect = true;
            openFileDialog1.Title = "Select Photos";
            var result = STAShowDialog(openFileDialog1);

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                double ct = 0;
                double count = openFileDialog1.FileNames.Length;
                foreach (String file in openFileDialog1.FileNames)
                {
                    inLoop = true;
                    var breaker = false;
                    this.Dispatcher.Invoke(new Action(delegate () { if ((!stopButton.IsEnabled) || deleteAllImages) { breaker = true; progress.Value = 0; stopButton.IsEnabled = true; } }));
                    if (breaker) { break; }
                    System.Diagnostics.Debug.WriteLine("---"+file);
                    this.Dispatcher.Invoke(new Action(delegate () { eventurl = container.GetDirectoryReference(userId+"/" + (((sender as Button).Parent as StackPanel).Parent as Expander).Name.ToString() + "/EventImages"); }));


                    //CloudBlockBlob blockBlob = container.GetBlockBlobReference(ImageToUpload);
                    string imageName = System.IO.Path.GetFileName(file);
                    CloudBlockBlob blockBlob = eventurl.GetBlockBlobReference(imageName);
                    Task t = blockBlob.UploadFromFileAsync(file, FileMode.Open);
                    t.Wait();
                    ct++;

                    progress.Dispatcher.Invoke(
                          new UpdateTextCallback(this.UpdateText),
                          new object[] { ct, count }
                        );

                    Thread.Sleep(2000);
                    this.Dispatcher.Invoke(new Action(delegate () { Photos.Path(userId, (((sender as Button).Parent as StackPanel).Parent as Expander).Name.ToString()); }));
                }
                inLoop = false;
            }
            this.Dispatcher.Invoke(new Action(delegate () { Photos.Path(userId, (((sender as Button).Parent as StackPanel).Parent as Expander).Name.ToString()); progress.Value = 0; }));
        }
            private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (inLoop) { stopButton.IsEnabled = false; }
            
            
        }

        private void Delete_All_Images(object sender, RoutedEventArgs e) {
            
            

            this.Dispatcher.Invoke(new Action(delegate () { eventurl = container.GetDirectoryReference(userId+"/" + (((sender as Button).Parent as StackPanel).Parent as Expander).Name.ToString() + "/EventImages"); }));
                
            double count = (double)eventurl.ListBlobs().Count();
            double i = 0;
            foreach (var blob in eventurl.ListBlobs())
            {
                inLoop = true;
                var breaker = false;
                this.Dispatcher.Invoke(new Action(delegate () { if ((!stopButton.IsEnabled)) { breaker = true; progress.Value = 0; stopButton.IsEnabled = true; } }));
                if (breaker) { break; }
                System.Diagnostics.Debug.WriteLine(blob.Uri.Segments.Last());

                CloudBlockBlob blockBlob = eventurl.GetBlockBlobReference(blob.Uri.Segments.Last());
                blockBlob.DeleteAsync().Wait();
                i++;
                progress.Dispatcher.Invoke(new Action(delegate () { progress.Value = (double)i / count * 100; }));
                this.Dispatcher.Invoke(new Action(delegate () { Photos.Path(userId, (((sender as Button).Parent as StackPanel).Parent as Expander).Name.ToString()); }));
            }
            inLoop = false;
            this.Dispatcher.Invoke(new Action(delegate () { Photos.Path(userId, (((sender as Button).Parent as StackPanel).Parent as Expander).Name.ToString()); progress.Value = 0; }));
           
        }

        private static CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException)
            {
                System.Diagnostics.Debug.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                Console.ReadLine();
                throw;
            }
            catch (ArgumentException)
            {
                System.Diagnostics.Debug.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                Console.ReadLine();
                throw;
            }

            return storageAccount;
        }


        public class DialogState

        {

            public DialogResult result;

            public FolderBrowserDialog dialog;



            public void ThreadProcShowDialog()

            {

                result = dialog.ShowDialog();

            }

        }

        public class DialogStateOpenFile

        {

            public DialogResult result;

            public OpenFileDialog dialog;



            public void ThreadProcShowDialog()

            {

                result = dialog.ShowDialog();

            }

        }

        private DialogResult STAShowDialog(FolderBrowserDialog dialog)

        {
            DialogState state = new DialogState();

            state.dialog = dialog;

            System.Threading.Thread t = new System.Threading.Thread(state.ThreadProcShowDialog);

            t.SetApartmentState(System.Threading.ApartmentState.STA);

            t.Start();

            t.Join();

            return state.result;
        }

        private DialogResult STAShowDialog(OpenFileDialog dialog)

        {
            DialogStateOpenFile state = new DialogStateOpenFile();

            state.dialog = dialog;

            System.Threading.Thread t = new System.Threading.Thread(state.ThreadProcShowDialog);

            t.SetApartmentState(System.Threading.ApartmentState.STA);

            t.Start();

            t.Join();

            return state.result;
        }

    }
}

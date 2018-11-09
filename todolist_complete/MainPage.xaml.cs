using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using System.Linq;
using Windows.Security.Credentials;
using Newtonsoft.Json.Linq;

using Windows.Networking.PushNotifications;
using System.Net.Http;

// To add offline sync support, add the NuGet package Microsoft.WindowsAzure.MobileServices.SQLiteStore
// to your project. Then, uncomment the lines marked // offline sync
// For more information, see: http://go.microsoft.com/fwlink/?LinkId=717898
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using System.IO;
using Microsoft.AspNet.SignalR.Client;
using Windows.UI.Core;
using System.Net;

namespace todolist_complete
{
    public sealed partial class MainPage : Page
    {
        private MobileServiceCollection<TodoItem, TodoItem> items;
        //private IMobileServiceTable<TodoItem> todoTable = App.MobileService.GetTable<TodoItem>();

        // We are using an offline sync table implemented by SQLite.
        private IMobileServiceSyncTable<TodoItem> todoTable = App.MobileService.GetSyncTable<TodoItem>();
        public ChatMessageViewModel ChatVM { get; set; } = new ChatMessageViewModel();
        public HubConnection conn { get; set; }
        public IHubProxy proxy { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
            //this.DataContext = (Application.Current as App).ChatVM;
           
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //await InitLocalStoreAsync(); // offline sync
            // ButtonRefresh_Click(this, null);
        }

        // Push authentication code added in http://aka.ms/m79ei6
        #region push notifications
        // Registers for template push notifications.
        private async void InitNotificationsAsync()
        {
            var channel = await PushNotificationChannelManager
                .CreatePushNotificationChannelForApplicationAsync();

            // Define a toast templates for WNS.
            var toastTemplate =
                @"<toast><visual><binding template=""ToastText02""><text id=""1"">"
                                + @"New item:</text><text id=""2"">"
                                + @"$(messageParam)</text></binding></visual></toast>";

            JObject templateBody = new JObject();
            templateBody["body"] = toastTemplate;

            // Add the required WNS toast header.
            JObject wnsToastHeaders = new JObject();
            wnsToastHeaders["X-WNS-Type"] = "wns/toast";
            templateBody["headers"] = wnsToastHeaders;

            JObject templates = new JObject();
            templates["testTemplate"] = templateBody;

            try
            {
                // Register for template push notifications.
                await App.MobileService.GetPush()
                .RegisterAsync(channel.Uri, templates);

                // Define two new tags as a JSON array.
                var body = new JArray();
                body.Add("broadcast");
                body.Add("test");

                // Call the custom API '/api/updatetags/<installationid>' 
                // with the JArray of tags.
                var response = await App.MobileService
                    .InvokeApiAsync("updatetags/"
                    + App.MobileService.InstallationId, body);
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Push registration failed");
            }
        }
        #endregion
        #region authentication
        private async void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
           
            //if (await AuthenticateAsync())
            {
                SignalR();
                InitNotificationsAsync();

                // Switch the buttons and load items from the mobile app.
                ButtonLogin.Visibility = Visibility.Collapsed;
                ButtonSave.Visibility = Visibility.Visible;
            }
        }

        // Define a member variable for storing the signed-in user. 
        private MobileServiceUser user;

       
        private async System.Threading.Tasks.Task<bool> AuthenticateAsync()
        {
            string message;
            bool success = false;

           
            var provider = MobileServiceAuthenticationProvider.Google;

           
            PasswordVault vault = new PasswordVault();
            PasswordCredential credential = null;

            try
            {
                
                credential = vault.FindAllByResource(provider.ToString()).FirstOrDefault();
            }
            catch (Exception)
            {
                // When there is no matching resource an error occurs, which we ignore.
            }

           
            if (credential != null && !App.MobileService.IsTokenExpired(credential))
            {
                
                user = new MobileServiceUser(credential.UserName);
                credential.RetrievePassword();
                user.MobileServiceAuthenticationToken = credential.Password;

              
                App.MobileService.CurrentUser = user;

                // Notify the user that cached credentials were used.
                message = string.Format("Signed-in with cached credentials for user - {0}", user.UserId);
                success = true;
            }
            else
            {
                try
                {
                   
                    if (credential != null)
                    {
                        vault.Remove(credential);
                    }

                    
                    user = await App.MobileService
                        .LoginAsync(provider);

                    // Create and store the user credentials.
                    credential = new PasswordCredential(provider.ToString(),
                        user.UserId, user.MobileServiceAuthenticationToken);
                    vault.Add(credential);

                    // Welcome user and display login SID info.
                    message = string.Format("You are now logged in - {0}", user.UserId);
                    success = true;
                }

                catch (InvalidOperationException)
                {
                    message = "You must log in. Login Required";
                }
            }

            await new MessageDialog(message).ShowAsync();

            return success;
        }

        #endregion
        //private async Task InsertTodoItem(TodoItem todoItem)
        //{
        //    // This code inserts a new TodoItem into the database. When the operation completes
        //    // and Mobile Apps has assigned an Id, the item is added to the CollectionView
        //    await todoTable.InsertAsync(todoItem);
        //    items.Add(todoItem);

        //    // Upload offline changes to the backend.
        //    await App.MobileService.SyncContext.PushAsync();
        //}

       

        private async void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".png");
            
            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
            var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            var image = new BitmapImage();
            image.SetSource(stream);
            img.Source = image;

                var credentials = new StorageCredentials("mobilestoragedemo", "qIg2DW7C9D7QGAxyK6GOtzvUPAQmXOnVyYrYC6u4xl+bBUtqBtN+4nBpobvYb6EFlAoR60nkwDoZ39OKvKB43A==");
                var account = new CloudStorageAccount(credentials, "mobilestoragedemo", "core.windows.net", true);


                CloudBlobClient blobClient = account.CreateCloudBlobClient();

                CloudBlobContainer container = blobClient.GetContainerReference("images");

                await container.CreateIfNotExistsAsync();

                await container.SetPermissionsAsync(new BlobContainerPermissions
                {

                    PublicAccess = BlobContainerPublicAccessType.Blob
                });


                //upload a blob
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(file.Name);
                
                await blockBlob.UploadFromFileAsync(file);
                await new MessageDialog("Image Uploaded on blob").ShowAsync();  
            }
            //UploadOnStorageAccount(file.Path, file.Name,file);
        }
       
        public async void SignalR()
        {
            conn = new HubConnection("https://mobileappno2.azurewebsites.net");
            proxy = conn.CreateHubProxy("SHub");
            //conn.Credentials = crede
            //var credential = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes("vickswat@gmail.com" + ":" + "airbus@12345"));
            //conn.Headers.Add("Authorization", $"Basic {credential}");
            
            await conn.Start();
            //Execute();
            proxy.On<Object>("SendServerTime", async data =>
                             await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher
                                   .RunAsync(CoreDispatcherPriority.Normal,
                                             () => TextInput.Text=data.ToString()));

            
            var msg1 = new TodoItem { Text = "Topic"};
            //Task.Delay(10000);

                await proxy.Invoke("send",msg1);
            
            //await new MessageDialog("").ShowAsync();

        }

       

        private void TextInput_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ButtonSave.Focus(FocusState.Programmatic);
            }
        }

        #region Offline sync

        private async Task InitLocalStoreAsync()
        {
            if (!App.MobileService.SyncContext.IsInitialized)
            {
                var store = new MobileServiceSQLiteStore("localstore.db");
                store.DefineTable<TodoItem>();
                await App.MobileService.SyncContext.InitializeAsync(store);
            }

            await SyncAsync();
        }

        private async Task SyncAsync()
        {
            await App.MobileService.SyncContext.PushAsync();
            await todoTable.PullAsync("todoItems", todoTable.CreateQuery());
        }

        #endregion
    }
}

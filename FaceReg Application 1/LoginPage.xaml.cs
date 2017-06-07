#region Using Region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ViewManagement;
using Windows.UI.Core;
using Windows.Media.Playback;

#endregion

namespace FaceRegApplication1
{
    /// <summary>
    /// This is the backend class for the Login Page
    /// 
    /// The methods in this class describes the methods needed to 
    /// login a user to the system
    /// </summary>
    /// By Robin Skafte, Christoffer Huynh, Martin Nguyen, Osama Menim, Anujan Balasingam

    public sealed partial class LoginPage : Page
    {
        #region Variables Region

        private Database database = new Database();
        User user = new User();
        #endregion

        #region Constructor Region

        public LoginPage()
        {
            InitializeComponent();

            ApplicationView.PreferredLaunchWindowingMode = Windows.UI.ViewManagement.ApplicationViewWindowingMode.Auto;


        }

        #endregion

        #region Button Event Region

        /// <summary>
        /// Login function
        /// 
        /// Fired when pressing the login button in the GUI
        /// or by invokig the method enterPressed()
        /// </summary>
        private async void buttonLogin_Click(object sender, RoutedEventArgs e)
        {
            // bool ok = await database.LoginUser(tbxUserName.Text,tbxPassword.Password);
            if (database.LoginUser(tbxUsername.Text, tbxPassword.Password))
            {
                //  await Message.Show(Message.LOGIN_SUCCESSFUL);
                Frame.Navigate(typeof(RegisterPage));
            }
            else
            {
                await Message.Show(Message.LOGIN_ERROR);
            }

        }

        /// <summary>
        /// Error handling for the input box for username
        /// </summary>
        private void tbxUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            user.Username = tbxUsername.Text.Trim().Split(' ').First().ToLower();
            if (user.Username.Length > 0)
            {
                user.Username = user.Username.First().ToString().ToUpper() + user.Username.Substring(1);
            }
        }

        /// <summary>
        /// Detect keypress (Keys.Enter)
        /// 
        /// Initiate login sequence by calling the login function
        /// </summary>
        private void enterPressed(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                buttonLogin_Click(this, new RoutedEventArgs());
            }
        }


        #endregion

    }

}
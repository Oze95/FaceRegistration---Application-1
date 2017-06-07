#region Using Region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

#endregion

namespace FaceRegApplication1
{
    /// <summary>
    /// This class is used to display messages/alerts on the screen
    /// </summary>
    /// By Robin Skafte, Christoffer Huynh, Martin Nguyen, Osama Menim, Anujan Balasingam

    public static class Message
    {
        #region Variables Region

        public const int REGISTRATION_SUCCESSFUL = 0;
        public const int REGISTRATION_ERROR = 1;
        public const int LOGIN_SUCCESSFUL = 2;
        public const int LOGIN_ERROR = 3;
        public const int TAKE_PHOTO_ERROR = 4;
        public const int INVALID_CONNECTION = 5;
        public const int INVALID_LOGIN = 6;
        
        #endregion

        #region Static Methods Region

        /// <summary>
        /// Global (public static) method to show messages/alerts on the screen
        /// </summary>
        /// <param name="message"></param>
        /// <param name="username"></param>
        /// <param name="firstname"></param>
        /// <param name="lastname"></param>
        /// <param name="email"></param>
        /// <returns>A dialog Task</returns>
        public static async Task Show(int message, string username = "", string firstname = "", string lastname = "", string email = "")
        {
            var dialog = new MessageDialog("");

            switch (message)
            {
                case REGISTRATION_SUCCESSFUL:
                    dialog.Title = "Registration complete";
                    dialog.Content = "- Firstname: " + firstname + "\n" + "- Lastname: " + lastname + "\n" + "- Email: " + email;
                    break;
                case REGISTRATION_ERROR:
                    dialog.Title = "First- and lastname";
                    dialog.Content = "- Must be between 2-32 characters \n - Can only containt the letters A-Z and a-z";
                    break;
                case LOGIN_SUCCESSFUL:
                    dialog.Title = "Login successful";
                    dialog.Content = "- Successfully logged in: " + username;
                    break;
                case LOGIN_ERROR:
                    dialog.Title = "Login failed";
                    dialog.Content = "- Invalid username or password.";
                    break;
                case TAKE_PHOTO_ERROR:
                    dialog.Title = "Insufficient information";
                    dialog.Content = "- Please enter your email before taking a picture.";
                    break;
                case INVALID_CONNECTION:
                    dialog.Title = "Connection refused";
                    dialog.Content = "- Cannot connect to the server. Contact your administrator.";
                    break;
            }
            await dialog.ShowAsync();
        }

        public static async Task Show(string title, string content)
        {
            var dialog = new MessageDialog("");
            dialog.Title = title;
            dialog.Content = content;
            await dialog.ShowAsync();
        }

        #endregion
    }
}

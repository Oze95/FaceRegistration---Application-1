#region Using Region

using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Diagnostics;
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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

#endregion

namespace FaceRegApplication1
{
    /// <summary>
    /// This is the backend class for the Edit Page
    /// 
    /// The methods in this class describes the methods needed to 
    /// update, search for, and delete profiles from the database
    /// </summary>
    /// By Robin Skafte, Christoffer Huynh, Martin Nguyen, Osama Menim, Anujan Balasingam
    
    public sealed partial class EditPage : Page
    {
        #region Variables Region

        private readonly IFaceServiceClient faceServiceClient = new FaceServiceClient("bcba6dd40af64f31b200339e2322bf1c");

        private PersonGroup personGroup = new PersonGroup("haxxorgroup");
        private string firstName;
        private string lastName;
        private string email;
        private List<String> list = new List<String>();
        private Database database = new Database();

        #endregion

        #region Constructor Region

        public EditPage()
        {
            InitializeComponent();
            boxesOff();
        }

        #endregion

        #region Methods Region

        /// <summary>
        /// Make elements visible
        /// </summary>
        private void boxesOn()
        {
            tbxfirstName.Visibility = Visibility.Visible;
            tbxlastName.Visibility = Visibility.Visible;
            tbxEmailInfo.Visibility = Visibility.Visible;
            labelfirst.Visibility = Visibility.Visible;
            labellast.Visibility = Visibility.Visible;
            labelEmail.Visibility = Visibility.Visible;
            buttonUpdate.Visibility = Visibility.Visible;
            buttonDelete.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Hide all elements
        /// </summary>
        private void boxesOff()
        {
            tbxfirstName.Visibility = Visibility.Collapsed;
            tbxlastName.Visibility = Visibility.Collapsed;
            tbxEmailInfo.IsEnabled = false;
            tbxEmailInfo.Visibility = Visibility.Collapsed;
            labelfirst.Visibility = Visibility.Collapsed;
            labellast.Visibility = Visibility.Collapsed;
            labelEmail.Visibility = Visibility.Collapsed;
            buttonUpdate.Visibility = Visibility.Collapsed;
            buttonDelete.Visibility = Visibility.Collapsed;
        }

        private void infoFiller(string myInfo)
        {
            var myFirstName = myInfo.Remove(myInfo.IndexOf(' '));
            var myLastName = myInfo.Remove(0, myInfo.IndexOf(' ') + 1);
            myLastName = myLastName.Remove(myLastName.IndexOf(' '));
            var myEmail = myInfo.Remove(0, myInfo.IndexOf('|') + 2);
            tbxfirstName.Text = myFirstName;
            tbxlastName.Text = myLastName;
            tbxEmailInfo.Text = myEmail;
        }

        /// <summary>
        /// Search for a profile
        /// </summary>
        private async void buttonSearch_Click(object sender, RoutedEventArgs e)
        {
                listProfile.Items.Clear();
                listProfile.UpdateLayout();

                list = database.searchProfile(tbxEmail.Text);

                if (tbxEmail.Text.Equals(""))
                {
                    await Message.Show("No such profile", "");
                }

                else if (list.Any())
                {
                    for (int i = 0; i < list.Count(); i = i + 3)
                    {
                        firstName = list[i].ToString();
                        lastName = list[i + 1].ToString();
                        email = list[i + 2].ToString();

                        listProfile.Items.Add(firstName + " " + lastName + " " + "|" + " " + email);
                    }
                    listProfile.UpdateLayout();
                }
                else
                {
                    await Message.Show("No such profile", "Please search again");
                }
        }

        /// <summary>
        /// Edit a profile
        /// </summary>
        private void buttonEdit_Click(object sender, RoutedEventArgs e)
        {
            if (listProfile.SelectedItems.Count > 0)
            {
                boxesOn();
                var myInfo = listProfile.SelectedItems[0].ToString();
                infoFiller(myInfo);
            }
        }

        /// <summary>
        /// Update a profile
        /// </summary>
        private async void buttonUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (isValidName(tbxfirstName.Text) && isValidName(tbxlastName.Text))
            {
                database.updateProfile(tbxEmailInfo.Text, tbxfirstName.Text, tbxlastName.Text);
                await Message.Show("Profile updated", "");
                buttonSearch_Click(this, new RoutedEventArgs());
            }
            else
            {
                await Message.Show(Message.REGISTRATION_ERROR);
            }
        }

        /// <summary>
        /// Redirect the user to the register page
        /// </summary>
        private void buttonBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RegisterPage));
        }

        /// <summary>
        /// Delete a profile from the system
        /// </summary>
        private async void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (isValidName(tbxfirstName.Text) && isValidName(tbxlastName.Text))
            {
                // Delete profile from database
                database.deleteProfile(tbxEmailInfo.Text);

                // Delete profile from person group
                personGroup.deletePerson(tbxEmailInfo.Text);
                deleteBlob();

                await Message.Show("Profile deleted", "");
                listProfile.Items.Clear();
                listProfile.UpdateLayout();
                tbxfirstName.Text = "";
                tbxlastName.Text = "";
                tbxEmailInfo.Text = "";
                tbxEmail.Text = "";
                boxesOff();

            }
            else
            {
                await Message.Show(Message.REGISTRATION_ERROR);
            }
        }

        /// <summary>
        /// Delete a profile from blobstorage
        /// </summary>
        private async void deleteBlob()
        {
            string accountName = "chipfacestorage";
            string accountKey = "dRvktoO+Osvf945yw/stQd0gMsMdfyf8qzYcy0AZ0QbRAKY1XkMmJut/r8KD5T+8Jk3m/p/au/MVPwJSd3yzzw==";

            try
            {
                StorageCredentials creds = new StorageCredentials(accountName, accountKey);
                CloudStorageAccount account = new CloudStorageAccount(creds, useHttps: true);

                CloudBlobClient client = account.CreateCloudBlobClient();

                CloudBlobContainer sampleContainer = client.GetContainerReference("faceapi");

                CloudBlockBlob blob = sampleContainer.GetBlockBlobReference(tbxEmailInfo.Text + ".jpg");

                await blob.DeleteAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERROR: " + ex.StackTrace);
            }

        }

        /// <summary>
        /// Regex to validate name
        /// </summary>
        private bool isValidName(string name)
        {
            string szNamePattern = @"^[a-zA-Z]{2,32}$";
            Regex regex = new Regex(szNamePattern);

            bool isMatch = regex.IsMatch(name);

            return isMatch;
        }

        /// <summary>
        /// Regex to validate email
        /// </summary>
        private bool isValidEmail(string email)
        {
            string szEmailPattern = @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";
            Regex regex = new Regex(szEmailPattern);

            bool isMatch = regex.IsMatch(email);

            return isMatch;
        }

        #endregion
    }
}
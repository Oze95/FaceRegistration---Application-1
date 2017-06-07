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
    /// This is the backend class for the RegisterPage
    /// 
    /// The methods in this class describes the methods needed to 
    /// register profiles and add it to the database.
    /// </summary>
    /// By Robin Skafte, Christoffer Huynh, Martin Nguyen, Osama Menim, Anujan Balasingam
    public sealed partial class RegisterPage : Page
    {
        #region Variables Region

        private readonly IFaceServiceClient faceServiceClient = new FaceServiceClient("bcba6dd40af64f31b200339e2322bf1c");

        private PersonGroup personGroup = new PersonGroup("haxxorgroup");

        private StorageFile sfPhotoFile;

        private MediaCapture mediaCapture;

        private Database database = new Database();

        private bool bPictureTaken;
        private bool bIsPreviewing;

        

        #endregion

        #region Constructor Region

        public RegisterPage()
        {
            InitializeComponent();

            bPictureTaken = false;
            bIsPreviewing = false;

            sfPhotoFile = null;

            buttonTakePicture.Content = "Take Picture";

            previewElement.Visibility = Visibility.Visible;
            captureImage.Visibility = Visibility.Collapsed;
            startCameraPreview();
        }

        #endregion

        #region Button Event Region

        /// <summary>
        /// Take a picture.
        /// </summary>
        private async void buttonTakePicture_Click(object sender, RoutedEventArgs e)
        {
            if (!bPictureTaken)
            {
                if (!isValidEmail(tbxEmail.Text))
                {
                    await Message.Show(Message.TAKE_PHOTO_ERROR);
                    return;
                }

                previewElement.Visibility = Visibility.Collapsed;
                captureImage.Visibility = Visibility.Visible;

                bPictureTaken = true;
                buttonTakePicture.Content = "Take new picture";

                try
                {
                    captureImage.Source = null;

                    sfPhotoFile = await KnownFolders.PicturesLibrary.CreateFileAsync(
                        tbxFirstName.Text + "jpeg",
                        CreationCollisionOption.ReplaceExisting);

                    ImageEncodingProperties imageProperties = ImageEncodingProperties.CreateJpeg();

                    await mediaCapture.CapturePhotoToStorageFileAsync(imageProperties, sfPhotoFile);

                    IRandomAccessStream photoStream = await sfPhotoFile.OpenReadAsync();
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.SetSource(photoStream);
                    captureImage.Source = bitmap;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error: " + ex.StackTrace);
                }
            }
            else
            {
                previewElement.Visibility = Visibility.Visible;
                captureImage.Visibility = Visibility.Collapsed;
                bPictureTaken = false;
                buttonTakePicture.Content = "Take picture";
            }
        }

        /// <summary>
        /// Register profile.
        /// </summary>
        private async void buttonRegister_Click(object sender, RoutedEventArgs e)
        {

            var list = database.searchProfile(tbxEmail.Text);


            try
            {

                if (isValidName(tbxFirstName.Text) && isValidName(tbxLastName.Text) && isValidEmail(tbxEmail.Text) && bPictureTaken)
                {
                    // var list = database.searchProfile(tbxEmail.Text);
                    if (list.Any())
                    {
                        throw new FormatException();
                    }

                    database.addProfile(tbxEmail.Text, tbxFirstName.Text, tbxLastName.Text);
                    personGroup.addPerson(tbxEmail.Text, sfPhotoFile);
                    await Message.Show(Message.REGISTRATION_SUCCESSFUL, "", tbxFirstName.Text, tbxLastName.Text, tbxEmail.Text);
                    tbxFirstName.Text = "";
                    tbxLastName.Text = "";
                    tbxEmail.Text = "";

                    previewElement.Visibility = Visibility.Visible;
                    captureImage.Visibility = Visibility.Collapsed;
                    bPictureTaken = false;

                    buttonTakePicture.Content = "Take picture";

                    if (sfPhotoFile != null)
                    {
                        // Upload image to blob storage
                        sendToBlob(sfPhotoFile, tbxEmail.Text);
                    }
                }
                else
                {
                    if (!bPictureTaken)
                    {
                        await Message.Show("No picture taken", "You have to take a photo of yourself before you can register");
                    }
                    else
                    {
                        await Message.Show(Message.REGISTRATION_ERROR);
                    }
                }

            }
            catch (FormatException my)
            {
                System.Diagnostics.Debug.WriteLine(my.Message);
                await Message.Show("Cannot register", "Email is already in use");
            }
        }

        /// <summary>
        /// Logout, returns to LoginPage.
        /// </summary>
        private async void button_logout(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
            await mediaCapture.StopPreviewAsync();
        }


        /// <summary>
        /// Go to the EditPage.
        /// </summary>
        private async void button_edit(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(EditPage));
            await mediaCapture.StopPreviewAsync();
        }


        #endregion

        #region Other Private Methods


        /// <summary>
        /// Send data to Blobstorage.
        /// </summary>
        private async void sendToBlob(StorageFile file, string fileName)
        {
            string accountName = "chipfacestorage";
            string accountKey = "dRvktoO+Osvf945yw/stQd0gMsMdfyf8qzYcy0AZ0QbRAKY1XkMmJut/r8KD5T+8Jk3m/p/au/MVPwJSd3yzzw==";

            try
            {
                StorageCredentials creds = new StorageCredentials(accountName, accountKey);
                CloudStorageAccount account = new CloudStorageAccount(creds, useHttps: true);

                CloudBlobClient client = account.CreateCloudBlobClient();

                CloudBlobContainer sampleContainer = client.GetContainerReference("faceapi");
                await sampleContainer.CreateIfNotExistsAsync();

                CloudBlockBlob blob = sampleContainer.GetBlockBlobReference(fileName + ".jpg");

                System.Diagnostics.Debug.WriteLine("Uploaded to blob storage.");
                await blob.UploadFromFileAsync(file);


                await Task.Run(() =>
                {
                    Task.Yield();

                    System.Diagnostics.Debug.WriteLine("Fil raderad");
                    File.Delete(file.Path);

                }
                      );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// Activate camera.
        /// </summary>
        private async void startCameraPreview()
        {
            try
            {
                if (mediaCapture != null)
                {
                    // Cleanup MediaCapture object
                    if (bIsPreviewing)
                    {
                        await mediaCapture.StopPreviewAsync();
                        previewElement.Source = null;
                        bIsPreviewing = false;
                    }
                    mediaCapture.Dispose();
                    mediaCapture = null;
                }

                // Use default initialization
                mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync();

                // Start Preview                
                previewElement.Source = mediaCapture;
                await mediaCapture.StartPreviewAsync();
                bIsPreviewing = true;

            }

            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Unable to initialize camera for audio/video mode: " + ex.Message);
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

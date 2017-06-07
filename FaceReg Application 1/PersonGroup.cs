#region Using Region

using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

#endregion

namespace FaceRegApplication1
{
    /// <summary>
    /// Class that contains methods for the operation
    /// that may be done on the person group
    /// </summary>
    /// By Robin Skafte, Christoffer Huynh, Martin Nguyen, Osama Menim, Anujan Balasingam

    public class PersonGroup
    {
        #region Variables Region

        private readonly IFaceServiceClient faceServiceClient = new FaceServiceClient("bcba6dd40af64f31b200339e2322bf1c");
        private string szPersonGroupID;

        private Stream stream;

        #endregion

        #region Constructor Region

        public PersonGroup(string personGroupID)
        {
            szPersonGroupID = personGroupID;
        }

        #endregion

        #region Methods Region

        /// <summary>
        /// Add a new person(profile) to the person group
        /// </summary>
        /// <param name="email">Email of the person to be added</param>
        public async void addPerson(string email, StorageFile sf)
        {
            try
            {
                // Define Person
                CreatePersonResult person = await faceServiceClient.CreatePersonAsync(szPersonGroupID, email);

                string imagePath = sf.Path;
                Debug.WriteLine(imagePath);

                await Task.Run(() =>
                {
                    Task.Yield();
                    stream = File.OpenRead(sf.Path);

                    Debug.WriteLine("File read.");
                  
            
                });
               await faceServiceClient.AddPersonFaceAsync(szPersonGroupID, person.PersonId, stream);
                Debug.WriteLine("Face added.");
            }
            catch (Exception e)
            {
                Debug.WriteLine("ERROR:");
                Debug.WriteLine(e.StackTrace);
            }
            train();
        }

        /// <summary>
        /// Train the regarded person group
        /// </summary>
        private async void train()
        {
            await faceServiceClient.TrainPersonGroupAsync(szPersonGroupID);

            TrainingStatus trainingStatus = null;
            while (true)
            {
                trainingStatus = await faceServiceClient.GetPersonGroupTrainingStatusAsync(szPersonGroupID);
                if (trainingStatus.Status == Status.Succeeded)
                {
                    Debug.WriteLine(trainingStatus.Status);
                    break;
                }
                await Task.Delay(1000);
            }
            Debug.WriteLine("Group trained: " + szPersonGroupID);
        }

        /// <summary>
        /// Delete a person from the person group
        /// </summary>
        /// <param name="personToDelete">The name of the person to delete</param>
        public async void deletePerson(string personToDelete)
        {
            var vec = await faceServiceClient.GetPersonsAsync(szPersonGroupID);

            foreach (var id in vec)
            {
                String name = id.Name;
                if (name.Equals(personToDelete))
                {
                    await faceServiceClient.DeletePersonAsync(szPersonGroupID, id.PersonId);
                    train();
                }

            }

        }

        /// <summary>
        /// Create a new person group
        /// </summary>
        public async void create()
        {
            await faceServiceClient.CreatePersonGroupAsync(szPersonGroupID, "Face Api");
            Debug.WriteLine("Person group created: " + szPersonGroupID);
        }

        /// <summary>
        /// Delete the person group
        /// </summary>
        public async void delete()
        {
            await faceServiceClient.DeletePersonGroupAsync(szPersonGroupID);
            Debug.WriteLine("Person group deleted: " + szPersonGroupID);
        }

        #endregion
    }
}

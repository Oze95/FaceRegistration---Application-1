using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRegApplication1
{
    /// <summary>
    /// Help class/class struct to manage credentials for the User.
    /// Can be used as the means for data binding (One-way, or Two-Day)
    /// </summary>
    /// By Robin Skafte, Christoffer Huynh, Martin Nguyen, Osama Menim, Anujan Balasingam

    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}

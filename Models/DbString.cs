using Microsoft.Extensions.Configuration;
using System;

namespace CoreApi_BL_App.Models
{
    public class Dbstring
    {
        public string MyCon { get; private set; }

        public Dbstring(IConfiguration configuration)
        {
            // Fetch the connection string from the IConfiguration instance.
            this.MyCon = configuration.GetConnectionString("defaultConnectionbeta");

            // Optional: You can throw an exception or log an error if the connection string is missing.
            if (string.IsNullOrEmpty(this.MyCon))
            {
                throw new InvalidOperationException("Connection string 'defaultConnectionbeta' is missing or invalid.");
            }
        }

        
    }
}

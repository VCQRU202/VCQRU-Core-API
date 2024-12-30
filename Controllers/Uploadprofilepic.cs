using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Uploadprofilepic : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;

        public Uploadprofilepic(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }
    }
}

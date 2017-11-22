using Oblivion.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Oblivion.Api.Controllers
{
    public class StatisticsController : ApiController
    {
        private OblivionContext _context;

        public StatisticsController()
        {
            _context = new OblivionContext();
        }

        // GET: api/Statistics
        [HttpGet]
        public ServerStatusModel Get()
        {
            ServerStatusModel serverStatus = _context.ServerStatus.FirstOrDefault();
            
            if (serverStatus == null)
                return default(ServerStatusModel);
            else
                return serverStatus;
        }
    }
}

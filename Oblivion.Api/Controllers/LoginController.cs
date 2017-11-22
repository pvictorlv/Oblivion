using Oblivion.Api.ContentModels;
using Oblivion.Api.Models;
using Oblivion.Api.RenderModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Oblivion.Api.Controllers
{
    public class LoginController : ApiController
    {
        private OblivionContext _context;

        public LoginController()
        {
            _context = new OblivionContext();
        }

        // GET: api/Login
        [HttpPost]
        public AuthRenderModel Post([FromBody] Authenticate authenticate)
        {
            var user = _context.Users.FirstOrDefault(s => s.Username == authenticate.Username);

            AuthRenderModel renderModel = new AuthRenderModel();
            renderModel.UniqueId = user.Id;
            renderModel.Name = user.Username;
            renderModel.Email = user.Email;
            renderModel.Look = user.Look;
            renderModel.Motto = user.Motto;

            if (renderModel == null)
                return default(AuthRenderModel);
            else
                return renderModel;
        }
    }
}

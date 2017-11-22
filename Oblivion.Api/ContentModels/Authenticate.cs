using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Oblivion.Api.ContentModels
{
    public class Authenticate
    {
        [JsonProperty("email")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Oblivion.Api.RenderModels
{
    public class AuthRenderModel
    {
        public int UniqueId { get; set; }
        public string Name { get; set; }
        public string Look { get; set; }
        public string Motto { get; set; }
        public bool BuildersClubMember { get; set; }
        public bool HabboClubMember { get; set; }
        public string LastWebAccess { get => "2017-11-11T16:20:00.000+0000"; }
        public string CreationTime { get => "2017-11-11T16:20:00.000+0000"; }
        public long SessionLogId { get => UniqueId; }
        public int LoginLogId { get => UniqueId; }
        public string Email { get; set; }
        public int IdentityId { get => UniqueId; }
        public bool EmailVerified { get => true; }
        public bool IdentityVerified { get => true; }
        public string IdentityType { get => "HABBO"; }
        public bool Trusted { get => true; }
        public string[] Force { get => new string[] { "HABBO" }; }
        public int AccountId { get => UniqueId; }
        public string Country { get => "br"; }
        public string[] Traits { get => new[] { "USER" }; }
        public string Partner { get => "NO_PARTNER"; }
    }
}
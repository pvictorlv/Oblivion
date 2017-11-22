using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Oblivion.Api.Models
{
    [Table("users")]
    public class UsersModel
    {
        [Key]
        public int Id { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        [Column("mail")]
        public string Email { get; set; }

        public string Look { get; set; }

        [Column("auth_ticket")]
        public string AuthToken { get; set; }

        public string Motto { get; set; }
    }
}
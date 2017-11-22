using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Oblivion.Api.Models
{
    [Table("server_status")]
    public class ServerStatusModel
    {
        [Key]
        public int Id { get; set; }

        public int Status { get; set; }

        [Column("users_online")]
        public int OnlineUsers { get; set; }

        [Column("rooms_loaded")]
        public int RoomsLoaded { get; set; }

        [Column("server_ver")]
        public string ServerVersion { get; set; }
    }
}
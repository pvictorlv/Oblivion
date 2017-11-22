using Oblivion.Api.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Oblivion.Api
{
    public class OblivionContext : DbContext
    {
        //MySql Database connection String
        public OblivionContext() : base("OblivionConnection") { }

        public virtual DbSet<ServerStatusModel> ServerStatus { get; set; }
        public virtual DbSet<UsersModel> Users { get; set; }
    }
}
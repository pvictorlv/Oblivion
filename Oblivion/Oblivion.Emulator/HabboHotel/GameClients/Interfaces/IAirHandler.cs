using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oblivion.Encryption.Encryption.Hurlant.Crypto.Prng;

namespace Oblivion.HabboHotel.GameClients.Interfaces
{
    public interface IAirHandler
    {
        bool IsAir { get; set; }
        ARC4 ServerRc4 { get; set; }
    }
}

using System.Threading.Tasks;

namespace Oblivion.HabboHotel.Items.Wired.Interfaces
{
    public interface IWiredCycler
    {
        double TickCount { get; set; }
        Task<bool> OnCycle();

        
    }
}
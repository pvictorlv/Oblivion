namespace Oblivion.HabboHotel.Items.Wired.Interfaces
{
    public interface IWiredCycler
    {
        double TickCount { get; set; }
        bool OnCycle();

        
    }
}
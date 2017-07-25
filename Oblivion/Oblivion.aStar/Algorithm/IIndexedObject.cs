namespace Oblivion.AStar.Algorithm
{
    public interface IWeightAlterable<T>
    {
        T Weight { get; set; }
    }
}
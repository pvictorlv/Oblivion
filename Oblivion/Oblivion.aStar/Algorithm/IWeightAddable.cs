namespace Oblivion.AStar.Algorithm
{
    public interface IWeightAddable<T>
    {
        T WeightChange { get; set; }
    }
}
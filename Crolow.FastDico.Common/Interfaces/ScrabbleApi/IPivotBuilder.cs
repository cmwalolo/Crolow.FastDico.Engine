
namespace Crolow.FastDico.ScrabbleApi.Components.BoardSolvers;

public interface IPivotBuilder
{
    void Build();
    void Build(int grid, int targetGrid);
    uint[] GetMask(int x, int y, int direction);
    List<byte[]> SearchByPattern(byte[] bytePattern);
}
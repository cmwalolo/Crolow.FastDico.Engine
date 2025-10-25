namespace Crolow.FastDico.Utils
{
    public static class ArrayUtils
    {
        public static T[,] Transpose<T>(T[,] array)
        {
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);

            T[,] transposed = new T[cols, rows];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    transposed[j, i] = array[i, j];
                }
            }

            return transposed;
        }
    }
}

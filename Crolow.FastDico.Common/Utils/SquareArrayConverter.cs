using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Crolow.FastDico.Common.Utils
{
    public class MultiDimensionalArrayConverter : JsonConverter<Square[,]>
    {
        public override Square[,] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var jaggedArray = JsonSerializer.Deserialize<Square[][]>(ref reader, options);
            return ArrayConversionHelper.ToMultiDimensionalArray(jaggedArray);
        }

        public override void Write(Utf8JsonWriter writer, Square[,] value, JsonSerializerOptions options)
        {
            var jaggedArray = ArrayConversionHelper.ToJaggedArray(value);
            JsonSerializer.Serialize(writer, jaggedArray, options);
        }

        public static class ArrayConversionHelper
        {
            public static Square[][] ToJaggedArray(Square[,] multiDimensionalArray)
            {
                int rows = multiDimensionalArray.GetLength(0);
                int cols = multiDimensionalArray.GetLength(1);
                var jaggedArray = new Square[rows][];

                for (int i = 0; i < rows; i++)
                {
                    jaggedArray[i] = new Square[cols];
                    for (int j = 0; j < cols; j++)
                    {
                        jaggedArray[i][j] = multiDimensionalArray[i, j];
                    }
                }

                return jaggedArray;
            }

            public static Square[,] ToMultiDimensionalArray(Square[][] jaggedArray)
            {
                int rows = jaggedArray.Length;
                int cols = jaggedArray[0].Length;
                var multiDimensionalArray = new Square[rows, cols];

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        multiDimensionalArray[i, j] = jaggedArray[i][j];
                    }
                }

                return multiDimensionalArray;
            }
        }
    }
}
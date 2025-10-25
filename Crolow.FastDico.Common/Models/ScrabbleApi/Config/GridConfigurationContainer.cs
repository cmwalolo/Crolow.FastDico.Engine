using Crolow.FastDico.Common.Models.ScrabbleApi.Game;

namespace Crolow.FastDico.ScrabbleApi.Config
{
    public class GridConfigurationContainer
    {
        public int SizeH { get; set; }
        public int SizeV { get; set; }
        public Square[,] Grid { get; set; }

        public GridConfigurationContainer()
        {
        }

        public GridConfigurationContainer(GridConfigurationContainer config)
        {
            SizeH = config.SizeH;
            SizeV = config.SizeV;
            Grid = new Square[SizeH, SizeV];
            for (int i = 0; i < SizeH; i++)
            {
                for (int j = 0; j < SizeV; j++)
                {
                    Grid[i, j] = new Square
                    {
                        IsBorder = config.Grid[i, j].IsBorder,
                        LetterMultiplier = config.Grid[i, j].LetterMultiplier,
                        WordMultiplier = config.Grid[i, j].WordMultiplier
                    };
                }
            }
        }

        public GridConfigurationContainer(int sizeH, int sizeV)
        {
            SizeH = sizeH + 2;
            SizeV = sizeV + 2;
            Grid = new Square[SizeH, SizeV];

            for (int x = 0; x < SizeH; x++)
            {
                for (int y = 0; y < SizeV; y++)
                {
                    var isBorder = x == 0 || y == 0 || x == SizeH - 1 || y == SizeV - 1;
                    Grid[x, y] = new Square { IsBorder = isBorder };
                }
            }

        }
    }
}

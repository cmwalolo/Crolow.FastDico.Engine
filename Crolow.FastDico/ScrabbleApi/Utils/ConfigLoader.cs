using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Common.Models.ScrabbleApi;
using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.ScrabbleApi.Config;
using Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi;

namespace Crolow.FastDico.ScrabbleApi.Utils
{
    public class ConfigLoader
    {
        public class BoardData
        {
            public IBoardGridModel Grid { get; set; }
        }

        public PlayConfiguration ReadConfiguration(ToppingConfigurationContainer config, IDictionaryContainer dictionaryContainer)
        {
            PlayConfiguration pc = new PlayConfiguration();
            pc.SelectedConfig = config.GameConfig;
            FillGridConfig(config, pc, dictionaryContainer);
            return pc;
        }

        public static TileConfiguration ReadLetterConfig(ILetterConfigModel letterData)
        {
            var config = new TileConfiguration();

            config.Name = letterData.Name;
            foreach (var letter in letterData.Letters)
            {
                config.LettersByByte.Add(letter.Letter, letter);
                config.LettersByChar.Add(letter.Char[0], letter);
            }
            return config;
        }

        public static PlayConfiguration FillGridConfig(ToppingConfigurationContainer config, PlayConfiguration pc, IDictionaryContainer dictionaryContainer)
        {
            pc.TileConfig = ReadLetterConfig(dictionaryContainer.LetterConfig);
            pc.GridConfig = new GridConfigurationContainer(config.BoardGrid.SizeH, config.BoardGrid.SizeV);
            var boardData = config.BoardGrid;
            int sizeH = pc.GridConfig.SizeH;
            var grid = pc.GridConfig.Grid;
            foreach (var multiplierData in boardData.Configuration)
            {
                foreach (var position in multiplierData.Positions)
                {
                    int row = position[0]; // Adjusting for zero-based index
                    int col = position[1]; // Adjusting for zero-based index

                    if (multiplierData.Multiplier > 0)
                    {
                        grid[row, col].LetterMultiplier = multiplierData.Multiplier;
                    }
                    else
                    {
                        grid[row, col].WordMultiplier = Math.Abs(multiplierData.Multiplier);
                    }
                }
            }

            return pc;
        }
    }
}

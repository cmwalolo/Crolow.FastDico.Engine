using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Common.Models.ScrabbleApi;
using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.ScrabbleApi.Config;
using Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi;

namespace Crolow.FastDico.ScrabbleApi.Utils
{
    /// <summary>
    /// Builds play, grid, and tile configurations from top-machine configuration models.
    /// </summary>
    public class ConfigLoader
    {
        /// <summary>
        /// Contains board-grid data loaded from configuration.
        /// </summary>
        public class BoardData
        {
            /// <summary>
            /// Gets or sets the board grid model.
            /// </summary>
            public IBoardGridModel Grid { get; set; }
        }

        /// <summary>
        /// Reads the play configuration for a game.
        /// </summary>
        /// <param name="config">Topping configuration container to read.</param>
        /// <param name="dictionaryContainer">Dictionary container that supplies the letter configuration.</param>
        /// <returns>A populated play configuration.</returns>
        public PlayConfiguration ReadConfiguration(ToppingConfigurationContainer config, IDictionaryContainer dictionaryContainer)
        {
            PlayConfiguration pc = new PlayConfiguration();
            pc.SelectedConfig = config.GameConfig;
            FillGridConfig(config, pc, dictionaryContainer);
            return pc;
        }

        /// <summary>
        /// Converts a letter configuration model into the runtime tile configuration maps.
        /// </summary>
        /// <param name="letterData">Letter configuration model to convert.</param>
        /// <returns>A tile configuration indexed by byte and character.</returns>
        public static TilesConfiguration ReadLetterConfig(ILetterConfigModel letterData)
        {
            var config = new TilesConfiguration();

            config.Name = letterData.Name;
            foreach (var letter in letterData.Letters)
            {
                config.LettersByByte.Add(letter.Letter, letter);
                config.LettersByChar.Add(letter.Char[0], letter);
            }
            return config;
        }

        /// <summary>
        /// Fills a play configuration with grid and tile configuration data.
        /// </summary>
        /// <param name="config">Topping configuration container that includes board setup.</param>
        /// <param name="pc">Play configuration to populate.</param>
        /// <param name="dictionaryContainer">Dictionary container that supplies the letter configuration.</param>
        /// <returns>The populated play configuration.</returns>
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

using Crolow.FastDico.Builders.TopMachine;
using Crolow.FastDico.Common.Interfaces.ScrabbleApi;
using Crolow.FastDico.Common.Interfaces.Settings;
using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi;
using Crolow.FastDico.ScrabbleApi.Extensions;
using Crolow.TopMachine.Core.Client.Facades;
using System.Text;

namespace Crolow.FastDico.ScrabbleApi;

public class ScrabbleAIViewer : IScrabbleAIViewer
{
    private CurrentGame CurrentGame;
    private IGameDetailModel currentLoadedGame;
    private List<IGameUserDetailModel> currentGameHistory;
    private readonly ITopMachineSetting topMachineSettings;
    private readonly IGameSerializer gameSerializer;
    private readonly IServiceFacadeSwitcher facade;
    public ScrabbleAIViewer(IGameDetailModel detailModel, CurrentGame currentGame, ITopMachineSetting iTopMachineSettings, IGameSerializer gameSerializer, IServiceFacadeSwitcher facade)
    {
        this.CurrentGame = currentGame;
        this.currentLoadedGame = detailModel;
        this.topMachineSettings = iTopMachineSettings;
        this.gameSerializer = gameSerializer;
        this.facade = facade;
    }
    public async void SerializeGame()
    {
        currentLoadedGame = gameSerializer.GetGame(CurrentGame);
        facade.Current.GameService.UpdateGameAsync(currentLoadedGame);
    }

    public async void SerializeUser()
    {
        var userModel = gameSerializer.GetGameUser(currentLoadedGame, CurrentGame);
        facade.Current.GameService.UpdateUserGameAsync(currentLoadedGame, userModel);
        CurrentGame.History.Users.Add(userModel);
    }

    public void ExportHtml(bool doConsole = true)
    {
        var tilesUtils = CurrentGame.ControllersSetup.DictionaryContainer.TilesUtils;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<link rel=\"stylesheet\" href=\"../grid.css\" />");
        sb.AppendLine("</head>");

        sb.AppendLine("<body><div id='grid'>");

        sb.AppendLine("<table class='results' style='float:right'>");
        sb.AppendLine("<tr><th>#</th><th>Rack</th><th>Word</th><th>pos</th><th>pts</th></tr>");
        int ndx = 1;
        foreach (var r in CurrentGame.GameObjects.Rounds.Rounds)
        {
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td>{ndx++}</td>");
            sb.AppendLine($"<td>{r.Rack.GetString(tilesUtils)}</td>");
            sb.AppendLine($"<td>{r.GetWord(tilesUtils, true)}</td>");
            sb.AppendLine($"<td>{r.GetPosition()}</td>");
            sb.AppendLine($"<td>{r.Points}</td>");
            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</table>");

        sb.AppendLine("<table class='board'>");
        sb.AppendLine("<tr><td></td>");
        for (int col = 1; col < CurrentGame.GameObjects.Board.CurrentBoard[0].SizeH - 1; col++)
        {
            sb.AppendLine($"<td class='border'>{col}</td>");
        }
        sb.AppendLine("</tr>");

        for (int y = 1; y < CurrentGame.GameObjects.Board.CurrentBoard[0].SizeH - 1; y++)
        {
            var cc = ((char)(y + 64));
            sb.AppendLine($"<tr><td class='border'>{cc}</td>");
            for (int x = 1; x < CurrentGame.GameObjects.Board.CurrentBoard[0].SizeV - 1; x++)
            {
                var sq = CurrentGame.GameObjects.Board.GetSquare(0, x, y);
                var cclass = $"cell cell-{sq.LetterMultiplier} cell{sq.WordMultiplier}";

                if (sq.Status == 1)
                {
                    cclass += sq.CurrentLetter.IsJoker ? " tileJoker" : " tile";
                }

                sb.AppendLine($"<td class='{cclass}'>");
                if (sq.Status == 1)
                {
                    var c = (char)(sq.CurrentLetter.Letter + 97);
                    sb.AppendLine(char.ToUpper(c).ToString());
                }
                sb.AppendLine("</td>");
            }
            sb.AppendLine("</tr>");
        }
        sb.AppendLine("</table>");

        sb.AppendLine("</grid>");
        sb.AppendLine("</body></html>");

        var path = $"{topMachineSettings.OutputFolderPath}\\Html";

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        var dm = DateTime.Now;
        System.IO.File.WriteAllText($"{path}\\output-{dm.ToString("yyMMdd-hhmmss")}.html", sb.ToString());
        // We are done 
    }
    public PlayedRounds ReplayRound(int round, int max = 100000)
    {
        var b = CurrentGame.GameObjects.Board;
        b.ClearGrid();

        CurrentGame.GameObjects.Round = round;

        for (int x = 0; x < round; x++)
        {
            b.SetRound(CurrentGame, CurrentGame.GameObjects.Rounds.Rounds[x]);
        }

        var r = CurrentGame.GameObjects.Rounds.Rounds[round];

        CurrentGame.ControllersSetup.BoardSolver.Initialize();
        CurrentGame.ControllersSetup.Validator.Initialize();

        var originalRack = new PlayerRack(r.Rack.Tiles);

        CurrentGame.ControllersSetup.Validator.InitializeRound();
        var letters = originalRack.GetTiles().ToList();

        var filters = CurrentGame.ControllersSetup.Validator.InitializeFilters(true);
        var playedRounds = CurrentGame.ControllersSetup.BoardSolver.Solve(letters, filters);

        var rounds = playedRounds.AllRounds.GetBest().OrderByDescending(p => p.Points).Take(max).ToList(); ;
        rounds.ForEach(p => p.FinalizeRound());
        try
        {
            CurrentGame.GameObjects.SelectedRound = CurrentGame.ControllersSetup.Validator.FinalizeRound(playedRounds);
        }
        catch (Exception)
        {

            throw;
        }

        return playedRounds;
    }
}
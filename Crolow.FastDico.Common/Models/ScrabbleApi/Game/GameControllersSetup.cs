using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Common.Interfaces.ScrabbleApi;
using Crolow.FastDico.ScrabbleApi.Components.BoardSolvers;

namespace Crolow.FastDico.Common.Models.ScrabbleApi.Game;

public class GameControllersSetup
{
    public IPivotBuilder PivotBuilder;
    public IBoardSolver BoardSolver;
    public IBaseRoundValidator Validator;
    public IScrabbleAI ScrabbleEngine;
    public IScrabbleAIViewer ScrabbleViewEngine;
    public IDictionaryContainer DictionaryContainer;
    public IDictionaryContainer ReferenceDictionaryContainer;
}

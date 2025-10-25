namespace Crolow.FastDico.Common.Interfaces.Dictionaries;

public interface IDawgSearch
{

    ILetterNode Root { get; }
    List<string> SearchAllWords(int minLength, int maxLength);

    List<string> SearchByPattern(string pattern, int minLength = int.MinValue, int maxLength = int.MaxValue);
    List<string> SearchByPrefix(string prefix, int maxLength = int.MaxValue);
    List<string> SearchBySuffix(string suffix, int maxLength = int.MaxValue);
    List<string> FindAllWordsFromLetters(string pattern);
    List<string> FindAllWordsContainingLetters(string pattern);
    bool SearchWord(string word);
    bool SearchWord(List<byte> word);
}

using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Common.Interfaces.Lists;
using Crolow.FastDico.Common.Interfaces.ScrabbleApi.Services;
using Crolow.FastDico.Common.Interfaces.Settings;
using Crolow.FastDico.GadDag;
using Crolow.TopMachine.Core.Entities.Lists;
using Crolow.TopMachine.Data.Bridge.Entities.Definitions;
using Crolow.TopMachine.Data.Bridge.Entities.Lists;
using Crolow.TopMachine.Data.Bridge.Enums;
using Kalow.Apps.Common.DataTypes;
using System.Text.RegularExpressions;

namespace Crolow.TopMachine.Builders.Lists
{
    public class ListBuilderService : IListBuilderService
    {
        protected IServiceFacadeSwitcher facade;
        protected GadDagSearch searcher;

        public ListBuilderService(IServiceFacadeSwitcher facade, ITopMachineSetting config)
        {
            this.facade = facade;
        }
        public async Task BuildAsync(IListConfigurationModel config, IDictionaryContainer dicoContainer, IDictionaryContainer dicoContainerRef)
        {
            if (config.IsExtensionList)
            {
                await BuildExtensionsListAsync(config, dicoContainer, dicoContainerRef);
            }
            else
            {
                await BuildNormalListAsync(config, dicoContainer);
            }
        }

        private async Task BuildExtensionsListAsync(IListConfigurationModel config, IDictionaryContainer dicoContainer, IDictionaryContainer dicoContainerRef)
        {
            var dico = (await facade.Current.DictionaryService.LoadAllAsync()).First(p => p.Id == config.DictionaryId);
            var tileConfig = (await facade.Current.LetterService.LoadAllAsync()).First(p => p.Name == dico.Language);
            var searcher = dicoContainer.Searcher;//  new GadDagSearch(dicoContainer.Result.Dico.Root, dicoContainer.Result.TilesUtils);
            var searcherRef = dicoContainerRef.Searcher;//  new GadDagSearch(dicoContainer.Result.Dico.Root, dicoContainer.Result.TilesUtils);

            var allWords = await facade.Current.DictionaryService.GetAllWordsAsync(config.Language);

            var baseList = allWords
                .Where(word => word.Word.StartsWith("ajuste") && word.Word.Length >= config.MinWordLength && word.Word.Length <= config.MaxWordLength).OrderBy(p => p.WordType).ToList();

            var lookupSrc = new HashSet<string>(searcher.SearchAllWords(0, 15));
            var lookup = new HashSet<string>(searcherRef.SearchAllWords(0, 15));

            var refinedList = new List<IWordToDicoModel>();
            var results = new Dictionary<KalowId, List<(IWordToDicoModel, IWordToDicoModel)>>();

            foreach (var word in baseList)
            {
                if (word.Word.Length < config.MinWordLength || word.Word.Length > config.MaxWordLength)
                {
                    continue;
                }

                if (!lookupSrc.Contains(word.Word.ToUpper()))
                {
                    continue;
                }

                if (!CheckIfAllLetterIsInWord(word.Word, config.MandatoryLetters)
                    || !CheckIfAnyLetterIsInWord(word.Word, config.LettersInRack)
                    || !CheckIfNoneLetterIsInWord(word.Word, config.ExcludedLetters)
                    )
                {
                    continue;
                }

                if (!CheckCriteria(word.Word, config))
                {
                    continue;
                }

                var extensions = allWords.Where(p => p.Parent != word.Parent && p.Word.Contains(word.Word)).OrderBy(p => p.WordType);
                var accepted = new List<IWordToDicoModel>();

                foreach (var extension in extensions)
                {
                    if (lookup.Contains(extension.Word.ToUpper()))
                    {
                        var (before, after) = FindLimits(extension.Word, word.Word);
                        if (
                            before <= config.BeforeExtensionLetters
                            && before >= config.MinBeforeExtensionLetters
                            && after <= config.AfterExtensionLetters
                            && after >= config.MinAfterExtensionLetters
                            )
                        {
                            accepted.Add(extension);
                        }
                    }

                }

                if (accepted.Count > 0)
                {
                    List<(IWordToDicoModel, IWordToDicoModel)> acceptedList = new List<(IWordToDicoModel, IWordToDicoModel)>();

                    if (!results.ContainsKey(word.Parent))
                    {
                        acceptedList = new List<(IWordToDicoModel, IWordToDicoModel)>();
                        results.Add(word.Parent, acceptedList);
                    }
                    else
                    {
                        acceptedList = results[word.Parent];
                    }

                    foreach (var accept in accepted)
                    {
                        if (!acceptedList.Any(p => p.Item2.Parent == accept.Parent))
                        {
                            acceptedList.Add((word, accept));
                        }
                    }
                }
            }

            List<IListItemModel> newItems = new List<IListItemModel>();

            foreach (var result in results)
            {
                var groups = result.Value.GroupBy(p => p.Item1.Word);
                foreach (var group in groups)
                {
                    ListItemModel item = new ListItemModel();
                    item.ListId = config.Id;
                    item.Id = KalowId.NewObjectId();
                    item.Rack = group.First().Item1.Word.ToUpper();

                    foreach (var solution in group)
                    {
                        ListSolutionModel solutionModel = new ListSolutionModel();
                        solutionModel.Solution = solution.Item2.Word.ToUpper();
                        solutionModel.Suffix = ExtractSuffix(solution.Item2.Word, solution.Item1.Word).ToUpper();
                        solutionModel.Prefix = ExtractPrefix(solution.Item2.Word, solution.Item1.Word).ToUpper();
                        solutionModel.SuffixLength = solutionModel.Suffix.Length;
                        solutionModel.PrefixLength = solutionModel.Prefix.Length;
                        item.Solutions.Add(solutionModel);
                    }

                    if (item.Solutions.Count < config.MinPossilibies || item.Solutions.Count > config.MaxPossilibies)
                    {
                        continue;
                    }

                    item.EditState = Data.Bridge.EditState.New;
                    newItems.Add(item);
                }
            }

            await facade.Current.ListConfigService.UpdateItemsAsync(newItems);

            config.Stats = new ListStats
            {
                Count = newItems.Count(p => p.EditState != Data.Bridge.EditState.ToDelete),
                Found = 0,
                NotFound = 0,
                Isolated = 0
            };

            config.EditState = Data.Bridge.EditState.Update;
            await facade.Current.ListConfigService.UpdateListConfigAsync(config);
        }

        private string ExtractPrefix(string word1, string word2)
        {
            if (string.IsNullOrEmpty(word1) || string.IsNullOrEmpty(word2))
                return string.Empty;

            int index = word1.IndexOf(word2, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                return string.Empty;

            return word1.Substring(0, index);
        }

        private string ExtractSuffix(string word1, string word2)
        {
            if (string.IsNullOrEmpty(word1) || string.IsNullOrEmpty(word2))
                return string.Empty;

            int index = word1.IndexOf(word2, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                return string.Empty;

            return word1.Substring(index + word2.Length);
        }

        private (int before, int after) FindLimits(string word, string pattern)
        {
            int index = word.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);

            if (index < 0)
                return (0, 0);

            int before = index;
            int after = word.Length - (index + pattern.Length);
            return (before, after);
        }


        private async Task BuildNormalListAsync(IListConfigurationModel config, IDictionaryContainer dicoContainer)
        {
            var dico = (await facade.Current.DictionaryService.LoadAllAsync()).First(p => p.Id == config.DictionaryId);
            var tileConfig = (await facade.Current.LetterService.LoadAllAsync()).First(p => p.Name == dico.Language);


            searcher = new GadDagSearch(dicoContainer.Dico.Root, dicoContainer.TilesUtils);
            var words = searcher.SearchAllWords(config.MinWordLength, config.MaxWordLength);

            Dictionary<string, List<IListSolutionModel>> listOfWords = new Dictionary<string, List<IListSolutionModel>>();

            foreach (var word in words)
            {
                // We first check if the word length is within the limits.
                if (word.Length < config.MinWordLength || word.Length > config.MaxWordLength)
                {
                    continue;
                }

                if (!CheckIfAllLetterIsInWord(word, config.MandatoryLetters)
                    || !CheckIfAnyLetterIsInWord(word, config.LettersInRack)
                    || !CheckIfNoneLetterIsInWord(word, config.ExcludedLetters)
                    )
                {
                    continue;
                }

                if (!CheckCriteria(word, config))
                {
                    continue;
                }


                var racks = new List<string>();
                if (config.UseJokers)
                {

                    var l = GetSortedCombinations(word, config.LetterForJokers).Select(p => p + "?");
                    if (l.Count() > 0)
                    {
                        racks.AddRange(l);
                    }
                }
                else
                {
                    racks.Add(OrderByLetter(word));

                }

                foreach (var rack in racks)
                {
                    var list = new List<IListSolutionModel>();

                    if (listOfWords.ContainsKey(rack))
                    {
                        list = listOfWords[rack];
                    }
                    else
                    {
                        listOfWords.Add(rack, list);
                    }

                    list.Add(new ListSolutionModel
                    {
                        Solution = word
                    });
                }
            }

            List<IListItemModel> newItems = new List<IListItemModel>();

            var items = await facade.Current.ListConfigService.LoadItemsAsync(config);

            foreach (var i in items)
            {
                i.EditState = Data.Bridge.EditState.ToDelete;
            }

            foreach (var key in listOfWords.Keys)
            {

                var list = listOfWords[key];
                var i = items.FirstOrDefault(p => p.Rack == key);

                if (list.Count < config.MinPossilibies || list.Count > config.MaxPossilibies)
                {
                    continue;
                }
                else
                {
                    var k = listOfWords[key];
                    if (i != null)
                    {
                        if (!i.Solutions.SequenceEqual(k))
                        {
                            i.Solutions = k;
                            i.Status = StatusOfListItem.NotPlayed;
                            i.EditState = Data.Bridge.EditState.Update;
                        }
                        else
                        {
                            i.EditState = Data.Bridge.EditState.Unchanged;
                        }
                    }
                    else
                    {
                        newItems.Add(new ListItemModel
                        {
                            Id = KalowId.NewObjectId(),
                            ListId = config.Id,
                            Rack = key,
                            Solutions = k,
                            Status = StatusOfListItem.NotPlayed,
                            FoundCount = 0,
                            NotFoundCount = 0,
                            FoundInRowCount = 0,
                            EditState = Data.Bridge.EditState.New
                        });
                    }
                }
            }

            items.AddRange(newItems);
            await facade.Current.ListConfigService.UpdateItemsAsync(items);

            config.Stats = new ListStats
            {
                Count = items.Count(p => p.EditState != Data.Bridge.EditState.ToDelete),
                Found = 0,
                NotFound = 0,
                Isolated = 0
            };

            config.EditState = Data.Bridge.EditState.Update;
            await facade.Current.ListConfigService.UpdateListConfigAsync(config);

        }

        private bool CheckCriteria(string word, IListConfigurationModel config)
        {
            if (config.Criteria.Count == 0) return true;
            bool matchAll = config.MatchAllCriteria && config.Criteria.Count > 1;
            bool ok = matchAll;

            foreach (var c in config.Criteria)
            {
                bool resultInclusion = false;
                bool resultExclusion = false;
                switch (c.InclusionCriteria)
                {
                    case TypeOfCriteria.StartsWith:
                        {
                            resultInclusion |= word.StartsWith(c.InclusionParameter);
                            break;
                        }
                    case TypeOfCriteria.EndsWith:
                        {
                            resultInclusion |= word.EndsWith(c.InclusionParameter);
                            break;
                        }

                    case TypeOfCriteria.Contains:
                        {
                            resultInclusion |= word.Contains(c.InclusionParameter);
                            break;
                        }

                    case TypeOfCriteria.Regex:
                        {
                            resultInclusion |= Regex.IsMatch(word, c.InclusionParameter);
                            break;
                        }
                    default:
                        break;
                }

                switch (c.ExclusionCriteria)
                {
                    case TypeOfCriteria.StartsWith:
                        {
                            resultExclusion |= !word.StartsWith(c.ExclusionParameter);
                            break;
                        }
                    case TypeOfCriteria.EndsWith:
                        {
                            resultExclusion |= !word.EndsWith(c.InclusionParameter);
                            break;
                        }

                    case TypeOfCriteria.Contains:
                        {
                            resultExclusion |= !word.Contains(c.ExclusionParameter);
                            break;
                        }

                    case TypeOfCriteria.Regex:
                        {
                            resultExclusion |= !Regex.IsMatch(word, c.ExclusionParameter);
                            break;
                        }
                    default:
                        break;

                }

                var result = resultInclusion || resultExclusion;

                if (resultInclusion == true)
                {
                    Console.Write("ok");
                }

                if (matchAll)
                    ok = ok && result;
                else
                    ok = ok || result;
            }

            return ok;
        }

        private string OrderByLetter(string word)
        {
            var rack = word.Select(p => p)
                .OrderBy(letter => letter).ToArray();
            return new string(rack);
        }

        private bool CheckIfAnyLetterIsInWord(string word, string letters)
        {
            return string.IsNullOrEmpty(letters) ? true : letters.Any(letter => word.Contains(letter));
        }

        // Returns true if none of the letters from 'letters' are in 'word'
        private bool CheckIfNoneLetterIsInWord(string word, string letters)
        {
            return string.IsNullOrEmpty(letters) ? true : letters.All(letter => !word.Contains(letter));
        }

        // Returns true if all letters from 'letters' are in 'word'
        private bool CheckIfAllLetterIsInWord(string word, string letters)
        {
            return string.IsNullOrEmpty(letters) ? true : letters.All(letter => word.Contains(letter));
        }

        static List<string> GetSortedCombinations(string word, string excludedLetters)
        {
            if (string.IsNullOrEmpty(word))
                return new List<string>();

            // Sorted letters of the word
            var sorted = new string(word.ToUpper().OrderBy(c => c).ToArray());

            HashSet<char> excludedSet = null;
            if (!string.IsNullOrEmpty(excludedLetters))
                excludedSet = new HashSet<char>(excludedLetters.ToUpper());

            var results = new HashSet<string>();

            for (int i = 0; i < sorted.Length; i++)
            {
                char c = sorted[i];

                // If exclusions are provided, only allow removing letters in that set
                if (excludedSet != null && !excludedSet.Contains(c))
                    continue;

                // Skip duplicate removals (removing any identical letter occurrence would produce the same result)
                if (i > 0 && sorted[i] == sorted[i - 1])
                    continue;

                // Build the combination by removing the char at i
                string combo = sorted.Remove(i, 1); // result length = word.Length - 1
                results.Add(combo);
            }

            return results.OrderBy(s => s).ToList();
        }


    }
}

using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Common.Interfaces.Settings;
using Crolow.FastDico.GadDag;
using Crolow.TopMachine.Core.Service.Services.Dictionary;
using Crolow.TopMachine.Data.Bridge;
using Crolow.TopMachine.Data.Bridge.Entities.Definitions;
using Crolow.TopMachine.Data.Bridge.Enums;
using Kalow.Apps.Common.DataTypes;

namespace Crolow.TopMachine.Builders.Dictionaries
{
    public class DictionaryBuilderService : IDictionaryBuilderService
    {
        protected ITopMachineSetting config;
        public DictionaryBuilderService(ITopMachineSetting topMachineSetting)
        {
            this.config = topMachineSetting;
        }
        public void GenerateDictionary(IDictionaryContainer dictionaryContainer, List<IWordEntryModel> entries, List<IWordToDicoModel> words, string outputFilename, bool doEntries, bool doNocNop, bool doNoc)
        {
            List<IWordToDicoModel> thisWords = new List<IWordToDicoModel>();


            if (doEntries)
            {
                thisWords.AddRange(words.Where(w => w.WordType == WordType.Entry));
            }
            else if (doNocNop)
            {
                thisWords.AddRange(words.Where(w => !w.WordType.HasFlag(WordType.Conjugation) && !w.WordType.HasFlag(WordType.Plural)));
            }
            else if (doNoc)
            {
                thisWords.AddRange(words.Where(w => !w.WordType.HasFlag(WordType.Conjugation)));
            }
            else
            {
                thisWords.AddRange(words.ToList());
            }

            var entryIds = new HashSet<KalowId>(entries.Where(e => e != null).Select(e => e.Id));
            var wordList = thisWords.Where(w => entryIds.Contains(w.Parent)).Select(w => w.Word).Distinct().ToList();

            //var l = wordList.Where(p => p.StartsWith("aa"));

            GadDagDictionary dictionary = new GadDagDictionary(dictionaryContainer.TilesUtils);

            dictionary.Build(wordList);

            var path = config.AppDataFolderPath + "\\" + outputFilename;
            dictionary.SaveToFile(path);
        }

        public void ApplyTypes(List<IWordEntryModel> entries, List<IWordToDicoModel> words)
        {

            var batches = entries
                .Where(p => p.NormalizedWord.Equals("ouir"))
                .Select((item, index) => new { item, index })
                .GroupBy(x => x.index / 100)
                .Select(g => g.Select(x => x.item).ToList())
                .ToList();

            var options = new ParallelOptions();
            options.MaxDegreeOfParallelism = 1;

            Parallel.ForEach(batches, options, batch =>
            {
                var thisWords = new List<IWordToDicoModel>();
                foreach (var entry in batch)
                {
                    bool isVerb = entry.Definitions.Any(d => d.CatGram.StartsWith("verbe", StringComparison.InvariantCultureIgnoreCase));

                    if (entry.NormalizedWord == "ouir") isVerb = true;

                    bool isNotVerb = entry.Definitions.Any(d => !d.CatGram.StartsWith("verbe", StringComparison.InvariantCultureIgnoreCase));
                    thisWords = words.Where(w => w.Parent == entry.Id).ToList();


                    if (entry.Source.Equals("external") && thisWords.Count > 5) { isVerb = true; }

                    foreach (var word in thisWords)
                    {
                        word.WordType = WordType.Unknown;
                        if (word.Word.Equals(entry.NormalizedWord))
                        {
                            word.WordType = WordType.Entry;
                            word.EditState = EditState.Update;
                            continue;
                        }
                        else
                        {
                            if (isNotVerb)
                            {
                                if (isVerb)
                                {
                                    if (word.Word.Equals(entry.NormalizedWord + "s"))
                                    {
                                        word.WordType = WordType.Plural;
                                        word.EditState = EditState.Update;
                                    }
                                }

                                else
                                {
                                    word.EditState = EditState.Update;
                                    if (!word.Word.EndsWith("s", StringComparison.InvariantCultureIgnoreCase) && !word.Word.EndsWith("x", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        word.WordType = WordType.Variant;
                                        word.EditState = EditState.Update;
                                    }
                                    else
                                    {
                                        word.WordType = WordType.Plural;
                                        word.EditState = EditState.Update;
                                    }
                                }
                            }
                        }
                    }
                    if (isVerb)
                    {
                        CheckVerbTypes(entry, thisWords.Where(p => p.EditState != EditState.Update).ToList());
                    }
                }

            });
        }

        private void CheckVerbTypes(IWordEntryModel entry, List<IWordToDicoModel> thisWords)
        {
            var verbTypesCheck = new List<Tuple<string, List<string>, List<string>>>
            {
                new Tuple<string, List<string>, List<string>>
                (   "resoudre",
                    new List<string> { "resolu" },
                    new List<string> { "resolu", "resolue", "resolus", "resolues" }
                ),

                new Tuple<string, List<string>, List<string>>
                (   "faire",
                    new List<string> { "fait" },
                    new List<string> { "fait", "faite", "faits", "faites" }
                ),

                new Tuple<string, List<string>, List<string>>
                (   "soudre",
                    new List<string> { "sout" },
                    new List<string> { "sout", "soute", "souts", "soutes" }
                ),

                new Tuple<string, List<string>, List<string>>
                (   "nuire",
                    new List<string> { "nui" },
                    new List<string> { "nui", "nuie", "nuis", "nuies" }
                ),

                new Tuple<string, List<string>, List<string>>
                (   "uire",
                    new List<string> { "uit" },
                    new List<string> { "uit", "uite", "uits", "uites" }
                ),
                new Tuple<string, List<string>, List<string>>
                (   "naitre",
                    new List<string> { "ne" },
                    new List<string> { "ne", "nee", "nes", "nees" }
                ),
                new Tuple<string, List<string>, List<string>>
                (   "aitre",
                    new List<string> { "u" },
                    new List<string> { "u", "ue", "ues", "us" }
                ),
                new Tuple<string, List<string>, List<string>>
                (   "enir",
                    new List<string> { "enu" },
                    new List<string> { "enu", "enue", "enues", "enus" }
                ),
                new Tuple<string, List<string>, List<string>>
                (   "avoir",
                    new List<string> { "eu" },
                    new List<string> { "eu", "eue", "eues", "eus" }
                ),
                new Tuple<string, List<string>, List<string>>
                (   "etre",
                    new List<string> { "ete" },
                    new List<string> { "ete", "etee", "etes", "etees" }
                ),
                new Tuple<string, List<string>, List<string>>
                (   "eoir",
                    new List<string> { "is" },
                    new List<string> { "is", "ise", "ises", }
                ),
                new Tuple<string, List<string>, List<string>>
                (   "indre",
                    new List<string> { "int" },
                    new List<string> { "int", "inte", "ints", "intes" }
                ),
                new Tuple<string, List<string>, List<string>>
                (   "prendre",
                    new List<string> { "pris" },
                    new List<string> { "pris", "prise", "rises"}
                ),
                new Tuple<string, List<string>, List<string>>
                (   "oitre",
                    new List<string> { "u" },
                    new List<string> { "u", "ue", "ues", "us" }
                ),
                new Tuple<string, List<string>, List<string>>
                (   "evoir",
                    new List<string> { "u" },
                    new List<string> { "u", "ue", "us", "ues" }
                ),
                new Tuple<string, List<string>, List<string>>
                (   "alloir",
                    new List<string> { "allu" },
                    new List<string> { "allu", "allue", "allus", "allues" }
                ),
                new Tuple<string, List<string>, List<string>>
                (   "euvoir",
                    new List<string> { "u" },
                    new List<string> { "u", "ue", "us", "ues" }
                ),
                new Tuple<string, List<string>, List<string>>
                (   "ouvoir",
                    new List<string> { "u" },
                    new List<string> { "u", "ue", "us", "ues" }
                ),
                new Tuple<string, List<string>, List<string>>
                (   "ger",
                    new List<string> { "ge" },
                    new List<string> { "ge", "gee", "ges", "gees" }
                ),
                new Tuple<string, List<string>, List<string>>
                (   "yer",
                    new List<string> { "ye" },
                    new List<string> { "ye", "yee", "yes", "yees" }
                ),
                new Tuple<string, List<string>, List<string>>
                (   "oir",
                    new List<string> { "u" },
                    new List<string> { "u", "ue", "ues", "ues" }
                ),
                new Tuple<string, List<string>, List<string>>
                (   "er",
                    new List<string> { "e" },
                    new List<string> { "e", "ee", "es", "ees" }
                ),
                new Tuple<string, List<string>,List<string>>
                (   "ir",
                    new List<string> { "i" },
                    new List<string> { "i, ie", "is", "ies" }
                ),
                new Tuple<string, List<string>,List<string>>
                (   "re",
                    new List<string> { "u" },
                    new List<string> { "u, ue", "us", "ues" }
                ),

            };


            foreach (var word in thisWords)
            {
                if (word.WordType == WordType.Entry) continue;
                if (word.Word.EndsWith("ant", StringComparison.InvariantCultureIgnoreCase))
                {
                    word.WordType = WordType.Participle;
                    word.EditState = EditState.Update;
                    continue;
                }
            }

            foreach (var typeCheck in verbTypesCheck)
            {
                bool result = CheckVerbType(entry.NormalizedWord, thisWords, typeCheck);
                if (result)
                {
                    break;
                }
            }
        }

        private bool CheckVerbType(string entry, List<IWordToDicoModel> words, Tuple<string, List<string>, List<string>> typeCheck)
        {
            var (suffix, participleEndings, transitiveEndings) = typeCheck;

            if (!entry.EndsWith(suffix)) return false;

            var root = entry.Substring(0, entry.Length - suffix.Length);

            // --- Handle participle endings ---
            foreach (var ending in participleEndings)
            {
                var participleForm = root + ending;
                var match = words.FirstOrDefault(w => w.Word == participleForm && w.WordType != WordType.Entry);
                if (match != null)
                {
                    match.WordType |= WordType.Participle | WordType.Variant;
                    match.EditState = EditState.Update;
                }
            }

            // --- Handle transitive endings ---
            var allTransitiveMatches = new List<IWordToDicoModel>();
            var matchedWordsSet = new HashSet<string>();
            bool groupComplete = true;

            foreach (var endingGroup in transitiveEndings)
            {
                var endings = endingGroup.Split(',').Select(e => e.Trim()).ToList();
                var groupMatches = new List<IWordToDicoModel>();

                foreach (var ending in endings)
                {
                    var form = root + ending;
                    var match = words.FirstOrDefault(w => w.Word == form && w.WordType != WordType.Entry);
                    if (match != null)
                    {
                        groupMatches.Add(match);
                        matchedWordsSet.Add(match.Word);
                    }
                    else
                    {
                        groupComplete = false;
                        break;
                    }
                }

                if (groupComplete)
                {
                    allTransitiveMatches.AddRange(groupMatches);
                }
            }

            // Apply participle+variant/plural if all group forms matched
            if (groupComplete)
            {
                foreach (var match in allTransitiveMatches)
                {
                    match.WordType = WordType.Participle | (match.Word.EndsWith("s") ? WordType.Plural : WordType.Variant);
                    match.EditState = EditState.Update;
                }
            }

            // --- Handle unmatched words: mark as Conjugation if not entry+"s" ---
            foreach (var word in words)
            {
                if (word.EditState == EditState.Update) continue;
                if (matchedWordsSet.Contains(word.Word)) continue;
                //if (word.Word.StartsWith(root))
                //{
                //string ending = word.Word.Substring(root.Length);
                //if (!string.IsNullOrEmpty(ending) && word.Word != entry + "s")
                {
                    word.WordType = WordType.Conjugation;
                    word.EditState = EditState.Update;
                }
                //}
            }

            return true;
        }
    }

}

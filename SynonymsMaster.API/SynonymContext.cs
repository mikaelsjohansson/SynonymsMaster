using System.Collections.Concurrent;

namespace SynonymsMaster.API
{
    public class SynonymContext : ISynonymContext
    {
        private readonly ConcurrentDictionary<string, ConcurrentBag<string>> _synonyms;

        public SynonymContext()
        {
            _synonyms = new ConcurrentDictionary<string, ConcurrentBag<string>>();
        }

        public void Add(string word, string synonym)
        {
            var wordToLower = word.ToLowerInvariant();
            var synonymToLower = synonym.ToLowerInvariant();

            _synonyms.AddOrUpdate(wordToLower, new ConcurrentBag<string> { synonymToLower }, (key, factory) =>
            {
                factory.Add(synonymToLower.ToLowerInvariant());
                return factory;
            });

            _synonyms.AddOrUpdate(synonymToLower, new ConcurrentBag<string> { wordToLower }, (key, factory) =>
            {
                factory.Add(wordToLower.ToLowerInvariant());
                return factory;
            });
        }

        public IEnumerable<string> GetSynonyms(string word, bool includeTransitive)
        {
            var toLowerWord = word.ToLowerInvariant();
            if (!includeTransitive)
            {
                return _synonyms.TryGetValue(toLowerWord, out var synonyms) ? synonyms : Enumerable.Empty<string>();
            }

            var transitiveSynonyms = new HashSet<string>();
            FillSynonymsRecursive(toLowerWord, transitiveSynonyms);
            transitiveSynonyms.Remove(toLowerWord); // Removing the initial word since it can have been added by transitive search.

            return transitiveSynonyms;
        }

        private void FillSynonymsRecursive(string word, HashSet<string> foundSynonyms)
        {
            // Is there any synonyms?
            if (_synonyms.TryGetValue(word, out var values))
            {
                foreach (var synonym in values)
                {
                    // Is this one we already have found?
                    if (!foundSynonyms.Contains(synonym))
                    {
                        // We found a new synonym.
                        // Lets add it.
                        foundSynonyms.Add(synonym);

                        // Lets continue down the path with this one.
                        FillSynonymsRecursive(synonym, foundSynonyms);
                    }
                }
            }

            // No more matches, we are done.
        }
    }
}
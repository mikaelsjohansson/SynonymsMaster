namespace SynonymsMaster.API
{
    public interface ISynonymContext
    {
        void Add(string word, string synonym);
        IEnumerable<string> GetSynonyms(string word, bool includeTransitive);
    }
}
namespace SynonymsMaster.API.Actions
{
    public interface IGetSynonyms
    {
        IEnumerable<string> Get(string word, bool includeTransient);
    }

    public class GetSynonyms(ISynonymContext context) : IGetSynonyms
    {
        public IEnumerable<string> Get(string word, bool includeTransitive)
        {
            return context.GetSynonyms(word, includeTransitive);
        }
    }
}
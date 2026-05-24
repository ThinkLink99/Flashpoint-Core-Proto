public interface IHasKeywords
{
    public KeywordCollection Keywords { get; }

    public bool HasKeyword(string keywordId) => Keywords.HasKeyword(keywordId);
    public int GetKeywordValue(string keywordId) => Keywords.GetKeyword(keywordId).Value;
    public RuntimeKeyword GetKeyword(string keywordId) => Keywords.GetKeyword(keywordId);
}
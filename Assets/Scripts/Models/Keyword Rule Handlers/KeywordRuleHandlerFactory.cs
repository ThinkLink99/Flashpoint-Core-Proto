public class KeywordRuleHandlerFactory {
    public static IKeywordRuleHandler GetHandlerForKeyword(string keywordId)
    {
        // In a real implementation, this could be more dynamic, perhaps using reflection or a registration system.
        return keywordId switch
        {
            "energy_shield" => new EnergyShieldHandler(),
            "lethal" => new LethalHandler(),
            _ => null
        };
    }
}
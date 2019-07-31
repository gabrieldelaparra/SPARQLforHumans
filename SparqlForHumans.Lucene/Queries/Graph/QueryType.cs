namespace SparqlForHumans.Lucene.Queries.Graph
{
    public enum QueryType
    {
        Unkwown,
        ConstantTypeDoNotQuery,
        QueryTopProperties,
        QueryTopEntities,
        KnownSubjectTypeOnlyQueryDomainProperties,
        KnownObjectTypeOnlyQueryRangeProperties,
        KnownSubjectAndObjectTypesIntersectDomainRangeProperties,
        KnownSubjectTypeQueryInstanceEntities,
        KnownObjectTypeNotUsed,
        KnownSubjectAndObjectTypesQueryInstanceEntities,
        InferredDomainAndRangeTypeProperties,
        InferredDomainTypeProperties,
        InferredRangeTypeProperties,
        InferredDomainTypeEntities,
        InferredDomainAndRangeTypeEntities,
        InferredRangeTypeEntities,
    }


}

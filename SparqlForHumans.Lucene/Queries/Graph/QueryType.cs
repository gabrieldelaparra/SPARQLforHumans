namespace SparqlForHumans.Models.Query
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
        InferredSubjectType,
        //InferredSubjectType
        //InferredPredicateType
    }


}

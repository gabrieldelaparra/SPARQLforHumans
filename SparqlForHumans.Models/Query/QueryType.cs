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
        //InferredSubjectType
        //InferredPredicateType
    }


}

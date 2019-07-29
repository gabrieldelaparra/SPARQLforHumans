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
        KnownNodeTypeQueryInstanceEntities,
        KnownDomainTypeNotUsed,
        KnownNodeAndDomainTypesNotUsed,
    }


}

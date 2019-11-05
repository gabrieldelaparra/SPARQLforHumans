namespace SparqlForHumans.Lucene.Queries.Graph
{
    public enum QueryType
    {
        //Not used, still, wanted to add a different case.
        Unknown,

        //When the type is given directly to the element.
        ConstantTypeDoNotQuery,

        //For Unknown Types
        QueryTopProperties,
        QueryTopEntities,

        //For Properties inside Entities with Given Types;

        // ?var0 ?prop0 ?var1
        // ?var0 is Obama
        // ?var1 is USA
        // ?prop0 are properties between Obama and USA
        GivenSubjectAndObjectUrisQueryGivenProperties,

        // ?var0 ?prop0 ?var1
        // ?var0 is Obama
        // ?prop0 are properties of Obama
        GivenSubjectUrisQueryGivenProperties,

        // ?var0 ?prop0 ?var1
        // ?var1 is USA
        // ?prop0 are properties that get to USA
        GivenObjectUrisQueryGivenProperties,

        // For properties that point to known types (P31)

        // ?var0 ?prop0 ?var1
        // ?var0 P31 Human
        // ?prop0 are properties of Humans (Domain: Human)
        KnownSubjectTypeQueryDomainProperties,

        // ?var0 ?prop0 ?var1
        // ?var1 P31 Country
        // ?prop0 are properties targeting Countries (Range: Country)
        KnownObjectTypeQueryRangeProperties,

        // ?var0 ?prop0 ?var1
        // ?var0 P31 Human
        // ?var1 P31 Country
        // ?prop0 are properties from Humans, targeting Countries (Domain: Human Intersect Range: Country)
        KnownSubjectAndObjectTypesIntersectDomainRangeProperties,

        // For Entities that point to known types (P31)

        // ?var0 P31 Country
        // ?var0 are entities of type Countries
        KnownSubjectTypeQueryInstanceEntities,

        // ?var0 ?prop0 ?var1
        // ?var0 P31 Country
        // ?var0 are entities of type Countries
        KnownSubjectAndObjectTypesQueryInstanceEntities,

        KnownObjectTypeNotUsed,
        KnownPredicateAndObjectNotUsed,


        InferredDomainAndRangeTypeProperties,
        InferredDomainTypeProperties,
        InferredRangeTypeProperties,


        InferredDomainTypeEntities,
        InferredDomainAndRangeTypeEntities,
        InferredRangeTypeEntities,
    }


}

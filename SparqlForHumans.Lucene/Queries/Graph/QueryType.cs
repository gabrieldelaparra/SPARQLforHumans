namespace SparqlForHumans.Lucene.Queries.Graph
{
    //public enum QueryType
    //{
    //    //Base Case
    //    Unknown,

    //    //When the type is given directly to the element.
    //    GivenEntityTypeNoQuery,
    //    GivenPredicateTypeNoQuery,

    //    //For Unknown Types
    //    QueryTopProperties,
    //    QueryTopEntities,

    //    //For Elements inside Entities with Given Types;

    //    // ?var0 ?prop0 ?var1
    //    // ?var0 is Obama
    //    // --> ?var1 are entities that go from Obama
    //    GivenSubjectTypeQueryDirectlyEntities,
    //    // --> ?prop0 are properties of Obama
    //    GivenSubjectTypeDirectQueryOutgoingProperties,

    //    DirectQuery,

    //    // ?var0 ?prop0 ?var1
    //    // ?var1 is USA
    //    // --> ?var0 are entities that get to USA
    //    GivenObjectTypeQueryDirectlyEntities,
    //    // --> ?prop0 are properties that get to USA
    //    GivenObjectTypeDirectQueryIncomingProperties,

    //    // ?var0 ?prop0 ?var1
    //    // ?var0 is Obama
    //    // ?var1 is USA
    //    // --> ?prop0 are properties between Obama and USA
    //    GivenSubjectAndObjectTypeDirectQueryIntersectOutInProperties,

    //    // ?var0 P31 Country
    //    // --> ?var0 are entities of type Countries
    //    SubjectIsInstanceOfTypeQueryEntities,

    //    // For properties that point to known types (P31)

    //    // ?var0 P31 ?var1
    //    // ?var1 is Human
    //    // ?var0 ?prop0 ?var2
    //    // --> ?prop0 are properties of Humans (Domain: Human)
    //    KnownSubjectTypeQueryDomainProperties,
    //    // --> ?var2 ?
    //    //TODO: Create this case

    //    // ?var0 ?prop0 ?var1
    //    // ?var1 P31 Country
    //    // ?prop0 are properties targeting Countries (Range: Country)
    //    KnownObjectTypeQueryRangeProperties,

    //    // ?var0 ?prop0 ?var1
    //    // ?var0 P31 Human
    //    // ?var1 P31 Country
    //    // ?prop0 are properties from Humans, targeting Countries (Domain: Human Intersect Range: Country)
    //    KnownSubjectAndObjectTypesIntersectDomainRangeProperties,

    //    //Inferred Predicates
    //    InferredDomainAndRangeTypeProperties,
    //    InferredDomainTypeProperties,
    //    InferredRangeTypeProperties,

    //    //Inferred Entities
    //    InferredDomainTypeEntities,
    //    InferredDomainAndRangeTypeEntities,
    //    InferredRangeTypeEntities,
    //}


}

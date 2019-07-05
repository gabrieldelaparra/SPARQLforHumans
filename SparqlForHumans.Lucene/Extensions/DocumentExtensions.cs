using System;
using Lucene.Net.Documents;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.Models.Wikidata;

namespace SparqlForHumans.Lucene.Extensions
{
    public static class DocumentExtensions
    {
        private static string GetValue(this Document document, string labelValue)
        {
            var documentValue = document?.Get(labelValue);
            return documentValue ?? string.Empty;
        }

        public static string GetValue(this Document document, Labels label)
        {
            return document?.GetValue(label.ToString());
        }

        public static string[] GetValues(this Document document, Labels label)
        {
            var toSplit = document?.GetValues(label.ToString());
            if (toSplit.Length.Equals(1))
            {
                var singleJoin = string.Join("", toSplit);
                var split = singleJoin.Split(new[] {WikidataDump.PropertyValueSeparator},
                    StringSplitOptions.RemoveEmptyEntries);
                if (split.Length > 1) toSplit = split;
            }

            return toSplit;
        }
    }
}
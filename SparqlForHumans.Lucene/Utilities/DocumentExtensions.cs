using Lucene.Net.Documents;
using SparqlForHumans.Models;

namespace SparqlForHumans.Core.Utilities
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
            return document?.GetValues(label.ToString());
        }
    }
}
using System;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Store;

namespace SparqlForHumans.Core.Helpers
{
    public class QueryHelper : IDisposable
    {
        private Analyzer analyzer;
        public Analyzer Analyzer
        {
            get => analyzer;
            set
            {
                analyzer.Close();
                analyzer = value;
            }
        }

        public Directory LuceneDirectory { get; private set; }

        public Document SearchDocument(string fieldValue)
        {

            return null;
        }

        public QueryHelper(Analyzer analyzer, Directory luceneDirectory)
        {
            Analyzer = analyzer;
            LuceneDirectory = luceneDirectory;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                analyzer.Close();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

namespace SparqlForHumans.Models
{
    public class Subject : ISubject
    {
        public string Id { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;

        public override bool Equals(object obj)
        {
            var y = obj as Subject;
            if (y == null) return false;
            if (ReferenceEquals(this, y)) return true;
            if (ReferenceEquals(this, null) || ReferenceEquals(y, null)) return false;
            return Id.Equals(y.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Label} ({Id})";
        }
    }
}
using System.Collections.Generic;

namespace SparqlForHumans.Models
{
    public class BaseEntity : Subject, IBaseEntity
    {
        //Constructor
        public BaseEntity()
        {
        }

        public BaseEntity(string id) : base(id)
        {
        }

        public BaseEntity(string id, string label) : base(id, label)
        {
        }

        public BaseEntity(ISubject baseSubject) : base(baseSubject)
        {
        }

        // IBasicEntity
        public string Description { get; set; } = string.Empty;
        public IList<string> InstanceOf { get; set; } = new List<string>();
        public IList<string> SubClass { get; set; } = new List<string>();
        public IList<string> AltLabels { get; set; } = new List<string>();
        public bool IsType { get; set; } = false;

        public override string ToString()
        {
            return $"{base.ToString()} - ({string.Join("-", InstanceOf)}) - {Description}";
        }
    }
}
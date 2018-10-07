using System.Collections.Generic;
using System.Linq;

namespace SparqlForHumans.Models
{
    public class Entity : Subject, IEntity
    {
        public Entity()
        {
        }

        public Entity(ISubject iEntity)
        {
            Id = iEntity.Id;
            Label = iEntity.Label;
        }

        public string Description { get; set; } = string.Empty;

        public IEnumerable<string> InstanceOf { get; set; } = new List<string>();

        public string InstanceOfId => InstanceOf?.FirstOrDefault();

        //TODO: Modify
        public string InstanceOfLabel => InstanceOf?.FirstOrDefault();

        public List<Property> Properties { get; set; } = new List<Property>();

        public IEnumerable<string> AltLabels { get; set; } = new List<string>();

        public string Rank { get; set; } = string.Empty;

        public double RankValue => double.TryParse(Rank, out var value) ? value : 0;

        public string ToRankedString()
        {
            return $"[{Rank}] {ToString()}";
        }

        public override string ToString()
        {
            return $"{base.ToString()} - ({InstanceOfId}) - {Description}";
        }
    }
}
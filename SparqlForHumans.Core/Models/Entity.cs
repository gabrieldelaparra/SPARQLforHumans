using System;
using System.Collections.Generic;
using System.Linq;

namespace SparqlForHumans.Core.Models
{
    public interface IEntity
    {
        string Id { get; set; }
        string Label { get; set; }
    }

    public class BaseEntity : IEntity
    {
        public string Id { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{Label} ({Id})";
        }
    }

    public class Entity : BaseEntity
    {
        public string Description { get; set; } = string.Empty;

        //TODO: Move InstanceOf to Property
        public List<Property> InstanceOf { get; set; } = new List<Property>();

        public string InstanceOfId => InstanceOf?.FirstOrDefault()?.Id;
        public string InstanceOfLabel => InstanceOf?.FirstOrDefault()?.Label;

        public List<Property> Properties { get; set; } = new List<Property>();
        public IEnumerable<string> AltLabels { get; set; } = new List<string>();

        public override string ToString()
        {
            return $"{base.ToString()} - {InstanceOfLabel} ({InstanceOfId}) - {Description}";
        }
    }
}
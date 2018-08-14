using System;
using System.Collections.Generic;

namespace SparqlForHumans.Core.Models
{
    public interface IEntity
    {
        string Id { get; set; }
        string Label { get; set; }
    }

    public abstract class BaseEntity : IEntity
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
        public string InstanceOf { get; set; } = string.Empty;
        public string InstanceOfLabel { get; set; } = string.Empty;

        public IEnumerable<Property> Properties { get; set; } = new List<Property>();
        public IEnumerable<string> AltLabels { get; set; } = new List<string>();

        public override string ToString()
        {
            return $"{base.ToString()} - {InstanceOfLabel} ({InstanceOf}) - {Description}";
        }
    }
}
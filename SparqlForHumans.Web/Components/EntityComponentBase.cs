using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor.Components;
using RPedretti.Blazor.Components;
using SparqlForHumans.Core.Services;
using SparqlForHumans.Core.Utilities;
using SparqlForHumans.Models;

namespace SparqlForHumans.Web.Components
{
    public class EntityComponentBase :  BaseComponent
    {
        private string _query;
        public string Query
        {
            get => _query;
            set => SetParameter(ref _query, value);
        }

        private bool _loadingSuggestions;
        protected bool LoadingSuggestions
        {
            get => _loadingSuggestions;
            set => SetParameter(ref _loadingSuggestions, value);
        }

        private Entity selectedEntity;
        public Entity SelectedEntity
        {
            get => selectedEntity;
            set => SetParameter(ref selectedEntity, value);
        }

        protected void SuggestionSelected(string suggestion)
        {
            FilteredList = null;
            Query = suggestion;
            selectedEntity = FilteredList.Find(x => x.Label.Equals(suggestion));
        }

        protected List<Entity> FilteredList { get; set; }

        protected List<string> FilteredLabelsList
        {
            get
            {
                return FilteredList?.Select(x => x.Label).ToList();
            }
        }

        protected async Task FetchSuggestions(string query)
        {
            Query = query;
            Console.WriteLine($"Query: {query}");
            if (!string.IsNullOrWhiteSpace(query))
            {
                LoadingSuggestions = true;
                StateHasChanged();
                await Task.Delay(1000);

                FilteredList = MultiDocumentQueries.QueryEntitiesByLabel(query).ToList();
                LoadingSuggestions = false;
            }
            else
            {
                FilteredList = null;
            }
            Console.WriteLine($"ResultCount: {FilteredList.Count}");
            StateHasChanged();
        }

        public Property[] properties = new Property[0];

        int i = 0;
        public void addProperty()
        {
            properties = properties.Append(new Property()
            {
                Id = i++.ToString(),
            }).ToArray();
        }


        public void removeProperty()
        {
            properties = properties.ToList().Take(properties.Length - 1).ToArray();
        }
    }
}

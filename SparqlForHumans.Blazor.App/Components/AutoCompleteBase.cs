using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor.Components;
using SparqlForHumans.Models;

namespace SparqlForHumans.Blazor.App.Components
{
    public class AutoCompleteBase : BlazorComponent
    {
        private string _inputQuery;
        private Entity[] _queryResult;
        private string _placeHolder;
        private bool _waitingSelection;

        public Entity[] QueryResults
        {
            get { return _queryResult; }
            set
            {
                _queryResult = value;
                StateHasChanged();
            }
        }

        public bool WaitingSelection
        {
            get => _waitingSelection;
            set
            {
                _waitingSelection = value;
                StateHasChanged();
            }
        }

        protected string InputQuery
        {
            get { return _inputQuery; }
            set
            {
                if (value.Length <= MinimumLength) return;
                WaitingSelection = true;

                if (AutocompleteSourceProvider != null)
                    ValidateText(AutocompleteSourceProvider);

                _inputQuery = value;
            }
        }

        bool isBusyProcessing = false;

        private async Task ValidateText(Func<string, Task<Entity[]>> function)
        {
            if (isBusyProcessing)
                return;

            isBusyProcessing = true;
            await Task.Delay(Delay);
            isBusyProcessing = false;

            Console.WriteLine("Invoke");
            QueryResults = await function?.Invoke(InputQuery);

        }

        [Parameter]
        protected string PlaceHolder
        {
            get => _placeHolder;
            set
            {
                _placeHolder = value;
                StateHasChanged();
            }
        }

        [Parameter]
        protected int Delay { get; set; }

        [Parameter]
        protected int MinimumLength { get; set; }

        protected void OnEntitySelected(Entity entity)
        {
            QueryResults = null;
            WaitingSelection = false;
            OnSelectionChanged?.Invoke(entity);
        }

        [Parameter]
        protected Action<Entity> OnSelectionChanged { get; set; }

        [Parameter]
        protected Func<string, Task<Entity[]>> AutocompleteSourceProvider { get; set; }
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor.Components;
using SparqlForHumans.Blazor.App.Shared;

namespace SparqlForHumans.Blazor.App.Components
{
    public class AutoCompleteBase : BlazorComponent
    {
        private string _inputQuery;
        private SelectableValue[] _queryResult;

        public SelectableValue[] QueryResults
        {
            get { return _queryResult; }
            set
            {
                _queryResult = value;
                StateHasChanged();
            }
        }


        protected string InputQuery
        {
            get { return _inputQuery; }
            set
            {
                if (value.Length <= MinimumLength) return;

                if (AutocompleteSourceProvider != null)
                    ValidateText(AutocompleteSourceProvider);

                _inputQuery = value;
            }
        }

        bool isBusyProcessing = false;

        private async Task ValidateText(Func<string, Task<SelectableValue[]>> function)
        {
            if (isBusyProcessing)
                return;

            isBusyProcessing = true;
            await Task.Delay(Delay);
            isBusyProcessing = false;

            Console.WriteLine("Invoke");
            QueryResults = await function?.Invoke(InputQuery);

        }

        private string _placeHolder;

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


        [Parameter]
        protected Action<object> OnSelectionChanged { get; set; }

        [Parameter]
        protected Func<string, Task<SelectableValue[]>> AutocompleteSourceProvider { get; set; }
    }
}

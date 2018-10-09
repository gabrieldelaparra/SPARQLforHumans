using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.JSInterop;
using SparqlForHumans.Blazor.App.Shared;

namespace SparqlForHumans.Blazor.App.Components
{
    public class AutoCompleteBase : BlazorComponent
    {
        protected ElementRef InputElementRef { get; set; }
        protected DotNetObjectRef DotNetObjectRef { get; set; }

        private string _placeHolder;
        private Func<string, Task<SelectableValue[]>> _autocompleteSourceProvider;
        private int _minimumLength;
        private int _delay;
        private Action<object> _onSelectionChanged;

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
        protected int Delay
        {
            get => _delay;
            set
            {
                _delay = value;
                JSRuntime.Current.InvokeAsync<object>("autoCompleteElement.setDelay", InputElementRef, _delay);
            }
        }

        [Parameter]
        protected int MinimumLength
        {
            get => _minimumLength;
            set
            {
                _minimumLength = value;
                JSRuntime.Current.InvokeAsync<object>("autoCompleteElement.setMinLength", InputElementRef, _minimumLength);
            }
        }

        protected override void OnAfterRender()
        {
            DotNetObjectRef = new DotNetObjectRef(this);

            JSRuntime.Current.InvokeAsync<object>("autoCompleteElement.initAutoComplete", InputElementRef);

            MinimumLength = _minimumLength;
            Delay = _delay;
            AutocompleteSourceProvider = _autocompleteSourceProvider;
            OnSelectionChanged = _onSelectionChanged;

            base.OnAfterRender();
        }

        [JSInvokable]
        public void OnSelectEventListener(object selectedJSONObject)
        {
            OnSelectionChanged?.Invoke(selectedJSONObject);
        }

        [JSInvokable]
        public Task<SelectableValue[]> AutocompleteSourceDelegate(string query)
        {
            return AutocompleteSourceProvider?.Invoke(query);
        }

        [Parameter]
        protected Action<object> OnSelectionChanged
        {
            get => _onSelectionChanged;
            set
            {
                _onSelectionChanged = value;
                JSRuntime.Current.InvokeAsync<object>("autoCompleteElement.setSelect",
                    InputElementRef, DotNetObjectRef);
            }
        }

        [Parameter]
        protected Func<string, Task<SelectableValue[]>> AutocompleteSourceProvider
        {
            get => _autocompleteSourceProvider;
            set
            {
                _autocompleteSourceProvider = value;
                JSRuntime.Current.InvokeAsync<object>("autoCompleteElement.setSourceFunction",
                    InputElementRef, DotNetObjectRef);
            }
        }
    }
}

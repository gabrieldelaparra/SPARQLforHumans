using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.JSInterop;
using SparqlForHumans.Blazor.App.Shared;
using SparqlForHumans.Core.Services;

namespace SparqlForHumans.Blazor.App.Components
{
    public class AutoCompleteBase : BlazorComponent
    {
        private System.Threading.Timer _timer;

        /// <summary>
        /// Debounce reset timer and after last item recieved give you last item.
        /// <exception cref="http://demo.nimius.net/debounce_throttle/">See this example for understanding what is RateLimiting and Debounce</exception>
        /// </summary>
        /// <param name="obj">Your object</param>
        /// <param name="interval">Milisecond interval</param>
        /// <param name="debounceAction">Called when last item call this method and after interval was finished</param>
        public void Debounce(int interval, Task action, string param = null)
        {
            SelectableValue[] toReturn = null;
            _timer?.Dispose();
            _timer = new System.Threading.Timer(state =>
            {
                _timer.Dispose();
                if (_timer != null)
                {
                    action?.Start();
                }

                _timer = null;
            }, param, interval, interval);
        }

        private string _inputQuery;
        private SelectableValue[] queryResults = new SelectableValue[0];

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
            await Task.Delay(2000);
            isBusyProcessing = false;

            Console.WriteLine("Invoke");
            queryResults = await function?.Invoke(InputQuery);

        }

        

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
                //JSRuntime.Current.InvokeAsync<object>("autoCompleteElement.setDelay", InputElementRef, _delay);
            }
        }

        [Parameter]
        protected int MinimumLength
        {
            get => _minimumLength;
            set
            {
                _minimumLength = value;
                //JSRuntime.Current.InvokeAsync<object>("autoCompleteElement.setMinLength", InputElementRef, _minimumLength);
            }
        }

        protected override void OnAfterRender()
        {
            DotNetObjectRef = new DotNetObjectRef(this);

            //JSRuntime.Current.InvokeAsync<object>("autoCompleteElement.initAutoComplete", InputElementRef);

            //MinimumLength = _minimumLength;
            //Delay = _delay;
            //AutocompleteSourceProvider = _autocompleteSourceProvider;
            //OnSelectionChanged = _onSelectionChanged;

            base.OnAfterRender();
        }

        //[JSInvokable]
        //public void OnSelectEventListener(object selectedJSONObject)
        //{
        //    OnSelectionChanged?.Invoke(selectedJSONObject);
        //}

        [JSInvokable]
        public void AutocompleteSourceDelegate(string query)
        {
            Console.WriteLine("METHOD INVOKED: " + query);
        }

        //[JSInvokable]
        //public Task<SelectableValue[]> AutocompleteSourceDelegate(string query)
        //{
        //    return AutocompleteSourceProvider?.Invoke(query);
        //}

        [Parameter]
        protected Action<object> OnSelectionChanged
        {
            get => _onSelectionChanged;
            set
            {
                _onSelectionChanged = value;
                //JSRuntime.Current.InvokeAsync<object>("autoCompleteElement.setSelect", InputElementRef, DotNetObjectRef);
            }
        }

        [Parameter]
        protected Func<string, Task<SelectableValue[]>> AutocompleteSourceProvider { get; set; }
        //{
        //    get => _autocompleteSourceProvider;
        //    set
        //    {
        //        _autocompleteSourceProvider = value;
        //        //JSRuntime.Current.InvokeAsync<object>("autoCompleteElement.setSourceFunction", InputElementRef, DotNetObjectRef);
        //    }
        //}
    }
}

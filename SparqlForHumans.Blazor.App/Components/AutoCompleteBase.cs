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
        protected ElementRef InputTextRef { get; set; }

        private string _placeHolder;
        private Func<string, Task<SelectableValue[]>> _sourceFunc;
        private int _minimumLength;
        private int _delay;
        private Action<object> _onSelect;

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
                JSRuntime.Current.InvokeAsync<object>("autoCompleteElement.setDelay", InputTextRef, _delay);
            }
        }

        [Parameter]
        protected int MinimumLength
        {
            get => _minimumLength;
            set
            {
                _minimumLength = value;
                JSRuntime.Current.InvokeAsync<object>("autoCompleteElement.setMinLength", InputTextRef, _minimumLength);
            }
        }

        protected override void OnAfterRender()
        {
            JSRuntime.Current.InvokeAsync<object>("autoCompleteElement.initAutoComplete", InputTextRef);

            MinimumLength = _minimumLength;
            Delay = _delay;
            SourceFunc = _sourceFunc;
            OnSelect = _onSelect;

            base.OnAfterRender();
        }

        [Parameter]
        protected Action<object> OnSelect
        {
            get => _onSelect;
            set
            {
                _onSelect = value;
                JSRuntime.Current.InvokeAsync<object>("autoCompleteElement.setSelect",
                    InputTextRef,
                    _onSelect?.Method.ReflectedType?.Assembly.GetName().Name,
                    _onSelect?.Method.Name);
            }
        }

        [Parameter]
        protected Func<string, Task<SelectableValue[]>> SourceFunc
        {
            get => _sourceFunc;
            set
            {
                _sourceFunc = value;
                JSRuntime.Current.InvokeAsync<object>("autoCompleteElement.setSourceFunction",
                    InputTextRef,
                        _sourceFunc?.Method.ReflectedType?.Assembly.GetName().Name,
                        _sourceFunc?.Method.Name);
            }
        }
    }
}

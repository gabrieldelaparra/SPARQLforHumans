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

        protected ElementRef InputTextBox { get; set; }

        protected override void OnAfterRender()
        {
            JSRuntime.Current.InvokeAsync<object>("autoCompleteElement.initAutoComplete",
            InputTextBox,
            Source?.Method.ReflectedType?.Assembly.GetName().Name,
            Source?.Method.Name);

            base.OnAfterRender();
        }

        [Parameter] protected Func<string, Task<SelectableValue[]>> Source { get; set; }

    }
}

using System;
using Microsoft.AspNetCore.Components;

namespace Jenga.BlazorUI.Services.Common
{
    public class ModalRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Action<bool>? OnResult { get; set; }
        public RenderFragment? Content { get; set; }
        public bool ShowConfirmationButtons { get; set; } = false;
        public Action? Close { get; set; }

        public string DialogClass { get; set; } = "modal-xl modal-xxl-custom";
    }
}

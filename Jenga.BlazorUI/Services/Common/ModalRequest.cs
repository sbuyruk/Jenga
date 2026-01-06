using System;
using Microsoft.AspNetCore.Components;

namespace Jenga.BlazorUI.Services.Common
{
    public class ModalRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Action<bool>? OnResult { get; set; }

        // allow rendering a Blazor component or arbitrary fragment inside the modal body
        public RenderFragment? Content { get; set; }

        // choose confirmation buttons (Yes/No) or default Close button
        public bool ShowConfirmationButtons { get; set; } = false;

        // allow the modal content to request the modal to close
        public Action? Close { get; set; }
    }
}

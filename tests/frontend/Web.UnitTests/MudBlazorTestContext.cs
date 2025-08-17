using Bunit;
using MudBlazor.Services;
using MudBlazor;
using Microsoft.AspNetCore.Components;

namespace Web.UnitTests;

/// <summary>
/// Base test context class that configures MudBlazor services for bunit tests.
/// This class ensures all necessary MudBlazor services are registered to avoid
/// errors when testing components that use MudBlazor components.
/// </summary>
public class MudBlazorTestContext : TestContext
{
    public MudBlazorTestContext()
    {
        // Add MudBlazor services
        Services.AddMudServices();
        
        ConfigureMudBlazorJSInterop();
    }
    
    /// <summary>
    /// Configures JSInterop to handle common MudBlazor JavaScript function calls
    /// that are needed for testing components without errors.
    /// </summary>
    private void ConfigureMudBlazorJSInterop()
    {
        // Handle MudBlazor popover initialization calls
        JSInterop.SetupVoid("mudPopover.initialize", _ => true);
        JSInterop.SetupVoid("mudPopover.connect", _ => true);
        JSInterop.SetupVoid("mudPopover.disconnect", _ => true);
        JSInterop.Setup<int>("mudpopoverHelper.countProviders").SetResult(1);
        
        // Handle MudBlazor element reference calls
        JSInterop.SetupVoid("mudElementRef.addOnBlurEvent", _ => true);
        JSInterop.SetupVoid("mudElementRef.removeOnBlurEvent", _ => true);
        JSInterop.SetupVoid("mudElementRef.focus", _ => true);
        JSInterop.SetupVoid("mudElementRef.select", _ => true);
        JSInterop.SetupVoid("mudElementRef.selectRange", _ => true);
        
        // Handle common MudBlazor JS calls that might be needed
        JSInterop.SetupVoid("mudScrollManager.lockScroll", _ => true);
        JSInterop.SetupVoid("mudScrollManager.unlockScroll", _ => true);
        JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
        JSInterop.SetupVoid("mudKeyInterceptor.updatekey", _ => true);
        JSInterop.SetupVoid("mudKeyInterceptor.disconnect", _ => true);
        JSInterop.SetupVoid("mudElementReference.focusFirst", _ => true);
        JSInterop.SetupVoid("mudElementReference.focusLast", _ => true);
        JSInterop.SetupVoid("mudElementReference.saveFocus", _ => true);
        JSInterop.SetupVoid("mudElementReference.restoreFocus", _ => true);
        
        // Handle table/data grid specific calls if needed
        JSInterop.Setup<object>("mudTable.getSelectedRowElement", _ => true).SetResult(new { });
        
        // Handle MudBlazor drag and drop calls
        JSInterop.SetupVoid("mudDragAndDrop.initDropZone", _ => true);
        JSInterop.SetupVoid("mudDragAndDrop.disposeDropZone", _ => true);
        
        // Handle MudBlazor resize observer calls
        JSInterop.SetupVoid("mudResizeObserver.connect", _ => true);
        JSInterop.SetupVoid("mudResizeObserver.disconnect", _ => true);
    }
    
    /// <summary>
    /// Renders a component wrapped with MudPopoverProvider to ensure MudBlazor components work correctly.
    /// </summary>
    public IRenderedComponent<TComponent> RenderMudComponent<TComponent>(Action<ComponentParameterCollectionBuilder<TComponent>>? parameterBuilder = null)
        where TComponent : IComponent
    {
        var renderFragment = new RenderFragment(builder =>
        {
            builder.OpenComponent<MudPopoverProvider>(0);
            builder.AddContent(1, new RenderFragment(innerBuilder =>
            {
                innerBuilder.OpenComponent<TComponent>(0);
                
                if (parameterBuilder != null)
                {
                    var parameters = new ComponentParameterCollectionBuilder<TComponent>();
                    parameterBuilder(parameters);
                    
                    foreach (var parameter in parameters.Build())
                    {
                        innerBuilder.AddAttribute(1, parameter.Name!, parameter.Value);
                    }
                }
                
                innerBuilder.CloseComponent();
            }));
            builder.CloseComponent();
        });
        
        return Render<TComponent>(renderFragment);
    }
}

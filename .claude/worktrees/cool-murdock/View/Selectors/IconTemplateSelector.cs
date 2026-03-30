using MyriaRPG.Model;
using System.Windows;
using System.Windows.Controls;

namespace MyriaRPG.View.Selectors
{
    public class IconTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (container is FrameworkElement element && item is ItemVm itemVm)
            {
                // Try to find a resource with the key "Icon.{ItemId}"
                string resourceKey = $"Icon.{itemVm.Id}";
                
                object resource = element.TryFindResource(resourceKey);

                if (resource is DataTemplate template)
                {
                    return template;
                }

                // Fallback to Default
                return element.TryFindResource("Icon.Default") as DataTemplate;
            }

            return null;
        }
    }
}

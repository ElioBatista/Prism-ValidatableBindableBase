using Prism.Regions;
using System;
using System.Collections.Specialized;
using System.Windows;

namespace Apollo.Infrastructure.Prism
{
    public class RegionManagerAwareBehavior : RegionBehavior
    {
        public const String BehaviorKey = "RegionManagerAwareBehavior";

        protected override void OnAttach()
        {
            Region.ActiveViews.CollectionChanged += ActiveViews_CollectionChanged;
        }

        void ActiveViews_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    var regionManager = Region.RegionManager;
                    if (item is FrameworkElement element)
                    {
                        if (element.GetValue(RegionManager.RegionManagerProperty) is IRegionManager scopedRegionManager)
                            regionManager = scopedRegionManager;
                    }                    
                    InvokeOnRegionManagerAwareElement(item, x => x.RegionManager = regionManager);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    InvokeOnRegionManagerAwareElement(item, x => x.RegionManager = null);
                }
            }
        }

        private static void InvokeOnRegionManagerAwareElement(object item, Action<IRegionManagerAware> invocation)
        {
            if (item is IRegionManagerAware regionManagerAwareItem)
                invocation(regionManagerAwareItem);

            if (item is FrameworkElement frameworkElement)
            {
                if (frameworkElement.DataContext is IRegionManagerAware regionManagerAwareDataContext)
                {
                    if (frameworkElement.Parent is FrameworkElement frameworkElementParent)
                    {
                        if (frameworkElementParent.DataContext is IRegionManagerAware regionManagerAwareDataContextParent && 
                            regionManagerAwareDataContext == regionManagerAwareDataContextParent)
                            return;
                    }
                    invocation(regionManagerAwareDataContext);
                }
            }
        }
    }
}
using Apollo.Infrastructure;
using Apollo.Infrastructure.Controls;
using Apollo.Infrastructure.Prism;
using Prism.Regions;
using System;

namespace Apollo.Culinary.Views
{
    public partial class MenuStructureViewModel : ICreateRegionManagerScope
    {
        public bool CreateRegionManagerScope => true;

        private void NavigateToProperties(string view)
        {
            var uri = new Uri(view, UriKind.Relative);
            var navParam = GetPropetiesNavParam(LeftSideItem.Id);

            RegionManager.RequestNavigate("PropertiesRegion", uri, navParam);
        }

        private void NavigateToProperties(FocusedRowChangingEventArgs arg)
        {
            var newSel = arg.NewRow as MenuStructureProxy;
            string view;
            if (newSel.ParentId == null)
                view= Constants.CategoryCompaniesView;
            else
                view = Constants.CategorySeasonsView;

            var uri = new Uri(view, UriKind.Relative);
            var navParam = GetPropetiesNavParam(newSel.Id);

            RegionManager.RequestNavigate("PropertiesRegion", uri, new Action<NavigationResult>(result =>
            {
                if (result.Result == false)
                {
                    arg.Cancel = true;
                }
            }), navParam);

        }

        private NavigationParameters GetPropetiesNavParam(Guid id)
        {            
            return new NavigationParameters
            {
                {
                    NavigationParametersNames.IdNavigationParameter, new IdNavigationParameter<Guid>
                    {
                        Id = id
                }   }
            };
        }
    }
}
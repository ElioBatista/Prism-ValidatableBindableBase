using Apollo.Common.Extensions;
using Apollo.Core;
using Apollo.Culinary.Core;
using Apollo.Culinary.Services;
using Apollo.Infrastructure;
using Apollo.Infrastructure.Dialogs;
using Apollo.Infrastructure.Prism;
using Nito.Mvvm;
using Prism.Commands;
using Prism.Events;
using Prism.Logging;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Apollo.Culinary.Views
{
    public partial class MenuStructureViewModel : NavigationAwareViewModelBase,
        IRegionManagerAware
    {
        private ILoggerFacade _logger;
        private IMenuStructureProvider _provider;        
        private NotifyTask _initialization;

        #region Constructor

        public MenuStructureViewModel(IEventAggregator eventAggregator,
            IMessageBoxService messageBoxService,
            IDialogService dialogService,
            ILoggerFacade logger,
            IMenuStructureProvider provider,
            SessionContext sessionContext
            )
            : base(eventAggregator, messageBoxService, sessionContext)
        {
            _logger = logger;
            _provider = provider;

            DialogService = dialogService;

            //Commands            
            NewRootCategoryCommand = new DelegateCommand(ExecuteNewRootCategoryCommand,
                CanExecuteNewRootCategoryCommand);

            NewMenuCommand = new DelegateCommand<MenuStructureProxy>(ExecuteNewMenuCommand, 
                CanExecuteNewMenuCommand);

            ReloadCommand = new DelegateCommand(ExecuteReloadCommand, 
                CanExecuteReloadCommand);

            InitializeSubCategoryCommands();
            Title = Resources.CulinaryResourceStrings.MenuStructure;
            eventAggregator.GetEvent<ViewItemsChangedEvent>()
                .Subscribe(async e => await OnCategorySaved(e));
        }

        //MenuStructureCommands
        #endregion Constructor

        #region Commands

        //NewMenuCommand
        public DelegateCommand<MenuStructureProxy> NewMenuCommand { get; private set; }

        private bool CanExecuteNewMenuCommand(MenuStructureProxy parent)
        {
            return true;
        }

        private void ExecuteNewMenuCommand(MenuStructureProxy parent)
        {
            //var parentCat = await MenuCategoryService.GetGraphAsync(parent.Id, SessionContext.ActiveCompanyGroupId);
            //var newMenu = Menu.CreateNewMenu(parentCat);
            //newMenu.CompanyGroupId = SessionContext.ActiveCompanyGroupId;

            //var view = typeof(Views.MenuView).FullName;
            //var uri = new Uri(view, UriKind.Relative);
            //RegionManager.RequestNavigate(RegionNames.DocumentRegion, uri,
            //new NavigationParameters
            //{
            //        {
            //            NavigationParametersNames.IdNavigationParameter, new GuidNavigationParameter
            //            {
            //                Id = item.Id
            //            }
            //        }
            //});
        }

        //ReloadCommand
        public DelegateCommand ReloadCommand { get; private set; }

        private bool CanExecuteReloadCommand()
        {
            return CanExecuteOpenCommand();
        }

        private async void ExecuteReloadCommand()
        {
            await LoadStructureFromDataSorage();
        }

        #endregion //Commands

        #region Properties

        public IRegionManager RegionManager { get; set; }        

        public NotifyTask Initialization
        {
            get => _initialization;
            set => SetProperty(ref _initialization, value);
        }

        private IDialogService DialogService { get; set; }

        #endregion

        #region Methods

        private async Task SaveAsync(object structureElement)
        {
            try
            {
                using (var db = new CulinaryDbContext())
                {
                    db.ChangeTracker.AutoDetectChangesEnabled = false;

                    db.ChangeTracker.TrackGraph(structureElement, EFChangeTrackerHelper.TrackEntity);
                    await db.SaveChangesAsync();

                    await LoadStructureFromDataSorage();
                    IsDirty = false;

                    RaiseCanExecuteCommands();
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                Notify(ex.Message);
            }
            finally { Processing = false; }
        }

        public ObservableCollection<MenuStructureProxy> MenuStructure { get; set; }

        private async Task InitializeAsync()
        {
            await LoadStructureFromDataSorage();
            LeftSideItem = MenuStructure.FirstOrDefault();

            if (LeftSideItem != null)
            {
                var subItems = MenuStructure
                    .Where(subItem => subItem.ParentId == LeftSideItem.Id);

                SubItems = new ObservableCollection<MenuStructureProxy>(subItems);
                NavigateToProperties(Constants.CategoryCompaniesView);
            }
        }

        private async Task LoadStructureFromDataSorage()
        {
            var leftSideId = LeftSideItem?.Id;
            var rightSideId = RightSideItem?.Id;

            var items = await _provider.GetStructureAsync();
            MenuStructure = new ObservableCollection<MenuStructureProxy>(items);
            SubItems?.Clear();
            RaisePropertyChanged(nameof(MenuStructure));

            if (leftSideId != null)
                LeftSideItem = MenuStructure
                    .FirstOrDefault(e => e.Id == leftSideId);

            if (LeftSideItem != null)
            {
                var subItems = MenuStructure
                    .Where(subItem => subItem.ParentId == LeftSideItem.Id);

                SubItems = new ObservableCollection<MenuStructureProxy>(subItems);                
            }

            if (rightSideId != null)
                RightSideItem = MenuStructure
                    .FirstOrDefault(e => e.Id == rightSideId);

        }

        protected override void RaiseCanExecuteCommands()
        {
            ReloadCommand.RaiseCanExecuteChanged();
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            if(CanExecuteOpenCommand())
                Initialization = NotifyTask.Create(InitializeAsync);
        }
        
        #endregion
    }
}
using System.Windows;
using Apollo.Common;
using Apollo.Infrastructure;
using Apollo.Infrastructure.Dialogs;
using Apollo.Infrastructure.Documents;
using Apollo.Infrastructure.Prism;
using Apollo.Infrastructure.Services;
using Apollo.Modularity;
using Apollo.Views;
using DevExpress.Xpf.Docking;
using DevExpress.Xpf.NavBar;
using DevExpress.Xpf.Ribbon;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Logging;
using Prism.Modularity;
using Prism.Regions;
using Prism.Unity;

namespace Apollo
{
	public class Bootstrapper : UnityBootstrapper
	{
		private readonly IProgressAsync<string> ProgressAsync;

		public Bootstrapper(IProgressAsync<string> progressAsync = null)
			: base()
		{
			ProgressAsync = progressAsync;
		}

		protected override void ConfigureContainer()
		{
			base.ConfigureContainer();

			Container.RegisterType<IAppUpdater, AppUpdater>()
					 .RegisterType<IDialogService, DialogService>()
					 .RegisterType<IMessageBoxService, MessageBoxService>()
					 .RegisterType<IViewSettingsViewModel, ViewSettingsViewModel>()
					 .RegisterType<INotificationsManager, NotificationManager>(new ContainerControlledLifetimeManager())
					 .RegisterType<IRegionNavigationContentLoader, ScopedRegionNavigationContentLoader>(new ContainerControlledLifetimeManager());
		}

		protected override DependencyObject CreateShell()
		{
			return Container.Resolve<Shell>();
		}

		protected override void InitializeShell()
		{
			base.InitializeShell();

			//Services
			Container.RegisterType<IAuthenticationService, AuthenticationService>()
					 //Navigation
					 .RegisterTypeForNavigation<LoginView>(Constants.LoginView)
					 .RegisterTypeForNavigation<DocumentView>(Constants.DocumentView)
					 .RegisterTypeForNavigation<AboutView>(Constants.AboutView)
					 .RegisterTypeForNavigation<UserPasswordChangeView>(Constants.UserPasswordChangeView)
					 .RegisterTypeForNavigation<SetupServersSettingsView>(Constants.SetupServersSettingsView);

			App.Current.MainWindow = Shell as Window;
			App.Current.MainWindow.WindowState = WindowState.Maximized;
			App.Current.MainWindow.Loaded += (s, evt) =>
			{
				var regionManager = Container.Resolve<IRegionManager>();
				regionManager.Regions[RegionNames.ContentRegion].Add(Container.Resolve<LoginView>(), Constants.LoginView);
			};
		}

		protected override RegionAdapterMappings ConfigureRegionAdapterMappings()
		{
			var mappings = base.ConfigureRegionAdapterMappings();
			mappings.RegisterMapping(typeof(NavBarControl), ServiceLocator.Current.GetInstance<DxNavBarRegionAdapter>());
			mappings.RegisterMapping(typeof(RibbonControl), ServiceLocator.Current.GetInstance<DxRibbonRegionAdapter>());
			mappings.RegisterMapping(typeof(DocumentGroup), ServiceLocator.Current.GetInstance<DocumentGroupAdapter>());
			mappings.RegisterMapping(typeof(DocumentPanel), ServiceLocator.Current.GetInstance<DocumentPanelRegionAdapter>());

			return mappings;
		}

		protected override IModuleCatalog CreateModuleCatalog()
		{
			return new PrismDirectoryModuleCatalog(ProgressAsync);
		}

		protected override IRegionBehaviorFactory ConfigureDefaultRegionBehaviors()
		{
			var behaviors = base.ConfigureDefaultRegionBehaviors();
			behaviors.AddIfMissing(RibbonRegionBehavior.BehaviorKey, typeof(RibbonRegionBehavior));
			behaviors.AddIfMissing(RegionManagerAwareBehavior.BehaviorKey, typeof(RegionManagerAwareBehavior));
			behaviors.AddIfMissing(DisposableRegionBehavior.BehaviorKey, typeof(DisposableRegionBehavior));

			return behaviors;
		}

		protected override ILoggerFacade CreateLogger()
		{
			return new Log4NetLogger();
		}
	}
}
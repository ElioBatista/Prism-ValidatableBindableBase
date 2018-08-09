using Apollo.Common;
using Apollo.Common.Extensions;
using Apollo.Core;
using Apollo.Core.Extensions;
using Apollo.Infrastructure;
using Apollo.Infrastructure.Dialogs;
using Apollo.Security;
using Apollo.Survey.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Logging;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace Apollo.Survey.Views
{
    public class QuestionViewModel : NavigationAwareViewModelBase
    {
        private Question _question;
        private Guid _companyGroupId;
        private ILoggerFacade _logger;
        private Object _thisLock = new Object();
        private bool _hasError;
        private Expression<Func<Question, bool>> _whereClause;

        public QuestionViewModel(IEventAggregator eventAggregator,
            IMessageBoxService messageBoxService,
            IRegionManager regionManager,
            IDialogService dialogService,
            IUnityContainer unityContainer,
            ILoggerFacade logger)
            : base(eventAggregator, messageBoxService)
        {
            _logger = logger;

            DialogService = dialogService;

            //Commands
            NewCommand = new DelegateCommand(ExecuteNewCommand, CanExecuteNewCommand).ObservesProperty(() => Question);

            OpenCommand = new DelegateCommand(ExecuteOpenCommand, CanExecuteOpenCommand).ObservesProperty(() => QuestionNo);

            CloseCommand = new DelegateCommand(ExecuteCloseCommand, CanExecuteCloseCommand)
                .ObservesProperty(() => Question);

            SaveCommand = new DelegateCommand(ExecuteSaveCommand, CanExecuteSaveCommand)
                .ObservesProperty(() => Question)
                .ObservesProperty(() => IsDirty)
                .ObservesProperty(() => HasError);

            DeleteCommand = new DelegateCommand(ExecuteDeleteCommand, CanExecuteDeleteCommand)
                .ObservesProperty(() => Question);

            ReloadCommand = new DelegateCommand(ExecuteReloadCommand, CanExecuteReloadCommand)
                .ObservesProperty(() => Question);

            //Pager
            NextPageCommand = new DelegateCommand(ExecuteNextPageCommand, CanExecuteNextPageCommand)
                .ObservesProperty(() => Question)
                .ObservesProperty(() => Processing);
            PreviousPageCommand = new DelegateCommand(ExecutePreviousPageCommand, CanExecutePreviousPageCommand)
                .ObservesProperty(() => Question)
                .ObservesProperty(() => Processing);
            FirstPageCommand = new DelegateCommand(ExecuteFirstPageCommand, CanExecuteFirstPageCommand)
                .ObservesProperty(() => Question)
                .ObservesProperty(() => Processing);
            LastPageCommand = new DelegateCommand(ExecuteLastPageCommand, CanExecuteLastPageCommand)
                .ObservesProperty(() => Question)
                .ObservesProperty(() => Processing);

            SearchCommand = new DelegateCommand(ExecuteSearchCommand, CanExecuteSearchCommand)
                .ObservesProperty(() => Processing);

            PostCommand = new DelegateCommand(ExecutePostCommand);

            PrintCommand = new DelegateCommand(ExecutePrintCommand, CanExecutePrintCommand)
                .ObservesProperty(() => Question)
                .ObservesProperty(() => HasError);

            _companyGroupId = Guid.Parse(CacheLayer.Instance.Get<string>("CompanyGroupId"));
            _whereClause = (subject) =>
             !subject.IsDeleted && subject.CompanyGroupId.Equals(_companyGroupId);


            Title = Resources.SurveyResourceStrings.Question;
        }

        private void ExecuteOpenCommand()
        {
            Open();
            if (Question == null)
            {
                MessageBoxService.Show(Resources.SurveyResourceStrings.Question,
                        $"{Resources.SurveyResourceStrings.QuestionNotFound} {QuestionNo}",
                        MessageBoxButtons.Ok,
                        MessageBoxResult.Ok);
                return;
            }
        }

        private bool CanExecuteOpenCommand()
        {
            return CanExecuteProcess(Constants.ModuleName, ProcessNames.QuestionManager, PermissionType.View);
        }

        #region Commands

        public DelegateCommand OpenCommand { get; set; }
        public DelegateCommand NewCommand { get; private set; }
        public DelegateCommand SaveCommand { get; private set; }

        //CloseCommand
        public DelegateCommand CloseCommand { get; private set; }
        private bool CanExecuteCloseCommand()
        {
            return Question != null;
        }
        private void ExecuteCloseCommand()
        {
            var canClose = CanClose();
            if (canClose)
            {
                Unwire();
                Close();
            }
        }

        //ReloadCommand
        public DelegateCommand ReloadCommand { get; private set; }
        private bool CanExecuteReloadCommand()
        {
            return Question != null;
        }
        private void ExecuteReloadCommand()
        {
            var canClose = CanClose();
            if (canClose)
            {
                var id = Question.Id;
                Unwire();
                _question = null;
                Open(id);
            }
        }

        //DeleteCommand
        public DelegateCommand DeleteCommand { get; private set; }
        private bool CanExecuteDeleteCommand()
        {
            return Question != null &&
                CanExecuteProcess(Constants.ModuleName, ProcessNames.QuestionManager, PermissionType.Delete);
        }
        private async void ExecuteDeleteCommand()
        {
            if (!Confirm(Resources.SurveyResourceStrings.Question_ConfirmDelete))
                return;

            using (var db = new SurveyDbContext())
            {
                if (Question.IsNewState)
                {
                    Close();
                }
                else
                {
                    Question.SetAsDeleted();
                    db.ChangeTracker.TrackGraph(Question, EFChangeTrackerHelper.TrackEntity);
                    await db.SaveChangesAsync();
                    db.SaveChanges();
                    Close();
                }
            }
        }
        //NextPageCommand
        public DelegateCommand NextPageCommand { get; private set; }
        private bool CanExecuteNextPageCommand()
        {
            return Question != null;
        }
        private void ExecuteNextPageCommand()
        {
            MoveTo(GetNextBusinessId());
        }

        //PreviousPageCommand
        public DelegateCommand PreviousPageCommand { get; private set; }
        private bool CanExecutePreviousPageCommand()
        {
            return Question != null;
        }
        private void ExecutePreviousPageCommand()
        {
            MoveTo(GetPrevBusinessId());
        }
        //FirstPageCommand
        public DelegateCommand FirstPageCommand { get; private set; }
        private bool CanExecuteFirstPageCommand()
        {
            return true;
        }
        private void ExecuteFirstPageCommand()
        {
            MoveTo(GetFirstBusinessId());
        }

        //LastPageCommand
        public DelegateCommand LastPageCommand { get; private set; }
        private bool CanExecuteLastPageCommand()
        {
            return true;
        }
        private void ExecuteLastPageCommand()
        {
            MoveTo(GetLastBusinessId());
        }

        private void ExecuteNewCommand()
        {
            var canClose = CanClose();
            if (Question == null || canClose)
            {
                CreateNewEntity();
            }
        }

        private bool CanExecuteNewCommand()
        {
            return CanExecuteProcess(Constants.ModuleName, ProcessNames.QuestionManager, PermissionType.Add);
        }
        private void CreateNewEntity()
        {
            QuestionNo = null;
            Question = Question.CreateNew<Question>();
            Question.CompanyGroupId = _companyGroupId;
            Question.IsActive = true;
            Question.ValidateProperties();
            IsDirty = true;
            _shouldUpdateSurveyTemplateQuestions = false;
            RefreshAll();
            RaiseCanExecuteCommands();
        }

        private bool CanExecuteSaveCommand()
        {
            if (Question == null)
                return false;

            Question.ValidateProperties();

            return !Question.HasErrors &&
                IsDirty &&
                CanExecuteProcess(Constants.ModuleName, ProcessNames.QuestionManager, PermissionType.Update);
        }

        private async void ExecuteSaveCommand()
        {
            try
            {
                using (var db = new SurveyDbContext())
                {
                    db.ChangeTracker.AutoDetectChangesEnabled = false;

                    if (Question.PersistentState == PersistentState.Added)
                    {
                        var companyGroupSiteId = new Guid(CacheLayer.Instance.Get("CompanyGroupSiteId").ToString());

                        long currentNo = 1;

                        var currentCounter = db.Counters
                            .Where(e => e.Type == 14)
                            .Where(e => e.CompanyGroupSiteId == companyGroupSiteId)
                            .FirstOrDefault();

                        if (currentCounter != null)
                        {
                            currentCounter.Current = currentCounter.Current + 1;
                            currentNo = currentCounter.Current;
                        }
                        else
                        {
                            currentNo = await db.Questions
                                .MaxAsync(e => e.QuestionNo);

                            currentNo = currentNo + 1;

                            currentCounter = Counter.CreateNew<Counter>();
                            currentCounter.CompanyGroupSiteId = companyGroupSiteId;
                            currentCounter.Current = currentNo;
                            currentCounter.Type = 14;
                        }

                        Question.QuestionNo = currentNo;
                        db.ChangeTracker.TrackGraph(currentCounter, EFChangeTrackerHelper.TrackEntity);
                    }

                    db.ChangeTracker.TrackGraph(Question, EFChangeTrackerHelper.TrackEntity);

                    Action callback = null;
                    var parameters = new object[] { Thread.CurrentPrincipal.Identity.Name, DateTime.Now, Question.Id };
                    var cmd = "UPDATE Survey.SurveyTemplateQuestion SET LastModifiedUser = {0}, LastModifiedDate = {1} WHERE QuestionKey = {2};";
                    if (_shouldUpdateSurveyTemplateQuestions)
                    {
                        callback = async () =>
                        {
                            //var parameters = new object[] { Thread.CurrentPrincipal.Identity.Name, DateTime.Now, Question.Id };
                            await db.Database.ExecuteSqlCommandAsync(
                                "UPDATE Survey.SurveyTemplateQuestion SET LastModifiedUser = {0}, LastModifiedDate = {1} WHERE QuestionKey = {2};"
                                , default(CancellationToken)
                                , parameters);
                        };

                    }

                    await db.SaveChangesAsync(cmd,default(CancellationToken), parameters);

                    IsDirty = false;
                    _shouldUpdateSurveyTemplateQuestions = false;
                    QuestionNo = Question.QuestionNo;
                    RaisePropertyChanged(nameof(QuestionNo));
                    RaiseCanExecuteCommands();
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Notify("Apollo ERP", ex.GetFriendly().Message);
            }
            catch (Exception ex)
            {
                Notify("Error", ex.Message);
            }
            finally
            {
                Processing = false;
            }
        }

        public DelegateCommand SearchCommand { get; private set; }

        private bool CanExecuteSearchCommand()
        {
            return true;
        }
        private void ExecuteSearchCommand()
        {
            string view = Constants.QuestionSearchView;
            var selectionView = DialogService.ShowSelectDialog(view,true);

            if (selectionView.SelectionConfirmed)
            {
                var selection = ((List<QuestionProxy>)selectionView.Selection)[0];
                Open(selection.Id);
            }
        }
        public DelegateCommand PrintCommand { get; }
        private bool CanExecutePrintCommand()
        {
            return Payloaded && !Question.HasErrors && !Question.IsNewState;
        }
        private void ExecutePrintCommand()
        {
        }

        public DelegateCommand PostCommand { get; private set; }
        private void ExecutePostCommand()
        {

        }

        #endregion //Commands

        #region Properties

        public bool HasError
        {
            get { return _hasError; }
            set { SetProperty(ref _hasError, value); }
        }

        public long? QuestionNo { get; set; }

        public Question Question
        {
            get { return _question; }
            set
            {
                if (SetProperty(ref _question, value))
                {
                    if (Question != null)
                        Question.PropertyChanged += Question_PropertyChanged;
                }
            }
        }
        private IDialogService DialogService { get; set; }
        private IUnityContainer UnityContainer { get; set; }
        public bool Payloaded { get { return Question != null; } }

        public bool CanEdit
        {
            get
            {
                return Payloaded &&
                        CanExecuteProcess(Constants.ModuleName, ProcessNames.QuestionManager, PermissionType.Update);
            }
        }
        public IEnumerable<EntityProxy> Categories
        {
            get
            {
                using (var db = new SurveyDbContext())
                {
                    var retVal = (from item in db.QuestionCategories
                                  where !item.IsDeleted && item.IsActive && item.CompanyGroupId.Equals(_companyGroupId)
                                  orderby item.Code
                                  select new EntityProxy
                                  {
                                      Id = item.Id,
                                      Code = item.Code,
                                      Name = item.Name
                                  })
                                  .ToList();

                    return retVal;
                }
            }
        }
        public IEnumerable<EntityProxy> Types
        {
            get
            {
                using (var db = new SurveyDbContext())
                {
                    var retVal = (from item in db.QuestionTypes
                                  where item.CompanyGroupId.Equals(_companyGroupId)
                                  orderby item.Code
                                  select new EntityProxy
                                  {
                                      Id = item.Id,
                                      Code = item.Code,
                                      Name = item.Name
                                  })
                                  .ToList();

                    return retVal;
                }
            }
        }

        #endregion

        #region Methods

        private void MoveTo(Guid? businessKey)
        {
            if (!ValidMove(businessKey))
                return;

            var canClose = CanClose();
            if (Question == null || canClose)
            {
                Open(businessKey);
            }
        }

        private bool ValidMove(Guid? businessKey)
        {
            if (!businessKey.IsEmpty() && (Question == null || businessKey != Question.Id))
                return true;

            return false;
        }

        private Guid GetNextBusinessId()
        {
            using (var db = new SurveyDbContext())
            {
                var retVal = db.Questions
                    .Where(e => e.QuestionNo > Question.QuestionNo)
                    .Where(_whereClause)
                    .OrderBy(e => e.QuestionNo)
                    .Select(e => e.Id)
                    .FirstOrDefault();

                return retVal;
            }
        }

        private Guid GetPrevBusinessId()
        {
            using (var db = new SurveyDbContext())
            {
                return db.Questions
                    .Where(e => e.QuestionNo < Question.QuestionNo)
                    .Where(_whereClause)
                    .OrderByDescending(e => e.QuestionNo)
                    .Select(e => e.Id)
                    .FirstOrDefault();
            }
        }

        private Guid GetLastBusinessId()
        {
            using (var db = new SurveyDbContext())
            {
                return db.Questions
                    .Where(_whereClause)
                    .OrderByDescending(m => m.QuestionNo)
                    .Select(e => e.Id)
                    .FirstOrDefault();
            }
        }
        private Guid GetFirstBusinessId()
        {
            using (var db = new SurveyDbContext())
            {
                return db.Questions
                    .Where(_whereClause)
                    .OrderBy(e => e.QuestionNo)
                    .Select(e => e.Id)
                    .FirstOrDefault();
            }
        }
        private void Open(Guid? oid = null)
        {
            Processing = true;
            Unwire();
            IsDirty = false;
            _shouldUpdateSurveyTemplateQuestions = false;
            Question theQuestion = null;
            using (var db = new SurveyDbContext())
            {
                Expression<Func<Question, bool>> findBy;
                if (oid.HasValue)
                    findBy = (subject) => subject.Id.Equals(oid.Value);
                else
                    findBy = (subject) => subject.QuestionNo == QuestionNo;

                Expression<Func<Question, bool>> whereClause = findBy.CombineWithAnd(_whereClause);

                theQuestion = db.Questions
                    .Where(whereClause)
                    .FirstOrDefault();
            }

            Question = theQuestion;
            if (Question == null)
            {
                Processing = false;
                RefreshAll();
                return;
            }

            QuestionNo = Question.QuestionNo;
            Processing = false;
            RefreshAll();
        }

        private void RefreshAll()
        {
            RaisePropertyChanged(nameof(Question));
            RaisePropertyChanged(nameof(Payloaded));
            RaisePropertyChanged(nameof(CanEdit));
            RaisePropertyChanged(nameof(QuestionNo));
        }

        private bool CanClose()
        {
            return (Question != null && !IsDirty) || (IsDirty && DiscardChanges());
        }

        private void Unwire()
        {
            if (Question != null)
                Question.PropertyChanged -= Question_PropertyChanged;
        }

        private void Close()
        {
            Question = null;
            QuestionNo = null;
            IsDirty = false;
            RaisePropertyChanged(nameof(QuestionNo));
            RaisePropertyChanged(nameof(CanEdit));
        }

        private bool _shouldUpdateSurveyTemplateQuestions = false;
        private void Question_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(Question.IsDirty)))
            {
                IsDirty = Question.IsDirty;
            }
            else if (e.PropertyName.Equals(nameof(Question.HasErrors)))
            {
                HasErrors = Question.HasErrors;
            }
            else if (e.PropertyName.Equals(nameof(Question.QuestionTypeId)))
            {
                _shouldUpdateSurveyTemplateQuestions = true;
            }
            RaiseCanExecuteCommands();
        }

        protected override void RaiseCanExecuteCommands()
        {
            OpenCommand.RaiseCanExecuteChanged();
            CloseCommand.RaiseCanExecuteChanged();
            //NextPageCommand.RaiseCanExecuteChanged();
            //PreviousPageCommand.RaiseCanExecuteChanged();
            //FirstPageCommand.RaiseCanExecuteChanged();
            //LastPageCommand.RaiseCanExecuteChanged();
            SaveCommand.RaiseCanExecuteChanged();
            NewCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
            ReloadCommand.RaiseCanExecuteChanged();
        }

        private bool DiscardChanges()
        {
            var discard = MessageBoxService.Show(Resources.SurveyResourceStrings.Question,
                        Resources.SurveyResourceStrings.UnsavedChanges_Message,
                        MessageBoxButtons.YesNo,
                        MessageBoxResult.No);
            return discard == InteractionResult.Yes;
        }

        #endregion

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            var businessKey = navigationContext.Parameters[NavigationParametersNames.WorkItem];
            if (businessKey != null)
            {
                QuestionNo = (long)businessKey;
                Open();
            }
        }
    }
}
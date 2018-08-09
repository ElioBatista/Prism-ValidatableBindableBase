using Apollo.Common;
using Apollo.Common.Extensions;
using Apollo.Core;
using Apollo.Infrastructure;
using Apollo.Infrastructure.Dialogs;
using Apollo.Survey.Core;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Apollo.Survey.Views
{
    public class QuestionSearchViewModel : NavigationAwareViewModelBase, IDialogAware, ISelectViewModel
    {
        private Guid _companyGroupId;
        private List<EntityProxy> _categories;

        public QuestionSearchViewModel(IEventAggregator eventAggregator, IMessageBoxService messageBoxService)
            : base(eventAggregator, messageBoxService)
        {
            PostCommand = new DelegateCommand(ExecutePostCommand);
            ResetCommand = new DelegateCommand(ExecuteResetCommand);
            SearchCommand = new DelegateCommand(ExecuteSearchCommand);
            GoBackToFilterCommand = new DelegateCommand(ExecuteGoBackToFilterCommand);
            Title = Resources.SurveyResourceStrings.QuestionSearch;
            CancelCommand = new DelegateCommand(ExecuteCancelCommand);
            _companyGroupId = Guid.Parse(CacheLayer.Instance.Get<string>("CompanyGroupId"));
        }

        #region Commands

        public DelegateCommand PostCommand { get; private set; }
        private void ExecutePostCommand()
        {
            if (Selection != null)
                SelectionConfirmed = true;

            RequestClose?.Invoke();
        }

        public DelegateCommand ResetCommand { get; private set; }
        private void ExecuteResetCommand()
        {
            QuestionNo = null;
            QuestionText = string.Empty;
            Frequency = Frequency.Undefined;
            Notification = "Any";
            Compliance = "Any";
            MatchStrategy = "All";

            RaisePropertyChanged(nameof(QuestionNo));
            RaisePropertyChanged(nameof(QuestionText));
            RaisePropertyChanged(nameof(Frequency));
            RaisePropertyChanged(nameof(Notification));
            RaisePropertyChanged(nameof(Compliance));
            RaisePropertyChanged(nameof(MatchStrategy));
        }

        public DelegateCommand GoBackToFilterCommand { get; private set; }
        private void ExecuteGoBackToFilterCommand()
        {
            SelectedTabIndex = 0;
            RaisePropertyChanged(nameof(SelectedTabIndex));
        }

        public DelegateCommand SearchCommand { get; private set; }
        private void ExecuteSearchCommand()
        {
            using (var db = new SurveyDbContext())
            {
                Expression<Func<Question, bool>> criterias = BuildWhereExpression();

                var retVal = db.Questions
                    .Where(e => e.IsActive && !e.IsDeleted)
                    .Where(criterias)
                    .Select(e => new QuestionProxy
                    {
                        Id = e.Id
                        ,
                        QuestionNo = e.QuestionNo
                        ,
                        QuestionText = e.QuestionText
                        ,
                        Frequency = e.Frequency.ToString()
                        ,
                        Notification = e.Notification
                        ,
                        Compliance = e.Compliance
                        ,
                        CreationUser = e.CreationUser
                        ,
                        CreationDate = e.CreationDate
                        ,
                        LastModifiedUser = e.LastModifiedUser
                        ,
                        LastModifiedDate = e.LastModifiedDate
                    })
                    .ToList();

                SearchResult = retVal;
                SelectedTabIndex = 1;
                RaisePropertyChanged(nameof(SearchResult));
                RaisePropertyChanged(nameof(SelectedTabIndex));
            }
        }
        public DelegateCommand CancelCommand { get; private set; }
        private void ExecuteCancelCommand()
        {
            RequestClose();
        }
        private Expression<Func<Question, bool>> BuildWhereExpression()
        {
            List<Expression<Func<Question, bool>>> criterias = new List<Expression<Func<Question, bool>>>();
            if (QuestionNo.HasValue)
            {
                Expression<Func<Question, bool>> questionNoCriteria = (subject) => subject.QuestionNo == QuestionNo;
                criterias.Add(questionNoCriteria);
            }

            if (!string.IsNullOrEmpty(QuestionText))
            {
                Expression<Func<Question, bool>> questionTextCriteria = (subject) => subject.QuestionText.Contains(QuestionText);
                criterias.Add(questionTextCriteria);
            }

            if (SelectedCategory != null)
            {
                Expression<Func<Question, bool>> categoryCriteria = (subject) => subject.QuestionCategoryId.Equals(SelectedCategory.Id);
                criterias.Add(categoryCriteria);
            }

            if (Frequency != Frequency.Undefined)
            {
                Expression<Func<Question, bool>> frequencyCriteria = (subject) => subject.Frequency == Frequency;
                criterias.Add(frequencyCriteria);
            }

            if (!Notification.Equals("Any", StringComparison.InvariantCultureIgnoreCase))
            {
                Expression<Func<Question, bool>> notificationCriteria = (subject) =>
                    Notification.Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ? subject.Notification : !subject.Notification;
                criterias.Add(notificationCriteria);
            }

            if (!Compliance.Equals("Any", StringComparison.InvariantCultureIgnoreCase))
            {
                Expression<Func<Question, bool>> complianceCriteria = (subject) =>
                    Compliance.Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ? subject.Compliance : !subject.Compliance;
                criterias.Add(complianceCriteria);
            }

            if (SelectedSettings != QuestionSettings.None)
            {
                Expression<Func<Question, bool>> settingsCriteria = (subject) =>
                    subject.Flags == ( subject.Flags | this.SelectedSettings )  ;
                criterias.Add(settingsCriteria);
            }
            
            return MatchStrategy.Equals("All", StringComparison.InvariantCultureIgnoreCase)
                ? criterias.CombineWithAnd()
                : criterias.CombineWithOr();
        }

        #endregion

        #region Properties
        public QuestionProxy SelectedItem { get; set; }
        public long? QuestionNo { get; set; }
        public string QuestionText { get; set; }
        public Frequency Frequency { get; set; } = Frequency.Undefined;
        public string Notification { get; set; } = "Any";
        public string Compliance { get; set; } = "Any";
        public QuestionSettings SelectedSettings { get; set; } = QuestionSettings.None;
        public int SelectedTabIndex { get; set; }
        public string MatchStrategy { get; set; } = "All";
        public EntityProxy SelectedCategory { get; set; }
        public IEnumerable<EntityProxy> Categories
        {
            get
            {
                if (_companyGroupId == Guid.Empty)
                    return null;

                using (var db = new SurveyDbContext())
                {
                    if (_categories != null)
                        return _categories;

                    _categories = (from item in db.QuestionCategories
                                  where !item.IsDeleted && item.CompanyGroupId.Equals(_companyGroupId)
                                  orderby item.Code
                                  select new EntityProxy
                                  {
                                      Id = item.Id,
                                      Code = item.Code,
                                      Name = item.Name
                                  })
                                  .ToList();

                    return _categories;
                }
            }
        }
        public IEnumerable<QuestionProxy> SearchResult { get; set; }

        #endregion

        #region Methods


        #endregion

        #region Base Overrides

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            object objQuestionCategoryId = navigationContext.Parameters[NavigationParametersNames.QuestionCategoryId];
            if (objQuestionCategoryId != null)
            {
                var questionCategoryId = Guid.Parse(navigationContext.Parameters[NavigationParametersNames.QuestionCategoryId].ToString());
                SelectedCategory = Categories.FirstOrDefault(e=>e.Id == questionCategoryId);
                RaisePropertyChanged(nameof(SelectedCategory));
            }
        }

        #endregion

        #region IDialogAware Members

        public bool CanCloseDialog()
        {
            return true;
        }

        public event Action RequestClose;

        public void OnLoaded() { }

        #endregion //IDialogAware Members

        #region ISelectViewModel Members
        public object Selection { get; set; } = new List<QuestionProxy>();
        public bool SelectionConfirmed { get; set; } = false;

        #endregion //ISelectViewModel Members
    }
}
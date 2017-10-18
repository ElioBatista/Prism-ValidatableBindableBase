using Apollo.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Collections.ObjectModel;
using Apollo.Core.Validators;

namespace Apollo.Survey.Core
{
    [SurveyTemplateRules]
    public class SurveyTemplate : ValidatableBindablePersistentEntity, IAuditAware, ISoftDeleteAware
    {
        private long _surveyTemplateNo = 0;
        private string _name = string.Empty;
        private SurveyTemplateSettings _flags = 0;
        private Orientation _orientation = Orientation.Undefined;
        private Navigation _navigation = Navigation.Undefined;
        private Frequency _frequency = Frequency.Undefined;
        private Guid _concessionCompanyId = Guid.Empty;

        private Guid _departmentId = Guid.Empty;
        private bool _isActive;
        private bool _isDeleted;
        private int _totalWeight;

        public SurveyTemplate()
            : base()
        {            
            Details = new HashSet<SurveyTemplateQuestion>();
        }

        #region Properties
        
        [Required]
        public long SurveyTemplateNo
        {
            get { return _surveyTemplateNo; }
            set { SetProperty(ref this._surveyTemplateNo, value); }
        }
        [Required, StringLength(50, MinimumLength = 2)]
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref this._name, value); }
        }
        public SurveyTemplateSettings Flags
        {
            get { return _flags; }
            set { SetProperty(ref this._flags, value); }
        }
        public Orientation Orientation
        {
            get { return _orientation; }
            set { SetProperty(ref this._orientation, value); }
        }
        public Navigation Navigation
        {
            get { return _navigation; }
            set { SetProperty(ref this._navigation, value); }
        }
        public Frequency Frequency
        {
            get { return _frequency; }
            set { SetProperty(ref this._frequency, value); }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set { SetProperty(ref this._isActive, value); }

        }
        public bool IsDeleted
        {
            get { return _isDeleted; }
            set { SetProperty(ref this._isDeleted, value); }

        }

        [RequiredGuid]
        public Guid ConcessionCompanyId
        {
            get { return _concessionCompanyId; }
            set {
                var result = SetProperty(ref _concessionCompanyId, value);
                if (result)
                    DepartmentId = Guid.Empty;
            }
        }
        public ConcessionCompany ConcessionCompany { get; set; }

        [RequiredGuid]
        public Guid DepartmentId
        {
            get { return _departmentId; }
            set { SetProperty(ref _departmentId, value); }
        }

        public virtual ICollection<SurveyTemplateQuestion> Details { get; set; }
                
        [IgnorePersistentState]
        public int TotalWeight
        {
            get { return _totalWeight; }
            private set { SetProperty(ref _totalWeight, value); }
        }

        #endregion //Properties

        #region Methods

        public SurveyTemplateQuestion CreateDetail()
        {
            var result = CreateNew<SurveyTemplateQuestion>();            
            AddDetail(result);
            return result;
        }

        public void AddDetail(SurveyTemplateQuestion item)
        {
            item.SurveyTemplateId = this.Id;
            item.SurveyTemplate = this;
            item.IsActive = true;
            item.CreationUser = System.Threading.Thread.CurrentPrincipal.Identity.Name;
            item.CreationDate = DateTime.Now;            

            Details.Add(item);
            SetAsModified();
        }
        public void SetDetailsRowNumber(IEnumerable<SurveyTemplateQuestion> items, int startNumber)
        {
            items.ToList().ForEach(item => {                
                item.RowNumber = (short)startNumber;
                startNumber = startNumber + 1;
            });
            SetAsModified();
        }
        public void SetDetailRowNumber(SurveyTemplateQuestion item, int newRowNumber)
        {
            item.RowNumber = (short)newRowNumber;
            SetAsModified();
        }
        public void IncrementDetailsFrom(int offset, int startRow)
        {
            var tmpDetails = this.Details
                .Where(d => !d.IsDeletedState)
                .Where(d => d.RowNumber >= startRow)
                .OrderBy(d => d.RowNumber)
                .ToList();
                        
            tmpDetails.ForEach(item =>
            {
                int newValue = item.RowNumber + offset;
                item.RowNumber = (short)newValue;                
            });
            SetAsModified();
        }
        public void RemoveDetail(SurveyTemplateQuestion item)
        {
            if (item.PersistentState == PersistentState.Added)
            {
                Details.Remove(item);
            }
            else
            {
                item.SetAsDeleted();
            }

            //Renumber
            int removedRowMumber = item.RowNumber;
            if (removedRowMumber == 0)
                return;

            var tmpDetails = this.Details
                .Where(d => !d.IsDeletedState)
                .Where(d => d.RowNumber > removedRowMumber)
                .OrderBy(d => d.RowNumber)
                .ToList();

            int newValue = removedRowMumber;
            tmpDetails.ForEach(nextItem =>
            {
                nextItem.RowNumber = (short)newValue;
                newValue = newValue + 1;
            });

            CalculateTotalWeight();
            SetAsModified();
        }
        public void RemoveDetails(List<SurveyTemplateQuestion> items)
        {
            if (items == null || items.Count == 0)
                return;

            items.ForEach(item =>
            {
                if (item.PersistentState == PersistentState.Added)
                {
                    Details.Remove(item);
                }
                else
                {
                    item.SetAsDeleted();
                }
            });

            
            //Renumber
            int removedRowMumber = items.Min( d=>d.RowNumber );
            var tmpDetails = this.Details
                .Where(d => !d.IsDeletedState)
                .Where(d => d.RowNumber > removedRowMumber)
                .OrderBy(d => d.RowNumber)
                .ToList();

            int newValue = removedRowMumber;
            tmpDetails.ForEach(nextItem =>
            {
                nextItem.RowNumber = (short)newValue;
                newValue = newValue + 1;
            });

            CalculateTotalWeight();
            SetAsModified();
        }
        public int MoveDetailsAfter(List<SurveyTemplateQuestion> items, int precedingRowMumber)
        {
            var newValue = precedingRowMumber;
            items.ForEach(item => 
            {
                newValue = newValue + 1;
                item.RowNumber = (short)newValue;                
            });
            SetAsModified();

            return newValue;
        }
        public void SwapDetails(SurveyTemplateQuestion moved, SurveyTemplateQuestion displaced)
        {
            short movedNewNumber = displaced.RowNumber;
            short displacedNewNumber = moved.RowNumber;

            moved.RowNumber = movedNewNumber;
            displaced.RowNumber = displacedNewNumber;

            SetAsModified();
        }
        public void ConfigureDetail(SurveyTemplateQuestion detail, Question question, AreaSection areaSection)
        {
            AssignQuestion(detail, question);
            AssignAreaSection(detail, areaSection);
        }
        public void AssignQuestion(SurveyTemplateQuestion detail, Question question)
        {
            detail.QuestionId = question.Id;
            detail.Question = question;
            SetAsModified();
        }
        public bool AssignAreaSection(SurveyTemplateQuestion detail, AreaSection areaSection)
        {   
                     
            bool exists = Details.Any(e => e.AreaSectionId.Equals(areaSection.Id) && !e.IsDeletedState);

            //if (exists)
            //{
            //    return false;
            //}

            detail.AreaSectionId = areaSection.Id;
            detail.AreaSection = areaSection;
            SetAsModified();

            return !exists;
        }

        public void CalculateTotalWeight()
        {
            TotalWeight = Details.Where(d => !d.IsDeletedState).Sum(d=>d.Weight);
        }

        public override bool ValidateProperties()
        {
            base.ValidateProperties();

            //Aggregate Invariants 
            var results = new List<ValidationResult>();
            var context = new ValidationContext(this);
            var propertyErrors = new List<string>();

            Validator.TryValidateObject(this, context, results);
            if (results.Any())
            {
                propertyErrors.AddRange(results.Select(c => c.ErrorMessage));
            }

            ErrorsContainer.SetErrors(nameof(Details), propertyErrors);
            return ErrorsContainer.HasErrors;
        }

        public void MakeTransient()
        {
            SurveyTemplateNo = 0;
            SetAsNew();
            Id = Guid.Empty;
            Details.ToList().ForEach(item=> {
                item.MakeTrantient();
            });
        }

        public bool HasDetails()
        {
            bool has = this.Details.Where(d => !d.IsDeletedState).Count() > 0;
            return has;
        }

        public void ChangeAreaSectionTo(AreaSection newAreaSection, IEnumerable<SurveyTemplateQuestion> details)
        {
            details.ToList().ForEach((item) => 
            {
                item.AreaSectionId = newAreaSection.Id;
                item.AreaSection = newAreaSection;
                item.SetAsModified();
            });
            SetAsModified();
        }

        #endregion //Methods
    }
}

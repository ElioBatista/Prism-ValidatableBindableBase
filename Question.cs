using Apollo.Core;
using Apollo.Core.Validators;
using System;
using System.ComponentModel.DataAnnotations;

namespace Apollo.Survey.Core
{
    public class Question : ValidatableBindablePersistentEntity, IAuditAware, ISoftDeleteAware
    {
        private long _questionNo = 0;
        private string _questionText = string.Empty;

        private Frequency _frequency = Frequency.Undefined;
        private QuestionSettings _flags = 0;
        private bool _notification = false;
        private bool _compliance = false;
        private Guid _questionTypeId;
        private Guid _questionCategoryId;

        private string _customColumnLabel1 = string.Empty;
        private string _customColumnValue1 = string.Empty;
        private string _customColumnLabel2 = string.Empty;
        private string _customColumnValue2 = string.Empty;
        private string _customColumnLabel3 = string.Empty;
        private string _customColumnValue3 = string.Empty;

        private bool _isActive;
        private bool _isDeleted;

        public Question(){
            _flags = QuestionSettings.None;
        }

        [Required]
        public long QuestionNo
        {
            get { return _questionNo; }
            set { SetProperty(ref this._questionNo, value); }
        }
        [Required, StringLength(500)]
        public string QuestionText
        {
            get { return _questionText; }
            set { SetProperty(ref this._questionText, value??value.Trim()); }
        }

        public Frequency Frequency
        {
            get { return _frequency; }
            set { SetProperty(ref this._frequency, value); }
        }

        public QuestionSettings Flags
        {
            get { return _flags; }
            set { SetProperty(ref this._flags, value); }
        }
        public bool Notification
        {
            get { return _notification; }
            set { SetProperty(ref this._notification, value); }
        }
        public bool Compliance
        {
            get { return _compliance; }
            set { SetProperty(ref this._compliance, value); }
        }

        [StringLength(150)]
        public string CustomColumnLabel1
        {
            get { return _customColumnLabel1; }
            set { SetProperty(ref this._customColumnLabel1, value); }
        }

        [StringLength(150)]
        public string CustomColumnValue1
        {
            get { return _customColumnValue1; }
            set { SetProperty(ref this._customColumnValue1, value); }
        }

        [StringLength(150)]
        public string CustomColumnLabel2
        {
            get { return _customColumnLabel2; }
            set { SetProperty(ref this._customColumnLabel2, value); }
        }

        [StringLength(150)]
        public string CustomColumnValue2
        {
            get { return _customColumnValue2; }
            set { SetProperty(ref this._customColumnValue2, value); }
        }

        [StringLength(150)]
        public string CustomColumnLabel3
        {
            get { return _customColumnLabel3; }
            set { SetProperty(ref this._customColumnLabel3, value); }
        }

        [StringLength(150)]
        public string CustomColumnValue3
        {
            get { return _customColumnValue3; }
            set { SetProperty(ref this._customColumnValue3, value); }
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

        public Guid CompanyGroupId { get; set; }
        
        [RequiredGuid]
        public Guid QuestionCategoryId
        {
            get { return _questionCategoryId; }
            set { SetProperty(ref _questionCategoryId, value); }
        }
        public QuestionCategory QuestionCategory { get; set; }
                
        [RequiredGuid]
        public Guid QuestionTypeId
        {
            get { return _questionTypeId; }
            set { SetProperty(ref _questionTypeId, value); }
        }
        public QuestionType QuestionType { get; set; }
    }
}

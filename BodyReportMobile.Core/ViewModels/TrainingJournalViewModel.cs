﻿using System;
using BodyReport.Message;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using System.Threading.Tasks;
using BodyReport.Framework;
using XLabs.Ioc;
using BodyReportMobile.Core.Framework;
using BodyReportMobile.Core.Framework.Binding;
using BodyReportMobile.Core.WebServices;
using BodyReportMobile.Core.Data;
using BodyReportMobile.Core.Message.Binding;
using BodyReportMobile.Core.ServiceLayers;
using System.Globalization;
using Acr.UserDialogs;

namespace BodyReportMobile.Core.ViewModels
{
	public class TrainingJournalViewModel : BaseViewModel
	{
		List<TrainingWeek> _trainingWeekList = null;
		public ObservableCollection<GenericGroupModelCollection<BindingTrainingWeek>> GroupedTrainingWeeks { get; set; } = new ObservableCollection<GenericGroupModelCollection<BindingTrainingWeek>>();

        IUserDialogs _userDialog;
        private ApplicationDbContext _dbContext;
		private TrainingWeekService _trainingWeekService;

		private string _createLabel = string.Empty;

		public TrainingJournalViewModel () : base()
        {
            _userDialog = Resolver.Resolve<IUserDialogs>();
            _dbContext = Resolver.Resolve<ISQLite> ().GetConnection ();
			_trainingWeekService = new TrainingWeekService(_dbContext);
		}

		protected override async Task ShowAsync()
		{
			await base.ShowAsync();
            
            RetreiveLocalData();
            SynchronizeData();

            await RefreshDataActionAsync();
        }

		protected override void InitTranslation()
		{
			base.InitTranslation ();

			TitleLabel = Translation.Get (TRS.TRAINING_JOURNAL);
			CreateLabel = Translation.Get (TRS.CREATE);
            CopyLabel = Translation.Get(TRS.COPY);
            DeleteLabel = Translation.Get(TRS.DELETE);
        }

        private void RetreiveLocalData()
        {
            var trainingWeekScenario = new TrainingWeekScenario() { ManageTrainingDay = false };
            _trainingWeekList = _trainingWeekService.FindTrainingWeek(null, trainingWeekScenario);
        }

		public void SynchronizeData ()
		{
            try
            {
                //Create BindingCollection
                int currentYear = 0;
                GroupedTrainingWeeks.Clear();

                if (_trainingWeekList != null)
                {
                    _trainingWeekList = _trainingWeekList.OrderByDescending(m => m.Year).ThenByDescending(m => m.WeekOfYear).ToList();

                    DateTime dateTime;
                    var localGroupedTrainingWeeks = new ObservableCollection<GenericGroupModelCollection<BindingTrainingWeek>>();
                    GenericGroupModelCollection<BindingTrainingWeek> collection = null;
                    foreach (var trainingWeek in _trainingWeekList)
                    {
                        if (collection == null || currentYear != trainingWeek.Year)
                        {
                            currentYear = trainingWeek.Year;
                            collection = new GenericGroupModelCollection<BindingTrainingWeek>();
                            collection.LongName = currentYear.ToString();
                            collection.ShortName = currentYear.ToString();
                            localGroupedTrainingWeeks.Add(collection);
                        }

                        dateTime = Utils.YearWeekToPlanningDateTime(trainingWeek.Year, trainingWeek.WeekOfYear);
                        collection.Add(new BindingTrainingWeek()
                        {
                            Date = string.Format(Translation.Get(TRS.FROM_THE_P0TH_TO_THE_P1TH_OF_P2_P3), dateTime.Day, dateTime.AddDays(6.0d).Day, Translation.Get(((TMonthType)dateTime.Month).ToString().ToUpper()), dateTime.Year),
                            Week = Translation.Get(TRS.WEEK_NUMBER) + ' ' + trainingWeek.WeekOfYear.ToString(),
                            TrainingWeek = trainingWeek
                        });
                    }

                    foreach (var trainingWeek in localGroupedTrainingWeeks)
                    {
                        GroupedTrainingWeeks.Add(trainingWeek);
                    }
                }
            }
            catch
            {
            }
		}

        private async Task<bool> RefreshDataActionAsync()
        {
            DataIsRefreshing = true;
            bool result = await DataSync.SynchronizeTrainingWeeksAsync(_dbContext);
            RetreiveLocalData();
            SynchronizeData();
            DataIsRefreshing = false;
            return result;
        }
        
		private async Task CreateNewTrainingWeekActionAsync()
		{
            try
            {                
                var userInfo = UserData.Instance.UserInfo;
                if (userInfo == null)
                    userInfo = new UserInfo();

                DateTime dateTime = DateTime.Now;
                var trainingWeek = new TrainingWeek()
                {
                    UserId = userInfo.UserId,
                    Year = dateTime.Year,
                    WeekOfYear = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday),
                    UserHeight = userInfo.Height,
                    UserWeight = userInfo.Weight,
                    Unit = userInfo.Unit
                };

                if (await EditTrainingWeekViewModel.ShowAsync(trainingWeek, TEditMode.Create, this))
                {
                    //Refresh data
                    RetreiveLocalData();
                    SynchronizeData();
                }
            }
            catch(Exception except)
            {
                await _userDialog.AlertAsync(except.Message, Translation.Get(TRS.ERROR), Translation.Get(TRS.OK));
            }
		}

        private async Task CopyActionAsync(BindingTrainingWeek bindingTrainingWeek)
        {
            try
            {
                if (bindingTrainingWeek == null || bindingTrainingWeek.TrainingWeek == null)
                    return;
                
                if (await CopyTrainingWeekViewModel.ShowAsync(bindingTrainingWeek.TrainingWeek, this))
                {
                    //Refresh data
                    RetreiveLocalData();
                    SynchronizeData();
                }
            }
            catch (Exception except)
            {
                await _userDialog.AlertAsync(except.Message, Translation.Get(TRS.ERROR), Translation.Get(TRS.OK));
            }
		}

        private async Task DeleteActionAsync(BindingTrainingWeek bindingTrainingWeek)
		{
            try
            {
                if (bindingTrainingWeek == null)
                    return;

                var trainingWeek = bindingTrainingWeek.TrainingWeek;
                if (trainingWeek != null)
                {
                    await TrainingWeekWebService.DeleteTrainingWeekByKeyAsync(trainingWeek);
                    bool onlineDataRefreshed = await RefreshDataActionAsync();
                    if (!onlineDataRefreshed)
                    {
                        // delete data in local database
                        _trainingWeekService.DeleteTrainingWeek(trainingWeek);
                    }
                    //Refresh data
                    RetreiveLocalData();
                    SynchronizeData();
                }
            }
            catch(Exception except)
            {
                await _userDialog.AlertAsync(except.Message, Translation.Get(TRS.ERROR), Translation.Get(TRS.OK));
            }
		}
        
        private async Task ViewTrainingWeekActionAsync(BindingTrainingWeek bindingTrainingWeek)
        {
            try
            {
                if (bindingTrainingWeek != null && bindingTrainingWeek.TrainingWeek != null)
                {
                    TrainingWeek trainingWeek = null;
                    var trainingWeekScenario = new TrainingWeekScenario() { ManageTrainingDay = true };
                    try
                    {
                        //load server data (only header)
                        trainingWeek = await TrainingWeekWebService.GetTrainingWeekAsync(bindingTrainingWeek.TrainingWeek, false);
                        if (trainingWeek != null)
                        {
                            if (trainingWeek.ModificationDate != bindingTrainingWeek.TrainingWeek.ModificationDate)
                            {
                                //load server data (full)
                                trainingWeek = await TrainingWeekWebService.GetTrainingWeekAsync(bindingTrainingWeek.TrainingWeek, true);
                                if (trainingWeek != null)
                                {
                                    //Save data on local database
                                    _trainingWeekService.UpdateTrainingWeek(trainingWeek, trainingWeekScenario);
                                }
                            }
                            else
                                trainingWeek = null; // force reload local data
                        }
                    }
                    catch(Exception except)
                    {
                        // Unable to retreive local data
                        ILogger.Instance.Info("Unable to retreive TrainingWeek on server");
                        trainingWeek = null;
                    }

                    if (trainingWeek == null)
                    { //load local data
                        trainingWeek = _trainingWeekService.GetTrainingWeek(bindingTrainingWeek.TrainingWeek, trainingWeekScenario);
                    }

                    if (trainingWeek != null)
                    {
                        //Display view model
                        if (await TrainingWeekViewModel.ShowAsync(trainingWeek, this))
                        {
                            //Refresh data
                            RetreiveLocalData();
                            SynchronizeData();
                        }
                    }
                }
            }
            catch (Exception except)
            {
                await _userDialog.AlertAsync(except.Message, Translation.Get(TRS.ERROR), Translation.Get(TRS.OK));
            }
        }

        #region accessor
        
        public string CreateLabel {
			get {
				return _createLabel;
			}
			set {
				_createLabel = value;
				OnPropertyChanged ();
			}
		}

        private string _copyLabel;
        public string CopyLabel
        {
            get { return _copyLabel; }
            set
            {
                _copyLabel = value;
                OnPropertyChanged();
            }
        }

        private string _deleteLabel;
        public string DeleteLabel
        {
            get { return _deleteLabel; }
            set
            {
                _deleteLabel = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Command

        private ICommand _viewTrainingWeekCommand = null;
        public ICommand ViewTrainingWeekCommand
        {
            get
            {
                if (_viewTrainingWeekCommand == null)
                {
                    _viewTrainingWeekCommand = new ViewModelCommandAsync(this, async (selectItem) =>
                    {
                        await ViewTrainingWeekActionAsync(selectItem as BindingTrainingWeek);
                    });
                }
                return _viewTrainingWeekCommand;
            }
        }

        private ICommand _refreshDataCommand = null;
        public ICommand RefreshDataCommand
        {
            get
            {
                if (_refreshDataCommand == null)
                {
                    _refreshDataCommand = new ViewModelCommandAsync(this, async () =>
                    {
                        await RefreshDataActionAsync();
                    });
                }
                return _refreshDataCommand;
            }
        }

        private ICommand _createNewCommand = null;
        public ICommand CreateNewCommand
        {
            get
            {
                if (_createNewCommand == null)
                {
                    _createNewCommand = new ViewModelCommandAsync(this, async () =>
                    {
                        await CreateNewTrainingWeekActionAsync();
                    });
                }
                return _createNewCommand;
            }
        }

        private ICommand _copyCommand = null;
        public ICommand CopyCommand
        {
            get
            {
                if (_copyCommand == null)
                {
                    _copyCommand = new ViewModelCommandAsync(this, async (bindingTrainingWeekObject) =>
                    {
                        await CopyActionAsync(bindingTrainingWeekObject as BindingTrainingWeek);
                    });
                }
                return _copyCommand;
            }
        }

        private ICommand _deleteCommand = null;
        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                {
                    _deleteCommand = new ViewModelCommandAsync(this, async (bindingTrainingWeekObject) =>
                    {
                        await DeleteActionAsync(bindingTrainingWeekObject as BindingTrainingWeek);
                    });
                }
                return _deleteCommand;
            }
        }

        #endregion
    }
}


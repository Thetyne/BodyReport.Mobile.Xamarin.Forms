﻿using System;
using BodyReportMobile.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using System.Collections.ObjectModel;
using Message;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using System.Collections.Generic;

namespace BodyReportMobile.Core
{
	public class EditTrainingWeekViewModel : BaseViewModel
	{
		private static readonly string TRAINING_WEEK_VALUE = "P1";
		private static readonly string EDIT_MODE = "P2";

		public TEditMode EditMode {get; set;}
		public TrainingWeek TrainingWeek {get; set;}

		#region translation
		public string Title {get; set;}
		public string EditTitle {get; set;}
		public string ValidateLabel {get; set;}
		public string YearLabel {get; set;}
		public string WeekNumberLabel {get; set;}
		public string HeightLabel {get; set;}
		public string WeightLabel {get; set;}

		protected override void InitTranslation()
		{
			base.InitTranslation ();

			Title = "Training week";
			if(EditMode == TEditMode.Create)
				EditTitle = EditMode == TEditMode.Create ? "Create" : "Edit";
			ValidateLabel = "Validate";
			YearLabel = "Year";
			WeekNumberLabel = "Week number";
			HeightLabel = "Height (cm)";
			WeightLabel = "Weight (kg)";
		}
		#endregion

		public EditTrainingWeekViewModel (IMvxMessenger messenger) : base(messenger)
		{
			TrainingWeek = new TrainingWeek(){
				Year = 2015
			};
		}

		public override void Init(string viewModelGuid, bool autoClearViewModelDataCollection)
		{
			TrainingWeek = ViewModelDataCollection.Get<TrainingWeek>(viewModelGuid, TRAINING_WEEK_VALUE);
			EditMode = ViewModelDataCollection.Get<TEditMode> (viewModelGuid, EDIT_MODE);

			base.Init (viewModelGuid, autoClearViewModelDataCollection);
		}

		public static async Task<bool> Show(TrainingWeek trainingWeek, TEditMode editMode, BaseViewModel parent = null)
		{
			string viewModelGuid = Guid.NewGuid ().ToString();
			ViewModelDataCollection.Push (viewModelGuid, TRAINING_WEEK_VALUE, trainingWeek);
			ViewModelDataCollection.Push (viewModelGuid, EDIT_MODE, editMode);

			return await ShowModalViewModel<EditTrainingWeekViewModel> (viewModelGuid, true, parent);
		}

		public ICommand ValidateCommand
		{
			get
			{
				return new MvxCommand (() => {
					if(ValidateFields())
					{
						CloseViewModel();
					}
				});
			}
		}

		public ICommand ChangeYearCommand
		{
			get
			{
				return new MvxAsyncCommand (async () => {

					var datas = new List<Message.GenericData> ();

					int currentYear = DateTime.Now.Year;
					Message.GenericData data, currentData = null;
					for(int i = currentYear; i >= currentYear-99; i--)
					{
						data = new Message.GenericData(){ Tag = i, Name = i.ToString()};
						datas.Add(data);

						if(i == TrainingWeek.Year)
							currentData = data;
					}

					var result = await ListViewModel.ShowGenericList ("Select Year", datas, currentData, this);

					if(result.ViewModelValidated && result.SelectedTag != null)
					{
						if(((int)result.SelectedTag) > 0)
							TrainingWeek.Year = (int)result.SelectedTag;
					}
				}, null, true);
			}
		}

		private bool ValidateFields()
		{
			return TrainingWeek != null && TrainingWeek.Year > 0 && TrainingWeek.WeekOfYear > 0 && 
				TrainingWeek.UserHeight > 0 && TrainingWeek.UserWeight > 0;
			//TODO verify training week doesn't exist
		}
	}
}

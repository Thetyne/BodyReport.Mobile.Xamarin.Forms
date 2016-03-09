﻿using System;
using Xamarin.Forms;
using BodyReportMobile.Core;
using MvvmCross.Plugins.Messenger;
using MvvmCross.Platform;
using BodyReportMobile.Core.ViewModels;

namespace BodyReport
{
	public class BaseContentPage : ContentPage
	{
		public bool DisableBackButton { get; set;} = false;
		public string BackButtonTitle { get; set;} = "Return";

		public BaseContentPage ()
		{
		}

		public virtual bool CanBackButtonPressing()
		{
			return true;
		}

		protected override bool OnBackButtonPressed()
		{
			if (CanBackButtonPressing ()) {
				this.Navigation.PopAsync ();

				if (this.BindingContext != null && this.BindingContext is BaseViewModel) {
					var baseViewModel = this.BindingContext as BaseViewModel;

					var messenger = Mvx.Resolve<IMvxMessenger>();
					messenger.Publish (new MvxMessageFormClosed (this, baseViewModel.ViewModelGuid, true));
				}
			}
			return true;
		}
	}
}

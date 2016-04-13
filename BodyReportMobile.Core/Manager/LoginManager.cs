﻿using System;
using Acr.UserDialogs;
using Message;
using System.Threading.Tasks;
using XLabs.Ioc;
using BodyReportMobile.Core.Framework;
using BodyReportMobile.Core.MvxMessages;

namespace BodyReportMobile.Core.Manager
{
	public class LoginManager
	{
		private ISecurity _security;
		private bool _busy = false;
		private static LoginManager _instance = null;

		private string _userName = string.Empty;
		private string _password = string.Empty;

		private LoginManager ()
		{
			_security = Resolver.Resolve<ISecurity> ();
            AppMessenger.AppInstance.Register<MvxMessageLoginEntry>(this, OnLoginEntry);
            //TODO unscribe
		}

		public static LoginManager Instance {
			get {
				if(_instance == null)
					_instance = new LoginManager ();
				return _instance;
			}
		}

		public async Task Init()
		{
			try
			{
				_busy = true;
				bool result;
				if(_security.GetUserInfo (out _userName, out _password))
					result = await HttpConnector.Instance.ConnectUser(_userName, _password);
			}
			catch
			{
			}
			finally
			{
				_busy = false;
			}
		}

		private async void OnLoginEntry(MvxMessageLoginEntry message)
		{
			if (message == null || _busy)
				return;

			try
			{
				_busy = true;
				LoginConfig.DefaultCancelText = Translation.Get(TRS.CANCEL);
				LoginConfig.DefaultOkText = Translation.Get(TRS.OK);
				LoginConfig.DefaultTitle = Translation.Get(TRS.LOG_IN);
				LoginConfig.DefaultLoginPlaceholder = Translation.Get(TRS.USER_NAME);
				LoginConfig.DefaultPasswordPlaceholder = Translation.Get(TRS.PASSWORD);
				bool connectionOK = false;
				LoginResult loginResult;
				do
				{
					loginResult = await UserDialogs.Instance.LoginAsync(new LoginConfig
					{
						LoginValue = _userName,
						Message = Translation.Get(TRS.USE_A_LOCAL_ACCOUNT_TO_LOG_IN)
					});

					if(loginResult.Ok)
					{
						_userName = loginResult.LoginText;
						_password = loginResult.Password;
						if(!string.IsNullOrWhiteSpace(loginResult.LoginText) && !string.IsNullOrWhiteSpace(_password))
						{
							connectionOK = await HttpConnector.Instance.ConnectUser(_userName, _password);
							if(connectionOK)
								_security.SaveUserInfo(_userName, _password);
						}
					}
				}
				while(loginResult != null && loginResult.Ok && !connectionOK);
			}
			catch (Exception exception)
			{
				//TODO LOG
			}
			finally
			{
				_busy = false;
			}
		}
	}
}

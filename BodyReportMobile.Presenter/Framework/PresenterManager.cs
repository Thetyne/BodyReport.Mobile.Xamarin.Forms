﻿using BodyReportMobile.Core.ViewModels;
using BodyReportMobile.Presenter.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BodyReportMobile.Core.Framework
{
    public class PresenterManager : IPresenterManager
    {
        public PresenterManager()
        {
        }

        private Dictionary<Type, Type> _viewDependencies = new Dictionary<Type, Type>();
        private NavigationPage _mainNavigationPage = null;

        public NavigationPage MainNavigationPage
        {
            get
            {
                return _mainNavigationPage;
            }
        }

        public void AddViewDependency<TViewModel, TView>() where TViewModel : class where TView : class
        {
            Type viewModelType = typeof(TViewModel);
            Type viewType = typeof(TView);
            if (_viewDependencies.ContainsKey(viewModelType))
                _viewDependencies.Remove(viewModelType);

            _viewDependencies.Add(viewModelType, viewType);
        }

        private object GetView(BaseViewModel viewModel)
        {
            if (viewModel == null)
                return null;
            object view = null;
            Type viewModelType = viewModel.GetType();
            if (_viewDependencies.ContainsKey(viewModelType))
            {
                Type viewType = _viewDependencies[viewModelType];
                view = Activator.CreateInstance(viewType, viewModel);
            }

            return view;
        }

        public async Task<bool> TryDisplayViewAsync(BaseViewModel viewModel, BaseViewModel parentViewModel)
        {
            bool result = false;

            var parentView = GetView(parentViewModel);
            var view = GetView(viewModel);
            if (view != null && view is Page)
            {
                var page = view as Page;
                if(viewModel != null && page.BindingContext == null)
                    page.BindingContext = viewModel;

                if (parentView == null && _mainNavigationPage == null)
                {
                    _mainNavigationPage = new NavigationPage(page);
                    result = true;
                }
                else
                {
                    try
                    {
                        // calling this sync blocks UI and never navigates hence code continues regardless here
                        /*if(parentView != null && parentView is Page)
                        {
                            (parentView as Page).Navigation.PushAsync(page);
                        }
                        else*/
                            MainNavigationPage.PushAsync(page);
                        result = true;
                    }
                    catch (Exception e)
                    {
                        //Mvx.Error("Exception pushing {0}: {1}\n{2}", page.GetType(), e.Message, e.StackTrace);
                        return false;
                    }
                }
            }


            return result;
        }
    }
}
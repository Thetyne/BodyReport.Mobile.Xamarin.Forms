﻿using BodyReportMobile.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BodyReportMobile.Presenter.Pages
{
	[XamlCompilation (XamlCompilationOptions.Compile)]
    public partial class RegisterAccountPage : BaseContentPage
    {
        public RegisterAccountPage(RegisterAccountViewModel baseViewModel) : base(baseViewModel)
        {
            InitializeComponent();
        }
    }
}

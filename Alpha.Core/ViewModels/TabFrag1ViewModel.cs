

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;


namespace Alpha.Core.ViewModels
{
	public class TaskItem : MvxViewModel
	{
		private int _ratingValue ;
		public int RatingValue
		{
			get { return _ratingValue; }
			set
			{
				if (_ratingValue != value)
				{
					_ratingValue = value;
					RaisePropertyChanged("RatingValue");
				}
			}
		}
		private int _scaleValue ;
		public int ScaleValue {
			get { return _scaleValue; }
			set {
				if ( _scaleValue != value ) {
					_scaleValue = value;
					RaisePropertyChanged ( "ScaleValue" );
				}
			}
		}
	}

	public class TabFrag1ViewModel : AbstractViewModel
	{

		public TabFrag1ViewModel ( )
		{
		}


		private readonly TaskItem[] _list = new TaskItem[]
		{
			new TaskItem() {ScaleValue = 2, RatingValue = 1},
			new TaskItem() {ScaleValue = 2, RatingValue = 0},
			new TaskItem() {ScaleValue = 2, RatingValue = 1},
			new TaskItem() {ScaleValue = 2, RatingValue = 0}
		};
		public  TaskItem[] Listing { get { return _list; } }

		public IMvxCommand OnGotoPager { get; private set; }
		public IMvxCommand OnShowDialog { get; set; }

		private string _hello = "Tab (fragment) one.";
		public string Hello {
			get { return _hello; }
			set { _hello = value; RaisePropertyChanged ( ( ) => Hello ); }
		}

		public int Scale
		{
			get
			{
				return 2;
			}
		}

		public int Rating
		{
			get
			{
				return 3;
			}
		}
	}
}


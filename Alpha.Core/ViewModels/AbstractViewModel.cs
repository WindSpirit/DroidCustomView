

using System;
using System.Runtime.CompilerServices;

using Cirrious.MvvmCross.ViewModels;


// Cannot be used with Silverlight

namespace Alpha.Core.ViewModels
{
	public abstract class AbstractViewModel : MvxViewModel
	{
		protected virtual bool SetProperty<T> ( ref T property, T value, [CallerMemberName] string propertyName = null ) {
			if (Object.Equals(property, value))
			{
				return false;
			}
			else
			{
				property = value;
				OnPropertyChanged ( propertyName );
				return true;
			}
		}

		protected virtual void OnPropertyChanged ( [CallerMemberName] string propertyName = null )
		{
			RaisePropertyChanged ( propertyName );
		}

	}
}


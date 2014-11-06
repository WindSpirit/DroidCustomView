

using System.Collections.Generic;
using System.Reflection;
using Android.Content;
using Asgl.Android.Views;
using Cirrious.MvvmCross.Droid.Platform;
using Cirrious.MvvmCross.ViewModels;

using Alpha.Core;


namespace Alpha.Droid
{
    public class Setup : MvxAndroidSetup
    {
		private static Assembly CoreAssembly { get { return typeof ( App ).Assembly; } }
		public Setup ( Context applicationContext ) : base ( applicationContext ) { }
        protected override IMvxApplication CreateApp() { return new App(); }
		protected override IList<Assembly> AndroidViewAssemblies {
			get {
				var result = base.AndroidViewAssemblies;
				result.Add ( GetType ( ).Assembly );
				result.Add ( typeof ( RatingView ).Assembly );
				return result;
			}
		}
	}
}


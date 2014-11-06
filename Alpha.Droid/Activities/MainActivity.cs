

using Android.App;
using Android.OS;

using Cirrious.MvvmCross.Droid.Fragging.Fragments;
using Cirrious.MvvmCross.ViewModels;

using Alpha.Core.ViewModels;
using Alpha.Droid.Compatibility;
using Alpha.Droid.Fragments;
using Alpha.Droid.Specifics;


namespace Alpha.Droid.Activities
{
	[Activity(
		Label = "My Alpha Droid",
		Theme = "@style/Theme.AppCompat.Light",
		Icon = "@drawable/icon" ,
		MainLauncher = true
	)]
	public class MainActivity : MvxActionBarActivity
	{
		// We use these fragment references for tab navigation, so we know what to show & hide
		public MvxFragment Fragment1 { get; private set; }

		protected override void OnCreate ( Bundle bundle )
		{
			base.OnCreate ( bundle );
			SetContentView ( Resource.Layout.Main );

			if ( SupportActionBar != null ) {
				SupportActionBar.SetDisplayHomeAsUpEnabled ( false );					// This is home and there is no up from here
				SupportActionBar.SetHomeButtonEnabled ( false );						// We do not need the home button click event
				SupportActionBar.NavigationMode = ( int ) ActionBarNavigationMode.Tabs;	// We want *tab* navigation
			}

			// Launching the main activity for the very first time
			Fragment1 = CreateActionBarTab<TabFragment1> ( "Tab 1", ( ( MainViewModel ) ViewModel ).TabFrag1 );		// Add our first tab
			var transaction = SupportFragmentManager.BeginTransaction ( );
			transaction.Add ( Resource.Id.fragment_container, Fragment1 );
			transaction.Commit ( );
		}


		protected MvxFragment CreateActionBarTab <T> ( string caption, MvxViewModel viewModel ) where T : MvxFragment, new ( )
		{
			var tab = SupportActionBar.NewTab().SetText(caption);		// Create tab
			var frag = new T()											// Create fragment for our tab
			{
				ViewModel = viewModel									// Associate container-fragment ViewModels
			};
			tab.SetTabListener ( new TabListenerImpl ( this ) );		// Setup tab listener
			SupportActionBar.AddTab ( tab );							// Add tab to ActionBar

			return frag;
		}
	}
}


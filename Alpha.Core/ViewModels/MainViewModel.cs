

namespace Alpha.Core.ViewModels
{
	public class MainViewModel : AbstractViewModel
    {
		private TabFrag1ViewModel _tabFrag1 = new TabFrag1ViewModel( );
		public TabFrag1ViewModel TabFrag1 {
			get { return _tabFrag1; }
			set { _tabFrag1 = value; RaisePropertyChanged ( ( ) => TabFrag1 ); }
		}
    }
}


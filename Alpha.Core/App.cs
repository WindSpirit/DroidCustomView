

using Cirrious.CrossCore.IoC;

using Alpha.Core.ViewModels;


namespace Alpha.Core
{
    public class App : Cirrious.MvvmCross.ViewModels.MvxApplication
    {
        public override void Initialize()
        {
            CreatableTypes().EndingWith("Service").AsInterfaces().RegisterAsLazySingleton();
        }
    }
}


using EpreuveDeBloc.ViewModels;

namespace EpreuveDeBloc;

public partial class MainPage : ContentPage
{

	public MainPage(MainPageViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}

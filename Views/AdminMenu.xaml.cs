using EpreuveDeBloc.ViewModels;

namespace EpreuveDeBloc.Views;

public partial class AdminMenu : ContentPage
{

	public AdminMenu(AdminMenuViewModel adminMenuViewModel)
	{
		BindingContext = adminMenuViewModel;
		InitializeComponent();
	}
}
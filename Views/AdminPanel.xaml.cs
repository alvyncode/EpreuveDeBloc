using EpreuveDeBloc.ViewModels;

namespace EpreuveDeBloc.Views;

public partial class AdminPanel : ContentPage
{
	public AdminPanel(AdminPanelViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
namespace EpreuveDeBloc;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute("AdminPanel", typeof(Views.AdminPanel));
	}
}

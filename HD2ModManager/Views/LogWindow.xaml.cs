using HD2ModManager.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace HD2ModManager.Views;

public partial class LogWindow : Window
{
	public LogWindow(LogViewModel vm)
	{
		DataContext = vm;

		InitializeComponent();
	}

	protected override void OnClosing(CancelEventArgs e)
	{
		base.OnClosing(e);

		e.Cancel = true;
		Hide();
	}
}

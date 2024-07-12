using HD2ModManager.ViewModels;
using System.Windows;

namespace HD2ModManager.Views;

public partial class MainWindow : Window
{
	public MainWindow(MainViewModel vm)
	{
		DataContext = vm;

		InitializeComponent();
	}
}
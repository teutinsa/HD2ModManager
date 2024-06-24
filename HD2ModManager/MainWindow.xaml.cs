using HD2ModManager.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace HD2ModManager;

public partial class MainWindow : Window
{
	public MainWindow()
	{
		DataContext = new MainViewModel();

		InitializeComponent();
	}
}
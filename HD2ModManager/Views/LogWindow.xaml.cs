using HD2ModManager.ViewModels;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace HD2ModManager.Views;

public partial class LogWindow : Window
{
	private const int GWL_STYLE = -16;
	private const int WS_MAXIMIZEBOX = 0x10000;
	private const int WS_MINIMIZEBOX = 0x20000;

	public LogWindow(LogViewModel vm)
	{
		DataContext = vm;

		InitializeComponent();
	}

	protected override void OnSourceInitialized(EventArgs e)
	{
		base.OnSourceInitialized(e);
		var handle = new WindowInteropHelper(this).Handle;
		var value = GetWindowLongW(handle, GWL_STYLE);
		SetWindowLongW(handle, GWL_STYLE, value & ~(WS_MAXIMIZEBOX | WS_MINIMIZEBOX));
	}

	protected override void OnClosing(CancelEventArgs e)
	{
		base.OnClosing(e);
		e.Cancel = true;
		Hide();
	}

	[LibraryImport("user32.dll")]
	private static partial int GetWindowLongW(IntPtr hWnd, int nIndex);

	[LibraryImport("user32.dll")]
	private static partial int SetWindowLongW(IntPtr hWnd, int nIndex, int dwNewLong);
}

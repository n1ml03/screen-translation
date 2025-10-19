﻿using System.Windows;
using Application = System.Windows.Application;

namespace ScreenTranslation;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private MainWindow? _mainWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Set up application-wide keyboard handling
        this.DispatcherUnhandledException += App_DispatcherUnhandledException;

        // We'll hook keyboard events in the main window and other windows instead
        // of at the application level (which isn't supported in this context)

        // Initialize ChatBoxWindow instance without showing it
        // This ensures ChatBoxWindow.Instance is available immediately
        new ChatBoxWindow();

        // Create and show main window
        _mainWindow = new MainWindow();
        _mainWindow.Show();

        // Attach key handler to other windows once main window is shown
        AttachKeyHandlersToAllWindows();
    }

    // Ensure all windows are initialized and loaded
    private void AttachKeyHandlersToAllWindows()
    {
        // Each window now automatically attaches its own keyboard handler
        // when it's loaded, using PreviewKeyDown and its own Application_KeyDown method.
        // We don't need to do anything here anymore.
    }

    // Handle application-level keyboard events
    private void Application_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        // No need to check window focus since this is only called when a window has focus
        KeyboardShortcuts.HandleKeyDown(e);
    }

    // Handle any unhandled exceptions to prevent app crashes
    private void App_DispatcherUnhandledException(object sender,
                                               System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        // Validate input parameters
        if (e == null || e.Exception == null)
        {
            System.Diagnostics.Debug.WriteLine("Unhandled exception event received with null exception");
            if (e != null)
            {
                e.Handled = true;
            }
            return;
        }

        // Log the exception
        System.Diagnostics.Debug.WriteLine($"Unhandled application exception: {e.Exception.Message}");
        if (e.Exception.StackTrace != null)
        {
            System.Diagnostics.Debug.WriteLine($"Stack trace: {e.Exception.StackTrace}");
        }

        // Mark as handled to prevent app from crashing
        e.Handled = true;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
    }
}
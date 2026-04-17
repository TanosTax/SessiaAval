using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SessiaAval.Services;
using SessiaAval.Views.User;
using SessiaAval.Views.Moderator;
using SessiaAval.Views.Admin;
using SessiaAval.Views.Master;

namespace SessiaAval;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            
            loginWindow.Closed += (s, args) =>
            {
                if (loginWindow.loggedInUser != null)
                {
                    var user = loginWindow.loggedInUser;
                    Window? mainWindow = null;
                    
                    // Открываем соответствующее окно в зависимости от роли
                    if (AuthorizationService.isUser(user))
                    {
                        mainWindow = new UserDashboardWindow(user);
                    }
                    else if (AuthorizationService.isModerator(user))
                    {
                        mainWindow = new ModeratorDashboardWindow(user);
                    }
                    else if (AuthorizationService.isAdmin(user))
                    {
                        mainWindow = new AdminDashboardWindow(user);
                    }
                    else if (AuthorizationService.isMaster(user))
                    {
                        mainWindow = new MasterDashboardWindow(user);
                    }
                    
                    if (mainWindow != null)
                    {
                        desktop.MainWindow = mainWindow;
                        desktop.MainWindow.Show();
                    }
                    else
                    {
                        desktop.Shutdown();
                    }
                }
                else
                {
                    desktop.Shutdown();
                }
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
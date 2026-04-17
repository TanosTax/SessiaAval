using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SessiaAval.Controllers;
using SessiaAval.Data;
using SessiaAval.Models;
using SessiaAval.Services;

namespace SessiaAval;

public partial class LoginWindow : Window
{
    private readonly AuthController authController;
    public User? loggedInUser { get; private set; }

    public LoginWindow()
    {
        InitializeComponent();
        
        const string connectionString = "Host=localhost;Port=5432;Database=matye_db;Username=postgres;Password=0206";
        var dbContextFactory = new DbContextFactory(connectionString);
        var authService = new AuthService(dbContextFactory);
        authController = new AuthController(authService);
    }

    private async void onLoginClick(object? sender, RoutedEventArgs e)
    {
        errorTextBlock.IsVisible = false;

        var email = emailTextBox.Text?.Trim();
        var password = passwordTextBox.Text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            showError("Заполните все поля");
            return;
        }

        try
        {
            var user = await authController.loginAsync(email, password);
            
            if (user != null)
            {
                loggedInUser = user;
                Close();
            }
            else
            {
                showError("Неверный email или пароль");
            }
        }
        catch (Exception ex)
        {
            showError($"Ошибка входа: {ex.Message}");
        }
    }

    private void onRegisterClick(object? sender, RoutedEventArgs e)
    {
        var registerWindow = new RegisterWindow();
        registerWindow.ShowDialog(this);
        
        if (registerWindow.registeredUser != null)
        {
            loggedInUser = registerWindow.registeredUser;
            Close();
        }
    }

    private void showError(string message)
    {
        errorTextBlock.Text = message;
        errorTextBlock.IsVisible = true;
    }
}

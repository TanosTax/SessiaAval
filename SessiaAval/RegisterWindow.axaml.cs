using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SessiaAval.Controllers;
using SessiaAval.Data;
using SessiaAval.Models;
using SessiaAval.Services;

namespace SessiaAval;

public partial class RegisterWindow : Window
{
    private readonly AuthController authController;
    public User? registeredUser { get; private set; }

    public RegisterWindow()
    {
        InitializeComponent();
        
        const string connectionString = "Host=localhost;Port=5432;Database=matye_db;Username=postgres;Password=0206";
        var dbContextFactory = new DbContextFactory(connectionString);
        var authService = new AuthService(dbContextFactory);
        authController = new AuthController(authService);
    }

    private async void onRegisterClick(object? sender, RoutedEventArgs e)
    {
        errorTextBlock.IsVisible = false;

        var firstName = firstNameTextBox.Text?.Trim();
        var lastName = lastNameTextBox.Text?.Trim();
        var email = emailTextBox.Text?.Trim();
        var phone = phoneTextBox.Text?.Trim();
        var password = passwordTextBox.Text;
        var confirmPassword = confirmPasswordTextBox.Text;

        if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || 
            string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            showError("Заполните все обязательные поля");
            return;
        }

        if (password.Length < 6)
        {
            showError("Пароль должен содержать минимум 6 символов");
            return;
        }

        if (password != confirmPassword)
        {
            showError("Пароли не совпадают");
            return;
        }

        if (!isValidEmail(email))
        {
            showError("Введите корректный email");
            return;
        }

        try
        {
            var (success, message, user) = await authController.registerAsync(
                email, password, firstName, lastName, phone);
            
            if (success && user != null)
            {
                registeredUser = user;
                Close();
            }
            else
            {
                showError(message);
            }
        }
        catch (Exception ex)
        {
            showError($"Ошибка регистрации: {ex.Message}");
        }
    }

    private void onCancelClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void showError(string message)
    {
        errorTextBlock.Text = message;
        errorTextBlock.IsVisible = true;
    }

    private bool isValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

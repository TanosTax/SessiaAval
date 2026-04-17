using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using SessiaAval.Controllers;
using SessiaAval.Data;
using SessiaAval.Models;
using SessiaAval.Services;

namespace SessiaAval;

public partial class MainWindow : Window
{
    private readonly ServiceController serviceController = null!;
    private readonly User currentUser;
    private int currentPage = 1;
    private const int pageSize = 3;
    private int totalPages;
    private int totalCount;
    private string? selectedCategory = null;
    private int? selectedCollectionId = null;
    private string sortBy = "name";
    
    public ObservableCollection<Service> services { get; set; } = new();
    public ObservableCollection<Collection> collections { get; set; } = new();

    public MainWindow(User user)
    {
        InitializeComponent();
        
        currentUser = user;
        const string connectionString = "Host=localhost;Port=5432;Database=matye_db;Username=postgres;Password=0206";
        
        try
        {
            var dbContextFactory = new DbContextFactory(connectionString);
            var serviceService = new ServiceService(dbContextFactory);
            serviceController = new ServiceController(serviceService);
            
            DataContext = this;
            
            setupUIForRole();
            _ = initializeAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка инициализации: {ex.Message}");
        }

        Closing += onWindowClosing;
    }

    private void setupUIForRole()
    {
        // Модератор - управление услугами
        var canManageServices = AuthorizationService.hasPermission(currentUser, Models.Permission.ManageServices);
        addButton.IsVisible = canManageServices;
        
        // Скрываем кнопки редактирования/удаления для пользователей и мастеров
        // Это будет контролироваться в шаблоне через биндинг
    }

    private async Task initializeAsync()
    {
        var collectionsList = await serviceController.getCollectionsAsync();
        collections.Clear();
        collections.Add(new Collection { collectionId = 0, collectionName = "Все" });
        foreach (var col in collectionsList)
        {
            collections.Add(col);
        }
        collectionComboBox.ItemsSource = collections;
        collectionComboBox.SelectedIndex = 0;
        
        await loadServicesAsync();
    }

    private async Task loadServicesAsync()
    {
        try
        {
            loadingOverlay.IsVisible = true;
            
            totalCount = await serviceController.getTotalCountAsync(selectedCategory, selectedCollectionId);
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            var loadedServices = await serviceController.getServicesAsync(
                currentPage, pageSize, sortBy, selectedCategory, selectedCollectionId);
            
            services.Clear();
            foreach (var service in loadedServices)
            {
                services.Add(service);
            }
            
            updatePaginationButtons();
            var startItem = (currentPage - 1) * pageSize + 1;
            var endItem = Math.Min(currentPage * pageSize, totalCount);
            pageInfoText.Text = $"{startItem}-{endItem} из {totalCount}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
        finally
        {
            loadingOverlay.IsVisible = false;
        }
    }

    private void updatePaginationButtons()
    {
        previousButton.IsEnabled = currentPage > 1;
        nextButton.IsEnabled = currentPage < totalPages;
    }

    private async void onPreviousClick(object? sender, RoutedEventArgs e)
    {
        if (currentPage > 1)
        {
            currentPage--;
            await loadServicesAsync();
        }
    }

    private async void onNextClick(object? sender, RoutedEventArgs e)
    {
        if (currentPage < totalPages)
        {
            currentPage++;
            await loadServicesAsync();
        }
    }

    private async void onCategoryClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string category)
        {
            selectedCategory = category;
            currentPage = 1;
            await loadServicesAsync();
        }
    }

    private async void onCollectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (collectionComboBox.SelectedItem is Collection collection)
        {
            selectedCollectionId = collection.collectionId == 0 ? null : collection.collectionId;
            currentPage = 1;
            await loadServicesAsync();
        }
    }

    private async void onResetClick(object? sender, RoutedEventArgs e)
    {
        selectedCategory = null;
        selectedCollectionId = null;
        sortBy = "name";
        collectionComboBox.SelectedIndex = 0;
        currentPage = 1;
        await loadServicesAsync();
    }

    private async void onSortByNameClick(object? sender, RoutedEventArgs e)
    {
        sortBy = "name";
        currentPage = 1;
        await loadServicesAsync();
    }

    private async void onAddClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var editWindow = new ServiceEditWindow(serviceController);
            await editWindow.ShowDialog(this);
            
            if (editWindow.isSaved)
            {
                await loadServicesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при добавлении услуги: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
        }
    }

    private async void onEditClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (!AuthorizationService.hasPermission(currentUser, Models.Permission.ManageServices))
            {
                // Показываем простое сообщение без MessageBox
                Console.WriteLine("Доступ запрещен: У вас нет прав для редактирования услуг");
                return;
            }
            
            if (sender is Button button && button.DataContext is Service service)
            {
                var editWindow = new ServiceEditWindow(serviceController, service);
                await editWindow.ShowDialog(this);
                
                if (editWindow.isSaved)
                {
                    await loadServicesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при редактировании услуги: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
        }
    }

    private async void onDeleteClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Service service)
        {
            var dialog = new Window
            {
                Title = "Подтверждение",
                Width = 400,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            var panel = new StackPanel { Margin = new Avalonia.Thickness(20) };
            panel.Children.Add(new TextBlock 
            { 
                Text = $"Удалить услугу '{service.serviceName}'?",
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Margin = new Avalonia.Thickness(0, 0, 0, 20)
            });

            var buttonPanel = new StackPanel 
            { 
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
            };

            var yesButton = new Button 
            { 
                Content = "Да", 
                Width = 80,
                Margin = new Avalonia.Thickness(0, 0, 10, 0)
            };
            yesButton.Click += (s, args) => { dialog.Close(true); };

            var noButton = new Button 
            { 
                Content = "Нет", 
                Width = 80
            };
            noButton.Click += (s, args) => { dialog.Close(false); };

            buttonPanel.Children.Add(yesButton);
            buttonPanel.Children.Add(noButton);
            panel.Children.Add(buttonPanel);

            dialog.Content = panel;

            var result = await dialog.ShowDialog<bool>(this);
            
            if (result)
            {
                await serviceController.deleteServiceAsync(service.serviceId);
                await loadServicesAsync();
            }
        }
    }

    private async void onWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        
        var dialog = new Window
        {
            Title = "Выход",
            Width = 400,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false
        };

        var panel = new StackPanel { Margin = new Avalonia.Thickness(20) };
        panel.Children.Add(new TextBlock 
        { 
            Text = "Вы действительно хотите закрыть приложение?",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Avalonia.Thickness(0, 0, 0, 20)
        });

        var buttonPanel = new StackPanel 
        { 
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
        };

        var yesButton = new Button 
        { 
            Content = "Да", 
            Width = 80,
            Margin = new Avalonia.Thickness(0, 0, 10, 0)
        };
        yesButton.Click += (s, args) => { dialog.Close(true); };

        var noButton = new Button 
        { 
            Content = "Нет", 
            Width = 80
        };
        noButton.Click += (s, args) => { dialog.Close(false); };

        buttonPanel.Children.Add(yesButton);
        buttonPanel.Children.Add(noButton);
        panel.Children.Add(buttonPanel);

        dialog.Content = panel;

        var result = await dialog.ShowDialog<bool>(this);
        
        if (result)
        {
            Closing -= onWindowClosing;
            Close();
        }
    }

    private void onCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SessiaAval.Controllers;
using SessiaAval.Models;
using SessiaAval.Services;

namespace SessiaAval;

public partial class ServiceEditWindow : Window
{
    private readonly ServiceController serviceController;
    private Service? service;
    
    public bool isSaved { get; private set; }

    public ServiceEditWindow(ServiceController controller, Service? existingService = null)
    {
        InitializeComponent();
        serviceController = controller;
        service = existingService;
        
        _ = loadDataAsync();
    }

    private async System.Threading.Tasks.Task loadDataAsync()
    {
        var categories = await serviceController.getCategoriesAsync();
        categoryComboBox.ItemsSource = categories;

        var collections = await serviceController.getCollectionsAsync();
        var collectionsList = collections.ToList();
        collectionsList.Insert(0, new Collection { collectionId = 0, collectionName = "Нет" });
        collectionComboBox.ItemsSource = collectionsList;

        if (service != null)
        {
            Title = "Редактирование услуги";
            nameTextBox.Text = service.serviceName;
            descriptionTextBox.Text = service.description;
            priceTextBox.Text = service.price.ToString();
            durationTextBox.Text = service.durationMinutes.ToString();
            
            categoryComboBox.SelectedItem = categories.FirstOrDefault(c => c.categoryId == service.categoryId);
            collectionComboBox.SelectedItem = collectionsList.FirstOrDefault(c => c.collectionId == (service.collectionId ?? 0));
        }
        else
        {
            Title = "Добавление услуги";
            categoryComboBox.SelectedIndex = 0;
            collectionComboBox.SelectedIndex = 0;
        }
    }

    private async void onSaveClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                await showMessageAsync("Ошибка", "Введите название услуги");
                return;
            }

            if (!decimal.TryParse(priceTextBox.Text, out var price) || price <= 0)
            {
                await showMessageAsync("Ошибка", "Введите корректную цену");
                return;
            }

            if (!int.TryParse(durationTextBox.Text, out var duration) || duration <= 0)
            {
                await showMessageAsync("Ошибка", "Введите корректную длительность");
                return;
            }

            if (categoryComboBox.SelectedItem is not ServiceCategory category)
            {
                await showMessageAsync("Ошибка", "Выберите категорию");
                return;
            }

            var collection = collectionComboBox.SelectedItem as Collection;
            var collectionId = collection?.collectionId == 0 ? null : collection?.collectionId;

            if (service == null)
            {
                service = new Service
                {
                    serviceName = nameTextBox.Text,
                    description = descriptionTextBox.Text,
                    price = price,
                    durationMinutes = duration,
                    categoryId = category.categoryId,
                    collectionId = collectionId
                };
                await serviceController.addServiceAsync(service);
            }
            else
            {
                service.serviceName = nameTextBox.Text;
                service.description = descriptionTextBox.Text;
                service.price = price;
                service.durationMinutes = duration;
                service.categoryId = category.categoryId;
                service.collectionId = collectionId;
                await serviceController.updateServiceAsync(service);
            }

            isSaved = true;
            Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка сохранения: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            await showMessageAsync("Ошибка", $"Не удалось сохранить услугу: {ex.Message}");
        }
    }

    private void onCancelClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private async System.Threading.Tasks.Task showMessageAsync(string title, string message)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 400,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false
        };

        var panel = new StackPanel { Margin = new Avalonia.Thickness(20) };
        panel.Children.Add(new TextBlock 
        { 
            Text = message,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Avalonia.Thickness(0, 0, 0, 20)
        });

        var okButton = new Button 
        { 
            Content = "OK", 
            Width = 80,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
        };
        okButton.Click += (s, args) => { dialog.Close(); };

        panel.Children.Add(okButton);
        dialog.Content = panel;

        await dialog.ShowDialog(this);
    }
}

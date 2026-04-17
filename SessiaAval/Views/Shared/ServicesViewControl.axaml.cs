using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SessiaAval.Models;
using SessiaAval.Services;
using UserModel = SessiaAval.Models.User;

namespace SessiaAval.Views.Shared;

public partial class ServicesViewControl : UserControl
{
    private readonly UserModel currentUser;
    public ObservableCollection<Service> services { get; set; } = new();

    public ServicesViewControl(UserModel user)
    {
        InitializeComponent();
        currentUser = user;
        DataContext = this;
        
        setupUIForRole();
    }

    private void setupUIForRole()
    {
        // Показываем кнопку добавления только модераторам
        addButton.IsVisible = AuthorizationService.hasPermission(currentUser, Permission.ManageServices);
    }

    private void onAddClick(object? sender, RoutedEventArgs e)
    {
        // TODO: Открыть окно добавления услуги
    }

    private void onSortClick(object? sender, RoutedEventArgs e)
    {
        // TODO: Сортировка
    }

    private void onCategoryClick(object? sender, RoutedEventArgs e)
    {
        // TODO: Фильтрация по категории
    }

    private void onEditClick(object? sender, RoutedEventArgs e)
    {
        // TODO: Редактирование услуги
    }

    private void onDeleteClick(object? sender, RoutedEventArgs e)
    {
        // TODO: Удаление услуги
    }

    private void onBookClick(object? sender, RoutedEventArgs e)
    {
        // TODO: Запись на услугу
    }

    private void onPreviousClick(object? sender, RoutedEventArgs e)
    {
        // TODO: Предыдущая страница
    }

    private void onNextClick(object? sender, RoutedEventArgs e)
    {
        // TODO: Следующая страница
    }
}

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SessiaAval.Data;
using SessiaAval.Models;
using UserModel = SessiaAval.Models.User;
using MasterModel = SessiaAval.Models.Master;

namespace SessiaAval.Views.Moderator;

public partial class ModeratorDashboardWindow : Window
{
    private UserModel? currentUser;
    private readonly AppDbContext dbContext;
    private ObservableCollection<Service> services;
    private ObservableCollection<MasterModel> masters;

    public ModeratorDashboardWindow()
    {
        InitializeComponent();
        
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=matye_db;Username=postgres;Password=0206");
        dbContext = new AppDbContext(optionsBuilder.Options);
        
        services = new ObservableCollection<Service>();
        masters = new ObservableCollection<MasterModel>();
    }

    public ModeratorDashboardWindow(UserModel user) : this()
    {
        currentUser = user;
        userNameText.Text = currentUser.fullName;
        
        showServicesView();
    }

    private void loadServices()
    {
        services.Clear();
        var allServices = dbContext.services
            .Include(s => s.category)
            .Include(s => s.collection)
            .ToList();
        
        foreach (var service in allServices)
        {
            services.Add(service);
        }
    }

    private void loadMasters()
    {
        masters.Clear();
        var allMasters = dbContext.masters
            .Include(m => m.user)
            .ToList();
        
        foreach (var master in allMasters)
        {
            masters.Add(master);
        }
    }

    private void showServicesView()
    {
        loadServices();
        
        var view = new StackPanel { Margin = new Avalonia.Thickness(30) };
        
        var title = new TextBlock 
        { 
            Text = "УПРАВЛЕНИЕ УСЛУГАМИ", 
            FontSize = 28, 
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#2C2C2C")),
            Margin = new Avalonia.Thickness(0, 0, 0, 20)
        };
        view.Children.Add(title);
        
        var buttonPanel = new StackPanel 
        { 
            Orientation = Orientation.Horizontal, 
            Spacing = 15,
            Margin = new Avalonia.Thickness(0, 0, 0, 20)
        };
        
        var addBtn = new Button { Content = "Добавить услугу", Width = 150, Height = 35 };
        addBtn.Classes.Add("primary");
        addBtn.Click += (s, e) => showAddServiceDialog();
        buttonPanel.Children.Add(addBtn);
        
        view.Children.Add(buttonPanel);
        
        // Таблица услуг
        var scrollViewer = new ScrollViewer { Height = 500 };
        var dataGrid = new DataGrid
        {
            ItemsSource = services,
            AutoGenerateColumns = false,
            IsReadOnly = true,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            Background = Brushes.White,
            Foreground = Brushes.Black
        };
        
        dataGrid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "Название", 
            Binding = new Avalonia.Data.Binding("serviceName"),
            Width = new DataGridLength(250)
        });
        
        dataGrid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "Категория", 
            Binding = new Avalonia.Data.Binding("category.categoryName"),
            Width = new DataGridLength(150)
        });
        
        dataGrid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "Цена", 
            Binding = new Avalonia.Data.Binding("price") { StringFormat = "{0:C}" },
            Width = new DataGridLength(100)
        });
        
        dataGrid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "Длительность", 
            Binding = new Avalonia.Data.Binding("durationMinutes") { StringFormat = "{0} мин" },
            Width = new DataGridLength(120)
        });
        
        var actionsColumn = new DataGridTemplateColumn { Header = "Действия", Width = new DataGridLength(250) };
        actionsColumn.CellTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<Service>((service, _) =>
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5 };
            
            if (service != null)
            {
                var editBtn = new Button 
                { 
                    Content = "Изменить", 
                    Width = 80, 
                    Height = 35,
                    FontSize = 11,
                    Padding = new Avalonia.Thickness(5)
                };
                editBtn.Classes.Add("primary");
                editBtn.Click += (s, e) => showEditServiceDialog(service);
                panel.Children.Add(editBtn);
                
                var assignBtn = new Button 
                { 
                    Content = "Мастера", 
                    Width = 80, 
                    Height = 35,
                    FontSize = 11,
                    Padding = new Avalonia.Thickness(5)
                };
                assignBtn.Classes.Add("secondary");
                assignBtn.Click += (s, e) => showAssignMastersDialog(service);
                panel.Children.Add(assignBtn);
            }
            
            return panel;
        });
        dataGrid.Columns.Add(actionsColumn);
        
        scrollViewer.Content = dataGrid;
        view.Children.Add(scrollViewer);
        
        contentArea.Content = view;
    }

    private void onServicesClick(object? sender, RoutedEventArgs e) => showServicesView();
    
    private async void showAddServiceDialog()
    {
        var dialog = new Window
        {
            Title = "Добавить услугу",
            Width = 550,
            Height = 600,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var panel = new StackPanel { Margin = new Avalonia.Thickness(20), Spacing = 10 };
        
        panel.Children.Add(new TextBlock { Text = "Название услуги:", FontWeight = FontWeight.Bold });
        var nameBox = new TextBox { Watermark = "Например: Костюм Наруто" };
        panel.Children.Add(nameBox);
        
        panel.Children.Add(new TextBlock { Text = "Описание:", FontWeight = FontWeight.Bold });
        var descBox = new TextBox { Watermark = "Описание услуги", Height = 80, TextWrapping = Avalonia.Media.TextWrapping.Wrap };
        panel.Children.Add(descBox);
        
        panel.Children.Add(new TextBlock { Text = "Цена (₽):", FontWeight = FontWeight.Bold });
        var priceBox = new TextBox { Watermark = "15000" };
        panel.Children.Add(priceBox);
        
        panel.Children.Add(new TextBlock { Text = "Длительность (минуты):", FontWeight = FontWeight.Bold });
        var durationBox = new TextBox { Watermark = "120" };
        panel.Children.Add(durationBox);
        
        panel.Children.Add(new TextBlock { Text = "Категория:", FontWeight = FontWeight.Bold });
        var categoryCombo = new ComboBox { Width = 300 };
        var categories = dbContext.serviceCategories.ToList();
        foreach (var cat in categories)
        {
            categoryCombo.Items.Add(cat);
        }
        categoryCombo.DisplayMemberBinding = new Avalonia.Data.Binding("categoryName");
        categoryCombo.SelectedIndex = 0;
        panel.Children.Add(categoryCombo);
        
        var buttonPanel = new StackPanel 
        { 
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10,
            Margin = new Avalonia.Thickness(0, 20, 0, 0)
        };

        var saveButton = new Button { Content = "Сохранить", Width = 100 };
        saveButton.Classes.Add("primary");
        saveButton.Click += (s, args) => 
        {
            if (string.IsNullOrWhiteSpace(nameBox.Text) || categoryCombo.SelectedItem == null)
                return;
            
            var newService = new Service
            {
                serviceName = nameBox.Text,
                description = descBox.Text,
                price = decimal.TryParse(priceBox.Text, out var p) ? p : 0,
                durationMinutes = int.TryParse(durationBox.Text, out var d) ? d : 60,
                categoryId = ((ServiceCategory)categoryCombo.SelectedItem).categoryId,
                isActive = true,
                createdDate = DateTime.UtcNow,
                lastModified = DateTime.UtcNow
            };
            
            dbContext.services.Add(newService);
            dbContext.SaveChanges();
            showServicesView();
            dialog.Close();
        };
        buttonPanel.Children.Add(saveButton);
        
        var cancelButton = new Button { Content = "Отмена", Width = 100 };
        cancelButton.Click += (s, args) => { dialog.Close(); };
        buttonPanel.Children.Add(cancelButton);
        
        panel.Children.Add(buttonPanel);
        dialog.Content = panel;
        await dialog.ShowDialog(this);
    }
    
    private async void showEditServiceDialog(Service service)
    {
        var dialog = new Window
        {
            Title = "Изменить услугу",
            Width = 550,
            Height = 600,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var panel = new StackPanel { Margin = new Avalonia.Thickness(20), Spacing = 10 };
        
        panel.Children.Add(new TextBlock { Text = "Название услуги:", FontWeight = FontWeight.Bold });
        var nameBox = new TextBox { Text = service.serviceName };
        panel.Children.Add(nameBox);
        
        panel.Children.Add(new TextBlock { Text = "Описание:", FontWeight = FontWeight.Bold });
        var descBox = new TextBox { Text = service.description, Height = 80, TextWrapping = Avalonia.Media.TextWrapping.Wrap };
        panel.Children.Add(descBox);
        
        panel.Children.Add(new TextBlock { Text = "Цена (₽):", FontWeight = FontWeight.Bold });
        var priceBox = new TextBox { Text = service.price.ToString() };
        panel.Children.Add(priceBox);
        
        panel.Children.Add(new TextBlock { Text = "Длительность (минуты):", FontWeight = FontWeight.Bold });
        var durationBox = new TextBox { Text = service.durationMinutes.ToString() };
        panel.Children.Add(durationBox);
        
        panel.Children.Add(new TextBlock { Text = "Категория:", FontWeight = FontWeight.Bold });
        var categoryCombo = new ComboBox { Width = 300 };
        var categories = dbContext.serviceCategories.ToList();
        foreach (var cat in categories)
        {
            categoryCombo.Items.Add(cat);
        }
        categoryCombo.DisplayMemberBinding = new Avalonia.Data.Binding("categoryName");
        categoryCombo.SelectedItem = categories.FirstOrDefault(c => c.categoryId == service.categoryId);
        panel.Children.Add(categoryCombo);
        
        var buttonPanel = new StackPanel 
        { 
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10,
            Margin = new Avalonia.Thickness(0, 20, 0, 0)
        };

        var saveButton = new Button { Content = "Сохранить", Width = 100 };
        saveButton.Classes.Add("primary");
        saveButton.Click += (s, args) => 
        {
            if (string.IsNullOrWhiteSpace(nameBox.Text) || categoryCombo.SelectedItem == null)
                return;
            
            service.serviceName = nameBox.Text;
            service.description = descBox.Text;
            service.price = decimal.TryParse(priceBox.Text, out var p) ? p : service.price;
            service.durationMinutes = int.TryParse(durationBox.Text, out var d) ? d : service.durationMinutes;
            service.categoryId = ((ServiceCategory)categoryCombo.SelectedItem).categoryId;
            service.lastModified = DateTime.UtcNow;
            
            dbContext.SaveChanges();
            showServicesView();
            dialog.Close();
        };
        buttonPanel.Children.Add(saveButton);
        
        var cancelButton = new Button { Content = "Отмена", Width = 100 };
        cancelButton.Click += (s, args) => { dialog.Close(); };
        buttonPanel.Children.Add(cancelButton);
        
        panel.Children.Add(buttonPanel);
        dialog.Content = panel;
        await dialog.ShowDialog(this);
    }
    
    private async void showAssignMastersDialog(Service service)
    {
        var dialog = new Window
        {
            Title = $"Мастера для услуги: {service.serviceName}",
            Width = 700,
            Height = 600,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var mainPanel = new StackPanel { Margin = new Avalonia.Thickness(20), Spacing = 15 };
        
        // Текущие мастера
        mainPanel.Children.Add(new TextBlock 
        { 
            Text = "Назначенные мастера:", 
            FontSize = 16,
            FontWeight = FontWeight.Bold 
        });
        
        var assignedMasters = dbContext.masterServices
            .Include(ms => ms.master)
            .ThenInclude(m => m!.user)
            .Where(ms => ms.serviceId == service.serviceId)
            .Select(ms => ms.master)
            .ToList();
        
        var assignedPanel = new StackPanel { Spacing = 5 };
        foreach (var master in assignedMasters)
        {
            var masterPanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal, 
                Spacing = 10,
                Margin = new Avalonia.Thickness(0, 5, 0, 5)
            };
            
            masterPanel.Children.Add(new TextBlock 
            { 
                Text = master?.user?.fullName ?? "Неизвестно",
                VerticalAlignment = VerticalAlignment.Center,
                Width = 200
            });
            
            var removeBtn = new Button 
            { 
                Content = "Отвязать", 
                Width = 80, 
                Height = 35,
                FontSize = 11,
                Padding = new Avalonia.Thickness(5)
            };
            removeBtn.Click += (s, e) =>
            {
                var ms = dbContext.masterServices
                    .FirstOrDefault(x => x.masterId == master!.masterId && x.serviceId == service.serviceId);
                if (ms != null)
                {
                    dbContext.masterServices.Remove(ms);
                    dbContext.SaveChanges();
                    dialog.Close();
                    showAssignMastersDialog(service);
                }
            };
            masterPanel.Children.Add(removeBtn);
            
            assignedPanel.Children.Add(masterPanel);
        }
        
        if (assignedMasters.Count == 0)
        {
            assignedPanel.Children.Add(new TextBlock 
            { 
                Text = "Нет назначенных мастеров",
                Foreground = new SolidColorBrush(Color.Parse("#999999"))
            });
        }
        
        mainPanel.Children.Add(assignedPanel);
        
        // Доступные мастера
        mainPanel.Children.Add(new TextBlock 
        { 
            Text = "Доступные мастера:", 
            FontSize = 16,
            FontWeight = FontWeight.Bold,
            Margin = new Avalonia.Thickness(0, 20, 0, 0)
        });
        
        var assignedIds = assignedMasters.Select(m => m!.masterId).ToList();
        var availableMasters = dbContext.masters
            .Include(m => m.user)
            .Where(m => !assignedIds.Contains(m.masterId))
            .ToList();
        
        var availablePanel = new StackPanel { Spacing = 5 };
        foreach (var master in availableMasters)
        {
            var masterPanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal, 
                Spacing = 10,
                Margin = new Avalonia.Thickness(0, 5, 0, 5)
            };
            
            masterPanel.Children.Add(new TextBlock 
            { 
                Text = master.user?.fullName ?? "Неизвестно",
                VerticalAlignment = VerticalAlignment.Center,
                Width = 200
            });
            
            var assignBtn = new Button 
            { 
                Content = "Привязать", 
                Width = 80, 
                Height = 35,
                FontSize = 11,
                Padding = new Avalonia.Thickness(5)
            };
            assignBtn.Classes.Add("primary");
            assignBtn.Click += (s, e) =>
            {
                var newMs = new MasterService
                {
                    masterId = master.masterId,
                    serviceId = service.serviceId,
                    assignedDate = DateTime.UtcNow,
                    lastModified = DateTime.UtcNow
                };
                dbContext.masterServices.Add(newMs);
                dbContext.SaveChanges();
                dialog.Close();
                showAssignMastersDialog(service);
            };
            masterPanel.Children.Add(assignBtn);
            
            availablePanel.Children.Add(masterPanel);
        }
        
        if (availableMasters.Count == 0)
        {
            availablePanel.Children.Add(new TextBlock 
            { 
                Text = "Все мастера уже назначены",
                Foreground = new SolidColorBrush(Color.Parse("#999999"))
            });
        }
        
        mainPanel.Children.Add(availablePanel);
        
        var closeButton = new Button 
        { 
            Content = "Закрыть", 
            Width = 100,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Avalonia.Thickness(0, 20, 0, 0)
        };
        closeButton.Click += (s, args) => { dialog.Close(); };
        mainPanel.Children.Add(closeButton);

        var scrollViewer = new ScrollViewer { Content = mainPanel };
        dialog.Content = scrollViewer;
        await dialog.ShowDialog(this);
    }
    
    private void onMastersClick(object? sender, RoutedEventArgs e)
    {
        loadMasters();
        
        var view = new StackPanel { Margin = new Avalonia.Thickness(30) };
        
        var title = new TextBlock 
        { 
            Text = "УПРАВЛЕНИЕ МАСТЕРАМИ", 
            FontSize = 28, 
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#2C2C2C")),
            Margin = new Avalonia.Thickness(0, 0, 0, 20)
        };
        view.Children.Add(title);
        
        var statsPanel = new StackPanel 
        { 
            Orientation = Orientation.Horizontal, 
            Spacing = 20,
            Margin = new Avalonia.Thickness(0, 0, 0, 20)
        };
        
        var totalBox = createStatBox("Всего мастеров", masters.Count.ToString(), "#4A90E2");
        var requestsBox = createStatBox("Заявок на повышение", masters.Count(m => m.qualificationRequestPending).ToString(), "#FFA726");
        
        statsPanel.Children.Add(totalBox);
        statsPanel.Children.Add(requestsBox);
        view.Children.Add(statsPanel);
        
        // Таблица мастеров
        var scrollViewer = new ScrollViewer { Height = 450 };
        var dataGrid = new DataGrid
        {
            ItemsSource = masters,
            AutoGenerateColumns = false,
            IsReadOnly = true,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            Background = Brushes.White,
            Foreground = Brushes.Black
        };
        
        dataGrid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "ФИО", 
            Binding = new Avalonia.Data.Binding("user.fullName"),
            Width = new DataGridLength(200)
        });
        
        dataGrid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "Квалификация", 
            Binding = new Avalonia.Data.Binding("qualificationLevel"),
            Width = new DataGridLength(120)
        });
        
        dataGrid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "Специализация", 
            Binding = new Avalonia.Data.Binding("specialization"),
            Width = new DataGridLength(250)
        });
        
        dataGrid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "Дата найма", 
            Binding = new Avalonia.Data.Binding("hireDate") { StringFormat = "dd.MM.yyyy" },
            Width = new DataGridLength(120)
        });
        
        var actionsColumn = new DataGridTemplateColumn { Header = "Действия", Width = new DataGridLength(200) };
        actionsColumn.CellTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<MasterModel>((master, _) =>
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5 };
            
            if (master != null)
            {
                if (master.qualificationRequestPending)
                {
                    var approveBtn = new Button 
                    { 
                        Content = "Повысить", 
                        Width = 90, 
                        Height = 35,
                        FontSize = 11,
                        Padding = new Avalonia.Thickness(5)
                    };
                    approveBtn.Classes.Add("primary");
                    approveBtn.Click += (s, e) => approveQualification(master);
                    panel.Children.Add(approveBtn);
                }
                else
                {
                    var upgradeBtn = new Button 
                    { 
                        Content = "Повысить", 
                        Width = 90, 
                        Height = 35,
                        FontSize = 11,
                        Padding = new Avalonia.Thickness(5)
                    };
                    upgradeBtn.Classes.Add("secondary");
                    upgradeBtn.Click += (s, e) => upgradeQualification(master);
                    panel.Children.Add(upgradeBtn);
                }
            }
            
            return panel;
        });
        dataGrid.Columns.Add(actionsColumn);
        
        scrollViewer.Content = dataGrid;
        view.Children.Add(scrollViewer);
        
        contentArea.Content = view;
    }
    
    private Border createStatBox(string label, string value, string color)
    {
        var box = new Border
        {
            Background = new SolidColorBrush(Color.Parse(color)),
            CornerRadius = new Avalonia.CornerRadius(8),
            Padding = new Avalonia.Thickness(20),
            Width = 200
        };
        
        var panel = new StackPanel();
        
        panel.Children.Add(new TextBlock
        {
            Text = label,
            FontSize = 14,
            Foreground = Brushes.White,
            Margin = new Avalonia.Thickness(0, 0, 0, 5)
        });
        
        panel.Children.Add(new TextBlock
        {
            Text = value,
            FontSize = 32,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White
        });
        
        box.Child = panel;
        return box;
    }
    
    private void approveQualification(MasterModel master)
    {
        master.qualificationLevel++;
        master.qualificationRequestPending = false;
        master.lastModified = DateTime.UtcNow;
        dbContext.SaveChanges();
        onMastersClick(null, new RoutedEventArgs());
    }
    
    private void upgradeQualification(MasterModel master)
    {
        master.qualificationLevel++;
        master.lastModified = DateTime.UtcNow;
        dbContext.SaveChanges();
        onMastersClick(null, new RoutedEventArgs());
    }
    
    private void onCollectionsClick(object? sender, RoutedEventArgs e)
    {
        var view = new StackPanel { Margin = new Avalonia.Thickness(30) };
        
        var title = new TextBlock 
        { 
            Text = "КОЛЛЕКЦИИ", 
            FontSize = 28, 
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#2C2C2C")),
            Margin = new Avalonia.Thickness(0, 0, 0, 30)
        };
        view.Children.Add(title);
        
        var description = new TextBlock 
        { 
            Text = "Основные направления лавки по коллекциям:", 
            FontSize = 16,
            Foreground = new SolidColorBrush(Color.Parse("#666666")),
            Margin = new Avalonia.Thickness(0, 0, 0, 20)
        };
        view.Children.Add(description);
        
        var collections = new StackPanel { Spacing = 10, Margin = new Avalonia.Thickness(20, 0, 0, 30) };
        
        collections.Children.Add(new TextBlock 
        { 
            Text = "• Аниме", 
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.Parse("#444444"))
        });
        collections.Children.Add(new TextBlock 
        { 
            Text = "• Новый год", 
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.Parse("#444444"))
        });
        collections.Children.Add(new TextBlock 
        { 
            Text = "• Хэллоуин", 
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.Parse("#444444"))
        });
        collections.Children.Add(new TextBlock 
        { 
            Text = "• Киберпанк", 
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.Parse("#444444"))
        });
        collections.Children.Add(new TextBlock 
        { 
            Text = "• Нуар", 
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.Parse("#444444"))
        });
        
        view.Children.Add(collections);
        
        contentArea.Content = view;
    }
    
    private void onLogoutClick(object? sender, RoutedEventArgs e)
    {
        var loginWindow = new LoginWindow();
        
        // Устанавливаем обработчик для открытия нового dashboard после входа
        loginWindow.Closed += (s, args) =>
        {
            if (loginWindow.loggedInUser != null)
            {
                var user = loginWindow.loggedInUser;
                Window? newWindow = null;
                
                if (SessiaAval.Services.AuthorizationService.isUser(user))
                {
                    newWindow = new Views.User.UserDashboardWindow(user);
                }
                else if (SessiaAval.Services.AuthorizationService.isModerator(user))
                {
                    newWindow = new ModeratorDashboardWindow(user);
                }
                else if (SessiaAval.Services.AuthorizationService.isAdmin(user))
                {
                    newWindow = new Views.Admin.AdminDashboardWindow(user);
                }
                else if (SessiaAval.Services.AuthorizationService.isMaster(user))
                {
                    newWindow = new Views.Master.MasterDashboardWindow(user);
                }
                
                if (newWindow != null)
                {
                    newWindow.Show();
                }
            }
        };
        
        loginWindow.Show();
        Close();
    }
}

// Конвертер для преобразования минут в читаемый формат (используется повторно)
public class MinutesToHoursConverter : Avalonia.Data.Converters.IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is int minutes)
        {
            if (minutes < 60)
                return $"{minutes} мин";
            
            var hours = minutes / 60;
            var remainingMinutes = minutes % 60;
            
            if (remainingMinutes == 0)
                return $"{hours} ч";
            
            return $"{hours} ч {remainingMinutes} мин";
        }
        return "—";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

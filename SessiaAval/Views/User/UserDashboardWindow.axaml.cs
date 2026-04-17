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

namespace SessiaAval.Views.User;

public partial class UserDashboardWindow : Window
{
    private readonly UserModel currentUser;
    private readonly AppDbContext dbContext;
    private ObservableCollection<Appointment> myAppointments;
    private ObservableCollection<Service> availableServices;
    private ObservableCollection<Review> myReviews;
    private ObservableCollection<BalanceTransaction> transactions;

    public UserDashboardWindow(UserModel user)
    {
        InitializeComponent();
        
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=matye_db;Username=postgres;Password=0206");
        dbContext = new AppDbContext(optionsBuilder.Options);
        
        currentUser = dbContext.users.Find(user.userId)!;
        userNameText.Text = currentUser.fullName;
        
        myAppointments = new ObservableCollection<Appointment>();
        availableServices = new ObservableCollection<Service>();
        myReviews = new ObservableCollection<Review>();
        transactions = new ObservableCollection<BalanceTransaction>();
        
        showBalanceView();
    }

    private void showBalanceView()
    {
        // Обновляем данные пользователя из БД
        var user = dbContext.users.Find(currentUser.userId);
        if (user != null)
        {
            currentUser.balance = user.balance;
        }
        
        var balanceView = new StackPanel { Margin = new Avalonia.Thickness(30) };
        
        var title = new TextBlock 
        { 
            Text = "МОЙ БАЛАНС", 
            FontSize = 28, 
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#2C2C2C")),
            Margin = new Avalonia.Thickness(0, 0, 0, 30)
        };
        balanceView.Children.Add(title);
        
        var balanceBox = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#E8F5E9")),
            BorderBrush = new SolidColorBrush(Color.Parse("#4CAF50")),
            BorderThickness = new Avalonia.Thickness(2),
            CornerRadius = new Avalonia.CornerRadius(12),
            Padding = new Avalonia.Thickness(30),
            Margin = new Avalonia.Thickness(0, 0, 0, 30),
            Width = 400
        };
        
        var balancePanel = new StackPanel();
        balancePanel.Children.Add(new TextBlock 
        { 
            Text = "Текущий баланс:", 
            FontSize = 16,
            Foreground = new SolidColorBrush(Color.Parse("#2E7D32")),
            Margin = new Avalonia.Thickness(0, 0, 0, 10)
        });
        balancePanel.Children.Add(new TextBlock 
        { 
            Text = $"{currentUser.balance:N2} ₽", 
            FontSize = 36,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#1B5E20"))
        });
        balanceBox.Child = balancePanel;
        balanceView.Children.Add(balanceBox);
        
        var description = new TextBlock 
        { 
            Text = "Используйте баланс для оплаты услуг. Пополнить баланс можно с банковской карты.", 
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.Parse("#666666")),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Avalonia.Thickness(0, 0, 0, 20),
            MaxWidth = 500
        };
        balanceView.Children.Add(description);
        
        var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 15 };
        
        var topUpButton = new Button 
        { 
            Content = "Пополнить баланс", 
            Width = 200,
            Height = 40,
            FontSize = 14
        };
        topUpButton.Classes.Add("primary");
        topUpButton.Click += onTopUpClick;
        buttonPanel.Children.Add(topUpButton);
        
        balanceView.Children.Add(buttonPanel);
        
        // История транзакций
        balanceView.Children.Add(new TextBlock 
        { 
            Text = "История операций:", 
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Margin = new Avalonia.Thickness(0, 30, 0, 15)
        });
        
        loadTransactions();
        
        var transactionsGrid = new DataGrid
        {
            ItemsSource = transactions,
            AutoGenerateColumns = false,
            IsReadOnly = true,
            GridLinesVisibility = DataGridGridLinesVisibility.All,
            Background = Brushes.White,
            Foreground = Brushes.Black,
            MaxHeight = 300
        };
        
        transactionsGrid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "Дата", 
            Binding = new Avalonia.Data.Binding("transactionDate") { StringFormat = "dd.MM.yyyy HH:mm" },
            Width = new DataGridLength(150)
        });
        transactionsGrid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "Тип", 
            Binding = new Avalonia.Data.Binding("transactionType"),
            Width = new DataGridLength(120)
        });
        transactionsGrid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "Сумма", 
            Binding = new Avalonia.Data.Binding("amount") { StringFormat = "{0:N2} ₽" },
            Width = new DataGridLength(120)
        });
        transactionsGrid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "Карта", 
            Binding = new Avalonia.Data.Binding("cardLastDigits"),
            Width = new DataGridLength(100)
        });
        
        balanceView.Children.Add(transactionsGrid);
        
        contentArea.Content = balanceView;
    }
    
    private void loadTransactions()
    {
        transactions.Clear();
        var txList = dbContext.balanceTransactions
            .Where(t => t.userId == currentUser.userId)
            .OrderByDescending(t => t.transactionDate)
            .Take(20)
            .ToList();
        
        foreach (var tx in txList)
        {
            transactions.Add(tx);
        }
    }

    private void onBalanceClick(object? sender, RoutedEventArgs e) => showBalanceView();
    
    private void onAppointmentsClick(object? sender, RoutedEventArgs e)
    {
        var view = new StackPanel { Margin = new Avalonia.Thickness(30) };
        
        var title = new TextBlock 
        { 
            Text = "МОИ ЗАПИСИ", 
            FontSize = 28, 
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#2C2C2C")),
            Margin = new Avalonia.Thickness(0, 0, 0, 30)
        };
        view.Children.Add(title);
        
        loadAppointments();
        
        if (myAppointments.Count == 0)
        {
            var infoBox = new Border
            {
                Background = new SolidColorBrush(Color.Parse("#FFF3E0")),
                BorderBrush = new SolidColorBrush(Color.Parse("#FF9800")),
                BorderThickness = new Avalonia.Thickness(1),
                CornerRadius = new Avalonia.CornerRadius(8),
                Padding = new Avalonia.Thickness(20),
                Margin = new Avalonia.Thickness(0, 0, 0, 20)
            };
            
            var infoText = new TextBlock
            {
                Text = "У вас пока нет записей на услуги.",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.Parse("#E65100")),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };
            infoBox.Child = infoText;
            view.Children.Add(infoBox);
        }
        else
        {
            var appointmentsGrid = new DataGrid
            {
                ItemsSource = myAppointments,
                AutoGenerateColumns = false,
                IsReadOnly = true,
                GridLinesVisibility = DataGridGridLinesVisibility.All,
                Background = Brushes.White,
                Foreground = Brushes.Black,
                MaxHeight = 400
            };
            
            appointmentsGrid.Columns.Add(new DataGridTextColumn 
            { 
                Header = "Дата/Время", 
                Binding = new Avalonia.Data.Binding("appointmentDate") { StringFormat = "dd.MM.yyyy HH:mm" },
                Width = new DataGridLength(150)
            });
            appointmentsGrid.Columns.Add(new DataGridTextColumn 
            { 
                Header = "Услуга", 
                Binding = new Avalonia.Data.Binding("service.serviceName"),
                Width = new DataGridLength(200)
            });
            appointmentsGrid.Columns.Add(new DataGridTextColumn 
            { 
                Header = "Мастер", 
                Binding = new Avalonia.Data.Binding("master.user.fullName"),
                Width = new DataGridLength(150)
            });
            appointmentsGrid.Columns.Add(new DataGridTextColumn 
            { 
                Header = "Номер в очереди", 
                Binding = new Avalonia.Data.Binding("queueNumber"),
                Width = new DataGridLength(120)
            });
            appointmentsGrid.Columns.Add(new DataGridTextColumn 
            { 
                Header = "Статус", 
                Binding = new Avalonia.Data.Binding("status"),
                Width = new DataGridLength(120)
            });
            
            view.Children.Add(appointmentsGrid);
        }
        
        var button = new Button 
        { 
            Content = "Записаться на услугу", 
            Width = 200,
            Height = 40,
            FontSize = 14,
            Margin = new Avalonia.Thickness(0, 20, 0, 0)
        };
        button.Classes.Add("primary");
        button.Click += onBookServiceClick;
        view.Children.Add(button);
        
        contentArea.Content = view;
    }
    
    private void loadAppointments()
    {
        myAppointments.Clear();
        var appointments = dbContext.appointments
            .Include(a => a.service)
            .Include(a => a.master)
            .ThenInclude(m => m!.user)
            .Where(a => a.userId == currentUser.userId)
            .OrderByDescending(a => a.appointmentDate)
            .ToList();
        
        foreach (var apt in appointments)
        {
            myAppointments.Add(apt);
        }
    }
    
    private void onBookServiceClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new Window
        {
            Title = "Запись на услугу",
            Width = 550,
            Height = 500,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var panel = new StackPanel { Margin = new Avalonia.Thickness(20), Spacing = 15 };
        
        panel.Children.Add(new TextBlock 
        { 
            Text = "ЗАПИСЬ НА УСЛУГУ",
            FontSize = 18,
            FontWeight = FontWeight.Bold
        });
        
        // Загружаем услуги
        var services = dbContext.services
            .Include(s => s.category)
            .OrderBy(s => s.serviceName)
            .ToList();
        
        panel.Children.Add(new TextBlock { Text = "Выберите услугу:" });
        var serviceCombo = new ComboBox { Width = 400 };
        foreach (var service in services)
        {
            serviceCombo.Items.Add(new ComboBoxItem 
            { 
                Content = $"{service.serviceName} - {service.price:N2} ₽ ({service.durationMinutes} мин)",
                Tag = service
            });
        }
        if (services.Count > 0)
            serviceCombo.SelectedIndex = 0;
        panel.Children.Add(serviceCombo);
        
        panel.Children.Add(new TextBlock { Text = "Выберите мастера:" });
        var masterCombo = new ComboBox { Width = 400 };
        panel.Children.Add(masterCombo);
        
        // При выборе услуги загружаем мастеров
        serviceCombo.SelectionChanged += (s, args) =>
        {
            masterCombo.Items.Clear();
            if (serviceCombo.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is Service selectedService)
            {
                // Загружаем ВСЕХ мастеров, независимо от назначения услуг
                var masters = dbContext.masters
                    .Include(m => m.user)
                    .ToList() // Загружаем в память сначала
                    .OrderBy(m => m.user?.fullName ?? "")
                    .ToList();
                
                foreach (var master in masters)
                {
                    if (master != null && master.user != null)
                    {
                        masterCombo.Items.Add(new ComboBoxItem 
                        { 
                            Content = $"{master.user.fullName} (Квалификация: {master.qualificationLevel})",
                            Tag = master
                        });
                    }
                }
                
                if (masters.Count > 0)
                    masterCombo.SelectedIndex = 0;
            }
        };
        
        // Загружаем мастеров для первой услуги
        if (services.Count > 0)
        {
            serviceCombo.SelectedIndex = 0;
            
            // Вручную загружаем ВСЕХ мастеров для первой услуги
            var allMasters = dbContext.masters
                .Include(m => m.user)
                .ToList() // Загружаем в память сначала
                .OrderBy(m => m.user?.fullName ?? "")
                .ToList();
            
            foreach (var master in allMasters)
            {
                if (master != null && master.user != null)
                {
                    masterCombo.Items.Add(new ComboBoxItem 
                    { 
                        Content = $"{master.user.fullName} (Квалификация: {master.qualificationLevel})",
                        Tag = master
                    });
                }
            }
            
            if (allMasters.Count > 0)
                masterCombo.SelectedIndex = 0;
        }
        
        panel.Children.Add(new TextBlock { Text = "Дата записи:" });
        var datePicker = new DatePicker { SelectedDate = DateTimeOffset.Now.AddDays(1) };
        panel.Children.Add(datePicker);
        
        panel.Children.Add(new TextBlock { Text = "Время (часы):" });
        var timeCombo = new ComboBox { Width = 150 };
        for (int h = 9; h <= 20; h++)
        {
            timeCombo.Items.Add($"{h:D2}:00");
            timeCombo.Items.Add($"{h:D2}:30");
        }
        timeCombo.SelectedIndex = 0;
        panel.Children.Add(timeCombo);
        
        var buttonPanel = new StackPanel 
        { 
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10,
            Margin = new Avalonia.Thickness(0, 20, 0, 0)
        };

        var bookButton = new Button { Content = "Записаться", Width = 100 };
        bookButton.Classes.Add("primary");
        bookButton.Click += (s, args) => 
        {
            if (serviceCombo.SelectedItem is ComboBoxItem sItem && sItem.Tag is Service service &&
                masterCombo.SelectedItem is ComboBoxItem mItem && mItem.Tag is MasterModel master &&
                datePicker.SelectedDate.HasValue &&
                timeCombo.SelectedItem is string timeStr)
            {
                var timeParts = timeStr.Split(':');
                var appointmentDate = datePicker.SelectedDate.Value.DateTime
                    .AddHours(int.Parse(timeParts[0]))
                    .AddMinutes(int.Parse(timeParts[1]));
                
                // Генерируем номер в очереди
                var existingCount = dbContext.appointments
                    .Count(a => a.masterId == master.masterId && 
                                a.appointmentDate.Date == appointmentDate.Date &&
                                a.status != "Отменена");
                var queueNumber = existingCount + 1;
                
                var appointment = new Appointment
                {
                    userId = currentUser.userId,
                    masterId = master.masterId,
                    serviceId = service.serviceId,
                    appointmentDate = appointmentDate,
                    status = "pending",
                    queueNumber = queueNumber,
                    createdDate = DateTime.UtcNow,
                    lastModified = DateTime.UtcNow
                };
                
                dbContext.appointments.Add(appointment);
                dbContext.SaveChanges();
                
                dialog.Close();
                onAppointmentsClick(null, new RoutedEventArgs());
                
                // Показываем уведомление
                var successDialog = new Window
                {
                    Title = "Успешно",
                    Width = 400,
                    Height = 200,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                
                var successPanel = new StackPanel { Margin = new Avalonia.Thickness(20), Spacing = 15 };
                successPanel.Children.Add(new TextBlock 
                { 
                    Text = $"Вы успешно записаны!\n\nУслуга: {service.serviceName}\nМастер: {master.user?.fullName}\nДата: {appointmentDate:dd.MM.yyyy HH:mm}\nВаш номер в очереди: {queueNumber}",
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    FontSize = 14
                });
                
                var okBtn = new Button 
                { 
                    Content = "OK", 
                    Width = 100,
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                okBtn.Click += (s2, e2) => successDialog.Close();
                successPanel.Children.Add(okBtn);
                
                successDialog.Content = successPanel;
                successDialog.ShowDialog(this);
            }
        };
        buttonPanel.Children.Add(bookButton);
        
        var cancelButton = new Button { Content = "Отмена", Width = 100 };
        cancelButton.Click += (s, args) => { dialog.Close(); };
        buttonPanel.Children.Add(cancelButton);
        
        panel.Children.Add(buttonPanel);

        dialog.Content = panel;
        dialog.ShowDialog(this);
    }
    
    private StackPanel? currentServicesPanel;
    
    private void onServicesClick(object? sender, RoutedEventArgs e)
    {
        var mainPanel = new StackPanel { Margin = new Avalonia.Thickness(30) };
        
        var title = new TextBlock 
        { 
            Text = "КАТАЛОГ УСЛУГ", 
            FontSize = 28, 
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#2C2C2C")),
            Margin = new Avalonia.Thickness(0, 0, 0, 20)
        };
        mainPanel.Children.Add(title);
        
        // Панель фильтров (кнопки коллекций)
        var filterPanel = new StackPanel 
        { 
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Margin = new Avalonia.Thickness(0, 0, 0, 20)
        };
        
        // Кнопка "Все"
        var allButton = new Button 
        { 
            Content = "Все",
            Width = 100,
            Height = 35,
            FontSize = 13
        };
        allButton.Classes.Add("secondary");
        allButton.Click += (s, args) => filterServicesByCollection(null);
        filterPanel.Children.Add(allButton);
        
        // Загружаем коллекции из БД
        var collections = dbContext.Set<Collection>().ToList();
        foreach (var collection in collections)
        {
            var collectionButton = new Button 
            { 
                Content = collection.collectionName,
                Width = 120,
                Height = 35,
                FontSize = 13,
                Tag = collection.collectionId
            };
            collectionButton.Classes.Add("secondary");
            collectionButton.Click += (s, args) => 
            {
                if (s is Button btn && btn.Tag is int collId)
                    filterServicesByCollection(collId);
            };
            filterPanel.Children.Add(collectionButton);
        }
        
        mainPanel.Children.Add(filterPanel);
        
        // ScrollViewer для списка услуг
        var scrollViewer = new ScrollViewer 
        { 
            Height = 480,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled
        };
        
        currentServicesPanel = new StackPanel 
        { 
            Spacing = 15
        };
        
        loadServices();
        
        // Создаем горизонтальные карточки
        foreach (var service in availableServices)
        {
            var card = createHorizontalServiceCard(service);
            currentServicesPanel.Children.Add(card);
        }
        
        scrollViewer.Content = currentServicesPanel;
        mainPanel.Children.Add(scrollViewer);
        
        contentArea.Content = mainPanel;
    }
    
    private void filterServicesByCollection(int? collectionId)
    {
        if (currentServicesPanel == null)
            return;
            
        availableServices.Clear();
        
        var query = dbContext.services
            .Include(s => s.category)
            .AsQueryable();
        
        if (collectionId.HasValue)
        {
            query = query.Where(s => s.collectionId == collectionId.Value);
        }
        
        var services = query.OrderBy(s => s.serviceName).ToList();
        
        foreach (var service in services)
        {
            availableServices.Add(service);
        }
        
        // Обновляем только панель с карточками
        currentServicesPanel.Children.Clear();
        foreach (var service in availableServices)
        {
            var card = createHorizontalServiceCard(service);
            currentServicesPanel.Children.Add(card);
        }
    }
    
    private Border createHorizontalServiceCard(Service service)
    {
        var card = new Border
        {
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.Parse("#E0E0E0")),
            BorderThickness = new Avalonia.Thickness(1),
            CornerRadius = new Avalonia.CornerRadius(8),
            Padding = new Avalonia.Thickness(0),
            Height = 150,
            BoxShadow = new BoxShadows(new BoxShadow 
            { 
                OffsetX = 0, 
                OffsetY = 2, 
                Blur = 4, 
                Color = Color.Parse("#30000000") 
            })
        };
        
        var horizontalPanel = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("200,*")
        };
        
        // Левая часть - фото
        Border imageContainer;
        try
        {
            var image = new Image
            {
                Width = 200,
                Height = 150,
                Stretch = Avalonia.Media.Stretch.UniformToFill,
                Source = service.imageSource
            };
            imageContainer = new Border
            {
                Width = 200,
                Height = 150,
                Child = image,
                ClipToBounds = true
            };
        }
        catch
        {
            imageContainer = new Border
            {
                Width = 200,
                Height = 150,
                Background = new SolidColorBrush(Color.Parse("#F5F5F5")),
                Child = new TextBlock
                {
                    Text = "Нет фото",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Color.Parse("#999999"))
                }
            };
        }
        
        Grid.SetColumn(imageContainer, 0);
        horizontalPanel.Children.Add(imageContainer);
        
        // Правая часть - информация
        var infoPanel = new StackPanel 
        { 
            Margin = new Avalonia.Thickness(20, 15, 20, 15),
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Center
        };
        
        // Название услуги
        var serviceName = new TextBlock
        {
            Text = service.serviceName,
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#2C2C2C")),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        };
        infoPanel.Children.Add(serviceName);
        
        // Описание (если есть)
        if (!string.IsNullOrEmpty(service.description))
        {
            var description = new TextBlock
            {
                Text = service.description.Length > 100 
                    ? service.description.Substring(0, 100) + "..." 
                    : service.description,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.Parse("#666666")),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                MaxHeight = 35
            };
            infoPanel.Children.Add(description);
        }
        
        // Панель с ценой и длительностью
        var detailsPanel = new StackPanel 
        { 
            Orientation = Orientation.Horizontal,
            Spacing = 20
        };
        
        var price = new TextBlock
        {
            Text = $"Цена: {service.price:N0} руб.",
            FontSize = 14,
            FontWeight = FontWeight.SemiBold,
            Foreground = new SolidColorBrush(Color.Parse("#4CAF50"))
        };
        detailsPanel.Children.Add(price);
        
        var duration = new TextBlock
        {
            Text = $"Обновлено: {service.lastModified:dd.MM.yyyy}",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#999999"))
        };
        detailsPanel.Children.Add(duration);
        
        infoPanel.Children.Add(detailsPanel);
        
        // Кнопка "Записаться"
        var bookButton = new Button
        {
            Content = "Записаться",
            Width = 150,
            Height = 35,
            FontSize = 13,
            Margin = new Avalonia.Thickness(0, 5, 0, 0)
        };
        bookButton.Classes.Add("primary");
        bookButton.Click += (s, e) => onBookServiceClick(s, e);
        infoPanel.Children.Add(bookButton);
        
        Grid.SetColumn(infoPanel, 1);
        horizontalPanel.Children.Add(infoPanel);
        
        card.Child = horizontalPanel;
        return card;
    }
    
    private void loadServices()
    {
        availableServices.Clear();
        var services = dbContext.services
            .Include(s => s.category)
            .OrderBy(s => s.serviceName)
            .ToList();
        
        foreach (var service in services)
        {
            availableServices.Add(service);
        }
    }
    
    private void onReviewsClick(object? sender, RoutedEventArgs e)
    {
        var view = new StackPanel { Margin = new Avalonia.Thickness(30) };
        
        var title = new TextBlock 
        { 
            Text = "МОИ ОТЗЫВЫ", 
            FontSize = 28, 
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#2C2C2C")),
            Margin = new Avalonia.Thickness(0, 0, 0, 30)
        };
        view.Children.Add(title);
        
        loadReviews();
        
        if (myReviews.Count == 0)
        {
            var infoBox = new Border
            {
                Background = new SolidColorBrush(Color.Parse("#F3E5F5")),
                BorderBrush = new SolidColorBrush(Color.Parse("#9C27B0")),
                BorderThickness = new Avalonia.Thickness(1),
                CornerRadius = new Avalonia.CornerRadius(8),
                Padding = new Avalonia.Thickness(20),
                Margin = new Avalonia.Thickness(0, 0, 0, 20)
            };
            
            var infoText = new TextBlock
            {
                Text = "Вы еще не оставляли отзывов. Ваши отзывы помогают другим клиентам выбрать услугу и мастера.",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.Parse("#6A1B9A")),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };
            infoBox.Child = infoText;
            view.Children.Add(infoBox);
        }
        else
        {
            var reviewsGrid = new DataGrid
            {
                ItemsSource = myReviews,
                AutoGenerateColumns = false,
                IsReadOnly = true,
                GridLinesVisibility = DataGridGridLinesVisibility.All,
                Background = Brushes.White,
                Foreground = Brushes.Black,
                MaxHeight = 400
            };
            
            reviewsGrid.Columns.Add(new DataGridTextColumn 
            { 
                Header = "Дата", 
                Binding = new Avalonia.Data.Binding("reviewDate") { StringFormat = "dd.MM.yyyy" },
                Width = new DataGridLength(100)
            });
            reviewsGrid.Columns.Add(new DataGridTextColumn 
            { 
                Header = "Услуга", 
                Binding = new Avalonia.Data.Binding("service.serviceName"),
                Width = new DataGridLength(180)
            });
            reviewsGrid.Columns.Add(new DataGridTextColumn 
            { 
                Header = "Мастер", 
                Binding = new Avalonia.Data.Binding("master.user.fullName"),
                Width = new DataGridLength(150)
            });
            reviewsGrid.Columns.Add(new DataGridTextColumn 
            { 
                Header = "Оценка", 
                Binding = new Avalonia.Data.Binding("rating"),
                Width = new DataGridLength(80)
            });
            reviewsGrid.Columns.Add(new DataGridTextColumn 
            { 
                Header = "Комментарий", 
                Binding = new Avalonia.Data.Binding("comment"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            });
            
            view.Children.Add(reviewsGrid);
        }
        
        var button = new Button 
        { 
            Content = "Написать отзыв", 
            Width = 200,
            Height = 40,
            FontSize = 14,
            Margin = new Avalonia.Thickness(0, 20, 0, 0)
        };
        button.Classes.Add("primary");
        button.Click += onWriteReviewClick;
        view.Children.Add(button);
        
        contentArea.Content = view;
    }
    
    private void loadReviews()
    {
        myReviews.Clear();
        var reviews = dbContext.reviews
            .Include(r => r.service)
            .Include(r => r.master)
            .ThenInclude(m => m!.user)
            .Where(r => r.userId == currentUser.userId)
            .OrderByDescending(r => r.reviewDate)
            .ToList();
        
        foreach (var review in reviews)
        {
            myReviews.Add(review);
        }
    }
    
    private void onWriteReviewClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new Window
        {
            Title = "Написать отзыв",
            Width = 550,
            Height = 500,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var panel = new StackPanel { Margin = new Avalonia.Thickness(20), Spacing = 15 };
        
        panel.Children.Add(new TextBlock 
        { 
            Text = "НАПИСАТЬ ОТЗЫВ",
            FontSize = 18,
            FontWeight = FontWeight.Bold
        });
        
        // Загружаем завершенные записи пользователя
        var completedAppointments = dbContext.appointments
            .Include(a => a.service)
            .Include(a => a.master)
            .ThenInclude(m => m!.user)
            .Where(a => a.userId == currentUser.userId && a.status == "completed")
            .OrderByDescending(a => a.appointmentDate)
            .ToList();
        
        if (completedAppointments.Count == 0)
        {
            panel.Children.Add(new TextBlock 
            { 
                Text = "У вас нет завершенных записей для написания отзыва.",
                Foreground = new SolidColorBrush(Color.Parse("#FF5722")),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            });
            
            var okBtn = new Button 
            { 
                Content = "OK", 
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            okBtn.Click += (s, args) => dialog.Close();
            panel.Children.Add(okBtn);
            
            dialog.Content = panel;
            dialog.ShowDialog(this);
            return;
        }
        
        panel.Children.Add(new TextBlock { Text = "Выберите запись:" });
        var appointmentCombo = new ComboBox { Width = 400 };
        foreach (var apt in completedAppointments)
        {
            appointmentCombo.Items.Add(new ComboBoxItem 
            { 
                Content = $"{apt.service?.serviceName} - {apt.master?.user?.fullName} ({apt.appointmentDate:dd.MM.yyyy})",
                Tag = apt
            });
        }
        appointmentCombo.SelectedIndex = 0;
        panel.Children.Add(appointmentCombo);
        
        panel.Children.Add(new TextBlock { Text = "Оценка (1-5):" });
        var ratingCombo = new ComboBox { Width = 100 };
        for (int i = 1; i <= 5; i++)
            ratingCombo.Items.Add(i.ToString());
        ratingCombo.SelectedIndex = 4;
        panel.Children.Add(ratingCombo);
        
        panel.Children.Add(new TextBlock { Text = "Комментарий:" });
        var commentBox = new TextBox 
        { 
            Watermark = "Ваш отзыв...", 
            Height = 120,
            AcceptsReturn = true,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        };
        panel.Children.Add(commentBox);
        
        var buttonPanel = new StackPanel 
        { 
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10,
            Margin = new Avalonia.Thickness(0, 20, 0, 0)
        };

        var sendButton = new Button { Content = "Отправить", Width = 100 };
        sendButton.Classes.Add("primary");
        sendButton.Click += (s, args) => 
        {
            if (appointmentCombo.SelectedItem is ComboBoxItem item && item.Tag is Appointment apt &&
                ratingCombo.SelectedItem is string ratingStr &&
                !string.IsNullOrWhiteSpace(commentBox.Text))
            {
                var review = new Review
                {
                    userId = currentUser.userId,
                    serviceId = apt.serviceId,
                    masterId = apt.masterId,
                    rating = int.Parse(ratingStr),
                    comment = commentBox.Text,
                    reviewDate = DateTime.UtcNow,
                    lastModified = DateTime.UtcNow
                };
                
                dbContext.reviews.Add(review);
                dbContext.SaveChanges();
                
                dialog.Close();
                onReviewsClick(null, new RoutedEventArgs());
                
                // Показываем уведомление
                var successDialog = new Window
                {
                    Title = "Успешно",
                    Width = 350,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                
                var successPanel = new StackPanel { Margin = new Avalonia.Thickness(20), Spacing = 15 };
                successPanel.Children.Add(new TextBlock 
                { 
                    Text = "Спасибо за ваш отзыв!",
                    FontSize = 14
                });
                
                var okBtn2 = new Button 
                { 
                    Content = "OK", 
                    Width = 100,
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                okBtn2.Click += (s2, e2) => successDialog.Close();
                successPanel.Children.Add(okBtn2);
                
                successDialog.Content = successPanel;
                successDialog.ShowDialog(this);
            }
        };
        buttonPanel.Children.Add(sendButton);
        
        var cancelButton = new Button { Content = "Отмена", Width = 100 };
        cancelButton.Click += (s, args) => { dialog.Close(); };
        buttonPanel.Children.Add(cancelButton);
        
        panel.Children.Add(buttonPanel);

        dialog.Content = panel;
        dialog.ShowDialog(this);
    }
    
    private void onTopUpClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new Window
        {
            Title = "Пополнение баланса",
            Width = 450,
            Height = 350,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false
        };

        var panel = new StackPanel { Margin = new Avalonia.Thickness(20), Spacing = 15 };
        
        panel.Children.Add(new TextBlock 
        { 
            Text = "ПОПОЛНЕНИЕ БАЛАНСА",
            FontSize = 18,
            FontWeight = FontWeight.Bold
        });
        
        panel.Children.Add(new TextBlock 
        { 
            Text = "Введите сумму пополнения:",
            FontSize = 14
        });
        
        var amountBox = new TextBox 
        { 
            Watermark = "Сумма в рублях",
            Width = 200
        };
        panel.Children.Add(amountBox);
        
        panel.Children.Add(new TextBlock 
        { 
            Text = "Данные карты:",
            FontSize = 14,
            Margin = new Avalonia.Thickness(0, 10, 0, 0)
        });
        
        var cardBox = new TextBox 
        { 
            Watermark = "Номер карты (16 цифр)",
            Width = 300
        };
        panel.Children.Add(cardBox);
        
        var expiryPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        var expiryBox = new TextBox 
        { 
            Watermark = "MM/YY",
            Width = 80
        };
        var cvvBox = new TextBox 
        { 
            Watermark = "CVV",
            Width = 80
        };
        expiryPanel.Children.Add(expiryBox);
        expiryPanel.Children.Add(cvvBox);
        panel.Children.Add(expiryPanel);
        
        var buttonPanel = new StackPanel 
        { 
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10,
            Margin = new Avalonia.Thickness(0, 20, 0, 0)
        };

        var okButton = new Button 
        { 
            Content = "Пополнить", 
            Width = 100
        };
        okButton.Classes.Add("primary");
        okButton.Click += (s, args) => 
        {
            // Валидация суммы
            if (!decimal.TryParse(amountBox.Text, out decimal amount) || amount <= 0)
            {
                showTopUpError("Введите корректную сумму (больше 0)");
                return;
            }
            
            // Валидация номера карты
            var cardNumber = cardBox.Text?.Trim().Replace(" ", "");
            if (string.IsNullOrWhiteSpace(cardNumber))
            {
                showTopUpError("Введите номер карты");
                return;
            }
            
            if (!cardNumber.All(char.IsDigit))
            {
                showTopUpError("Номер карты должен содержать только цифры");
                return;
            }
            
            if (cardNumber.Length != 16)
            {
                showTopUpError("Номер карты должен содержать 16 цифр");
                return;
            }
            
            // Валидация срока действия
            var expiry = expiryBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(expiry) || !System.Text.RegularExpressions.Regex.IsMatch(expiry, @"^\d{2}/\d{2}$"))
            {
                showTopUpError("Введите срок действия в формате MM/YY");
                return;
            }
            
            // Валидация CVV
            var cvv = cvvBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(cvv) || !cvv.All(char.IsDigit) || cvv.Length != 3)
            {
                showTopUpError("CVV должен содержать 3 цифры");
                return;
            }
            
            // Обновляем баланс пользователя
            var user = dbContext.users.Find(currentUser.userId);
            if (user != null)
            {
                user.balance += amount;
                user.lastModified = DateTime.UtcNow;
                
                // Создаем транзакцию
                var transaction = new BalanceTransaction
                {
                    userId = currentUser.userId,
                    amount = amount,
                    transactionType = "Пополнение",
                    cardLastDigits = cardNumber.Substring(12),
                    transactionDate = DateTime.UtcNow
                };
                
                dbContext.balanceTransactions.Add(transaction);
                dbContext.SaveChanges();
                
                currentUser.balance = user.balance;
                
                dialog.Close();
                showBalanceView();
                
                // Показываем уведомление
                var successDialog = new Window
                {
                    Title = "Успешно",
                    Width = 350,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                
                var successPanel = new StackPanel { Margin = new Avalonia.Thickness(20), Spacing = 15 };
                successPanel.Children.Add(new TextBlock 
                { 
                    Text = $"Баланс успешно пополнен на {amount:N2} ₽",
                    FontSize = 14
                });
                
                var okBtn = new Button 
                { 
                    Content = "OK", 
                    Width = 100,
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                okBtn.Click += (s2, e2) => successDialog.Close();
                successPanel.Children.Add(okBtn);
                
                successDialog.Content = successPanel;
                successDialog.ShowDialog(this);
            }
        };
        buttonPanel.Children.Add(okButton);
        
        var cancelButton = new Button 
        { 
            Content = "Отмена", 
            Width = 100
        };
        cancelButton.Click += (s, args) => { dialog.Close(); };
        buttonPanel.Children.Add(cancelButton);
        
        panel.Children.Add(buttonPanel);

        dialog.Content = panel;
        dialog.ShowDialog(this);
    }
    
    private void showTopUpError(string message)
    {
        var errorDialog = new Window
        {
            Title = "Ошибка",
            Width = 400,
            Height = 180,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        
        var errorPanel = new StackPanel { Margin = new Avalonia.Thickness(20), Spacing = 15 };
        errorPanel.Children.Add(new TextBlock 
        { 
            Text = message,
            Foreground = new SolidColorBrush(Color.Parse("#FF5722")),
            FontSize = 14,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        });
        
        var okBtn = new Button 
        { 
            Content = "OK", 
            Width = 100,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        okBtn.Click += (s, e) => errorDialog.Close();
        errorPanel.Children.Add(okBtn);
        
        errorDialog.Content = errorPanel;
        errorDialog.ShowDialog(this);
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
                    newWindow = new UserDashboardWindow(user);
                }
                else if (SessiaAval.Services.AuthorizationService.isModerator(user))
                {
                    newWindow = new Views.Moderator.ModeratorDashboardWindow(user);
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

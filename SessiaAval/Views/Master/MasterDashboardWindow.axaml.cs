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

namespace SessiaAval.Views.Master;

public partial class MasterDashboardWindow : Window
{
    private readonly UserModel currentUser;
    private readonly AppDbContext dbContext;
    private MasterModel? masterProfile;
    private ObservableCollection<Appointment> appointments;
    private ObservableCollection<Service> myServices;

    public MasterDashboardWindow(UserModel user)
    {
        InitializeComponent();
        currentUser = user;
        
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=matye_db;Username=postgres;Password=0206");
        dbContext = new AppDbContext(optionsBuilder.Options);
        
        appointments = new ObservableCollection<Appointment>();
        myServices = new ObservableCollection<Service>();
        
        loadMasterProfile();
        userNameText.Text = currentUser.fullName;
        
        showClientsView();
    }

    private void loadMasterProfile()
    {
        masterProfile = dbContext.masters.FirstOrDefault(m => m.userId == currentUser.userId);
        if (masterProfile == null)
        {
            // Создаем профиль мастера, если его нет
            masterProfile = new MasterModel
            {
                userId = currentUser.userId,
                qualificationLevel = 1,
                hireDate = DateTime.UtcNow,
                lastModified = DateTime.UtcNow
            };
            dbContext.masters.Add(masterProfile);
            dbContext.SaveChanges();
        }
    }

    private void loadAppointments()
    {
        appointments.Clear();
        if (masterProfile != null)
        {
            var allAppointments = dbContext.appointments
                .Include(a => a.user)
                .Include(a => a.service)
                .Where(a => a.masterId == masterProfile.masterId)
                .OrderBy(a => a.appointmentDate)
                .ToList();
            
            foreach (var appointment in allAppointments)
            {
                appointments.Add(appointment);
            }
        }
    }

    private void loadMyServices()
    {
        myServices.Clear();
        if (masterProfile != null)
        {
            var services = dbContext.masterServices
                .Include(ms => ms.service)
                .ThenInclude(s => s!.category)
                .Where(ms => ms.masterId == masterProfile.masterId)
                .Select(ms => ms.service)
                .ToList();
            
            foreach (var service in services)
            {
                if (service != null)
                    myServices.Add(service);
            }
        }
    }

    private void showClientsView()
    {
        loadAppointments();
        
        var view = new StackPanel { Margin = new Avalonia.Thickness(30) };
        
        var title = new TextBlock 
        { 
            Text = "МОИ КЛИЕНТЫ", 
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
        
        var totalBox = createStatBox("Всего записей", appointments.Count.ToString(), "#4A90E2");
        var activeBox = createStatBox("Активные", appointments.Count(a => a.status == "pending" || a.status == "Запланирована" || a.status == "confirmed").ToString(), "#FFA726");
        var completedBox = createStatBox("Завершено", appointments.Count(a => a.status == "completed" || a.status == "Завершена").ToString(), "#66BB6A");
        
        statsPanel.Children.Add(totalBox);
        statsPanel.Children.Add(activeBox);
        statsPanel.Children.Add(completedBox);
        view.Children.Add(statsPanel);
        
        // Таблица записей
        var scrollViewer = new ScrollViewer { Height = 450 };
        var dataGrid = new DataGrid
        {
            ItemsSource = appointments,
            AutoGenerateColumns = false,
            IsReadOnly = true,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            Background = Brushes.White,
            Foreground = Brushes.Black
        };
        
        dataGrid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "Дата", 
            Binding = new Avalonia.Data.Binding("appointmentDate") { StringFormat = "dd.MM.yyyy HH:mm" },
            Width = new DataGridLength(150)
        });
        
        dataGrid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "Клиент", 
            Binding = new Avalonia.Data.Binding("user.fullName"),
            Width = new DataGridLength(200)
        });
        
        dataGrid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "Услуга", 
            Binding = new Avalonia.Data.Binding("service.serviceName"),
            Width = new DataGridLength(250)
        });
        
        dataGrid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "Номер в очереди", 
            Binding = new Avalonia.Data.Binding("queueNumber"),
            Width = new DataGridLength(120)
        });
        
        dataGrid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "Статус", 
            Binding = new Avalonia.Data.Binding("status"),
            Width = new DataGridLength(120)
        });
        
        var actionsColumn = new DataGridTemplateColumn { Header = "Действия", Width = new DataGridLength(120) };
        actionsColumn.CellTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<Appointment>((appointment, _) =>
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5 };
            
            if (appointment != null && (appointment.status == "pending" || appointment.status == "Запланирована" || appointment.status == "confirmed"))
            {
                var completeBtn = new Button 
                { 
                    Content = "Завершить", 
                    Width = 100, 
                    Height = 35, 
                    FontSize = 11,
                    Padding = new Avalonia.Thickness(5)
                };
                completeBtn.Classes.Add("primary");
                completeBtn.Click += (s, e) => completeAppointment(appointment);
                panel.Children.Add(completeBtn);
            }
            else if (appointment != null && (appointment.status == "completed" || appointment.status == "Завершена"))
            {
                var completedText = new TextBlock 
                { 
                    Text = "Готово",
                    Foreground = new SolidColorBrush(Color.Parse("#4CAF50")),
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeight.SemiBold,
                    FontSize = 12
                };
                panel.Children.Add(completedText);
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
            Width = 180
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

    private void completeAppointment(Appointment appointment)
    {
        try
        {
            // Загружаем полную информацию о записи
            var fullAppointment = dbContext.appointments
                .Include(a => a.user)
                .Include(a => a.service)
                .FirstOrDefault(a => a.appointmentId == appointment.appointmentId);
            
            if (fullAppointment == null || fullAppointment.user == null || fullAppointment.service == null)
                return;
            
            // Проверяем баланс пользователя
            if (fullAppointment.user.balance < fullAppointment.service.price)
            {
                var errorDialog = new Window
                {
                    Title = "Ошибка",
                    Width = 400,
                    Height = 200,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                
                var errorPanel = new StackPanel { Margin = new Avalonia.Thickness(20), Spacing = 15 };
                errorPanel.Children.Add(new TextBlock 
                { 
                    Text = $"Недостаточно средств на балансе клиента!\n\nТребуется: {fullAppointment.service.price:N2} ₽\nДоступно: {fullAppointment.user.balance:N2} ₽",
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    FontSize = 14
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
                return;
            }
            
            // Списываем деньги с баланса пользователя
            fullAppointment.user.balance -= fullAppointment.service.price;
            fullAppointment.user.lastModified = DateTime.UtcNow;
            
            // Создаем транзакцию списания
            var transaction = new BalanceTransaction
            {
                userId = fullAppointment.user.userId,
                amount = -fullAppointment.service.price,
                transactionType = "Оплата услуги",
                cardLastDigits = null,
                transactionDate = DateTime.UtcNow
            };
            dbContext.balanceTransactions.Add(transaction);
            
            // Обновляем статус записи
            fullAppointment.status = "completed";
            fullAppointment.lastModified = DateTime.UtcNow;
            
            dbContext.SaveChanges();
            
            // Показываем уведомление об успехе
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
                Text = $"Заказ завершен!\n\nСписано с баланса клиента: {fullAppointment.service.price:N2} ₽\nОстаток: {fullAppointment.user.balance:N2} ₽",
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                FontSize = 14
            });
            
            var okBtn2 = new Button 
            { 
                Content = "OK", 
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            okBtn2.Click += (s, e) => successDialog.Close();
            successPanel.Children.Add(okBtn2);
            
            successDialog.Content = successPanel;
            successDialog.ShowDialog(this);
            
            showClientsView();
        }
        catch (Exception ex)
        {
            var errorDialog = new Window
            {
                Title = "Ошибка",
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            
            var errorPanel = new StackPanel { Margin = new Avalonia.Thickness(20), Spacing = 15 };
            errorPanel.Children.Add(new TextBlock 
            { 
                Text = $"Ошибка при завершении заказа:\n{ex.Message}",
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.Parse("#FF5722"))
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
    }

    private void onClientsClick(object? sender, RoutedEventArgs e) => showClientsView();
    
    private void onServicesClick(object? sender, RoutedEventArgs e)
    {
        loadMyServices();
        
        var view = new StackPanel { Margin = new Avalonia.Thickness(30) };
        
        var title = new TextBlock 
        { 
            Text = "МОИ УСЛУГИ", 
            FontSize = 28, 
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#2C2C2C")),
            Margin = new Avalonia.Thickness(0, 0, 0, 20)
        };
        view.Children.Add(title);
        
        var description = new TextBlock 
        { 
            Text = $"Всего услуг: {myServices.Count}", 
            FontSize = 16,
            Foreground = new SolidColorBrush(Color.Parse("#666666")),
            Margin = new Avalonia.Thickness(0, 0, 0, 20)
        };
        view.Children.Add(description);
        
        // Таблица услуг
        var scrollViewer = new ScrollViewer { Height = 500 };
        var dataGrid = new DataGrid
        {
            ItemsSource = myServices,
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
            Width = new DataGridLength(300)
        });
        
        dataGrid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "Категория", 
            Binding = new Avalonia.Data.Binding("category.categoryName"),
            Width = new DataGridLength(200)
        });
        
        dataGrid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "Цена", 
            Binding = new Avalonia.Data.Binding("price") { StringFormat = "{0:C}" },
            Width = new DataGridLength(120)
        });
        
        dataGrid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "Длительность", 
            Binding = new Avalonia.Data.Binding("durationMinutes") { Converter = new MinutesToHoursConverter() },
            Width = new DataGridLength(120)
        });
        
        scrollViewer.Content = dataGrid;
        view.Children.Add(scrollViewer);
        
        contentArea.Content = view;
    }
    
    private void onQualificationClick(object? sender, RoutedEventArgs e)
    {
        var view = new StackPanel { Margin = new Avalonia.Thickness(30) };
        
        var title = new TextBlock 
        { 
            Text = "МОЯ КВАЛИФИКАЦИЯ", 
            FontSize = 28, 
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#2C2C2C")),
            Margin = new Avalonia.Thickness(0, 0, 0, 30)
        };
        view.Children.Add(title);
        
        if (masterProfile != null)
        {
            var infoBox = new Border
            {
                Background = new SolidColorBrush(Color.Parse("#E3F2FD")),
                BorderBrush = new SolidColorBrush(Color.Parse("#2196F3")),
                BorderThickness = new Avalonia.Thickness(2),
                CornerRadius = new Avalonia.CornerRadius(12),
                Padding = new Avalonia.Thickness(30),
                Margin = new Avalonia.Thickness(0, 0, 0, 30),
                Width = 500
            };
            
            var infoPanel = new StackPanel { Spacing = 15 };
            
            infoPanel.Children.Add(new TextBlock 
            { 
                Text = "Текущий уровень квалификации:", 
                FontSize = 16,
                Foreground = new SolidColorBrush(Color.Parse("#1976D2"))
            });
            
            infoPanel.Children.Add(new TextBlock 
            { 
                Text = $"Уровень {masterProfile.qualificationLevel}", 
                FontSize = 42,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.Parse("#0D47A1"))
            });
            
            if (!string.IsNullOrEmpty(masterProfile.specialization))
            {
                infoPanel.Children.Add(new TextBlock 
                { 
                    Text = $"Специализация: {masterProfile.specialization}", 
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Color.Parse("#1976D2"))
                });
            }
            
            infoBox.Child = infoPanel;
            view.Children.Add(infoBox);
            
            var description = new TextBlock 
            { 
                Text = "Для повышения квалификации подайте заявку модератору. После одобрения ваш уровень будет повышен.", 
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.Parse("#666666")),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Margin = new Avalonia.Thickness(0, 0, 0, 20),
                MaxWidth = 600
            };
            view.Children.Add(description);
            
            var requestButton = new Button 
            { 
                Content = masterProfile.qualificationRequestPending ? "✓ Заявка подана" : "Подать заявку на повышение", 
                Width = 250,
                Height = 40,
                FontSize = 14,
                IsEnabled = !masterProfile.qualificationRequestPending
            };
            requestButton.Classes.Add("primary");
            requestButton.Click += onRequestUpgradeClick;
            view.Children.Add(requestButton);
            
            if (masterProfile.qualificationRequestPending)
            {
                var pendingNote = new TextBlock 
                { 
                    Text = "Ваша заявка ожидает рассмотрения модератором", 
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.Parse("#FF9800")),
                    Margin = new Avalonia.Thickness(0, 10, 0, 0)
                };
                view.Children.Add(pendingNote);
            }
        }
        
        contentArea.Content = view;
    }
    
    private void onReviewsClick(object? sender, RoutedEventArgs e)
    {
        var view = new StackPanel { Margin = new Avalonia.Thickness(30) };
        
        var title = new TextBlock 
        { 
            Text = "ОТЗЫВЫ ОБО МНЕ", 
            FontSize = 28, 
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#2C2C2C")),
            Margin = new Avalonia.Thickness(0, 0, 0, 20)
        };
        view.Children.Add(title);
        
        if (masterProfile == null)
        {
            view.Children.Add(new TextBlock { Text = "Профиль мастера не найден" });
            contentArea.Content = view;
            return;
        }
        
        // Загружаем отзывы о мастере
        var reviews = dbContext.reviews
            .Include(r => r.user)
            .Include(r => r.service)
            .Where(r => r.masterId == masterProfile.masterId)
            .OrderByDescending(r => r.reviewDate)
            .ToList();
        
        if (reviews.Count == 0)
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
                Text = "Пока нет отзывов о вашей работе. Продолжайте выполнять заказы качественно!",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.Parse("#E65100")),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };
            infoBox.Child = infoText;
            view.Children.Add(infoBox);
        }
        else
        {
            // Статистика
            var avgRating = reviews.Average(r => r.rating);
            var statsPanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal, 
                Spacing = 20,
                Margin = new Avalonia.Thickness(0, 0, 0, 20)
            };
            
            var totalBox = createStatBox("Всего отзывов", reviews.Count.ToString(), "#4A90E2");
            var ratingBox = createStatBox("Средняя оценка", $"{avgRating:F1} / 5", "#66BB6A");
            
            statsPanel.Children.Add(totalBox);
            statsPanel.Children.Add(ratingBox);
            view.Children.Add(statsPanel);
            
            // Таблица отзывов
            var scrollViewer = new ScrollViewer { Height = 450 };
            var dataGrid = new DataGrid
            {
                ItemsSource = reviews,
                AutoGenerateColumns = false,
                IsReadOnly = true,
                GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
                HeadersVisibility = DataGridHeadersVisibility.Column,
                Background = Brushes.White,
                Foreground = Brushes.Black
            };
            
            dataGrid.Columns.Add(new DataGridTextColumn 
            { 
                Header = "Дата", 
                Binding = new Avalonia.Data.Binding("reviewDate") { StringFormat = "dd.MM.yyyy" },
                Width = new DataGridLength(100)
            });
            
            dataGrid.Columns.Add(new DataGridTextColumn 
            { 
                Header = "Клиент", 
                Binding = new Avalonia.Data.Binding("user.fullName"),
                Width = new DataGridLength(150)
            });
            
            dataGrid.Columns.Add(new DataGridTextColumn 
            { 
                Header = "Услуга", 
                Binding = new Avalonia.Data.Binding("service.serviceName"),
                Width = new DataGridLength(200)
            });
            
            dataGrid.Columns.Add(new DataGridTextColumn 
            { 
                Header = "Оценка", 
                Binding = new Avalonia.Data.Binding("rating") { StringFormat = "{0} / 5" },
                Width = new DataGridLength(80)
            });
            
            dataGrid.Columns.Add(new DataGridTextColumn 
            { 
                Header = "Комментарий", 
                Binding = new Avalonia.Data.Binding("comment"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            });
            
            scrollViewer.Content = dataGrid;
            view.Children.Add(scrollViewer);
        }
        
        contentArea.Content = view;
    }
    
    private async void onRequestUpgradeClick(object? sender, RoutedEventArgs e)
    {
        if (masterProfile == null) return;
        
        masterProfile.qualificationRequestPending = true;
        masterProfile.lastModified = DateTime.UtcNow;
        dbContext.SaveChanges();
        
        var dialog = new Window
        {
            Title = "Заявка отправлена",
            Width = 400,
            Height = 180,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false
        };

        var panel = new StackPanel { Margin = new Avalonia.Thickness(20), Spacing = 15 };
        
        panel.Children.Add(new TextBlock 
        { 
            Text = "✓ Заявка успешно отправлена",
            FontSize = 16,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#4CAF50"))
        });
        
        panel.Children.Add(new TextBlock 
        { 
            Text = "Ваша заявка на повышение квалификации отправлена модератору. Вы получите уведомление после рассмотрения.",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            FontSize = 13
        });

        var okButton = new Button 
        { 
            Content = "OK", 
            Width = 100,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        okButton.Click += (s, args) => 
        { 
            dialog.Close();
            onQualificationClick(null, new RoutedEventArgs());
        };
        panel.Children.Add(okButton);

        dialog.Content = panel;
        await dialog.ShowDialog(this);
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
                    newWindow = new Views.Moderator.ModeratorDashboardWindow(user);
                }
                else if (SessiaAval.Services.AuthorizationService.isAdmin(user))
                {
                    newWindow = new Views.Admin.AdminDashboardWindow(user);
                }
                else if (SessiaAval.Services.AuthorizationService.isMaster(user))
                {
                    newWindow = new MasterDashboardWindow(user);
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

// Конвертер для преобразования минут в читаемый формат
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

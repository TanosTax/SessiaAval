using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SessiaAval.Data;
using UserModel = SessiaAval.Models.User;

namespace SessiaAval.Views.Admin;

public partial class AdminDashboardWindow : Window
{
    private UserModel? currentUser;
    private readonly AppDbContext dbContext;
    private ObservableCollection<UserModel> users;
    private ObservableCollection<UserModel> employees;

    public AdminDashboardWindow()
    {
        InitializeComponent();
        
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=matye_db;Username=postgres;Password=0206");
        dbContext = new AppDbContext(optionsBuilder.Options);
        
        users = new ObservableCollection<UserModel>();
        employees = new ObservableCollection<UserModel>();
    }

    public AdminDashboardWindow(UserModel user) : this()
    {
        currentUser = user;
        userNameText.Text = currentUser.fullName;
        
        loadUsers();
        loadEmployees();
        showUsersView();
    }

    private void loadUsers()
    {
        users.Clear();
        var allUsers = dbContext.users.Include(u => u.role).ToList();
        foreach (var user in allUsers)
        {
            users.Add(user);
        }
    }

    private void loadEmployees()
    {
        employees.Clear();
        // Загружаем роли из базы
        var employeeRoles = dbContext.roles
            .Where(r => r.roleName == "Мастер" || r.roleName == "Модератор" || r.roleName == "Администратор")
            .Select(r => r.roleId)
            .ToList();
        
        // Фильтруем пользователей по этим ролям
        var allEmployees = dbContext.users.Include(u => u.role).Where(u => employeeRoles.Contains(u.roleId)).ToList();
        foreach (var emp in allEmployees)
        {
            employees.Add(emp);
        }
    }

    private void showUsersView()
    {
        var view = new StackPanel { Margin = new Avalonia.Thickness(30) };
        
        var title = new TextBlock 
        { 
            Text = "УПРАВЛЕНИЕ ПОЛЬЗОВАТЕЛЯМИ", 
            FontSize = 28, 
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#2C2C2C")),
            Margin = new Avalonia.Thickness(0, 0, 0, 20)
        };
        view.Children.Add(title);
        
        var grid = new DataGrid
        {
            ItemsSource = users,
            AutoGenerateColumns = false,
            IsReadOnly = true,
            GridLinesVisibility = DataGridGridLinesVisibility.All,
            CanUserReorderColumns = true,
            CanUserResizeColumns = true,
            Height = 450,
            Background = Brushes.White,
            Foreground = Brushes.Black,
            BorderBrush = new SolidColorBrush(Color.Parse("#CCCCCC")),
            BorderThickness = new Avalonia.Thickness(1),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            RowBackground = Brushes.White
        };

        grid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "ID", 
            Binding = new Avalonia.Data.Binding("userId"),
            Width = new DataGridLength(60)
        });
        grid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "ФИО", 
            Binding = new Avalonia.Data.Binding("fullName"),
            Width = new DataGridLength(200)
        });
        grid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "Email", 
            Binding = new Avalonia.Data.Binding("email"),
            Width = new DataGridLength(200)
        });
        grid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "Телефон", 
            Binding = new Avalonia.Data.Binding("phone"),
            Width = new DataGridLength(130)
        });
        grid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "Роль", 
            Binding = new Avalonia.Data.Binding("role.roleName"),
            Width = new DataGridLength(120)
        });

        view.Children.Add(grid);
        
        var buttonPanel = new StackPanel 
        { 
            Orientation = Orientation.Horizontal, 
            Spacing = 10,
            Margin = new Avalonia.Thickness(0, 20, 0, 0)
        };
        
        var addBtn = new Button { Content = "Добавить", Width = 120 };
        addBtn.Classes.Add("primary");
        addBtn.Click += (s, e) => showAddUserDialog();
        buttonPanel.Children.Add(addBtn);
        
        var editBtn = new Button { Content = "Редактировать", Width = 140 };
        editBtn.Classes.Add("secondary");
        editBtn.Click += (s, e) => 
        {
            if (grid.SelectedItem is UserModel selectedUser)
                showEditUserDialog(selectedUser);
        };
        buttonPanel.Children.Add(editBtn);
        
        var deleteBtn = new Button { Content = "Удалить", Width = 120 };
        deleteBtn.Classes.Add("secondary");
        deleteBtn.Click += (s, e) => 
        {
            if (grid.SelectedItem is UserModel selectedUser)
                deleteUser(selectedUser);
        };
        buttonPanel.Children.Add(deleteBtn);
        
        var refreshBtn = new Button { Content = "Обновить", Width = 120 };
        refreshBtn.Click += (s, e) => { loadUsers(); showUsersView(); };
        buttonPanel.Children.Add(refreshBtn);
        
        view.Children.Add(buttonPanel);
        
        contentArea.Content = view;
    }

    private void showEmployeesView()
    {
        var view = new StackPanel { Margin = new Avalonia.Thickness(30) };
        
        var title = new TextBlock 
        { 
            Text = "УПРАВЛЕНИЕ СОТРУДНИКАМИ", 
            FontSize = 28, 
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#2C2C2C")),
            Margin = new Avalonia.Thickness(0, 0, 0, 20)
        };
        view.Children.Add(title);
        
        var grid = new DataGrid
        {
            ItemsSource = employees,
            AutoGenerateColumns = false,
            IsReadOnly = true,
            GridLinesVisibility = DataGridGridLinesVisibility.All,
            CanUserReorderColumns = true,
            CanUserResizeColumns = true,
            Height = 450,
            Background = Brushes.White,
            Foreground = Brushes.Black,
            BorderBrush = new SolidColorBrush(Color.Parse("#CCCCCC")),
            BorderThickness = new Avalonia.Thickness(1),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            RowBackground = Brushes.White
        };

        grid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "ID", 
            Binding = new Avalonia.Data.Binding("userId"),
            Width = new DataGridLength(60)
        });
        grid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "ФИО", 
            Binding = new Avalonia.Data.Binding("fullName"),
            Width = new DataGridLength(200)
        });
        grid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "Email", 
            Binding = new Avalonia.Data.Binding("email"),
            Width = new DataGridLength(200)
        });
        grid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "Телефон", 
            Binding = new Avalonia.Data.Binding("phone"),
            Width = new DataGridLength(130)
        });
        grid.Columns.Add(new DataGridTextColumn 
        { 
            Header = "Роль", 
            Binding = new Avalonia.Data.Binding("role.roleName"),
            Width = new DataGridLength(120)
        });

        view.Children.Add(grid);
        
        var buttonPanel = new StackPanel 
        { 
            Orientation = Orientation.Horizontal, 
            Spacing = 10,
            Margin = new Avalonia.Thickness(0, 20, 0, 0)
        };
        
        var addBtn = new Button { Content = "Добавить", Width = 120 };
        addBtn.Classes.Add("primary");
        addBtn.Click += (s, e) => showAddEmployeeDialog();
        buttonPanel.Children.Add(addBtn);
        
        var editBtn = new Button { Content = "Редактировать", Width = 140 };
        editBtn.Classes.Add("secondary");
        editBtn.Click += (s, e) => 
        {
            if (grid.SelectedItem is UserModel selectedEmployee)
                showEditEmployeeDialog(selectedEmployee);
        };
        buttonPanel.Children.Add(editBtn);
        
        var deleteBtn = new Button { Content = "Удалить", Width = 120 };
        deleteBtn.Classes.Add("secondary");
        deleteBtn.Click += (s, e) => 
        {
            if (grid.SelectedItem is UserModel selectedEmployee)
                deleteEmployee(selectedEmployee);
        };
        buttonPanel.Children.Add(deleteBtn);
        
        var refreshBtn = new Button { Content = "Обновить", Width = 120 };
        refreshBtn.Click += (s, e) => { loadEmployees(); showEmployeesView(); };
        buttonPanel.Children.Add(refreshBtn);
        
        view.Children.Add(buttonPanel);
        
        contentArea.Content = view;
    }

    private void onUsersClick(object? sender, RoutedEventArgs e) => showUsersView();
    
    private void onEmployeesClick(object? sender, RoutedEventArgs e) => showEmployeesView();
    
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
                    newWindow = new AdminDashboardWindow(user);
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

    // CRUD для пользователей
    private async void showAddUserDialog()
    {
        var dialog = new Window
        {
            Title = "Добавить пользователя",
            Width = 500,
            Height = 550,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var panel = new StackPanel { Margin = new Avalonia.Thickness(20), Spacing = 10 };
        
        panel.Children.Add(new TextBlock { Text = "ФИО:", FontWeight = FontWeight.Bold });
        var fullNameBox = new TextBox { Watermark = "Иван Иванов" };
        panel.Children.Add(fullNameBox);
        
        panel.Children.Add(new TextBlock { Text = "Email:", FontWeight = FontWeight.Bold });
        var emailBox = new TextBox { Watermark = "user@example.com" };
        panel.Children.Add(emailBox);
        
        panel.Children.Add(new TextBlock { Text = "Телефон:", FontWeight = FontWeight.Bold });
        var phoneBox = new TextBox { Watermark = "+79001234567" };
        panel.Children.Add(phoneBox);
        
        panel.Children.Add(new TextBlock { Text = "Пароль:", FontWeight = FontWeight.Bold });
        var passwordBox = new TextBox { Watermark = "Пароль", PasswordChar = '•' };
        panel.Children.Add(passwordBox);
        
        panel.Children.Add(new TextBlock { Text = "Роль:", FontWeight = FontWeight.Bold });
        var roleCombo = new ComboBox { Width = 200 };
        roleCombo.Items.Add("1 - Пользователь");
        roleCombo.Items.Add("2 - Мастер");
        roleCombo.Items.Add("3 - Модератор");
        roleCombo.Items.Add("4 - Администратор");
        roleCombo.SelectedIndex = 0;
        panel.Children.Add(roleCombo);
        
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
            var nameParts = (fullNameBox.Text ?? "").Split(' ', 2);
            var newUser = new UserModel
            {
                firstName = nameParts.Length > 0 ? nameParts[0] : "",
                lastName = nameParts.Length > 1 ? nameParts[1] : "",
                email = emailBox.Text ?? "",
                phone = phoneBox.Text ?? "",
                passwordHash = passwordBox.Text ?? "",
                roleId = roleCombo.SelectedIndex + 1,
                balance = 0,
                registrationDate = DateTime.UtcNow,
                lastModified = DateTime.UtcNow
            };
            
            dbContext.users.Add(newUser);
            dbContext.SaveChanges();
            loadUsers();
            showUsersView();
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

    private async void showEditUserDialog(UserModel user)
    {
        var dialog = new Window
        {
            Title = "Редактировать пользователя",
            Width = 500,
            Height = 550,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var panel = new StackPanel { Margin = new Avalonia.Thickness(20), Spacing = 10 };
        
        panel.Children.Add(new TextBlock { Text = "ФИО:", FontWeight = FontWeight.Bold });
        var fullNameBox = new TextBox { Text = user.fullName };
        panel.Children.Add(fullNameBox);
        
        panel.Children.Add(new TextBlock { Text = "Email:", FontWeight = FontWeight.Bold });
        var emailBox = new TextBox { Text = user.email };
        panel.Children.Add(emailBox);
        
        panel.Children.Add(new TextBlock { Text = "Телефон:", FontWeight = FontWeight.Bold });
        var phoneBox = new TextBox { Text = user.phone };
        panel.Children.Add(phoneBox);
        
        panel.Children.Add(new TextBlock { Text = "Баланс:", FontWeight = FontWeight.Bold });
        var balanceBox = new TextBox { Text = user.balance.ToString() };
        panel.Children.Add(balanceBox);
        
        panel.Children.Add(new TextBlock { Text = "Роль:", FontWeight = FontWeight.Bold });
        var roleCombo = new ComboBox { Width = 200 };
        
        // Загружаем роли из базы данных
        var roles = dbContext.roles.OrderBy(r => r.roleId).ToList();
        foreach (var role in roles)
        {
            roleCombo.Items.Add($"{role.roleId} - {role.roleName}");
        }
        
        // Устанавливаем текущую роль
        var currentRoleIndex = roles.FindIndex(r => r.roleId == user.roleId);
        if (currentRoleIndex >= 0)
        {
            roleCombo.SelectedIndex = currentRoleIndex;
        }
        
        panel.Children.Add(roleCombo);
        
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
            // Валидация
            if (string.IsNullOrWhiteSpace(fullNameBox.Text))
            {
                // TODO: показать ошибку
                return;
            }
            
            if (roleCombo.SelectedIndex < 0)
            {
                // TODO: показать ошибку
                return;
            }
            
            var nameParts = (fullNameBox.Text ?? "").Split(' ', 2);
            user.firstName = nameParts.Length > 0 ? nameParts[0] : "";
            user.lastName = nameParts.Length > 1 ? nameParts[1] : "";
            user.email = emailBox.Text ?? "";
            user.phone = phoneBox.Text ?? "";
            user.balance = decimal.TryParse(balanceBox.Text, out var bal) ? bal : 0;
            user.roleId = roles[roleCombo.SelectedIndex].roleId;
            user.lastModified = DateTime.UtcNow;
            
            dbContext.SaveChanges();
            loadUsers();
            showUsersView();
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

    private async void deleteUser(UserModel user)
    {
        // Подсчитываем связанные данные
        var appointmentsCount = dbContext.appointments.Count(a => a.userId == user.userId);
        var transactionsCount = dbContext.balanceTransactions.Count(t => t.userId == user.userId);
        var reviewsCount = dbContext.reviews.Count(r => r.userId == user.userId);
        var masterRecord = dbContext.masters.FirstOrDefault(m => m.userId == user.userId);
        
        // Показываем диалог подтверждения
        var confirmDialog = new Window
        {
            Title = "Подтверждение удаления",
            Width = 500,
            Height = 350,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var panel = new StackPanel { Margin = new Avalonia.Thickness(20), Spacing = 15 };
        
        panel.Children.Add(new TextBlock 
        { 
            Text = $"Вы уверены, что хотите удалить пользователя {user.fullName}?",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            FontSize = 16,
            FontWeight = FontWeight.Bold
        });
        
        // Показываем информацию о связанных данных
        var infoPanel = new StackPanel { Spacing = 8, Margin = new Avalonia.Thickness(0, 10, 0, 0) };
        
        infoPanel.Children.Add(new TextBlock 
        { 
            Text = "Будут удалены следующие связанные данные:",
            FontSize = 13,
            FontWeight = FontWeight.SemiBold,
            Foreground = new SolidColorBrush(Color.Parse("#FF6B6B"))
        });
        
        if (appointmentsCount > 0)
            infoPanel.Children.Add(new TextBlock 
            { 
                Text = $"• Записей на услуги: {appointmentsCount}",
                FontSize = 12,
                Margin = new Avalonia.Thickness(10, 0, 0, 0)
            });
            
        if (transactionsCount > 0)
            infoPanel.Children.Add(new TextBlock 
            { 
                Text = $"• Транзакций баланса: {transactionsCount}",
                FontSize = 12,
                Margin = new Avalonia.Thickness(10, 0, 0, 0)
            });
            
        if (reviewsCount > 0)
            infoPanel.Children.Add(new TextBlock 
            { 
                Text = $"• Отзывов: {reviewsCount}",
                FontSize = 12,
                Margin = new Avalonia.Thickness(10, 0, 0, 0)
            });
            
        if (masterRecord != null)
            infoPanel.Children.Add(new TextBlock 
            { 
                Text = "• Профиль мастера",
                FontSize = 12,
                Margin = new Avalonia.Thickness(10, 0, 0, 0)
            });
        
        if (appointmentsCount == 0 && transactionsCount == 0 && reviewsCount == 0 && masterRecord == null)
        {
            infoPanel.Children.Add(new TextBlock 
            { 
                Text = "• Нет связанных данных",
                FontSize = 12,
                Margin = new Avalonia.Thickness(10, 0, 0, 0),
                Foreground = new SolidColorBrush(Color.Parse("#4CAF50"))
            });
        }
        
        panel.Children.Add(infoPanel);
        
        panel.Children.Add(new TextBlock 
        { 
            Text = "Это действие необратимо!",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            FontSize = 13,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#FF6B6B")),
            Margin = new Avalonia.Thickness(0, 10, 0, 0)
        });
        
        var buttonPanel = new StackPanel 
        { 
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10,
            Margin = new Avalonia.Thickness(0, 20, 0, 0)
        };

        var deleteButton = new Button { Content = "Удалить", Width = 100 };
        deleteButton.Classes.Add("danger");
        deleteButton.Click += async (s, args) => 
        {
            try
            {
                dbContext.users.Remove(user);
                dbContext.SaveChanges();
                loadUsers();
                showUsersView();
                confirmDialog.Close();
                
                // Показываем уведомление об успехе
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
                    Text = "Пользователь успешно удален",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Color.Parse("#4CAF50"))
                });
                
                var okButton = new Button 
                { 
                    Content = "OK", 
                    Width = 100,
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                okButton.Click += (s2, args2) => { successDialog.Close(); };
                successPanel.Children.Add(okButton);
                
                successDialog.Content = successPanel;
                await successDialog.ShowDialog(this);
            }
            catch (Exception ex)
            {
                confirmDialog.Close();
                
                var errorDialog = new Window
                {
                    Title = "Ошибка удаления",
                    Width = 450,
                    Height = 250,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                var errorPanel = new StackPanel { Margin = new Avalonia.Thickness(20), Spacing = 15 };
                
                errorPanel.Children.Add(new TextBlock 
                { 
                    Text = "Ошибка при удалении пользователя",
                    FontSize = 16,
                    FontWeight = FontWeight.Bold,
                    Foreground = new SolidColorBrush(Color.Parse("#FF6B6B"))
                });
                
                errorPanel.Children.Add(new TextBlock 
                { 
                    Text = ex.Message,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    FontSize = 12
                });
                
                var okButton = new Button 
                { 
                    Content = "OK", 
                    Width = 100,
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                okButton.Click += (s2, args2) => { errorDialog.Close(); };
                errorPanel.Children.Add(okButton);
                
                errorDialog.Content = errorPanel;
                await errorDialog.ShowDialog(this);
            }
        };
        buttonPanel.Children.Add(deleteButton);
        
        var cancelButton = new Button { Content = "Отмена", Width = 100 };
        cancelButton.Click += (s, args) => { confirmDialog.Close(); };
        buttonPanel.Children.Add(cancelButton);
        
        panel.Children.Add(buttonPanel);
        confirmDialog.Content = panel;
        await confirmDialog.ShowDialog(this);
    }

    private async void deleteEmployee(UserModel employee)
    {
        // Показываем диалог подтверждения
        var confirmDialog = new Window
        {
            Title = "Подтверждение удаления",
            Width = 400,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var panel = new StackPanel { Margin = new Avalonia.Thickness(20), Spacing = 15 };
        
        panel.Children.Add(new TextBlock 
        { 
            Text = $"Вы уверены, что хотите удалить сотрудника {employee.fullName}?",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            FontSize = 14
        });
        
        panel.Children.Add(new TextBlock 
        { 
            Text = "Внимание: Если у сотрудника есть связанные записи (записи на услуги, отзывы и т.д.), удаление будет невозможно.",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#FF6B6B"))
        });
        
        var buttonPanel = new StackPanel 
        { 
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10,
            Margin = new Avalonia.Thickness(0, 20, 0, 0)
        };

        var deleteButton = new Button { Content = "Удалить", Width = 100 };
        deleteButton.Classes.Add("primary");
        deleteButton.Click += (s, args) => 
        {
            try
            {
                dbContext.users.Remove(employee);
                dbContext.SaveChanges();
                loadEmployees();
                showEmployeesView();
                confirmDialog.Close();
            }
            catch (Exception)
            {
                confirmDialog.Close();
                
                var errorDialog = new Window
                {
                    Title = "Ошибка удаления",
                    Width = 450,
                    Height = 250,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                var errorPanel = new StackPanel { Margin = new Avalonia.Thickness(20), Spacing = 15 };
                
                errorPanel.Children.Add(new TextBlock 
                { 
                    Text = "Невозможно удалить сотрудника",
                    FontSize = 16,
                    FontWeight = FontWeight.Bold,
                    Foreground = new SolidColorBrush(Color.Parse("#FF6B6B"))
                });
                
                errorPanel.Children.Add(new TextBlock 
                { 
                    Text = "У этого сотрудника есть связанные записи в системе (записи на услуги, отзывы и т.д.). Сначала необходимо удалить все связанные данные.",
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    FontSize = 13
                });
                
                var okButton = new Button 
                { 
                    Content = "OK", 
                    Width = 100,
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                okButton.Click += (s2, args2) => { errorDialog.Close(); };
                errorPanel.Children.Add(okButton);
                
                errorDialog.Content = errorPanel;
                errorDialog.ShowDialog(this);
            }
        };
        buttonPanel.Children.Add(deleteButton);
        
        var cancelButton = new Button { Content = "Отмена", Width = 100 };
        cancelButton.Click += (s, args) => { confirmDialog.Close(); };
        buttonPanel.Children.Add(cancelButton);
        
        panel.Children.Add(buttonPanel);
        confirmDialog.Content = panel;
        await confirmDialog.ShowDialog(this);
    }
    private async void showAddEmployeeDialog()
    {
        var dialog = new Window
        {
            Title = "Добавить сотрудника",
            Width = 500,
            Height = 550,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var panel = new StackPanel { Margin = new Avalonia.Thickness(20), Spacing = 10 };
        
        panel.Children.Add(new TextBlock { Text = "ФИО:", FontWeight = FontWeight.Bold });
        var fullNameBox = new TextBox { Watermark = "Иван Иванов" };
        panel.Children.Add(fullNameBox);
        
        panel.Children.Add(new TextBlock { Text = "Email:", FontWeight = FontWeight.Bold });
        var emailBox = new TextBox { Watermark = "employee@matye.ru" };
        panel.Children.Add(emailBox);
        
        panel.Children.Add(new TextBlock { Text = "Телефон:", FontWeight = FontWeight.Bold });
        var phoneBox = new TextBox { Watermark = "+79001234567" };
        panel.Children.Add(phoneBox);
        
        panel.Children.Add(new TextBlock { Text = "Пароль:", FontWeight = FontWeight.Bold });
        var passwordBox = new TextBox { Watermark = "Пароль", PasswordChar = '•' };
        panel.Children.Add(passwordBox);
        
        panel.Children.Add(new TextBlock { Text = "Роль:", FontWeight = FontWeight.Bold });
        var roleCombo = new ComboBox { Width = 200 };
        
        // Загружаем роли сотрудников из базы данных (roleId >= 2)
        var employeeRoles = dbContext.roles.Where(r => r.roleId >= 2).OrderBy(r => r.roleId).ToList();
        foreach (var role in employeeRoles)
        {
            roleCombo.Items.Add($"{role.roleId} - {role.roleName}");
        }
        roleCombo.SelectedIndex = 0;
        
        panel.Children.Add(roleCombo);
        
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
            // Валидация
            if (string.IsNullOrWhiteSpace(fullNameBox.Text))
            {
                // TODO: показать ошибку
                return;
            }
            
            if (roleCombo.SelectedIndex < 0)
            {
                // TODO: показать ошибку
                return;
            }
            
            var nameParts = (fullNameBox.Text ?? "").Split(' ', 2);
            var newEmployee = new UserModel
            {
                firstName = nameParts.Length > 0 ? nameParts[0] : "",
                lastName = nameParts.Length > 1 ? nameParts[1] : "",
                email = emailBox.Text ?? "",
                phone = phoneBox.Text ?? "",
                passwordHash = passwordBox.Text ?? "",
                roleId = employeeRoles[roleCombo.SelectedIndex].roleId,
                balance = 0,
                registrationDate = DateTime.UtcNow,
                lastModified = DateTime.UtcNow
            };
            
            dbContext.users.Add(newEmployee);
            dbContext.SaveChanges();
            loadEmployees();
            showEmployeesView();
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

    private async void showEditEmployeeDialog(UserModel employee)
    {
        var dialog = new Window
        {
            Title = "Редактировать сотрудника",
            Width = 500,
            Height = 500,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var panel = new StackPanel { Margin = new Avalonia.Thickness(20), Spacing = 10 };
        
        panel.Children.Add(new TextBlock { Text = "ФИО:", FontWeight = FontWeight.Bold });
        var fullNameBox = new TextBox { Text = employee.fullName };
        panel.Children.Add(fullNameBox);
        
        panel.Children.Add(new TextBlock { Text = "Email:", FontWeight = FontWeight.Bold });
        var emailBox = new TextBox { Text = employee.email };
        panel.Children.Add(emailBox);
        
        panel.Children.Add(new TextBlock { Text = "Телефон:", FontWeight = FontWeight.Bold });
        var phoneBox = new TextBox { Text = employee.phone };
        panel.Children.Add(phoneBox);
        
        panel.Children.Add(new TextBlock { Text = "Роль:", FontWeight = FontWeight.Bold });
        var roleCombo = new ComboBox { Width = 200 };
        
        // Загружаем роли сотрудников из базы данных (roleId >= 2)
        var employeeRoles = dbContext.roles.Where(r => r.roleId >= 2).OrderBy(r => r.roleId).ToList();
        foreach (var role in employeeRoles)
        {
            roleCombo.Items.Add($"{role.roleId} - {role.roleName}");
        }
        
        // Устанавливаем текущую роль
        var currentRoleIndex = employeeRoles.FindIndex(r => r.roleId == employee.roleId);
        if (currentRoleIndex >= 0)
        {
            roleCombo.SelectedIndex = currentRoleIndex;
        }
        
        panel.Children.Add(roleCombo);
        
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
            // Валидация
            if (string.IsNullOrWhiteSpace(fullNameBox.Text))
            {
                // TODO: показать ошибку
                return;
            }
            
            if (roleCombo.SelectedIndex < 0)
            {
                // TODO: показать ошибку
                return;
            }
            
            var nameParts = (fullNameBox.Text ?? "").Split(' ', 2);
            employee.firstName = nameParts.Length > 0 ? nameParts[0] : "";
            employee.lastName = nameParts.Length > 1 ? nameParts[1] : "";
            employee.email = emailBox.Text ?? "";
            employee.phone = phoneBox.Text ?? "";
            employee.roleId = employeeRoles[roleCombo.SelectedIndex].roleId;
            employee.lastModified = DateTime.UtcNow;
            
            dbContext.SaveChanges();
            loadEmployees();
            showEmployeesView();
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
}

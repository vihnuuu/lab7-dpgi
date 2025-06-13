using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.VisualBasic; 

namespace WpfApplProject
{
    public partial class PageTable1 : Page
    {
        private bool isDirty = true;

        public PageTable1()
        {
            InitializeComponent();
            LoadHealthHistory();
        }

        private void LoadHealthHistory()
        {
            try
            {
                int userId = (int)App.Current.Properties["UserId"];
                using (var db = new WellnessTrackerNewEntities())
                {
                    var data = db.HealthDays
                        .Where(h => h.UserId == userId)
                        .OrderByDescending(h => h.Date)
                        .ToList()
                        .Select(h => new
                        {
                            h.Id,
                            h.Date,
                            h.OverallFeeling,
                            h.MoodLevel,
                            h.EnergyLevel,
                            h.SleepQualityLevel,
                            h.SleepHours,
                            h.PainLevel,
                            SymptomSummary = string.Join(", ", h.Symptoms.Select(s => $"{s.Name} ({s.Severity})")),
                            h.Notes
                        })
                        .ToList();

                    historyGrid.ItemsSource = data;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка завантаження даних: " + ex.Message);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).frame1.Navigate(new PageMain());
        }

        private void UndoCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = historyGrid != null && historyGrid.SelectedItem != null;
        }

        private void UndoCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            historyGrid.SelectedItem = null;
        }

        private void NewCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
        private void NewCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).frame1.Navigate(new PageTable2());
        }

        private void ReplaceCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = historyGrid != null && historyGrid.SelectedItem != null;

        private void ReplaceCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (historyGrid.SelectedItem != null)
            {
                dynamic selected = historyGrid.SelectedItem;
                App.Current.Properties["EditEntryId"] = selected.Id; // передача ID
                ((MainWindow)Application.Current.MainWindow).frame1.Navigate(new PageTable2());
            }
        }

        private void SaveCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = isDirty;
        private void SaveCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var data = historyGrid.ItemsSource;
                if (data == null)
                {
                    MessageBox.Show("Немає даних для збереження.");
                    return;
                }

                // Вибір місця збереження
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = "HealthHistory",
                    DefaultExt = ".csv",
                    Filter = "CSV файли (*.csv)|*.csv|Текстові файли (*.txt)|*.txt|Усі файли (*.*)|*.*"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var writer = new System.IO.StreamWriter(saveFileDialog.FileName, false, System.Text.Encoding.UTF8))

                    {
                        // заголовок
                        writer.WriteLine("Дата,Самопочуття,Настрій,Енергія,Якість сну,Годин сну,Рівень болю,Симптоми,Нотатки");

                        foreach (var item in data)
                        {
                            dynamic row = item;
                            string Escape(string s) => s?.Replace("\"", "\"\"") ?? "";

                            string line = $"{row.Date:yyyy-MM-dd HH:mm},{row.OverallFeeling},{Escape(row.MoodLevel)},{Escape(row.EnergyLevel)}," +
                                          $"{Escape(row.SleepQualityLevel)},{row.SleepHours},{Escape(row.PainLevel)}," +
                                          $"\"{Escape(row.SymptomSummary)}\",\"{Escape(row.Notes)}\"";

                            writer.WriteLine(line);
                        }
                    }

                    MessageBox.Show("Дані успішно збережено!");
                    isDirty = false;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка під час збереження: {ex.Message}");
            }
        }


        private void FindCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
        private void FindCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string keyword = Interaction.InputBox("Введіть ключове слово для пошуку", "Пошук", "");
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var collectionView = CollectionViewSource.GetDefaultView(historyGrid.ItemsSource);
                if (collectionView != null)
                {
                    collectionView.Filter = item =>
                    {
                        var obj = item.ToString();
                        return obj != null && obj.ToLower().Contains(keyword.ToLower());
                    };
                }
            }
        }


        private void DeleteCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = isDirty;
        private void DeleteCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var result = MessageBox.Show("Видалити всі записи? Натисніть 'Ні' для видалення лише виділеного.", "Підтвердження", MessageBoxButton.YesNoCancel);
            using (var db = new WellnessTrackerNewEntities())
            {
                int userId = (int)App.Current.Properties["UserId"];

                if (result == MessageBoxResult.Yes)
                {
                    var all = db.HealthDays.Where(h => h.UserId == userId).ToList();
                    foreach (var hd in all)
                    {
                        db.Symptoms.RemoveRange(hd.Symptoms); // видаляємо пов’язані симптоми
                    }
                    db.HealthDays.RemoveRange(all);
                    db.SaveChanges();
                    LoadHealthHistory();
                    MessageBox.Show("Усі записи видалено.");
                }
                else if (result == MessageBoxResult.No && historyGrid.SelectedItem != null)
                {
                    dynamic selected = historyGrid.SelectedItem;
                    int idToDelete = selected.Id;
                    var toDelete = db.HealthDays.Include("Symptoms").FirstOrDefault(h => h.Id == idToDelete);
                    if (toDelete != null)
                    {
                        db.Symptoms.RemoveRange(toDelete.Symptoms); // видаляємо пов’язані симптоми
                        db.HealthDays.Remove(toDelete);
                        db.SaveChanges();
                        LoadHealthHistory();
                        MessageBox.Show("Запис видалено.");
                    }
                }
            }
        }

        private void UpdateCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;

        private void UpdateCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            LoadHealthHistory();
            MessageBox.Show("Дані оновлено");
        }

    }
}

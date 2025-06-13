using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfApplProject.pages;

namespace WpfApplProject
{
    public partial class PageTable2 : Page
    {
        private ObservableCollection<Symptoms> symptomsList = new ObservableCollection<Symptoms>();
        private double currentRating = 2.5;
        private int? editingId = null;

        public PageTable2()
        {
            InitializeComponent();
            symptomsGrid.ItemsSource = symptomsList;
            datePicker.SelectedDate = DateTime.Now;
            InitializeStarRating();
            TryLoadForEdit();
        }

        private void TryLoadForEdit()
        {
            if (App.Current.Properties.Contains("EditEntryId"))
            {
                editingId = (int)App.Current.Properties["EditEntryId"];

                using (var db = new WellnessTrackerNewEntities())
                {
                    var health = db.HealthDays.Find(editingId);
                    if (health != null)
                    {
                        datePicker.SelectedDate = health.Date;
                        currentRating = health.OverallFeeling / 2.0;
                        InitializeStarRating();
                        moodLevelBox.Text = health.MoodLevel;
                        energyLevelBox.Text = health.EnergyLevel;
                        sleepQualityLevelBox.Text = health.SleepQualityLevel;
                        sleepHoursBox.Text = health.SleepHours?.ToString();
                        painLevelBox.Text = health.PainLevel;
                        notesBox.Text = health.Notes;

                        symptomsList.Clear();
                        foreach (var s in health.Symptoms)
                        {
                            symptomsList.Add(new Symptoms { Name = s.Name, Severity = s.Severity });
                        }
                    }
                }
            }
        }

        private void InitializeStarRating()
        {
            starRating.Items.Clear();

            for (int i = 1; i <= 5; i++)
            {
                double value = i;

                Image starImage = new Image
                {
                    Width = 32,
                    Height = 32,
                    Margin = new Thickness(2),
                    Source = GetStarImageSource(i, currentRating)
                };

                Button starButton = new Button
                {
                    Content = starImage,
                    Tag = value,
                    Padding = new Thickness(0),
                    BorderThickness = new Thickness(0),
                    Background = Brushes.Transparent,
                    Cursor = Cursors.Hand
                };

                starButton.Click += (s, e) =>
                {
                    Point position = Mouse.GetPosition(starButton);
                    currentRating = position.X < starButton.ActualWidth / 2 ? value - 0.5 : value;
                    InitializeStarRating();
                };

                starRating.Items.Add(starButton);
            }

            ratingLabel.Text = $"{currentRating * 2:0.0} / 10";
        }

        private BitmapImage GetStarImageSource(int position, double rating)
        {
            string imageName;

            if (rating >= position)
                imageName = "star_full.png";
            else if (rating >= position - 0.5)
                imageName = "star_half.png";
            else
                imageName = "star_empty.png";

            return new BitmapImage(new Uri($"/Images/{imageName}", UriKind.Relative));
        }

        private void AddSymptom_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(symptomNameBox.Text) && symptomSeverityBox.SelectedItem is ComboBoxItem selectedSeverity)
            {
                symptomsList.Add(new Symptoms
                {
                    Name = symptomNameBox.Text,
                    Severity = selectedSeverity.Content.ToString()
                });

                symptomNameBox.Clear();
                symptomSeverityBox.SelectedIndex = -1;
            }
            else
            {
                MessageBox.Show("Будь ласка, введіть назву симптому та виберіть інтенсивність.");
            }
        }

        private void SaveHealthDay_Click(object sender, RoutedEventArgs e)
        {
            if (datePicker.SelectedDate == null)
            {
                MessageBox.Show("Будь ласка, оберіть дату.");
                return;
            }

            using (var db = new WellnessTrackerNewEntities())
            {
                try
                {
                    int userId = (int)App.Current.Properties["UserId"];
                    HealthDays healthDay;

                    if (editingId.HasValue)
                    {
                        healthDay = db.HealthDays.Include("Symptoms").FirstOrDefault(h => h.Id == editingId.Value);
                        db.Symptoms.RemoveRange(healthDay.Symptoms);
                    }
                    else
                    {
                        healthDay = new HealthDays();
                        db.HealthDays.Add(healthDay);
                    }

                    healthDay.UserId = userId;
                    healthDay.Date = datePicker.SelectedDate.Value.Date + DateTime.Now.TimeOfDay;
                    healthDay.OverallFeeling = (int)Math.Round(currentRating * 2);
                    healthDay.MoodLevel = (moodLevelBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                    healthDay.EnergyLevel = (energyLevelBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                    healthDay.SleepQualityLevel = (sleepQualityLevelBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                    healthDay.SleepHours = decimal.TryParse(sleepHoursBox.Text, out decimal hours) ? (decimal?)hours : null;
                    healthDay.PainLevel = (painLevelBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                    healthDay.Notes = notesBox.Text;

                    foreach (var symptom in symptomsList)
                    {
                        symptom.HealthDays = healthDay;
                        db.Symptoms.Add(symptom);
                    }

                    db.SaveChanges();
                    MessageBox.Show("Запис успішно збережено!");

                    ClearForm();
                    App.Current.Properties.Remove("EditEntryId");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при збереженні: {ex.Message}\n{ex.InnerException}");
                }
            }
        }

        private void ClearForm()
        {
            datePicker.SelectedDate = DateTime.Now;
            currentRating = 2.5;
            InitializeStarRating();
            moodLevelBox.SelectedIndex = -1;
            energyLevelBox.SelectedIndex = -1;
            sleepQualityLevelBox.SelectedIndex = -1;
            sleepHoursBox.Clear();
            painLevelBox.SelectedIndex = -1;
            notesBox.Clear();
            symptomsList.Clear();
            editingId = null;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Properties.Remove("EditEntryId");
            NavigationService?.GoBack();
        }
    }
}

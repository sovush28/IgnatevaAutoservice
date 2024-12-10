using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ignateva_Autoservice
{
    /// <summary>
    /// Логика взаимодействия для SignUpPage.xaml
    /// </summary>
    public partial class SignUpPage : Page
    {
        private Service _currentService = new Service();

        public SignUpPage(Service SelectedService)
        {
            InitializeComponent();
            if (SelectedService != null)
                this._currentService = SelectedService;

            DataContext = _currentService;

            var _currentClient = IgnatevaAutoserviceEntities.GetContext().Client.ToList();
            ComboClient.ItemsSource = _currentClient;
        }

        private void TBStart_TextChanged(object sender, TextChangedEventArgs e)
        {
            /**/
            if (TBStart.Text.Length == 2)
            {
                TBStart.Text += ":";
                TBStart.CaretIndex = TBStart.Text.Length; // курсор в конец
                return;
            }
            /**/

            string s = TBStart.Text;

            /*
            if (s.Length == 2)
                TBStart.Text += ":"; //умирает
            */

            if (s.Length < 5 || !s.Contains(':')) //5 = 4 цифры + двоеточие
            {
                TBEnd.Text = "";
                return;
            }
            else
            {
                string[] start = s.Split(new char[] { ':' });
                int startHour = Convert.ToInt32(start[0].ToString()) * 60;
                int startMin = Convert.ToInt32(start[1].ToString());

                if (startHour/60 < 0 || startHour/60 > 23 || startMin < 0 || startMin > 59)
                {
                    MessageBox.Show("Некорректный формат времени.\nЗначение часа должно находиться в пределах от 0 до 23, а минуты - от 0 до 59.");
                    TBStart.Text = "";
                    return;
                }

                int sum = startHour + startMin + _currentService.Duration;

                int EndHour = sum / 60;
                int EndMin = sum % 60;
                int daysToAdd = 0;
                while (EndHour > 23)
                {
                    EndHour -= 24;
                    daysToAdd++;
                }
                if (EndMin > 59)
                {
                    int addToHours = EndMin / 60;
                    EndMin %= 60;
                    EndHour += addToHours;
                }

                string StrEndMin = EndMin.ToString("D2");
                string StrEndHour = EndHour.ToString("D2");

                /*
                string StrEndMin;
                if (EndMin / 10 == 0)
                    StrEndMin = "0" + EndMin.ToString();
                else
                    StrEndMin = EndMin.ToString();
                string StrEndHour;
                if (EndHour / 10 == 0)
                    StrEndHour = "0" + EndHour.ToString();
                else
                    StrEndHour = EndHour.ToString();
                */

                s = StrEndHour + ":" + StrEndMin;

                if (!StartDate.SelectedDate.HasValue)
                {
                    MessageBox.Show("До ввода времени выберите дату начала услуги");
                    TBEnd.Text = "";
                    TBStart.Text = "";
                    return;
                }

                if (daysToAdd == 0)
                    TBEnd.Text = s;
                else
                {
                    DateTimeOffset EndDate = StartDate.SelectedDate.Value.AddDays(daysToAdd);
                    TBEnd.Text = s + "   " + EndDate.ToString("dd.MM.yyyy");
                }

            }
        }

        private void StartDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StartDate.SelectedDate.HasValue && StartDate.SelectedDate.Value < DateTimeOffset.Now)
            {
                MessageBox.Show("Вы не можете выбрать уже прошедшую дату");
                StartDate.Text = "";
                return;
            }
        }


        private ClientService _currentClientService = new ClientService();

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (ComboClient.SelectedItem == null)
                errors.AppendLine("Укажите ФИО клиента");

            if (StartDate.Text == "")
                errors.AppendLine("Укажите дату услуги");

            if (TBStart.Text == "")
                errors.AppendLine("Укажите время начала услуги");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            _currentClientService.ClientID = ComboClient.SelectedIndex + 1;
            _currentClientService.ServiceID = _currentService.ID;
            _currentClientService.StartTime = Convert.ToDateTime(StartDate.Text + " " + TBStart.Text);

            if (_currentClientService.ID == 0)
                IgnatevaAutoserviceEntities.GetContext().ClientService.Add(_currentClientService);

            try
            {
                IgnatevaAutoserviceEntities.GetContext().SaveChanges();
                MessageBox.Show("Информация сохранена");
                Manager.MainFrame.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

    }
}

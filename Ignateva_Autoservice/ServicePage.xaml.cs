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
    /// Логика взаимодействия для ServicePage.xaml
    /// </summary>
    public partial class ServicePage : Page
    {
        int CountRecords; //количесвто записей в таблице
        int CountPage; //общее количество страниц
        int CurrentPage = 0;
        List<Service> CurrentPageList = new List<Service>();
        List<Service> TableList;

        private void UpdateServices()
        {
            var currentServices = IgnatevaAutoserviceEntities.GetContext().Service.ToList();

            if (ComboType.SelectedIndex == 0)
            {
                currentServices = currentServices.Where(p => (Convert.ToInt32(p.DiscountInt) >= 0 && Convert.ToInt32(p.DiscountInt) <= 100)).ToList();
            }

            if (ComboType.SelectedIndex == 1)
            {
                currentServices = currentServices.Where(p => (Convert.ToInt32(p.DiscountInt) >= 0 && Convert.ToInt32(p.DiscountInt) < 5)).ToList();
            }
            if (ComboType.SelectedIndex == 2)
            {
                currentServices = currentServices.Where(p => (Convert.ToInt32(p.DiscountInt) >= 5 && Convert.ToInt32(p.DiscountInt) < 15)).ToList();
            }
            if (ComboType.SelectedIndex == 3)
            {
                currentServices = currentServices.Where(p => (Convert.ToInt32(p.DiscountInt) >= 15 && Convert.ToInt32(p.DiscountInt) < 30)).ToList();
            }
            if (ComboType.SelectedIndex == 4)
            {
                currentServices = currentServices.Where(p => (Convert.ToInt32(p.DiscountInt) >= 30 && Convert.ToInt32(p.DiscountInt) < 70)).ToList();
            }
            if (ComboType.SelectedIndex == 5)
            {
                currentServices = currentServices.Where(p => (Convert.ToInt32(p.DiscountInt) >= 70 && Convert.ToInt32(p.DiscountInt) < 100)).ToList();
            }

            currentServices = currentServices.Where(p => p.Title.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();
            ServiceListView.ItemsSource = currentServices.ToList();

            if (RButtonDown.IsChecked.Value)
            {
                ServiceListView.ItemsSource = currentServices.OrderByDescending(p => p.Cost).ToList();
            }
            if (RButtonUp.IsChecked.Value)
            {
                ServiceListView.ItemsSource = currentServices.OrderBy(p => p.Cost).ToList();
            }

            ServiceListView.ItemsSource = currentServices;
            TableList = currentServices;
            ChangePage(0, 0);

        }

        public ServicePage()
        {
            InitializeComponent();
            //
            var currentServices = IgnatevaAutoserviceEntities.GetContext().Service.ToList();
            ServiceListView.ItemsSource = currentServices;
            //
            ComboType.SelectedIndex = 0;

            UpdateServices();
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateServices();
        }

        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateServices();
        }

        private void RButtonUp_Checked(object sender, RoutedEventArgs e)
        {
            UpdateServices();
        }

        private void RButtonDown_Checked(object sender, RoutedEventArgs e)
        {
            UpdateServices();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage(null));
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage((sender as Button).DataContext as Service));
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility==Visibility.Visible)
            {
                IgnatevaAutoserviceEntities.GetContext().ChangeTracker.Entries().ToList().ForEach(p => p.Reload());
                ServiceListView.ItemsSource = IgnatevaAutoserviceEntities.GetContext().Service.ToList();
                UpdateServices();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var currentService=(sender as Button).DataContext as Service;

            var currentClientServices = IgnatevaAutoserviceEntities.GetContext().ClientService.ToList();
            currentClientServices = currentClientServices.Where(p => p.ServiceID == currentService.ID).ToList();

            if (currentClientServices.Count != 0)
                MessageBox.Show("Невозможно выполнить удаление, так как существуют записи на эту услугу");
            else
            {
                if (MessageBox.Show("Вы точно хотите выполнить удаление?", "Внимание!",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        IgnatevaAutoserviceEntities.GetContext().Service.Remove(currentService);
                        IgnatevaAutoserviceEntities.GetContext().SaveChanges();
                        ServiceListView.ItemsSource = IgnatevaAutoserviceEntities.GetContext().Service.ToList();
                        UpdateServices();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message.ToString());
                    }
                }
            }
        }

        private void ChangePage(int direction, int? selectedPage)
        {
            // направление: 0 начало, 1 предыдущая стр, 2 следующая стр
            // selectedPage при нажатии на стрелки передаётся null, при выборе опр стр - ее номер

            CurrentPageList.Clear();
            CountRecords = TableList.Count;
            if (CountRecords % 10 > 0)
            {
                CountPage = CountRecords / 10 + 1;
            }
            else
            {
                CountPage = CountRecords / 10;
            }

            Boolean Ifupdate = true;

            int min;

            if (selectedPage.HasValue)
            {
                if (selectedPage >= 0 && selectedPage <= CountPage)
                {
                    CurrentPage = (int)selectedPage;
                    min = CurrentPage * 10 + 10 < CountRecords ? CurrentPage * 10 + 10 : CountRecords;
                    for(int i=CurrentPage * 10; i < min; i++)
                    {
                        CurrentPageList.Add(TableList[i]);
                    }
                }
            }
            else //если нажата стрелка
            {
                switch(direction)
                {
                    case 1:
                        if (CurrentPage > 0)
                        {
                            CurrentPage--;
                            min = CurrentPage * 10 + 10 < CountRecords ? CurrentPage * 10 + 10 : CountRecords;
                            for (int i=CurrentPage*10;i<min; i++)
                            {
                                CurrentPageList.Add(TableList[i]);
                            }
                        }
                        else
                        {
                            Ifupdate = false;
                        }
                        break;

                    case 2:
                        if (CurrentPage < CountPage - 1)
                        {
                            CurrentPage++;
                            min = CurrentPage * 10 + 10 < CountRecords ? CurrentPage * 10 + 10 : CountRecords;
                            for (int i = CurrentPage * 10; i < min; i++)
                            {
                                CurrentPageList.Add(TableList[i]);
                            }
                        }
                        else
                        {
                            Ifupdate = false;
                        }
                        break;
                }

            }
            //
            if (Ifupdate)
            {
                PageListBox.Items.Clear();
                for (int i = 1; i <= CountPage; i++)
                {
                    PageListBox.Items.Add(i);
                }
                PageListBox.SelectedIndex = CurrentPage;

                min = CurrentPage * 10 + 10 < CountRecords ? CurrentPage * 10 + 10 : CountRecords;
                TBCount.Text = min.ToString();
                TBAllRecords.Text = " из " + CountRecords.ToString();

                ServiceListView.ItemsSource = CurrentPageList;
                ServiceListView.Items.Refresh();
            }
            //

        }

        private void LeftDirButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(1, null);
        }

        private void RightDirButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(2, null);
        }

        private void PageListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ChangePage(0, Convert.ToInt32(PageListBox.SelectedItem.ToString()) - 1);
        }
    }
}

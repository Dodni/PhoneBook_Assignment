using PhoneBook;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using System.Windows.Controls;

namespace WPFEFCMF_PhoneBook
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        cnPhoneBook cn;
        public MainWindow()
        {
            InitializeComponent();
            cn = new cnPhoneBook();
            InitializeDB();
        }

        private void InitializeDB()
        {
            cn.Database.EnsureCreated();
            if (cn.People != null)
            {
                if (!cn.People.Any())
                {
                    SeedDB();
                }
                //ShowData();
            }


        }

        private void ShowData() //Megejelenítjük az adatok egy message box-ban
        {
            var s = "";
            foreach (var p in cn.People.Include(pe => pe.City).Include(pe => pe.Numbers).ToList())
            {
                s += p.Name + ", " + p.Address + ", " + p.City.Zip + ", " + p.City.Name +
                p.Numbers.Aggregate("", (c, a) => c + ", " + a.NumberString) +
                "\n";
            }
            MessageBox.Show(s);
        }

        private void SeedDB()
        {
            var l1 = new Login { username = "John", password = "12345" };
            var l2 = new Login { username = "Jane", password = "12345" };


            var c1 = new City { Zip = 65234, Name = "Big City" };
            var c2 = new City { Zip = 65258, Name = "Small City" };

            var p1 = new Person { Name = "John Doe", Address = "10 Big Street", City = c1 };
            var p2 = new Person { Name = "Jane Doe", Address = "25 Small Street", City = c2 };
            c1.People.Add(p1);
            c2.People.Add(p2);

            var n1 = new Number { NumberString = "+99-99-9999999" };
            var n2 = new Number { NumberString = "+36-76-9998899" };
            var n3 = new Number { NumberString = "+40-88-9978991" };
            p1.Numbers.Add(n1);
            p1.Numbers.Add(n3);
            p2.Numbers.Add(n2);
            p2.Numbers.Add(n3);
            n1.People.Add(p1);
            n2.People.Add(p2);
            n3.People.Add(p1);
            n3.People.Add(p2);

            cn.People.AddRange(new Person[] { p1, p2 });
            cn.Cities.AddRange(new City[] { c1, c2 });
            cn.Numbers.AddRange(new Number[] { n1, n2, n3 });
            cn.Logins.AddRange(new Login[] { l1, l2 });
            cn.SaveChanges();

        }

        private void mi_CitiesClick(object sender, RoutedEventArgs e)
        {
            grCity.Visibility = Visibility.Collapsed;//eltuntetjük biztonság  kedvéért
            dgAll.Visibility = Visibility.Collapsed;  //elrejtjük a dgAll táblát            
            grCity.Visibility = Visibility.Collapsed;
            grNumber.Visibility = Visibility.Collapsed;
            dgCities.Visibility = Visibility.Visible;
            dgCities.ItemsSource = cn.Cities.ToList();
        }

        private void mi_AllClick(object sender, RoutedEventArgs e)
        {
            grCity.Visibility = Visibility.Collapsed; //eltuntetjük biztonság  kedvéért
            dgCities.Visibility = Visibility.Collapsed; //elrejtjük a dgCities táblát
            grCity.Visibility = Visibility.Collapsed;
            grNumber.Visibility = Visibility.Collapsed;
            dgAll.Visibility = Visibility.Visible;
            dgAll.ItemsSource = cn.People.Include(pe => pe.City).Include(pe => pe.Numbers).ToList();
        }

        private void mi_NMCitiesClick(object sender, RoutedEventArgs e)
        {
            dgAll.Visibility = Visibility.Collapsed;
            dgCities.Visibility = Visibility.Collapsed; // elrejtjük a táblákat, ha esetleg láthatóak lennének
            grNumber.Visibility = Visibility.Collapsed;
            grCity.Visibility = Visibility.Visible;
            grCity.DataContext = cn.Cities.ToList();
            cbZip.SelectedIndex = 0;
        }

        private void mi_NMPhoneNumbersClick(object sender, RoutedEventArgs e)
        {
            dgAll.Visibility = Visibility.Collapsed;
            dgCities.Visibility = Visibility.Collapsed; // elrejtjük a táblákat, ha esetleg láthatóak lennének
            grCity.Visibility = Visibility.Collapsed;
            grNumber.Visibility = Visibility.Visible;
            grNumber.DataContext = cn.Numbers.Include(n => n.People).ToList();
            cbNumbers.SelectedIndex = 0;
        }

        private void cbZip_SChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var c = ((ComboBox)sender).SelectedItem as City;
            tbName.Text = c.Name;
            tbZip.Text = c.Zip.ToString();
        }

        private void cbName_SChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void btNMSave_Click(object sender, RoutedEventArgs e)
        {
            if (!NameZipValidate(out int zip)) return;
            var c = cbName.SelectedItem as City;
            c.Name = tbName.Text;
            if (zip != c.Zip)
            {
                var nc = new City { Zip = zip, Name = c.Name };
                foreach (var p in c.People)
                {
                    p.City = nc;
                    nc.People.Add(p);
                }
                cn.Cities.Add(nc);
                cn.Cities.Remove(c);
            }
            cn.SaveChanges();
            grCity.Visibility = Visibility.Collapsed;
        }

        private void btNMSaveNew_Click(object sender, RoutedEventArgs e)
        {
            if (!NameZipValidate(out int zip)) return;
            if (cn.Cities.Any(c => c.Zip == zip))
            {
                MessageBox.Show("The Zip code is already in the database!");
                return;
            }
            cn.Cities.Add(new City { Zip = zip, Name = tbName.Text });
            cn.SaveChanges();
            grCity.Visibility = Visibility.Collapsed;

        }
        bool NameZipValidate(out int zip)
        {
            zip = 0;
            if (tbName.Text.Length == 0)
            {
                MessageBox.Show("Please enter the name of the city!");
                return false;
            }
            var res = int.TryParse(tbZip.Text, out zip);
            if (!res || (res && zip < 1))
            {
                MessageBox.Show("Please enter a valid Zip code!");
                return false;
            }
            return true;
        }
        private void btNMBack_Click(object sender, RoutedEventArgs e)
        {
            grCity.Visibility = Visibility.Collapsed;
        }

        private void mi_ExitClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btNMSaveNumber_Click(object sender, RoutedEventArgs e)
        {
            if(tbCurrentNumber.Text.Length<3)
            {
                MessageBox.Show("Number is not valid");
                return;
            }
            var n = cbNumbers.SelectedItem as Number;
            n.NumberString = tbCurrentNumber.Text;
            cn.SaveChanges();
            grNumber.Visibility = Visibility.Collapsed;
        }

        private void btNMSaveNewNumber_Click(object sender, RoutedEventArgs e)
        {
            if (tbCurrentNumber.Text.Length < 3)
            {
                MessageBox.Show("Number is not valid");
                return;
            }
            if (cn.Numbers.Any(n => n.NumberString == tbCurrentNumber.Text))
            {
                MessageBox.Show("Number is already stored in the database!");
                return;
            }
            cn.Numbers.Add(new Number { NumberString = tbCurrentNumber.Text });
            cn.SaveChanges();
            grNumber.Visibility = Visibility.Collapsed; 
        }

        private void btNMBackNumber_Click(object sender, RoutedEventArgs e)
        {
            grNumber.Visibility = Visibility.Collapsed;
        }

        private void cbNumbers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var n = ((ComboBox)sender).SelectedItem as Number;
            tbCurrentNumber.Text = n.NumberString; 
        }
    }
}

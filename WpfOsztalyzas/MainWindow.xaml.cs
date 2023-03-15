using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.Win32;

namespace WpfOsztalyzas
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string fajlNev = "naplo.txt";
        //Így minden metódus fogja tudni használni.
        List<Osztalyzat> jegyek = new List<Osztalyzat>();

        public MainWindow()
        {
            InitializeComponent();
            // todo Fájlok kitallózásával tegye lehetővé a naplófájl kiválasztását!
            // Ha nem választ ki semmit, akkor "naplo.csv" legyen az állomány neve. A későbbiekben ebbe fog rögzíteni a program.
            // todo A kiválasztott naplót egyből töltse be és a tartalmát jelenítse meg a datagrid-ben!
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.FileName = "naplo";
            openFileDialog.DefaultExt = "csv";
            if(openFileDialog.ShowDialog() == true)
            {
                fajlNev = openFileDialog.SafeFileName;
                StreamReader streamReader = new StreamReader(openFileDialog.FileName);
                while (!streamReader.EndOfStream)
                {
                    string[] lines = streamReader.ReadLine().Split(";");
                    Osztalyzat ujJegy = new Osztalyzat(lines[0], lines[1], lines[2], Convert.ToInt32(lines[3]));
                    jegyek.Add(ujJegy);
                }
                streamReader.Close();
                dgJegyek.ItemsSource = jegyek;
                // - A naplóban lévő jegyek száma
                lblJegySzam.Content = jegyek.Count();
                //Az átlag
                double sum = 0;
                foreach (var item in jegyek)
                {
                    sum += Convert.ToDouble(item.Jegy);
                }
                lblJegyAtlag.Content = Convert.ToDouble(sum / jegyek.Count()).ToString("N2");
            }
            // - A naplófájl neve
            this.Title = $"WPF Naplo | {openFileDialog.SafeFileName}";
        }

        private void btnRogzit_Click(object sender, RoutedEventArgs e)
        {
            //todo Ne lehessen rögzíteni, ha a következők valamelyike nem teljesül!
            //a) -A név legalább két szóból álljon és szavanként minimum 3 karakterből!
            //      Szó = A szöközökkel határolt karaktersorozat.
            bool passed = true;
            if (txtNev.Text.Split(" ").Length < 2)
            {
                MessageBox.Show("Minimum 2 szóból kell állnia!");
                passed = false;
            }
            // b) - A beírt dátum újabb, mint a mai dátum
            else if(datDatum.SelectedDate > DateTime.Now)
            {
                MessageBox.Show("Nem lehet jövőbeli dátum!");
                passed = false;
            }
            else
            {
                foreach (var item in txtNev.Text.Split(" "))
                {
                    if (item.Length < 3)
                    {
                        MessageBox.Show("Minimum 3 karakterből kell állnia a szavaknak!");
                        passed = false;
                        return;
                    }
                }
            }

            //todo A rögzítés mindig az aktuálisan megnyitott naplófájlba történjen!
            if (passed)
            {
                //A CSV szerkezetű fájlba kerülő sor előállítása
                string csvSor = $"{txtNev.Text};{datDatum.Text};{cboTantargy.Text};{sliJegy.Value}";
                //Megnyitás hozzáfűzéses írása (APPEND)
                StreamWriter sw = new StreamWriter(fajlNev, append: true);
                sw.WriteLine(csvSor);
                sw.Close();
                //todo Az újonnan felvitt jegy is jelenjen meg a datagrid-ben!
                Osztalyzat ujJegy = new Osztalyzat(txtNev.Text, datDatum.Text, cboTantargy.Text, Convert.ToInt32(sliJegy.Value));
                jegyek.Add(ujJegy);
                dgJegyek.ItemsSource = jegyek;
                dgJegyek.Items.Refresh();
                // - A naplóban lévő jegyek száma
                lblJegySzam.Content = jegyek.Count();
                //Az átlag
                double sum = 0;
                foreach (var item in jegyek)
                {
                    sum += Convert.ToDouble(item.Jegy);
                }
                lblJegyAtlag.Content = Convert.ToDouble(sum / jegyek.Count()).ToString("N2");
            }

        }

        private void btnBetolt_Click(object sender, RoutedEventArgs e)
        {
            jegyek.Clear();  //A lista előző tartalmát töröljük
            StreamReader sr = new StreamReader(fajlNev); //olvasásra nyitja az állományt
            while (!sr.EndOfStream) //amíg nem ér a fájl végére
            {
                string[] mezok = sr.ReadLine().Split(";"); //A beolvasott sort feltördeli mezőkre
                //A mezők értékeit felhasználva létrehoz egy objektumot
                Osztalyzat ujJegy = new Osztalyzat(mezok[0], mezok[1], mezok[2], int.Parse(mezok[3])); 
                jegyek.Add(ujJegy); //Az objektumot a lista végére helyezi
            }
            sr.Close(); //állomány lezárása

            //A Datagrid adatforrása a jegyek nevű lista lesz.
            //A lista objektumokat tartalmaz. Az objektumok lesznek a rács sorai.
            //Az objektum nyilvános tulajdonságai kerülnek be az oszlopokba.
            dgJegyek.ItemsSource = jegyek;
        }

        private void sliJegy_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblJegy.Content = sliJegy.Value; //Több alternatíva van e helyett! Legjobb a Data Binding!
        }

        private void rdo_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i != jegyek.Count; i++)
            {
                string reversedName = Osztalyzat.ForditottNev(jegyek[i].Nev);
                jegyek[i].Nev = reversedName;
                dgJegyek.Items.Refresh();
            }
        }

        //todo Felület bővítése: Az XAML átszerkesztésével biztosítsa, hogy láthatóak legyenek a következők!
        // - A naplófájl neve KÉSZ -> Title
        // - A naplóban lévő jegyek száma KÉSZ
        // - Az átlag KÉSZ

        //todo Új elemek frissítése: Figyeljen rá, ha új jegyet rögzít, akkor frissítse a jegyek számát és az átlagot is!

        //todo Helyezzen el alkalmas helyre 2 rádiónyomógombot! KÉSZ
        //Feliratok: [■] Vezetéknév->Keresztnév [O] Keresztnév->Vezetéknév KÉSZ
        //A táblázatban a név azserint szerepeljen, amit a rádiónyomógomb mutat! KÉSZ
        //A feladat megoldásához használja fel a ForditottNev metódust! KÉSZ
        //Módosíthatja az osztályban a Nev property hozzáférhetőségét!
        //Megjegyzés: Felételezzük, hogy csak 2 tagú nevek vannak
    }
}


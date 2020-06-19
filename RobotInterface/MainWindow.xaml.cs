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

namespace RobotInterface
{
    
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool ColorButtonEnvoyer = true;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void buttonEnvoyer_Click(object sender, RoutedEventArgs e)
        {
            TextBoxRéception.Text = "Reçu : " + TextBoxEmission.Text + "\n" + TextBoxRéception.Text;
            TextBoxEmission.Text = null ;

            buttonEnvoyer.Background = ColorButtonEnvoyer == true ? Brushes.RoyalBlue : Brushes.Beige;
            ColorButtonEnvoyer = !ColorButtonEnvoyer;
            
        }
    }
}

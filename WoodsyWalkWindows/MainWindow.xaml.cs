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
using WoodsyWalkWindows.GameModel;
using WoodsyWalkWindows.GameView;

namespace WoodsyWalkWindows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void PieceTest()
        {
            PieceView newPiece = new PieceView();
            newPiece.Width = 200; newPiece.Height = 200;
            int thisPiece = Pieces.createPiece(true, true, true, false, true, true, 24, 1, 2);
            thisPiece = Pieces.setDiagonals(thisPiece, false, true, true, false);
            newPiece.setPiece(thisPiece);
            MainGrid.Children.Add(newPiece);
            PieceView secondNewPiece = new PieceView();
            secondNewPiece.Width = 200; secondNewPiece.Height = 200;
            thisPiece = Pieces.createPiece(false, false, false, false, false, false, 1, 3, 0);
            thisPiece = Pieces.setAsLake(thisPiece);
            secondNewPiece.setPiece(thisPiece);
            MainGrid.Children.Add(secondNewPiece);
        }
    }
}

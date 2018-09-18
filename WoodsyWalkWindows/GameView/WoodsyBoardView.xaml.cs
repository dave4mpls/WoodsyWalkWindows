using System.Windows;
using System.Windows.Controls;
using WoodsyWalkWindows.GameModel;

namespace WoodsyWalkWindows.GameView
{
    /// <summary>
    /// Interaction logic for WoodsyBoardView.xaml
    /// </summary>
    public partial class WoodsyBoardView : UserControl
    {
        private WoodsyBoardData board = new WoodsyBoardData();
        private bool testMode = false;
        private bool readOnlyMode = false;

        public WoodsyBoardView()
        {
            InitializeComponent();
            setupTable();
        }

        public void setupTable()
        {
            int bheight = board.getHeight(); int bwidth = board.getWidth();
            if (this.testMode) this.setupTestBoard();
            BoardGrid.Children.Clear();
            BoardGrid.ColumnDefinitions.Clear();
            BoardGrid.RowDefinitions.Clear();
            //--- Create rows and columns and size them to equally divide the space.
            for (int i = 0; i < bwidth; i++)
            {
                ColumnDefinition cd = new ColumnDefinition();
                cd.Width = new GridLength(1, GridUnitType.Star);
                BoardGrid.ColumnDefinitions.Add(cd);
            }
            for (int i = 0; i < bheight; i++)
            {
                RowDefinition rd = new RowDefinition();
                rd.Height = new GridLength(1, GridUnitType.Star);
                BoardGrid.RowDefinitions.Add(rd);
            }
            //--- Now create the cells, each of them a Game Piece.
            for (int r = 0; r < bheight; r++)
            {
                for (int c = 0; c < bwidth; c++)
                {
                    PieceView pv = new PieceView();
                    pv.setCol(c); pv.setRow(r);
                    pv.setPiece(board.getCell(c, r));
                    Grid.SetRow(pv, r); Grid.SetColumn(pv, c);
                    BoardGrid.Children.Add(pv);
                }
            }
        }

        public void redrawBoard()
        {
            this.setupTable();
        }

        public void setBoard(WoodsyBoardData b)
        {
            this.board = b; this.redrawBoard();
        }

        public void setReadOnlyMode(bool m)
        {
            this.readOnlyMode = m;
        }

        public void setTestMode(bool m)
        {
            this.testMode = m;
            this.redrawBoard();
        }

        private void setupTestBoard()
        {
            //-- for testing, draws a variety of pieces around the board so we can see
            //-- how well the pieces draw themselves.
            int thisrow = 1;
            int thiscol = 1;
            int[] pieceList = Pieces.pieces();
            foreach (int thisPiece in pieceList)
            {
                this.board.setCell(thiscol, thisrow, thisPiece);
                thiscol++;
                if (thiscol >= this.board.getWidth() - 1) { thiscol = 1; thisrow++; }
            }
            int currentPiece = 0;
            for (int i = 0; i < 4; i++)
            {
                currentPiece = this.board.getCell(i + 1, 0);
                currentPiece = Pieces.setPersonNumber(currentPiece, i + 1);
                this.board.setCell(i + 1, 0, currentPiece);
                currentPiece = this.board.getCell(i + 1, this.board.getHeight() - 1);
                currentPiece = Pieces.setHouseNumber(currentPiece, i + 1);
                this.board.setCell(i + 1, this.board.getHeight() - 1, currentPiece);
            }
            currentPiece = this.board.getCell(1, 0);
            currentPiece = Pieces.setHouseNumber(currentPiece, 1);
            this.board.setCell(1, 0, currentPiece);
            this.board.setCell(0, 0, Pieces.createPersonPiece(1));
            this.board.setCell(0, 1, Pieces.createHousePiece(2));
        }
    }
}

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
using System.Windows.Media.Animation;
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
        public enum GameScreens { ScreenStartGame, ScreenPlayerName, ScreenPlayGame };
        private List<String> playerNames = new List<String>();
        private int _numPlayers = 2; private int currentPlayer = 0;
        private readonly int maxPlayers = 4;
        private TextBox[] PlayerNameFields;
        private TextBlock[] BoardCaptions;
        private WoodsyBoardView[] BoardViews;
        private WoodsyGameData WoodsyGame;

        public MainWindow()
        {
            InitializeComponent();
            PlayerNameFields = new TextBox[] { Player1Name, Player2Name, Player3Name, Player4Name };
            BoardCaptions = new TextBlock[] { BoardCaption1, BoardCaption2, BoardCaption3, BoardCaption4 };
            BoardViews = new WoodsyBoardView[] { Board1, Board2, Board3, Board4 };
            setScreen(GameScreens.ScreenStartGame);
        }

        public int numPlayers
        {       // use a getter/setter for numPlayers to ensure UI update
            get
            {
                return _numPlayers;
            }
            set
            {
                _numPlayers = value;
                for (int i = 0; i < maxPlayers; i++)
                {
                    if (i < _numPlayers)
                    {
                        PlayerNameFields[i].Visibility = Visibility.Visible;
                        BoardCaptions[i].Visibility = Visibility.Visible;
                        BoardViews[i].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        PlayerNameFields[i].Visibility = Visibility.Hidden;
                        BoardCaptions[i].Visibility = Visibility.Collapsed;
                        BoardViews[i].Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        public void setScreen(GameScreens nextScreen)
        {
            StartGameGrid.Visibility = Visibility.Hidden;
            PlayerNameGrid.Visibility = Visibility.Hidden;
            PlayGameGrid.Visibility = Visibility.Hidden;
            switch (nextScreen)
            {
                case GameScreens.ScreenStartGame: StartGameGrid.Visibility = Visibility.Visible; break;
                case GameScreens.ScreenPlayerName: PlayerNameGrid.Visibility = Visibility.Visible; break;
                case GameScreens.ScreenPlayGame: PlayGameGrid.Visibility = Visibility.Visible; break;
            }
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

        public void Toast(string msg)
        {
            ToastText.Text = msg;
            ToastText.Opacity = 1.0;
            ToastEdge.Opacity = 1.0;
            var anim = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(4)));
            ToastText.BeginAnimation(TextBlock.OpacityProperty, anim);
            var anim2 = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(4)));
            ToastEdge.BeginAnimation(TextBlock.OpacityProperty, anim);
        }

        private void OnePlayer_Click(object sender, RoutedEventArgs e)
        {
            numPlayers = 1; setScreen(GameScreens.ScreenPlayerName);
        }

        private void TwoPlayers_Click(object sender, RoutedEventArgs e)
        {
            numPlayers = 2; setScreen(GameScreens.ScreenPlayerName);
        }

        private void ThreePlayers_Click(object sender, RoutedEventArgs e)
        {
            numPlayers = 3; setScreen(GameScreens.ScreenPlayerName);
        }

        private void FourPlayers_Click(object sender, RoutedEventArgs e)
        {
            numPlayers = 4; setScreen(GameScreens.ScreenPlayerName);
        }

        public void displayGameBoard(WoodsyGameData W, String currentParticipantId)
        {
            //-- Get the player information and find our player.
            //-- Make a player pointer array to point to the game version's index for the actual order of players,
            //-- since in the display the active player is always on top.
            int wNumPlayers = W.getNumberOfParticipants();
            List<String> playerIds = W.getParticipantIds();
            int currentPlayerIndex = -1;
            for (int i = 0; i < wNumPlayers; i++)
            {
                if (playerIds[i].Equals(currentParticipantId)) currentPlayerIndex = i;
            }
            int[] playPointer = new int[wNumPlayers];
            playPointer[0] = currentPlayerIndex; int j = 1;
            for (int i = 0; i < wNumPlayers; i++)
            {
                if (i == currentPlayerIndex) continue;
                playPointer[j] = i; j++;
            }
            if (currentPlayerIndex == -1)
            {
                Toast("Unexpected problem locating player");
                currentPlayerIndex = 0;
                currentParticipantId = playerIds[0];
            }
            //-- Set the captions.  Set whether the board is read-only or not.
            for (int i = 0; i < 4; i++)
            {
                if (i < wNumPlayers)
                {
                    BoardCaptions[i].Text = playerIds[playPointer[i]] + "'s Board (Coins: " + W.getScore(playerIds[playPointer[i]]) + ")";
                    if (i == 0)
                    {       // active player
                        BoardViews[i].setReadOnlyMode(false);
                    }
                    else
                        BoardViews[i].setReadOnlyMode(true);
                }
            }
            //-- Now set the next piece.
            NextPiece.setPiece(W.getNextPiece());
            int nleft = WoodsyGame.piecesLeftInBag();
            PiecesLeftText.Text = ("(Pieces Left: " + nleft + ")");
            //-- Next, draw the game boards.
            for (int i = 0; i < wNumPlayers; i++)
            {
                if (i == 0)
                {
                    BoardViews[i].setBoard(W.getCurrentBoard());
                }
                else
                {
                    BoardViews[i].setBoard(W.getBoard(playerIds[playPointer[i]]));
                }
            }
        }


        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            setScreen(GameScreens.ScreenPlayGame);
            playerNames.Clear();
            for (int i = 0; i < numPlayers; i++)
                playerNames.Add(PlayerNameFields[i].Text);
            currentPlayer = 0;
            WoodsyGame = new WoodsyGameData(playerNames);
            WoodsyGame.beginTurn(playerNames[currentPlayer]);
            displayGameBoard(WoodsyGame, playerNames[currentPlayer]);
            Toast("Click on the board where you want your piece to go.");
        }

        private void BackToPlayersButton_Click(object sender, RoutedEventArgs e)
        {
            setScreen(GameScreens.ScreenStartGame);
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (!WoodsyGame.endTurn())
            {
                Toast(WoodsyGame.getLastErrorMessage());
                return;
            }
            currentPlayer++;
            if (currentPlayer >= numPlayers) currentPlayer = 0;
            WoodsyGame.beginTurn(playerNames[currentPlayer]);
            displayGameBoard(WoodsyGame, playerNames[currentPlayer]);
            // Saving not implemented yet: testSaveGame();
        }

        private void DiscardButton_Click(object sender, RoutedEventArgs e)
        {
            if (!WoodsyGame.piecesLeftThisTurn())
            {
                Toast("You have played all your pieces for this turn.  Click Undo if you want to change your play.  Click Play to submit your turn so the other player(s) can play.");
                return;
            }
            int r = WoodsyGame.playPieceDiscard(WoodsyGame.getNextPiece());
            if (r == Pieces.createFailurePiece())
            {
                Toast(WoodsyGame.getLastErrorMessage());
            }
            else
            {
                //-- success (or may send moving person piece if a person is being moved)
                displayGameBoard(WoodsyGame, playerNames[currentPlayer]);
                Toast("Piece played.");
            }
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            WoodsyGame.rewindTurn();
            displayGameBoard(WoodsyGame, playerNames[currentPlayer]);
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult mr = MessageBox.Show("Are you sure you want to end this game and start a new one?", "Restart", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
            if (mr == MessageBoxResult.Yes)
            {
                setScreen(GameScreens.ScreenStartGame);
            }
        }

        private void RulesButton_Click(object sender, RoutedEventArgs e)
        {

        }

        public void PlacePiece(WoodsyBoardView b, int x, int y)
        {
            if (!WoodsyGame.piecesLeftThisTurn())
            {
                Toast("You have played all your pieces for this turn.  Click Undo if you want to change your play.  Click Play to submit your turn so the other player(s) can play.");
                return;
            }
            int r = WoodsyGame.playPieceAt(WoodsyGame.getNextPiece(), x, y);
            if (r == Pieces.createFailurePiece())
            {
                Toast(WoodsyGame.getLastErrorMessage());
            }
            else
            {
                //-- success (or may send moving person piece if a person is being moved)
                displayGameBoard(WoodsyGame, playerNames[currentPlayer]);
                Toast("Piece played.");
                // Saving not implemented yet: testSaveGame();
            }
        }

        private void RotatePiece()
        {
            WoodsyGame.rotateNextPiece();
            displayGameBoard(WoodsyGame, playerNames[currentPlayer]);
            Toast(WoodsyGame.getLastErrorMessage());  // or success message, rotateNextPiece sends both.
        }

        private void NextPiece_Click(object sender, RoutedEventArgs e)
        {   // Clicking Next Piece rotates the piece.
            RotatePiece();
        }

        private void Board1_BoardCellClick(object sender, RoutedEventArgs r)
        {
            BoardCellClickEventArgs e = (BoardCellClickEventArgs)r;
            PlacePiece(Board1, e.col, e.row);
        }
    }
}

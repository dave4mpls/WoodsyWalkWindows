using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoodsyWalkWindows.GameModel
{
    //
    //  Class to represent an entire game of Woodsy Walk.  The toByteArray method produces a
    //  serialized version which is what we pass to Google Play Game Services as our game state.
    //  So this object should contain everything needed to play a game, by passing back and forth
    //  all the pieces in their shuffled order, which piece we are on, everyone's game boards,
    //  which colors of houses and people pieces have been placed, etc.
    //

    [Serializable]
    public class WoodsyGameData 
    {
        static readonly long serialVersionUID = 1L;
        private List<String> participantIds = new List<String>();
        private List<String> winners = new List<String>();        // when a game is completed, these are the participantIds of the winners in the participant array above
        private List<int> remainingHouses = new List<int>();
        private List<int> remainingPersons = new List<int>();
        private Dictionary<String, List<int>> piecesToPlay = new Dictionary<String, List<int>>();   // all the remaining pieces to play for each player.
        private Dictionary<String, WoodsyBoardData> boards = new Dictionary<String, WoodsyBoardData>();  // all the boards for the players
        private Dictionary<String, int> scores = new Dictionary<String, int>();    // all the player scores
        private int[] pieceBag;     // the bag of pieces shuffled in random order, that gets assigned to each new participant.
        private int minPiecesLeft;  // when we play a piece, if our individual player piece bag has fewer than this, we update the value.  If a new player enters, they discard pieces from the front until they have this number.  This keeps all the players playing the same piece.
        private int[] personScores;     // when a person meets a house, you get this number of points, and then the # of points is decremented until it is zero.

        private String currentParticipant;
        private int currentScoreAtStartOfTurn;
        private int[] personScoresAtStartOfTurn;     // when a person meets a house, you get this number of points, and then the # of points is decremented until it is zero.
        private WoodsyBoardData currentBoard = new WoodsyBoardData();   // the current board while we are making changes
        private List<int> currentTurnPieces = new List<int>();
        private List<int> currentTurnPlayedPieces = new List<int>();
        private bool _movingPerson = false;     // true if the current turn has become moving a person
        private int personMovesLeft = 0;  // During person moves, number of moves left.
        private bool turnFinished = false;
        private Coordinates personCoordinates = new Coordinates(0, 0); // track where the person is during person move
        private String lastErrorMessage = "";

        public WoodsyGameData(List<String> inputParticipantIds)
        {
            // Constructor: create a Woodsy game data structure based on the supplied player ID's.
            this.lastErrorMessage = "";
            this.participantIds.Clear();
            this.remainingHouses.Clear();
            this.remainingPersons.Clear();
            this.personScores = new int[Pieces.numberOfPeople()];
            this.personScoresAtStartOfTurn = new int[Pieces.numberOfPeople()];
            for (int i = 1; i <= Pieces.numberOfPeople(); i++)
            {
                this.remainingHouses.Add(Pieces.createHousePiece(i));
                this.remainingPersons.Add(Pieces.createPersonPiece(i));
                this.personScores[i - 1] = this.maxPointsForGoal();
            }
            this.piecesToPlay.Clear();
            this.boards.Clear();
            this.scores.Clear();
            //
            //  prepare the piece bag
            this.pieceBag = Pieces.pieces();
            this.minPiecesLeft = this.pieceBag.Length;
            //
            //  now, add all the known  participants.
            //
            foreach (String thisParticipantId in inputParticipantIds)
            {
                if (thisParticipantId != null) this.addParticipantIfNeeded(thisParticipantId);
            }
        }

        // methods that return game parameters-- currently constants
        public int pointsForGold() { return 2; }
        public int pointsForSilver() { return 1; }
        public int maxPointsForGoal() { return 5; }

        // private methods
        private void addParticipantIfNeeded(String participantId)
        {
            // Checks to see if a participant is already in the game and, if not, adds their
            // information.
            if (this.participantIds.Contains(participantId)) return;   // already exists
            this.participantIds.Add(participantId);
            this.scores[participantId] = 0; 
            this.boards[participantId] = new WoodsyBoardData();
            this.piecesToPlay[participantId] = new List<int>();
            //  The board manages itself, but for Pieces to Play, we have to copy the piece bag.
            //  If other players have played pieces, we remove the ones they played from the front.
            foreach (int thisPiece in this.pieceBag) this.piecesToPlay[participantId].Add(thisPiece);
            while (this.piecesToPlay[participantId].Count > this.minPiecesLeft)
                this.piecesToPlay[participantId].RemoveAt(0);
        }

        // methods for accessing data and modifying it during the game
        public WoodsyBoardData getBoard(String participantId)
        {
            // returns the board corresponding to a participant ID, or null if not found
            return this.boards[participantId];
        }

        public int getNumberOfParticipants()
        {
            return this.participantIds.Count;
        }

        public List<String> getParticipantIds()
        {
            return this.participantIds;
        }

        public int getScore(String participantId)
        {
            return this.scores[participantId];
        }

        public void beginTurn(String participantId)
        {
            // Begin the current turn by copying the current player's board to the current board property.
            // When you begin a turn, you also link the game data to an Android context that can get string resources.
            this.addParticipantIfNeeded(participantId);
            this.currentParticipant = participantId;
            this.currentBoard.copyFrom(this.getBoard(participantId));
            this.currentScoreAtStartOfTurn = this.scores[this.currentParticipant];
            for (int i = 0; i < this.personScores.Length; i++)
            {
                this.personScoresAtStartOfTurn[i] = this.personScores[i];
            }
            // Also determine the current pieces to play on this turn, based on
            // availability of house/person pieces and number of players (2 player game:
            // each player plays 2 houses and 2 persons at the start)
            this.currentTurnPieces.Clear();
            this.currentTurnPlayedPieces.Clear();
            if (this.remainingPersons.Count > 0)
            {
                int numPairs = 1;
                if (this.participantIds.Count == 2) numPairs = 2;
                for (int i = 0; i < numPairs; i++)
                {
                    if (i < this.remainingPersons.Count)
                        this.currentTurnPieces.Add(this.remainingPersons[i]);
                    if (i < this.remainingHouses.Count)
                        this.currentTurnPieces.Add(this.remainingHouses[i]);
                }
            }
            if (this.currentTurnPieces.Count == 0)
            {
                // if no people or houses to place, place the next regular piece
                if (this.piecesToPlay[participantId].Count > 0)
                    this.currentTurnPieces.Add(this.piecesToPlay[participantId][0]);
            }
            // Prepare various turn related properties.
            this._movingPerson = false;
            this.personMovesLeft = 0;
            this.turnFinished = false;
            this.personCoordinates = new Coordinates(0, 0);
            this.lastErrorMessage = "";
        }

        public void rewindTurn()
        {
            // use this if the user rewinds their turn to the beginning.
            this.scores[this.currentParticipant] = this.currentScoreAtStartOfTurn;
            for (int i = 0; i < this.personScores.Length; i++)
            {
                this.personScores[i] = this.personScoresAtStartOfTurn[i];
            }
            this.beginTurn(this.currentParticipant);
        }

        private bool hasEnoughCoins(int price)
        {
            if (this.scores[currentParticipant] < price) return false; else return true;
        }

        private bool spendCoins(int price)
        {
            if (!hasEnoughCoins(price)) return false;
            this.scores[currentParticipant] = this.scores[currentParticipant] - price;
            return true;
        }

        public bool rotateNextPiece()
        {
            // rotates a piece and sends a success or error message.  Returns true on success.
            setFailureBoolean("You spent " + priceRotation() + " to rotate the piece.");
            if (priceRotation() == 0) setFailureBoolean("You rotated the piece.");
            if (this._movingPerson) return setFailureBoolean("You can't rotate a person who is moving.");  // no rotating people)
            if (!this.piecesLeftThisTurn()) return setFailureBoolean("You have no pieces left to rotate.");
            int originalPiece = this.currentTurnPieces[0];
            if (Pieces.isPerson(originalPiece)) return setFailureBoolean("You can't rotate a person.");
            if (Pieces.isHouse(originalPiece)) return setFailureBoolean("You can't rotate a house.");
            if (!spendCoins(priceRotation())) return setFailureBoolean("You can't afford to rotate.  Rotation costs " + priceRotation() + " coins.");
            this.currentTurnPieces[0] = Pieces.rotatePiece(originalPiece);
            if (this.piecesToPlay[this.currentParticipant][0] == originalPiece)
                this.piecesToPlay[this.currentParticipant][0] = Pieces.rotatePiece(originalPiece);
            return true;
        }

        //--- Prices in coins for various in-game actions.
        public int priceRotation() { return 0; }
        public int priceOtherBoardPlacement() { return 5; }
        public int priceOwnRemoval() { return 10; }

        public bool piecesLeftThisTurn()
        {
            // returns whether there are pieces left to play this turn.
            if (this._movingPerson) return true;  // don't forget moving person mode! always return true so they get the best error message if they move too many times (from the play subroutine).
            if (this.currentTurnPieces.Count == 0) this.turnFinished = true;
            if (this.turnFinished) return false;
            return true;
        }

        public int getNextPiece()
        {
            // during a turn, this returns the next piece to play, which can be displayed
            // in the next piece area.  Returns the End of Turn piece if none remain.
            if (this._movingPerson) return Pieces.getPersonPieceFrom(this.currentBoard.getCell(this.personCoordinates));
            if (!this.piecesLeftThisTurn()) return Pieces.createEndOfTurnPiece();
            return this.currentTurnPieces[0];
        }

        public String getLastErrorMessage()
        {
            // returns the last error message generally produced by playPieceAt.
            return this.lastErrorMessage;
        }

        private int setFailure(String msg)
        {
            // Sets the last error message to msg, and returns the failure piece.
            this.lastErrorMessage = msg;
            return Pieces.createFailurePiece();
        }

        private bool setFailureBoolean(String msg)
        {
            // Sets the last error message then returns false.
            this.lastErrorMessage = msg;
            return false;
        }

        private void setCurrentPiecePlayed()
        {
            // used inside of playPieceAt to move a piece that was played from the "to-play" array to the "played" array.
            if (this.currentTurnPieces.Count == 0) return;
            this.currentTurnPlayedPieces.Add(this.currentTurnPieces[0]);
            this.currentTurnPieces.RemoveAt(0);
        }

        private void incrementScore(int x)
        {
            // Increments the score of the current player.
            int newScore = this.scores[this.currentParticipant] + x;
            this.scores[this.currentParticipant] = newScore;
        }

        private void determineWinners()
        {
            //--- This doesn't determine IF somebody ended the game-- that happens when there are no more pieces
            //--- or someone completes all their paths.  But it does determine which player(s) won-- there can be ties.
            int maxScore = -1; List<String> winningParticipants = new List<String>();
            for (int i = 0; i < this.participantIds.Count; i++)
            {
                int thisScore = this.scores[this.participantIds[i]];
                if (thisScore > maxScore)
                {
                    maxScore = thisScore;
                    winningParticipants.Clear();
                    winningParticipants.Add(this.participantIds[i]);
                }
                else if (thisScore == maxScore)
                    winningParticipants.Add(this.participantIds[i]);  // ties are possible
            }
            this.winners = winningParticipants;
        }

        public int playPieceAt(int p, int x, int y)
        {
            //  Plays the valid piece P at (x,y) on the board.  Returns the Success piece on regular success
            //  (with the board updated), the Failure piece if the move is invalid, or, if a person has
            //  begun moving, it returns a pure Person piece to indicate which person is moving.
            //  If you get a failure piece, you can get an error message using getLastErrorMessage.
            Coordinates c = new Coordinates(x, y);
            // check if there are no moves left
            if (this._movingPerson && this.personMovesLeft <= 0) return this.setFailure("You have no moves left for your person.");
            if (!this.piecesLeftThisTurn()) return this.setFailure("You have no pieces left for this turn.");
            // check for valid coordinates, retrieve the current piece on the board.
            if (!this.currentBoard.isValidCoords(c)) return this.setFailure("Internal Error: invalid coordinates");
            int currentPiece = this.currentBoard.getCell(c);
            // check game rules and place piece based on which kind it is.
            if (Pieces.isPerson(p) && this._movingPerson)
            {
                // Playing a single step in moving a person.  First, see if there is a path to move the person.
                // (The situation of having no moves left was already handled up top.)
                int sourcePiece = this.currentBoard.getCell(this.personCoordinates);
                int thisPersonPiece = Pieces.getPersonPieceFrom(sourcePiece);
                int thisPersonNumber = Pieces.personNumber(thisPersonPiece);
                int goalPiece = this.currentBoard.createGoalPiece(thisPersonNumber, 0, c);
                bool reachedGoal = (goalPiece == currentPiece);
                if (Pieces.personNumber(currentPiece) > 0)
                    return this.setFailure("There can be only one person in a square at a time.");
                if (!Pieces.piecesConnect(this.currentBoard.getCell(this.personCoordinates), currentPiece, this.personCoordinates.x(), this.personCoordinates.y(), x, y))
                    return this.setFailure("There is no path for the person to get to that square.  Make sure to move your person only one square at a time.");
                //-- it appears that we now have a valid move.
                //-- move the person
                sourcePiece = Pieces.setPersonNumber(sourcePiece, 0);
                currentPiece = Pieces.setPersonNumber(currentPiece, thisPersonNumber);
                this.currentBoard.setCell(this.personCoordinates, sourcePiece);
                this.personMovesLeft--;
                this.personCoordinates = c;
                //-- check for coins
                if (Pieces.gold(currentPiece) || Pieces.silver(currentPiece))
                {
                    if (Pieces.gold(currentPiece)) this.incrementScore(this.pointsForGold());
                    if (Pieces.silver(currentPiece)) this.incrementScore(this.pointsForSilver());
                    currentPiece = Pieces.setCoins(currentPiece, false, false);
                    this.currentBoard.setCell(c, currentPiece);
                }
                //-- check for reaching goal
                if (reachedGoal)
                {
                    int thisZeroBasedPersonNumber = thisPersonNumber - 1;
                    this.incrementScore(this.personScores[thisZeroBasedPersonNumber]);   // DEBUG: line where Nancy's game crashed 9/1 8:20pm: probably due to the zero-based/one-based thing, probably she got her purple person to the goal which caused the crash.
                    this.personScores[thisZeroBasedPersonNumber]--;
                    if (this.personScores[thisZeroBasedPersonNumber] < 0) this.personScores[thisZeroBasedPersonNumber] = 0;
                    //-- check: have we won by finding all the goals?
                    if (this.currentBoard.isWinningBoard())
                    {
                        this.determineWinners();
                    }
                }
                //-- now our piece is ready to store and we can return.
                this.currentBoard.setCell(c, currentPiece);
                return Pieces.createSuccessPiece();
            }
            else if (Pieces.isPerson(p) || Pieces.isHouse(p))
            {
                // Playing a person piece or house piece when you're not moving a person.
                if (!this.currentBoard.isOnEdge(c)) return this.setFailure("Person and house pieces can only be placed on the edges.");  // person piece has to be placed on edge
                if (this.currentBoard.isOnCorner(c)) return this.setFailure("You can't place person and house pieces on the corners.");
                if (this.currentBoard.distanceToPartner(p, c) < 5) return this.setFailure("The person piece and the house piece of each color have to be at least 5 spaces apart.");  // person piece can't be too close to house piece
                if (!Pieces.isGreenGrassPiece(currentPiece)) return this.setFailure("Person and house pieces can only be placed on green grass.");  // have to put house or person on green grass.
                // combine the pieces and store the result.  Also add a path for the person or house.
                int newPiece = Pieces.combinePieces(currentPiece, p);
                if (Pieces.isFailurePiece(newPiece)) return this.setFailure("There was an unexpected problem.");
                newPiece = this.currentBoard.setGoalDirections(newPiece, c);
                this.currentBoard.setCell(c, newPiece);
                this.setCurrentPiecePlayed();
                return Pieces.createSuccessPiece();
            }
            else
            {
                // Playing a regular piece.  If you play it against an existing piece with a person on it,
                // it starts person-moving mode.  If you play it on a blank square, it places the piece.
                // Other moves are invalid.
                if (Pieces.personNumber(currentPiece) > 0)
                {
                    // Begin person moving mode
                    this._movingPerson = true;
                    this.personCoordinates = c;
                    this.personMovesLeft = Pieces.numberMoves(p);
                    this.setCurrentPiecePlayed();
                    return Pieces.createSuccessPiece();
                }
                else if (this.currentBoard.isOnEdge(c))     //-- note: you CAN be on an edge if you're moving a person, see above.
                    return this.setFailure("You can't play a path piece on the edge of the board.");
                else if (Pieces.isBlank(currentPiece))
                {
                    // Placing tile in blank space
                    this.currentBoard.setCell(c, p);
                    this.setCurrentPiecePlayed();
                    return Pieces.createSuccessPiece();
                }
                else
                    return this.setFailure("You can't move there.");
            }
            // Shouldn't get here, and indeed, the compiler says you can't get here.
        }

        public int playPieceDiscard(int p)
        {
            //  Discards a piece.  Of course you can't discard people or house pieces, and discarding
            //  doesn't work if you started moving a person.
            //  Returns the success or failure piece.
            if (this._movingPerson) return this.setFailure("You can't discard a person piece that you are moving.");
            if (Pieces.isHouse(p)) return this.setFailure("You can't discard a house piece.");
            if (Pieces.isPerson(p)) return this.setFailure("You can't discard a person piece.");
            this.setCurrentPiecePlayed();  // put the piece in the played pile
            return Pieces.createSuccessPiece();
        }

        public bool movingPerson()
        {
            // True if a person is moving as a result of the last play.
            return this._movingPerson;
        }

        public int movingPersonMovesLeft()
        {
            if (!this._movingPerson) return 0;
            return this.personMovesLeft;
        }

        public int piecesLeftInBag()
        {
            // For display purposes, you might want to know how many pieces are left in the whole bag.
            return this.piecesToPlay[this.currentParticipant].Count;
        }

        public WoodsyBoardData getCurrentBoard()
        {
            // retrieve the current board, which is the one that has any modifications during the turn.
            return this.currentBoard;
        }

        private void removePiece(List<int> a, int p)
        {
            // Regular remove (in Java anyway, I think in C# this isn't a problem, but I'm leaving this routine for now) has trouble removing a particular int, it thinks we're talking about an index.  So this searches for the piece and removes it.
            // TODO: remove this function and use the native C# function which doesn't have the ambiguity problem that the function has in Java
            for (int i = a.Count - 1; i >= 0; i--)
            {
                if (a[i] == p) a.RemoveAt(i);
            }
        }

        public bool endTurn()
        {
            // call this when the turn is complete, right before serializing the data.
            // it saves the proposed turn into the actual current participant's board.
            // it also removes all the played pieces permanently from where they came from.
            // Returns true on success, false on error, and on error fills the last error message.
            if (!this._movingPerson && piecesLeftThisTurn()) return setFailureBoolean("You must play all pieces before clicking Play.");
            if (this._movingPerson) this.setCurrentPiecePlayed();  // if we were moving a person, we didn't actually put the piece in the played pile till now.
            bool housePersonPlayed = false;
            foreach (int thisPlayedPiece in this.currentTurnPlayedPieces)
            {
                if (Pieces.isPerson(thisPlayedPiece))
                {
                    removePiece(this.remainingPersons, thisPlayedPiece);
                    housePersonPlayed = true;
                }
                else if (Pieces.isHouse(thisPlayedPiece))
                {
                    removePiece(this.remainingHouses, thisPlayedPiece);
                    housePersonPlayed = true;
                }
                else
                    this.piecesToPlay[this.currentParticipant].Remove(thisPlayedPiece);
            }
            //--  check if there is now a new minimum number of pieces
            if (this.piecesLeftInBag() < this.minPiecesLeft) this.minPiecesLeft = this.piecesLeftInBag();
            //--  now check to see if anybody has pieces left!
            int participantsWithPieces = 0;
            foreach (String thisParticipantId in this.participantIds)
            {
                if (this.piecesToPlay[thisParticipantId].Count > 0) participantsWithPieces++;
            }
            if (participantsWithPieces == 0)
            {
                // nobody has pieces-- game is over!  find and declare the winner.
                determineWinners();
            }
            //--- now that we've removed played pieces, save the board.
            this.boards[this.currentParticipant].copyFrom(this.currentBoard);
            //--  if the user placed houses or people, they go on everybody's board the same way
            if (housePersonPlayed)
            {
                for (int i = 0; i < participantIds.Count; i++)
                {
                    this.boards[participantIds[i]].copyFrom(this.currentBoard);
                }
            }
            return true;
        }

        public List<String> getWinners()
        {
            // gets the winner participant ID, or "" if no winner yet
            return this.winners;
        }

        public bool gameOver()
        {
            // returns true if the game is over (determined by whether a winner has been set)
            return (this.winners.Count == 0);
        }
    }
}

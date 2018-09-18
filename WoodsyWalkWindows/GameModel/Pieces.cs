using System;
using System.Collections;

namespace WoodsyWalkWindows.GameModel
{
    //
    //  Pieces: An object with only static methods (so don't instantiate it, no need)
    //  that gives information about the pieces in the game-- the list of pieces and methods
    //  to discover information about each piece.
    //
    //  Each piece is a 21-bit positive integer, with the following bitmapped meanings:
    //  Bit 21:         Northwest  (diagonal road)
    //  Bit 20:         Northeast
    //  Bit 19:         Southwest
    //  Bit 18:         Southeast
    //  Bits 17-15:     0 if no house figure is on this piece, or 1,2,3,4 for the color of the house
    //  Bits 14-12:     0 if no person figure is on this piece, or 1,2,3,4 for the color of the person
    //  Bits 11-6:      The piece number, 1 through the number of pieces (currently 36, can range up to 63).
    //                  A piece that is all zeroes is blank.
    //                  A piece that has a piece number of 63 and no other data represents End of Turn in the game data's piece array.
    //                  A piece with a piece number of 62 and no other data represents Success when playing a piece.
    //                  A piece with a piece number of 61 and no other data represents Invalid Move when playing a piece.
    //                  A piece with a piece number of 60 and no directions/coins is a regular piece, that displays as just green grass.   You can add a person or house to it.
    //  Bit 5:          Set if the piece has an upward line.
    //  Bit 4:          Set if the piece has a downward line.
    //  Bit 3:          Set if the piece has a leftward line.
    //  Bit 2:          Set if the piece has a rightward line.
    //  Bit 1:          Set if the piece has a silver coin.
    //  Bit 0:          Set if the piece has a gold coin.
    //
    //  You don't need to know that, though, just call the methods in this class to process the pieces.
    //  The pieces are NOT exactly the same as any other game that may have inspired this one!
    //

    public class Pieces
    {
        static private Random rnd = new Random();

        public static void shuffleArray(int[] ar)
        {
            // Fisher-Yates shuffle as shown in Stack Overflow: https://stackoverflow.com/questions/1519736/random-shuffling-of-an-array
            for (int i = ar.Length - 1; i > 0; i--)
            {
                int index = Pieces.rnd.Next(i + 1);
                // Simple swap
                int a = ar[index];
                ar[index] = ar[i];
                ar[i] = a;
            }
        }

        public static int randomInt(int min, int max)
        {
            return (int)((Pieces.rnd.NextDouble() * ((max - min) + 1)) + min);
        }

        private static int[] minMaxRoads(int p)
        {
            // Returns minimum and maximum road numbers for a piece to weed out those dumb ones that just turn right around in a tight space.
            // Minimum is returnValue[0], max is returnValue[1].
            ArrayList a = new ArrayList(); a.Clear();
            if (Pieces.northwest(p)) a.Add(0);
            if (Pieces.up(p)) a.Add(1);
            if (Pieces.northeast(p)) a.Add(2);
            if (Pieces.right(p)) a.Add(3);
            if (Pieces.southeast(p)) a.Add(4);
            if (Pieces.down(p)) a.Add(5);
            if (Pieces.southwest(p)) a.Add(6);
            if (Pieces.left(p)) a.Add(7);
            int minRoad = 9, maxRoad = -1;
            for (int i = 0; i < a.Count; i++)
            {
                if ((int)a[i] < minRoad) minRoad = (int)a[i];
                if ((int)a[i] > maxRoad) maxRoad = (int)a[i];
            }
            int[] returnValue = new int[2];
            returnValue[0] = minRoad; returnValue[1] = maxRoad;
            return returnValue;
        }

        public static int[] pieces(int numPieces)
        {
            //--- New version of pieces, which generates the shuffled bag to use in the game; it is
            //--- randomly generated instead of static, and includes the new diagonal directions.
            int[] a = new int[numPieces]; int randomPiece = 0;
            int maxBitPattern = (int)Math.Pow(2, 22) - 1;    //by getting random bits of this size, we can make a whole mess of weird pieces.
            for (int i = 0; i < numPieces; i++)
            {
                bool pieceWanted = false;
                while (!pieceWanted)
                {
                    // Make a random, but valid, path piece. Its piece number is the array index.
                    randomPiece = Pieces.randomInt(0, maxBitPattern);
                    randomPiece &= 0b1111000000000000111111;        // zero out house #, person #, piece ID, etc.
                    randomPiece = Pieces.setPieceNumber(randomPiece, i);
                    // Throw out pieces that have various non-game-preferred attributes.  These are aspects of game play that I might adjust after playing.
                    int nm = Pieces.numberMoves(randomPiece);
                    if (nm > 4) continue;      // no more than 4 directions
                    if (nm < 2) continue;      // No dead-end pieces
                                               //-- Some code to alter proportions of pieces....
                    if (i < 3 * numPieces / 40) randomPiece = Pieces.setAsLake(randomPiece);  // 3 in every game are lakes.
                    else if (i < 20 * numPieces / 40) { if (nm != 2) continue; }    // then pieces 4 through 20 are two-roads
                    else if (i < 30 * numPieces / 40) { if (nm != 3) continue; }   // 20-30 are three-roads
                    else { if (nm != 4) continue; }   // rest are 4-roads
                                                      //-- not too many coins
                    if (Pieces.rnd.NextDouble() < 0.3) { randomPiece = Pieces.takeCoins(randomPiece); }  // 30% of the time coins get buried...
                                                                                               //-- Below, we don't want very many pieces with, say, 3 straight lines and 1 diagonal.  Two-move pieces with one straight and one diagonal are fine, and can be used to convert straight to diagonal.
                    if (Pieces.numberMovesHV(randomPiece) == 1 && Pieces.numberMoves(randomPiece) > 2 && Pieces.rnd.NextDouble() > 0.75) continue;
                    if (Pieces.numberMovesDiag(randomPiece) == 1 && Pieces.numberMoves(randomPiece) > 2 && Pieces.rnd.NextDouble() > 0.75) continue;
                    //-- not too scrunched up of a piece
                    int[] maxmin = Pieces.minMaxRoads(randomPiece);
                    int mmDiff = Math.Abs(maxmin[0] - maxmin[1]);
                    if (nm == 2 && (mmDiff < 2 || (maxmin[0] == 0 && maxmin[1] == 7))) continue;
                    if (nm == 3 && (mmDiff < 5)) continue;
                    // We like this piece
                    pieceWanted = true;
                }
                a[i] = randomPiece;
            }
            Pieces.shuffleArray(a);
            return a;
        }
        public static int[] pieces() { return pieces(40); }  // overload method to provide default value for pieces()

        public static int[] pieces_static()
        {
            // Returns the list of all pieces similar to the game that inspired Woodsy Walk.  Now deprecated, but keep around for reference.
            // The pieces are shuffled randomly before being returned.
            // Note that since they are expressed in binary, you can more easily modify them using the definition above.
            int[] pieceArray = new int[] {
                ((1 << 6) | 0b001100), ((7 << 6) | 0b001100), ((13 << 6) | 0b011110), ((19 << 6) | 0b011010), ((25 << 6) | 0b011100), ((31 << 6) | 0b011100),
                ((2 << 6) | 0b111100), ((8 << 6) | 0b111100), ((14 << 6) | 0b100110), ((20<< 6) | 0b101010), ((26<< 6) | 0b101110), ((32<< 6) | 0b101100),
                ((3 << 6) | 0b001110), ((9<< 6) | 0b001101), ((15<< 6) | 0b001110), ((21<< 6) | 0b001101), ((27<< 6) | 0b110000), ((33<< 6) | 0b110000),
                ((4 << 6) | 0b001100), ((10<< 6) | 0b001100), ((16<< 6) | 0b010000), ((22<< 6) | 0b001000), ((28<< 6) | 0b110100), ((34<< 6) | 0b111000),
                ((5 << 6) | 0b111100), ((11<< 6) | 0b111100), ((17<< 6) | 0b100110), ((23<< 6) | 0b101000), ((29<< 6) | 0b110100), ((35<< 6) | 0b111000),
                ((6 << 6) | 0b110010), ((12<< 6) | 0b110001), ((18<< 6) | 0b110010), ((24<< 6) | 0b110001), ((30<< 6) | 0b110000), ((36<< 6) | 0b110000)
        };
            Pieces.shuffleArray(pieceArray);
            return pieceArray;
        }

        // Number of people and house pairs in the game
        public static int numberOfPeople() { return 4; }

        // Static methods for finding directional status or coin status for a piece.
        public static bool up(int p) { return ((p & 0b100000) != 0); }
        public static bool down(int p) { return ((p & 0b10000) != 0); }
        public static bool left(int p) { return ((p & 0b1000) != 0); }
        public static bool right(int p) { return ((p & 0b100) != 0); }
        public static bool northwest(int p) { return ((p & (1 << 21)) != 0); }
        public static bool northeast(int p) { return ((p & (1 << 20)) != 0); }
        public static bool southwest(int p) { return ((p & (1 << 19)) != 0); }
        public static bool southeast(int p) { return ((p & (1 << 18)) != 0); }
        public static bool silver(int p) { return ((p & 0b10) != 0); }
        public static bool gold(int p) { return ((p & 1) != 0); }
        public static int pieceNumber(int p) { return ((p >> 6) & 0b111111); }
        public static int personNumber(int p) { return ((p >> 12) & 0b111); }
        public static int houseNumber(int p) { return ((p >> 15) & 0b111); }
        public static bool isHouse(int p)
        {      // true if the piece is JUST a house piece (all the rest zero)
            return ((p & 0b1111000111111111111111) == 0) && Pieces.houseNumber(p) > 0;
        }
        public static bool isPerson(int p)
        {     // true if the piece is JUST a person piece (all the rest zero)
            return ((p & 0b1111111000111111111111) == 0) && Pieces.personNumber(p) > 0;
        }
        public static bool isPersonAndHouse(int p)
        {     // true if the piece is a person AND house
            return ((p & 0b000000111111111111) == 0);
        }
        public static bool isTile(int p)
        {       // true if the piece is any other piece but a plain person or house piece
            return ((!Pieces.isHouse(p)) && (!Pieces.isPerson(p)) && (!Pieces.isPersonAndHouse(p)));
        }
        public static bool isBlank(int p) { return (p == 0); }
        public static bool isEndOfTurn(int p) { return (Pieces.pieceNumber(p) == 63); }
        public static int numberMovesHV(int p)
        {
            // Returns the number of Horizontal/Vertical moves you can make of a person piece by discarding this piece.
            // Also useful for divining the # of certain directions in a tile before deciding whether to put it in the bag or not
            return ((Pieces.up(p) ? 1 : 0) + (Pieces.down(p) ? 1 : 0) + (Pieces.left(p) ? 1 : 0)
                + (Pieces.right(p) ? 1 : 0));
        }
        public static int numberMovesDiag(int p)
        {
            // Returns the number of Horizontal/Vertical moves you can make of a person piece by discarding this piece.
            // Also useful for divining the # of certain directions in a tile before deciding whether to put it in the bag or not
            return ((Pieces.northwest(p) ? 1 : 0) + (Pieces.northeast(p) ? 1 : 0) + (Pieces.southwest(p) ? 1 : 0)
                    + (Pieces.southeast(p) ? 1 : 0));
        }
        public static int numberMoves(int p)
        {
            return Pieces.numberMovesDiag(p) + Pieces.numberMovesHV(p);
        }

        public static int rotatePieceOneEighth(int p)
        {
            // Rotates a piece clockwise.
            int newPiece = p;
            newPiece = Pieces.setDirections(newPiece, Pieces.northwest(p), Pieces.southeast(p), Pieces.southwest(p), Pieces.northeast(p));
            newPiece = Pieces.setDiagonals(newPiece, Pieces.left(p), Pieces.up(p), Pieces.down(p), Pieces.right(p));
            return newPiece;
        }
        public static int rotatePiece(int p)
        {
            return rotatePieceOneEighth(rotatePieceOneEighth(p));
        }

        private static void connectPlaceTile(int p /* piece */, int x, int y, int[,] a)
        {
            // The piecesConnect routine now uses a 7x7 array of integers to calculate connections, which
            // represents a 3x3 grid of tiles each of which has 3 sides (left, right, center) that can have roads.
            // They intersect of course (hence a 7x7 grid instead of 9x9), and the purpose of this is to
            // modify the array to add in a tile's connections, with a +1 to any node where a road is.  Connections
            // between tiles will appear as numbers > 1 at the tiles' edges.
            // The coordinates x and y are tile coordinates where the center is (0,0), so they range from -1 to +1.
            int cx = 3 + x * 2;     //-- Center X: the position of the center of the tile in the array.
            int cy = 3 + y * 2;
            a[cy, cx]++;        // tiles always serve their center
            if (Pieces.up(p)) a[cy - 1, cx]++; if (Pieces.down(p)) a[cy + 1, cx]++;
            if (Pieces.left(p)) a[cy, cx - 1]++; if (Pieces.right(p)) a[cy, cx + 1]++;
            if (Pieces.northwest(p)) a[cy - 1, cx - 1]++; if (Pieces.southeast(p)) a[cy + 1, cx + 1]++;
            if (Pieces.southwest(p)) a[cy + 1, cx - 1]++; if (Pieces.northeast(p)) a[cy - 1, cx + 1]++;
        }
        private static int countTwos(int[,] a)
        {
            //--- used in piecesConnect to count the # of overlapping nodes indicating a connection
            int c = 0;
            for (int i = 0; i < a.GetLength(0); i++)
            {
                for (int j = 0; j < a.GetLength(1); j++)
                {
                    if (a[i,j] >= 2) c++;
                }
            }
            return c;
        }
        private static void zeroArray(int[,] a)
        {
            for (int i = 0; i < a.GetLength(0); i++)
                for (int j = 0; j < a.GetLength(1); j++)
                    a[i,j] = 0;
        }
        public static bool piecesConnect(int a, int b, int ax, int ay, int bx, int by)
        {
            // Returns true if the given pieces, with the given coordinates (in any arbitrary grid)
            // have connected paths.  In that case, it is permissible to move a person to that square.
            // First, are the pieces adjacent?
            if (Math.Abs(ax - bx) > 1) return false;
            if (Math.Abs(ay - by) > 1) return false;
            // Are they on the same square?
            if (ax == bx && ay == by) return false;
            // With diagonals, this whole thing is more error-prone to code manually.  Use an integer array as described
            // above, under connectPlaceTile, to find the intersections.
            int dx = bx - ax; int dy = by - ay;   // dx/dy are change from a to get to b.  a will be seen as the "center."
            int[,] arr = new int[7,7];
            Pieces.zeroArray(arr);
            Pieces.connectPlaceTile(a, 0, 0, arr); // put tile A in the center
            Pieces.connectPlaceTile(b, dx, dy, arr);   // put tile B at its relative position
            if (Pieces.countTwos(arr) > 0) return true;
            else return false;
        }

        public static int setDirections(int p, bool up, bool down, bool left, bool right)
        {
            // Returns a piece with directions set.
            p &= 0b1111111111111111000011;
            if (up) p |= 0b100000;
            if (down) p |= 0b10000;
            if (left) p |= 0b1000;
            if (right) p |= 0b100;
            return p;
        }

        public static int setDiagonals(int p, bool northwest, bool northeast, bool southwest, bool southeast)
        {
            // Returns a piece with diagonal directions set.
            p &= 0b0000111111111111111111;
            if (northwest) p |= (1 << 21);
            if (northeast) p |= (1 << 20);
            if (southwest) p |= (1 << 19);
            if (southeast) p |= (1 << 18);
            return p;
        }

        public static int setAsLake(int p)
        {
            // Changes the piece so it is a lake (internally represented as all directions turned on).
            return Pieces.takeCoins(p | 0b1111000000000000111100);
            //-- We take the coins because all the coins in the lake sink to the bottom! :-)
        }

        public static bool isLake(int p)
        {
            //-- A lake is a piece where anybody can go in any direction (they can swim!).  It is
            //-- represented internally by having all the direction flags set, but drawn as a light blue square!
            return (p & 0b1111000000000000111100) == 0b1111000000000000111100;
        }

        public static int setCoins(int p, bool silver, bool gold)
        {
            int silverMask = (silver ? 0b10 : 0b00);
            int goldMask = (gold ? 0b01 : 0b00);
            return takeCoins(p) | silverMask | goldMask;
        }

        public static int setPieceNumber(int p, int pieceNumber)
        {
            return (p & 0b1111111111000000111111) | ((pieceNumber & 0b111111) << 6);
        }

        // Static method for creating a piece (which is of course actually an integer)
        public static int createPiece(bool up, bool down, bool left, bool right, bool silver, bool gold, int pieceNumber, int personNumber, int houseNumber)
        {
            int p = 0;
            p = Pieces.setDirections(p, up, down, left, right);
            p = Pieces.setCoins(p, silver, gold);
            p |= (pieceNumber << 6);
            p |= (personNumber << 12);
            p |= (houseNumber << 15);
            return p;
        }

        public static int createHousePiece(int h)
        {
            return Pieces.createPiece(false, false, false, false, false, false, 0, 0, h);
        }

        public static int createPersonPiece(int p)
        {
            return Pieces.createPiece(false, false, false, false, false, false, 0, p, 0);
        }


        public static int createBlankPiece() { return 0; }
        public static int createEndOfTurnPiece()
        {
            return Pieces.createPiece(false, false, false, false, false, false, 63, 0, 0);
        }
        public static int createSuccessPiece()
        {
            return Pieces.createPiece(false, false, false, false, false, false, 62, 0, 0);
        }
        public static int createFailurePiece()
        {
            return Pieces.createPiece(false, false, false, false, false, false, 61, 0, 0);
        }
        public static int createGreenGrassPiece()
        {
            return Pieces.createPiece(false, false, false, false, false, false, 60, 0, 0);
        }
        public static int getPersonPieceFrom(int p)
        {
            // given a piece with a person on it, get the corresponding person piece.
            // Returns the failure piece if no person is on it.
            if (Pieces.personNumber(p) == 0) return createFailurePiece();
            return createPersonPiece(Pieces.personNumber(p));
        }
        public static int getHousePieceFrom(int p)
        {
            // given a piece with a house on it, get the corresponding house piece.
            // Returns the failure piece if no house is on it.
            if (Pieces.houseNumber(p) == 0) return createFailurePiece();
            return createHousePiece(Pieces.houseNumber(p));
        }
        public static bool isEndOfTurnPiece(int p) { return p == Pieces.createEndOfTurnPiece(); }
        public static bool isSuccessPiece(int p) { return p == Pieces.createSuccessPiece(); }
        public static bool isFailurePiece(int p) { return p == Pieces.createFailurePiece(); }
        public static bool isGreenGrassPiece(int p) { return p == Pieces.createGreenGrassPiece(); }

        // Methods for modifying a piece
        public static int takeCoins(int p)
        {
            // take the coins off a piece
            return (p & 0b1111111111111111111100);
        }
        public static int setHouseNumber(int p, int houseNumber)
        {
            // set the house number for a piece (i.e. add or remove a house)
            return (p & 0b1111000111111111111111) | ((houseNumber & 0b111) << 15);
        }
        public static int setPersonNumber(int p, int personNumber)
        {
            // set the person number for a piece (i.e. add or remove a person)
            return (p & 0b1111111000111111111111) | ((personNumber & 0b111) << 12);
        }
        public static int combinePieces(int p1, int p2)
        {
            // Combines a piece that is a person or house, with a regular piece.  Order is unimportant.
            // Returns the failure piece if neither piece is a plain person or house.
            if (Pieces.isTile(p1)) { int t = p1; p1 = p2; p2 = t; }
            if (Pieces.isPerson(p1) && Pieces.isTile(p2)) return Pieces.setPersonNumber(p2, Pieces.personNumber(p1));
            if (Pieces.isHouse(p1) && Pieces.isTile(p2)) return Pieces.setHouseNumber(p2, Pieces.houseNumber(p1));
            return Pieces.createFailurePiece();
        }

    }
}

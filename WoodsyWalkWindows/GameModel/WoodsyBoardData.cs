using System;

namespace WoodsyWalkWindows.GameModel
{
    //  Object representing a game board for one player in Woodsy Walk.
    //  This is the internal data representation, NOT the UI view object-- that's WoodsyBoardView.

    [Serializable]
    public class WoodsyBoardData
    {
        static readonly long serialVersionUID = 1L;
        private int[,] board;
        private int height, width;

        public WoodsyBoardData()
        {
            // board constructor: board starts out empty.
            this.board = new int[8,8];  // rows first, then columns.  .
            this.height = 8;
            this.width = 8;
            // note: the edges have Green Grass pieces, showing where the people and houses go.
            for (int i = 0; i < this.height; i++)
                for (int j = 0; j < this.width; j++)
                {
                    if (i == 0 || j == 0 || i + 1 == this.height || j + 1 == this.width)
                        this.board[i,j] = Pieces.createGreenGrassPiece();
                    else
                        this.board[i,j] = Pieces.createBlankPiece();
                }
        }

        //-- Getters and setters
        public int getHeight() { return this.height; }
        public int getWidth() { return this.width; }
        public int getCell(int x, int y)
        {
            if (x < 0 || x >= this.width) return 0;
            if (y < 0 || y >= this.height) return 0;
            return this.board[y,x];
        }
        public void setCell(int x, int y, int p)
        {
            if (x < 0 || x >= this.width) return;
            if (y < 0 || y >= this.height) return;
            this.board[y,x] = p;
        }
        public int getCell(Coordinates c)
        {
            if (c.notFound()) return Pieces.createFailurePiece();
            return this.getCell(c.x(), c.y());
        }
        public void setCell(Coordinates c, int p)
        {
            if (c.notFound()) return;
            this.setCell(c.x(), c.y(), p);
        }
        //-- Locate people and houses
        public Coordinates locatePerson(int personNumber)
        {
            for (int i = 0; i < this.height; i++)
                for (int j = 0; j < this.width; j++)
                {
                    if (Pieces.personNumber(this.board[i,j]) == personNumber) return new Coordinates(j, i, false);
                }
            return new Coordinates(0, 0, true);
        }
        public Coordinates locateHouse(int houseNumber)
        {
            for (int i = 0; i < this.height; i++)
                for (int j = 0; j < this.width; j++)
                {
                    if (Pieces.houseNumber(this.board[i,j]) == houseNumber) return new Coordinates(j, i, false);
                }
            return new Coordinates(0, 0, true);
        }
        //-- Evaluate coordinates
        public bool isValidCoords(Coordinates a)
        {
            // return true for valid coordinates
            if (a.x() < 0 || a.y() < 0) return false;
            if (a.x() >= this.width) return false;
            if (a.y() >= this.height) return false;
            return true;
        }
        public bool isOnEdge(Coordinates a)
        {
            // return true for coordinates on the edge of the board (where only people and houses go)
            if (!isValidCoords(a)) return false;
            if (a.x() == 0 || a.x() == this.width - 1) return true;
            if (a.y() == 0 || a.y() == this.height - 1) return true;
            return false;
        }
        public bool isOnCorner(Coordinates a)
        {
            if (!isValidCoords(a)) return false;
            if (a.x() == 0 && a.y() == 0) return true;
            if (a.x() == 0 && a.y() == this.height - 1) return true;
            if (a.x() == this.width - 1 && a.y() == 0) return true;
            if (a.x() == this.width - 1 && a.y() == this.height - 1) return true;
            return false;
        }
        //-- Compute distance
        public int distance(Coordinates a, Coordinates b)
        {
            // Determines the Manhattan-style distance between two coordinate pairs.
            // Returns a distance of Integer.MAX_VALUE if one of the coordinates is Not Found.
            if (a.notFound() || b.notFound()) return int.MaxValue;
            return Math.Abs(a.x() - b.x()) + Math.Abs(a.y() - b.y());
        }
        public int distanceToPartner(int p, Coordinates c)
        {
            // Given a person or house piece, and proposed coordinates for placing it, compute
            // the distance to its corresponding partner.  Returns Integer.MAX_VALUE if the
            // partner is not found or the input is not a pure person/house piece.
            if (Pieces.isPerson(p))
                return this.distance(this.locateHouse(Pieces.personNumber(p)), c);
            else if (Pieces.isHouse(p))
                return this.distance(this.locatePerson(Pieces.houseNumber(p)), c);
            else
                return int.MaxValue;
        }
        //-- Creating a goal piece (with or without a house on it-- put zero for no house/person)
        //   at a particular place on the edge of the board.
        public int createGoalPiece(int houseNumber, int personNumber, Coordinates c)
        {
            int p = Pieces.createGreenGrassPiece();
            p = Pieces.setHouseNumber(p, houseNumber);
            p = Pieces.setPersonNumber(p, personNumber);
            p = this.setGoalDirections(p, c);
            return p;
        }
        //-- Sets the directions appropriately for a goal piece or origin piece.
        public int setGoalDirections(int p, Coordinates c)
        {
            p = Pieces.setDirections(p, (c.y() == this.height - 1), (c.y() == 0), (c.x() == this.width - 1), (c.x() == 0));
            //-- I decided diagonals on the goal piece made it too easy, but I'll keep the code in case I add levels or something.
            //if (c.y()==0) p = Pieces.setDiagonals(p,false,false,true,true);
            //if (c.x()==0) p = Pieces.setDiagonals(p,false,true,false,true);
            //if (c.y()==this.getHeight()-1) p = Pieces.setDiagonals(p,true,true,false,false);
            //if (c.x()==this.getWidth()-1) p = Pieces.setDiagonals(p ,true,false,true,false);
            return p;
        }
        //-- Determining if a board is a winning board
        public bool isWinningBoard()
        {
            for (int i = 1; i <= Pieces.numberOfPeople(); i++)
            {
                Coordinates ch = this.locateHouse(i);
                Coordinates cp = this.locatePerson(i);
                if (!ch.notFound() && !cp.notFound())
                {
                    if (!ch.equals(cp)) return false;
                }
                else
                    return false;
            }
            return true;
        }
        //-- Copy a board
        public void copyFrom(WoodsyBoardData b)
        {
            for (int i = 0; i < b.height; i++)
            {
                for (int j = 0; j < b.width; j++)
                {
                    this.setCell(j, i, b.board[i,j]);
                }
            }
        }

    }
}

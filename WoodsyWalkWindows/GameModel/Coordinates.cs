using System;

namespace WoodsyWalkWindows.GameModel
{
    //  Simple coordinate class for use by various other modules.

    [Serializable]
    public class Coordinates
    {
        // Coordinate class for use with some return values.
        static readonly long serialVersionUID = 1L;
        private int _x;
        private int _y;
        private bool _notFound;

        public Coordinates(int ax, int ay, bool aNotFound)
        {
            this._x = ax;
            this._y = ay;
            this._notFound = aNotFound;
        }
        public Coordinates(int ax, int ay)
        {
            this._x = ax;
            this._y = ay;
            this._notFound = false;
        }
        public bool equals(Coordinates b)
        {
            return (this._x == b._x && this._y == b._y && this._notFound == b._notFound);
        }
        public int x() { return this._x; }      // could use getter/setter, but rest of code, converted from Java, expects it this way
        public int y() { return this._y; }
        public bool notFound() { return this._notFound; }
    }
}

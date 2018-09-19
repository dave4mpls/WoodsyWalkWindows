using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using WoodsyWalkWindows.GameModel;

namespace WoodsyWalkWindows.GameView
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:WoodsyWalkWindows.GameView"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:WoodsyWalkWindows.GameView;assembly=WoodsyWalkWindows.GameView"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:PieceView/>
    ///
    /// </summary>
    public class PieceView : Button
    {
        private int mPiece;      // The piece integer with its bitmap is all we need to know to draw a piece.
        private int mRow, mCol;   // used by callers when this piece is part of a table

        private SolidColorBrush mLightGreenBrush, mDarkGreenBrush, mYellowBrush, mRedBrush, mPurpleBrush, mBlueBrush;
        private SolidColorBrush mBrownBrush, mGoldBrush, mSilverBrush, mBlackBrush, mEdgeGreenBrush, mLightBlueBrush;
        private SolidColorBrush[] personHouseBrushes;

        static PieceView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PieceView), new FrameworkPropertyMetadata(typeof(PieceView)));
        }

        public PieceView() : base()
        {
            init();
        }

        private void init()
        {
            // Default row and col are zero; these are convenience properties for the parent controls,
            // so they are just referenced using setters and getters
            mRow = 0; mCol = 0;

            // clear the click listener on open; set up a link to our onClick listener
            //this.clickListener = null;
            //super.setOnClickListener(this);

            // Set up color brush objects for painting parts of pieces.
            mLightGreenBrush = new SolidColorBrush(); mEdgeGreenBrush = new SolidColorBrush(); mDarkGreenBrush = new SolidColorBrush();
            mBlackBrush = new SolidColorBrush(); mBrownBrush = new SolidColorBrush(); mGoldBrush = new SolidColorBrush();
            mSilverBrush = new SolidColorBrush(); mRedBrush = new SolidColorBrush(); mLightBlueBrush = new SolidColorBrush();
            mBlueBrush = new SolidColorBrush(); mPurpleBrush = new SolidColorBrush(); mYellowBrush = new SolidColorBrush();
            mEdgeGreenBrush.Color = Color.FromArgb(255, 164, 255, 164);
            mDarkGreenBrush.Color = Color.FromArgb(255, 0, 193, 0);
            mLightGreenBrush.Color = Color.FromArgb(255, 200, 255, 200);
            mBlackBrush.Color = Color.FromArgb(255, 0, 0, 0);
            mBrownBrush.Color = Color.FromArgb(255, 184, 134, 1);
            mGoldBrush.Color = Color.FromArgb(255, 255, 234, 118);
            mSilverBrush.Color = Color.FromArgb(255, 189, 189, 189);
            mLightBlueBrush.Color = Color.FromArgb(255, 153, 217, 234);
            mRedBrush = Brushes.Red; mBlueBrush = Brushes.Blue; mYellowBrush = Brushes.Yellow;
            mPurpleBrush = Brushes.Purple;
            // Prepare the person house paints color array.
            this.personHouseBrushes = new SolidColorBrush[Pieces.numberOfPeople()];
            for (int i = 0; i < Pieces.numberOfPeople(); i++)
            {
                if (i == 0) this.personHouseBrushes[i] = this.mRedBrush;
                if (i == 1) this.personHouseBrushes[i] = this.mYellowBrush;
                if (i == 2) this.personHouseBrushes[i] = this.mBlueBrush;
                if (i == 3) this.personHouseBrushes[i] = this.mPurpleBrush;
                if (i == 4) this.personHouseBrushes[i] = this.mGoldBrush;  // right now we only have 4 pairs of houses/people.  If we get more maybe we will add more colors!
                if (i == 5) this.personHouseBrushes[i] = this.mSilverBrush;
            }

            // Invalidate the control so it redraws
            this.InvalidateVisual();
        }

        private void drawHouse(DrawingContext DC, SolidColorBrush brush, double centerX, double topY, double width, double height)
        {
            // Draws a house figure for the piece.
            double lineWidth = 1;       // line width is always 1 for a house, since it is filled and we want sharp corners
            topY += lineWidth / 2 + 1;
            height -= lineWidth + 2;
            width -= lineWidth;
            Pen p = new Pen(brush, lineWidth);
            p.EndLineCap = PenLineCap.Round;
            p.LineJoin = PenLineJoin.Round;
            PathSegment[] psa = new PathSegment[]
            {
                new PolyLineSegment(new Point[]
                {
                    new Point(centerX, topY),
                    new Point(centerX + width / 2, topY + height / 2),
                    new Point(centerX + width / 2, topY + height),
                    new Point(centerX - width / 2, topY + height),
                    new Point(centerX - width / 2, topY + height / 2),
                    new Point(centerX, topY)
                }, true)
            };
            PathFigure pf = new PathFigure(new Point(centerX, topY), psa, true);
            PathGeometry g = new PathGeometry(new PathFigure[] { pf });
            DC.DrawGeometry(brush, p, g);
        }

        private void drawPerson(DrawingContext DC, SolidColorBrush brush, double centerX, double topY, double width, double height)
        {
            // Draws a person figure for the piece.
            double lineWidth = (this.RenderSize.Width * 0.12);
            topY += lineWidth / 2 + 1;
            height -= lineWidth + 2;
            width -= lineWidth;
            double headRadius = (double)(height / 6.0);
            double personPartHeight = (double)(height / 3.0);
            double personLimbWidth = (double)(width / 2.0);
            Pen p = new Pen(brush, lineWidth);
            p.EndLineCap = PenLineCap.Round;
            p.LineJoin = PenLineJoin.Round;
            PathSegment[] psa = new PathSegment[]
            {
                new PolyLineSegment(new Point[]
                {
                    //-- left arm
                    new Point(centerX, topY + personPartHeight),
                    new Point (centerX - personLimbWidth, topY + personPartHeight * 2),
                    //-- right arm
                    new Point(centerX, topY + personPartHeight),
                    new Point(centerX + personLimbWidth, topY + personPartHeight * 2),
                    //-- torso
                    new Point(centerX, topY + personPartHeight),
                    new Point(centerX, topY + personPartHeight * 2),
                    //-- left leg
                    new Point(centerX, topY + personPartHeight * 2),
                    new Point(centerX - personLimbWidth, topY + personPartHeight * 3),
                    //-- right leg
                    new Point(centerX, topY + personPartHeight * 2),
                    new Point(centerX + personLimbWidth, topY + personPartHeight * 3),
                    new Point(centerX, topY + personPartHeight * 2)
                }, true)
            };
            p.Thickness = lineWidth;
            PathFigure pf = new PathFigure(new Point(centerX, topY + personPartHeight), psa, true);
            PathGeometry g = new PathGeometry(new PathFigure[] { pf });
            DC.DrawGeometry(brush, p, g);

            //--- head uses fill mode
            DC.DrawEllipse(brush, p, new Point(centerX, topY + headRadius), headRadius, headRadius);
        }

        public void drawRoadsAndCoins(DrawingContext DC, int p, double x1, double y1, double x2, double y2)
        {
            //-- Draws the roads and gems on the piece.
            // do roads first
            double strokeWidth = Math.Abs(x2 - x1) * 0.08;
            x1 += strokeWidth / 2; y1 += strokeWidth / 2; x2 -= strokeWidth / 2; y2 -= strokeWidth / 2;
            double cx = (x2 + x1) / 2;
            double cy = (y2 + y1) / 2;
            SolidColorBrush brown = this.mBrownBrush;
            Pen pn = new Pen(brown, strokeWidth);
            pn.EndLineCap = PenLineCap.Round;
            pn.StartLineCap = PenLineCap.Round;
            pn.LineJoin = PenLineJoin.Round;
            if (Pieces.up(p)) DC.DrawLine(pn, new Point(cx, y1), new Point(cx, cy));
            if (Pieces.down(p)) DC.DrawLine(pn, new Point(cx, y2), new Point(cx, cy));
            if (Pieces.left(p)) DC.DrawLine(pn, new Point(x1, cy), new Point(cx, cy));
            if (Pieces.right(p)) DC.DrawLine(pn, new Point(x2, cy), new Point(cx, cy));
            if (Pieces.northwest(p)) DC.DrawLine(pn, new Point(x1, y1), new Point(cx, cy));
            if (Pieces.northeast(p)) DC.DrawLine(pn, new Point(cx, cy), new Point(x2, y1));
            if (Pieces.southwest(p)) DC.DrawLine(pn, new Point(x1, y2), new Point(cx, cy));
            if (Pieces.southeast(p)) DC.DrawLine(pn, new Point(cx, cy), new Point(x2, y2));
            //
            // now do coins
            SolidColorBrush silver = this.mSilverBrush;
            SolidColorBrush gold = this.mGoldBrush;
            if (Pieces.silver(p)) DC.DrawEllipse(silver, new Pen(silver, 1), new Point((x1 + cx) / 2, (y1 + cy) / 2), (Math.Abs(cy - y1) / 2) * 0.65, (Math.Abs(cy - y1) / 2) * 0.65);
            if (Pieces.gold(p)) DC.DrawEllipse(gold, new Pen(gold, 1), new Point((x2 + cx) / 2, (y2 + cy) / 2), (Math.Abs(cy - y2) / 2) * 0.65, (Math.Abs(cy - y2) / 2) * 0.65);
        }

        public void setPiece(int p)
        {
            // Sets the piece to draw.  Of course setting the piece causes a redraw.
            if (this.mPiece == p) return;  // ignore if the piece is unchanged
            this.mPiece = p;
            this.InvalidateVisual();
        }

        public int getPiece()
        {
            return this.mPiece;
        }

        //-- The parent can set rows and columns on a piece to make it easier to know
        //-- which piece in a table was clicked.
        public void setRow(int r) { mRow = r; }
        public void setCol(int c) { mCol = c; }
        public int getRow() { return mRow; }
        public int getCol() { return mCol; }

        protected override void OnRender(DrawingContext DC)
        {
            base.OnRender(DC);
            // consider storing these as member variables to reduce
            // allocations per draw cycle.
            double paddingLeft = 0; 
            double paddingTop = 0;  
            double paddingRight = 0; 
            double paddingBottom = 0;

            double clientWidth = this.RenderSize.Width;
            double clientHeight = this.RenderSize.Height;

            double contentWidth = clientWidth - paddingLeft - paddingRight;
            double contentHeight = clientHeight - paddingTop - paddingBottom;

            // Draw the piece.  Start with the background fill.
            SolidColorBrush bgFill;
            bool pieceIsPerson = Pieces.isPerson(this.mPiece);
            bool pieceIsHouse = Pieces.isHouse(this.mPiece);
            bool pieceIsBlank = Pieces.isBlank(this.mPiece);
            bool pieceIsGreenGrass = Pieces.isGreenGrassPiece(this.mPiece);
            if (Pieces.isEndOfTurn(this.mPiece))
                bgFill = this.mLightGreenBrush;
            else if (Pieces.isLake(this.mPiece))
                bgFill = this.mLightBlueBrush;
            else if (pieceIsPerson || pieceIsHouse)
                bgFill = this.mDarkGreenBrush;
            else if (pieceIsBlank)
                bgFill = this.mLightGreenBrush;
            else if (pieceIsGreenGrass)
                bgFill = this.mEdgeGreenBrush;
            else
                bgFill = this.mDarkGreenBrush;
            Pen p = new Pen(bgFill, 1);
            DC.DrawRectangle(bgFill, p, new Rect(paddingLeft, paddingTop, contentWidth, contentHeight));
            // Now, add the black border.
            p = new Pen(mBlackBrush, 1);
            DC.DrawRectangle(null, p, new Rect(paddingLeft, paddingTop, contentWidth, contentHeight));
            // If it is the end of turn piece, draw a circle with a line, then we are done.
            if (Pieces.isEndOfTurn(this.mPiece))
            {
                Pen redPen = new Pen(mRedBrush, 3);
                DC.DrawEllipse(null, redPen, new Point(paddingLeft + contentWidth / 2, paddingTop + contentHeight / 2), contentWidth / 2 - 10, contentWidth / 2 - 10);
                DC.DrawLine(redPen, new Point(paddingLeft, paddingTop + contentHeight), new Point(paddingLeft + contentWidth, paddingTop));
                return;
            }
            // Now, draw the roads.  And the coins on the roads.  But not with lakes, they don't have those.
            if (!Pieces.isLake(this.mPiece))
                this.drawRoadsAndCoins(DC, this.mPiece, paddingLeft + 2, paddingTop + 2, paddingLeft + contentWidth - 2, paddingTop + contentHeight - 2);
            // Now, if there is a person or house on this piece, draw it.  If this is JUST a person or house
            // piece, draw it centered.  Make sure to draw it the right color.
            SolidColorBrush phBrush;
            if (Pieces.isPerson(this.mPiece))
            {
                phBrush = this.personHouseBrushes[Pieces.personNumber(this.mPiece) - 1];
                this.drawPerson(DC, phBrush, paddingLeft + contentWidth / 2, 2 + paddingTop, ((int)(contentWidth * 0.4)), contentHeight - 4);
            }
            else if (Pieces.isHouse(this.mPiece))
            {
                phBrush = this.personHouseBrushes[Pieces.houseNumber(this.mPiece) - 1];
                this.drawHouse(DC, phBrush, paddingLeft + contentWidth / 2, 2 + paddingTop, ((int)(contentWidth * 0.4)), contentHeight - 4);
            }
            else
            {
                if (Pieces.personNumber(this.mPiece) > 0)
                {
                    phBrush = this.personHouseBrushes[Pieces.personNumber(this.mPiece) - 1];
                    this.drawPerson(DC, phBrush, paddingLeft + contentWidth / 4, 2 + paddingTop, ((int)(contentWidth * 0.4)), contentHeight - 4);
                }
                if (Pieces.houseNumber(this.mPiece) > 0)
                {
                    phBrush = this.personHouseBrushes[Pieces.houseNumber(this.mPiece) - 1];
                    this.drawHouse(DC, phBrush, paddingLeft + (contentWidth * 3) / 4, 2 + paddingTop, ((int)(contentWidth * 0.4)), contentHeight - 4);
                }
            }
        }

    }
}

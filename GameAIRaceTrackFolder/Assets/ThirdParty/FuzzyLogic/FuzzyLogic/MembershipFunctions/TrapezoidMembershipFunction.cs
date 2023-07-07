using System.Linq;

namespace Tochas.FuzzyLogic.MembershipFunctions
{
    public class TrapezoidMembershipFunction : IMembershipFunction
    {

        private Coords[] points;

        public Coords P0 { get { return this.points[0]; } }
        public Coords P1 { get { return this.points[1]; } }
        public Coords P2 { get { return this.points[2]; } }
        public Coords P3 { get { return this.points[3]; } }

        public TrapezoidMembershipFunction(Coords p0, Coords p1, Coords p2, Coords p3)
        {
            this.SetPoints(p0, p1, p2, p3);
        }

        public void SetPoints(Coords p0, Coords p1, Coords p2, Coords p3)
        {
            if (this.points == null)
                this.points = new Coords[] { p0, p1, p2, p3 };

            this.points = this.points.OrderBy(x => x.X).ToArray();
        }

        public float fX(float x)
        {
            if (x <= this.P0.X)
                return this.P0.Y;
            if (x >= this.P3.X)
                return this.P3.Y;
            if (x == this.P1.X)
                return this.P1.Y;
            if (x == this.P2.X)
                return this.P2.Y;
             if(x > this.P1.X && x < this.P2.X )
            {
                return Coords.Lerp(this.P1, this.P2, x);
            }
            if (x < this.P1.X)
            {
                return Coords.Lerp(this.P0, this.P1, x);
            }
            if (x > this.P2.X)
            {
                return Coords.Lerp(this.P2, this.P3, x);
            }

            return 0f;

        }

        public float RepresentativeValue { get { return (this.P1.X*this.P1.Y + this.P2.X*this.P2.Y)/ (this.P1.Y+this.P2.Y); } }
    }
}

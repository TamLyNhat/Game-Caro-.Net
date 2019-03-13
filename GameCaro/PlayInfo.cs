using System.Drawing;

namespace GameCaro
{
    public class PlayInfo
    {
        private Point point;

        public Point Point
        {
            get { return point; }
            set { point = value; }
        }

        private int currentPlayer;

        public int CurrentPlayer
        {
            get { return currentPlayer; }
            set { currentPlayer = value; }
        }

        public PlayInfo(Point point, int currentPlayer)
        {
            this.Point = point;
            this.CurrentPlayer = currentPlayer;
        }
    }
}
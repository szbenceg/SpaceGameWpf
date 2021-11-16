using System;
using System.Collections.Generic;
using System.Windows.Shapes;

namespace SpaceGame.ViewModel
{
    class PlayerField : GameObjectField
    {
        public PlayerField(int width, int height, int positionX, int positionY) : base(width, height, positionX, positionY, 10, "/images/rocket.png")
        {
        }
    }
}

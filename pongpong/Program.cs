﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using SFML.Window;
using SFML.Graphics;
using SFML.System;

namespace pongpong
{
    public interface IVertexContaining
    {
        void InitVertexes();
    }
    public class BaseObject : IVertexContaining
    {
        public float speed = 0;
        public Vector2f startPos;
        public Vector2f dirVector = new Vector2f(0f, 0f);
        public Vector2f velocity;
        public Vector2f[] vertexes = new Vector2f[4];
        public Vector2f shapeSize;
        public Shape shape;

        
        public virtual void InitVertexes()
        {
            vertexes[0] = shape.Position;
            vertexes[1] = new Vector2f(shapeSize.X + shape.Position.X, shape.Position.Y);
            vertexes[2] = shapeSize + shape.Position;
            vertexes[3] = new Vector2f(shape.Position.X, shapeSize.Y + shape.Position.Y);
            /*
             *    [0]--------[1]
             *    |            |
             *    | BaseObject |
             *    |            |
             *    [3]--------[2]
             * 
             */
        }


        public virtual void MoveObject()
        {
            velocity = dirVector * speed;
            shape.Position += velocity;
            InitVertexes();
        }
    }

    public class Obstacle : BaseObject
    {
        public Obstacle(Vector2f startPos, Vector2f shapeSize)
        {
            this.startPos = startPos;
            this.shapeSize = shapeSize;
            
            shape = new RectangleShape(shapeSize);
            shape.Position = startPos;
            InitVertexes();
        }
    }

    public class Puck : BaseObject, IVertexContaining
    {
        public new void InitVertexes()
        {
            vertexes[0] = new Vector2f(/*0.5f * */shapeSize.X + shape.Position.X, shape.Position.Y);
            vertexes[1] = new Vector2f(2*shapeSize.X + shape.Position.X, /*0.5f * */shapeSize.Y + shape.Position.Y);
            vertexes[2] = new Vector2f(/*0.5f * */shapeSize.X + shape.Position.X, 2*shapeSize.Y + shape.Position.Y);
            vertexes[3] = new Vector2f(shape.Position.X, /*0.5f * */shapeSize.Y + shape.Position.Y);
            /*
             *    ./----[0]----\
             *    |            |
             *  [3]    Puck    [1]
             *    |            |
             *     \---[2]---/
             *
             *     It should look like a circle.
             */
        }

        public new void MoveObject(Vector2f collVector, RenderWindow window)
        {
            velocity = dirVector * speed;
            if (collVector == new Vector2f(0,0))
            {
                
                shape.Position += velocity;
            }
            else
            {
                if (collVector == vertexes[0] || collVector == vertexes[2])
                {
                    dirVector = new Vector2f(dirVector.X, -dirVector.Y);
                }
                else if (collVector == vertexes[1] || collVector == vertexes[3])
                {
                    dirVector = new Vector2f(-dirVector.X, dirVector.Y);
                }

                shape.Position -= 3*velocity;
            }
            InitVertexes();
        }

        public Puck(Vector2f shapeSize, float speed, Vector2f dirVector, Vector2f startPos, Color color)
        {
            this.dirVector = dirVector;
            this.startPos = startPos;
            this.speed = speed;
            this.shapeSize = shapeSize;
            
            shape = new CircleShape(shapeSize.X)
            {
                FillColor = color
            };
            shape.Position = startPos;
            InitVertexes();
        }
    }

    public class Player : BaseObject
    {
        public List<Keyboard.Key> playerKeys;

        public void Player_KeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Code == playerKeys[0])
            {
                dirVector = new Vector2f(1, 0);
                MoveObject();
            }

            if (e.Code == playerKeys[1])
            {
                dirVector = new Vector2f(-1, 0);
                MoveObject();
            }
        }

        public Player(Vector2f shapeSize, float speed, Vector2f startPos, List<Keyboard.Key> playerKeys)
        {
            shape = new RectangleShape(shapeSize);
            this.startPos = startPos;
            shape.Position = startPos;

            this.speed = speed;
            this.playerKeys = playerKeys;
            this.shapeSize = shapeSize;
            InitVertexes();
        }
    }

    public class Game
    {
        public RenderWindow window;
        public static List<Keyboard.Key> Player1_Keys = new List<Keyboard.Key>();
        public static List<Keyboard.Key> Player2_Keys = new List<Keyboard.Key>();
        
        public Player Player1 = new Player(new Vector2f(50f, 20f), 5f, new Vector2f(400, 550), Player1_Keys);
        public Player Player2 = new Player(new Vector2f(50f, 20f), 5f, new Vector2f(400, 50), Player2_Keys);
        public Puck puck = new Puck(new Vector2f(10f,10f), 0.2f,new Vector2f(-1f,0.5f), new Vector2f(400,300), Color.Red);
        
        public List<IVertexContaining> VertexContainings = new List<IVertexContaining>();
        public List<BaseObject> BaseObjects = new List<BaseObject>();

        private void Window_KeyPressed(object sender, KeyEventArgs e)
        {
            var window = (Window) sender;
            if (e.Code == Keyboard.Key.Escape)
            {
                window.Close();
            }
        }

        public void InitObstacles()
        {
            BaseObjects.Add(Player1);
            BaseObjects.Add(Player2);
            BaseObjects.Add(new Obstacle(new Vector2f(0f, 0f), new Vector2f(50f, 600f)));
            BaseObjects.Add(new Obstacle(new Vector2f(750f, 0f), new Vector2f(50f, 600f)));
        }

        public void InitKeys()
        {
            Player1_Keys.Add(Keyboard.Key.F);
            Player1_Keys.Add(Keyboard.Key.D);
            Player2_Keys.Add(Keyboard.Key.Right);
            Player2_Keys.Add(Keyboard.Key.Left);
        }

        public Vector2f DetectCollision(Puck baseObject, List<BaseObject> figures)
        {
            /*Puck tempBaseObject = baseObject;
            tempBaseObject.shape.Position += tempBaseObject.velocity;
            tempBaseObject.InitVertexes();*/
            foreach (var figure in figures)
            {
                for(int i = 0; i < baseObject.vertexes.Length; i++)
                {
                    if(
                        (
                        (baseObject.vertexes[i].X >= figure.vertexes[0].X || baseObject.vertexes[i].X >= figure.vertexes[3].X)
                        &&
                        (baseObject.vertexes[i].X <= figure.vertexes[1].X || baseObject.vertexes[i].X <= figure.vertexes[2].X)
                        )
                        &&
                        (
                        (baseObject.vertexes[i].Y >= figure.vertexes[0].Y || baseObject.vertexes[i].Y >= figure.vertexes[3].Y)
                         &&
                        (baseObject.vertexes[i].Y <= figure.vertexes[1].Y || baseObject.vertexes[i].Y <= figure.vertexes[2].Y)
                        )
                        )
                    {
                        return baseObject.vertexes[i];
                    }
                }   
            }
            return new Vector2f(0, 0);
        }

        public void Initialization()
        {
            InitKeys();
            InitObstacles();
        }

        public void Logic()
        {
            puck.MoveObject(DetectCollision(puck, BaseObjects), window);
        }

        public void MakeGraphic()
        {
            foreach (BaseObject figure in BaseObjects)
            {
                window.Draw(figure.shape);
            }
            window.Draw(puck.shape);
        }

        public void Run()
        {
            window = new RenderWindow(new VideoMode(800, 600), "dingdong");
            window.KeyPressed += Window_KeyPressed;
            window.KeyPressed += Player1.Player_KeyPressed;
            window.KeyPressed += Player2.Player_KeyPressed;

            Initialization();
            while (window.IsOpen)
            {
                Logic();
                MakeGraphic();
                window.DispatchEvents();
                window.Display();
                window.Clear();
            }
        }
    }

    internal class Program
    {
        public static void Main(string[] args)
        {
            var game = new Game();
            game.Run();
        }
    }
}
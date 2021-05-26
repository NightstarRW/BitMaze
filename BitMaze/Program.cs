using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//Original idea by: https://www.youtube.com/watch?v=tR30963rDig&ab_channel=%D0%94%D0%BC%D0%B8%D1%82%D1%80%D0%B8%D0%B9%D0%A1%D1%82%D0%BE%D0%BF%D0%BA%D0%B5%D0%B2%D0%B8%D1%87

namespace BitMaze
{
    class Program
    {
        private const int ScreenWidth = 100;
        private const int ScreenHeight = 50;

        private const int MapWidth = 32;
        private const int MapHeight = 32;
        private static readonly StringBuilder Map = new StringBuilder();

        private const double Fov = Math.PI / 3;
        private const double Depth = 16;

        private static double playerX = 2;
        private static double playerY = 2;
        private static double playerA = 0;

        private static readonly char[] Screen = new char[ScreenWidth * ScreenHeight];

        static void Main(string[] args)
        {
            Console.SetWindowSize(ScreenWidth, ScreenHeight);
            Console.SetBufferSize(ScreenWidth, ScreenHeight);
            Console.CursorVisible = false;

            InitMap();

            DateTime dateTimeFrom = DateTime.Now;

            while (true)
            {
                DateTime dateTimeTo = DateTime.Now;
                double elapsedTime = (dateTimeTo - dateTimeFrom).TotalSeconds;
                dateTimeFrom = DateTime.Now;

                if (Console.KeyAvailable)
                {
                    ConsoleKey consoleKey = Console.ReadKey(true).Key;

                    switch (consoleKey)
                    {
                        case ConsoleKey.A:
                            playerA += elapsedTime * 10;
                            break;
                        case ConsoleKey.D:
                            playerA -= elapsedTime * 10;
                            break;
                        case ConsoleKey.W:
                            {
                                playerX += Math.Sin(playerA) * 15 * elapsedTime;
                                playerY += Math.Cos(playerA) * 15 * elapsedTime;

                                if (Map[(int)playerY * MapWidth + (int)playerX] == '#')
                                {
                                    playerX -= Math.Sin(playerA) * 15 * elapsedTime;
                                    playerY -= Math.Cos(playerA) * 15 * elapsedTime;
                                }
                                break;
                            }
                        case ConsoleKey.S:
                            {
                                playerX -= Math.Sin(playerA) * 15 * elapsedTime;
                                playerY -= Math.Cos(playerA) * 15 * elapsedTime;

                                if (Map[(int)playerY * MapWidth + (int)playerX] == '#')
                                {
                                    playerX += Math.Sin(playerA) * 15 * elapsedTime;
                                    playerY += Math.Cos(playerA) * 15 * elapsedTime;
                                }

                                break;
                            }
                    }
                }

                if (Map[(int)playerY * MapWidth + (int)playerX] == 'W')
                {
                    Console.Clear();
                    Console.WriteLine("You win!");
                    Console.Beep();
                    break;
                }

                for (int x = 0; x < ScreenWidth; x++)
                {
                    double rayAngle = playerA + Fov / 2 - x * Fov / ScreenWidth;

                    double rayX = Math.Sin(rayAngle);
                    double rayY = Math.Cos(rayAngle);

                    double distanceToWall = 0;
                    bool hitWall = false;
                    bool isBound = false;

                    while (!hitWall && distanceToWall < Depth)
                    {
                        distanceToWall += 0.1;

                        int testX = (int)(playerX + rayX * distanceToWall);
                        int testY = (int)(playerY + rayY * distanceToWall);
                        if (testX < 0 || (testX >= Depth + playerX) || testY < 0 || (testY >= Depth + playerY))
                        {
                            hitWall = true;
                            distanceToWall = Depth;
                        }
                        else
                        {
                            char testCell = Map[testY * MapWidth + testX];

                            if (testCell == '#')
                            {
                                hitWall = true;

                                var boundsVectorList = new List<(double module, double cos)>();

                                for (int tx = 0; tx < 2; tx++)
                                {
                                    for (int ty = 0; ty < 2; ty++)
                                    {
                                        double vx = testX + tx - playerX;
                                        double vy = testY + ty - playerY;

                                        double vectorModule = Math.Sqrt(vx * vx + vy * vy);
                                        double cosAngle = rayX * vx / vectorModule + rayY * vy / vectorModule;

                                        boundsVectorList.Add((vectorModule, cosAngle));
                                    }
                                }

                                boundsVectorList = boundsVectorList.OrderBy(v => v.module).ToList();

                                double boundAngle = 0.03 / distanceToWall;

                                if (Math.Acos(boundsVectorList[0].cos) < boundAngle ||
                                    Math.Acos(boundsVectorList[1].cos) < boundAngle)
                                    isBound = true;
                            }
                            /* else
                             {
                                 Map[testY * MapWidth + testX] = '*';
                             }*/
                        }
                    }

                    int celling = (int)(ScreenHeight / 2d - ScreenHeight * Fov / distanceToWall);
                    int floor = ScreenHeight - celling;

                    char wallShade;
                    if (isBound)
                        wallShade = '|';
                    else if (distanceToWall <= Depth / 4d)
                        wallShade = '\u2588';
                    else if (distanceToWall < Depth / 3d)
                        wallShade = '\u2593';
                    else if (distanceToWall < Depth / 2d)
                        wallShade = '\u2592';
                    else if (distanceToWall < Depth)
                        wallShade = '\u2591';
                    else
                        wallShade = ' ';

                    for (int y = 0; y < ScreenHeight; y++)
                    {
                        if (y <= celling)
                        {
                            Screen[y * ScreenWidth + x] = ' ';
                        }
                        else if (y > celling && y <= floor)
                        {
                            Screen[y * ScreenWidth + x] = wallShade;
                        }
                        else
                        {
                            char floorShade;

                            double b = 1 - (y - ScreenHeight / 2d) / (ScreenHeight / 2d);

                            if (b < 0.25)
                                floorShade = '#';
                            else if (b < 0.5)
                                floorShade = '=';
                            else if (b < 0.75)
                                floorShade = '-';
                            else if (b < 0.9)
                                floorShade = '.';
                            else
                                floorShade = ' ';

                            Screen[y * ScreenWidth + x] = floorShade;
                        }
                    }
                }

                for (int x = 0; x < MapWidth; x++)
                {
                    for (int y = 0; y < MapHeight; y++)
                    {
                        Screen[y * ScreenWidth + x] = Map[y * MapWidth + x];
                    }
                }

                Screen[(int)playerY * ScreenWidth + (int)playerX] = 'P';

                Console.SetCursorPosition(0, 0);
                Console.Write(Screen);
            }
        }

        private static void InitMap()
        {
            Map.Clear();
            Map.Append("################################");
            Map.Append("#...#.................#........#");
            Map.Append("#...#..#....#...###...#........#");
            Map.Append("#......#....#.........#........#");
            Map.Append("#...#.####..#.........#........#");
            Map.Append("#...........#.#####.########...#");
            Map.Append("#...#...#...#.........#........#");
            Map.Append("#####..........#......#....#...#");
            Map.Append("#.........#....#......#........#");
            Map.Append("###########....#########.......#");
            Map.Append("#.........#...........#........#");
            Map.Append("#.........#......#....#........#");
            Map.Append("#..............................#");
            Map.Append("#......#.....##############....#");
            Map.Append("########.......................#");
            Map.Append("#......#..########....#....#####");
            Map.Append("#.....................#.....#..#");
            Map.Append("#......#..............#.....#..#");
            Map.Append("#...######################..#..#");
            Map.Append("#......#................#...#..#");
            Map.Append("#......#...........#....#......#");
            Map.Append("#....########.........######...#");
            Map.Append("#......#..............#........#");
            Map.Append("#..........############....#####");
            Map.Append("#########..#...................#");
            Map.Append("#..........#...#...#......#....#");
            Map.Append("#..........#...#..........#....#");
            Map.Append("#...########...#.....###########");
            Map.Append("#.......#......#..#..#...#.....#");
            Map.Append("#.......#......#..#.........W..#");
            Map.Append("#..............#..#......#.....#");
            Map.Append("################################");
        }
    }
}

﻿using InCollege.Core.Data.Base;
using System;
using System.Threading;

namespace InCollege.Server
{
    class Program
    {
        public static void Main()
        {
            foreach (var current in Enum.GetValues(typeof(ConsoleColor)))
            {
                PrintLogo((ConsoleColor)current);
                Thread.Sleep(50);
                Console.CursorLeft = Console.CursorTop = 0;
            }
            PrintLogo(ConsoleColor.Green);

            Console.WriteLine("Welcome to InCollege.Server! Don't hesitate, open http://localhost/ to see what we got!");
            Console.WriteLine("Made by [CYBOR] = Muhametshin R.A.");

            Console.WriteLine($"Initializing SQLite DB(thanks Frank A. Krueger and other 53 team members for sqlite-net engine) in \n{CommonVariables.DBLocation}\n");
            DBHolderSQL.Init(CommonVariables.DBLocation);
            Console.WriteLine($"Initializing uHttpSharp server engine(thanks Elad Zelingher and other 6 team members for uHttpSharp engine).");
            InCollegeServer.Start();

            while (true) Thread.Sleep(1000);
        }

        static void PrintLogo(ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine("######################################################");
            Console.WriteLine("#                                                    #");
            Console.WriteLine("#  ##  ##  #     ###  ###  #    #    ###  ####  ###  #");
            Console.WriteLine("#  ##  ##  #     #    # #  #    #    #    #     #    #");
            Console.WriteLine("#  ##  # # #     #    # #  #    #    ###  # ##  ###  #");
            Console.WriteLine("#  ##  #  ##     #    # #  #    #    #    #  #  #    #");
            Console.WriteLine("#  ##  #  ##     ###  ###  ###  ###  ###  ####  ###  #");
            Console.WriteLine("#                                                    #");
            Console.WriteLine("######################################################");
        }
    }
}

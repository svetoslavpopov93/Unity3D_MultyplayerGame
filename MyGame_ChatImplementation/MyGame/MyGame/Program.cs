using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Continental;
using Npgsql;
using System.IO;
using System.Diagnostics;
using System.Configuration;
using Continental.Shared;
using System.Data;
using Continental.Games;
using GCCEmulator;

namespace Continental
{
	class Program
	{
		public static Continental.Games.GameCore core;
		private static float deltatime;
		private static Stopwatch Timer;
		static void Main(string[] args)
		{
			core = new Continental.Games.GameCore();
			Emulator emulator = new Emulator();
			emulator.InitGameCore(core, ConfigurationManager.AppSettings["DBConnectionString"], new Version(ConfigurationManager.AppSettings["LastCompatibleClientsVersion"]),
				int.Parse(ConfigurationManager.AppSettings["ServerPort"]));
			Timer = new Stopwatch();
			Timer.Start();
			core.OnStart();
			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
			Console.WriteLine("Server started, listening at 0.0.0.0:{0} (CTRL+C to stop)", ConfigurationManager.AppSettings["ServerPort"]);

			//main game loop
			while (true)
			{
				try
				{
					sw.Start();
					NetLightMessage[] messages = emulator.networking.GetMessagesFromPlayers();
					deltatime = (float)Timer.Elapsed.TotalSeconds;
					Timer.Restart();
					core.OnFrame(deltatime);
					foreach (NetLightMessage msg in messages)
					{
						if (msg.context == CommandCodes.KEEP_CONNECTION_ALIVE) // emulator ignores this command
						{
							Console.WriteLine("KEEP_CONNECTION_ALIVE received");
							continue;
						}
						core.OnCommand(msg.connectionId, msg.context, msg.msg);
					}
					sw.Stop();
					if(sw.Elapsed.TotalMilliseconds>3000)
					{
						core.OnStop();
					}
					double msToSleep = 1f / 60f * 1000f - sw.Elapsed.TotalMilliseconds;
					sw.Reset();
					if (msToSleep > 1)
					{
						System.Threading.Thread.Sleep((int)msToSleep);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
					System.Threading.Thread.Sleep(20);
				}
			}
		}
	}
}

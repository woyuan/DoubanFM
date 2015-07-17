/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using DoubanFM.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Deployment.Application;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace DoubanFM
{
	/// <summary>
	/// App.xaml 的交互逻辑
	/// </summary>
	public partial class App : Application
	{
		private Mutex mutex;
		private static object exceptionObject = null;

		/// <summary>
		/// 启动时的时间
		/// </summary>
		public static DateTime StartTime { get; set; }

		/// <summary>
		/// 是否已启动
		/// </summary>
		public static bool Started { get; set; }

	    static App()
	    {
	        Started = false;

	        if (ApplicationDeployment.IsNetworkDeployed)
	        {
	            AppVersion = ApplicationDeployment.CurrentDeployment.CurrentVersion;
	        }
	        else
	            {
                AppVersion = typeof(App).Assembly.GetName().Version;
	        }
	    }

	    public App()
		{
			//只允许运行一个实例
			bool createdNew = false;
			mutex = new Mutex(true, "{DBFE3F28-BA77-4FF6-9EBF-4FED90151A3E}", out createdNew);
			if (!createdNew)
			{
				Channel channel = Channel.FromCommandLineArgs(System.Environment.GetCommandLineArgs().ToList());
				try
				{
					if (channel != null)
					{
						WriteStringToMappedFile(channel.ToCommandLineArgs());
					}
					else
					{
						WriteStringToMappedFile("-show");
					}
				}
				catch { }
				Debug.WriteLine("检测到已有一个豆瓣电台在运行，程序将关闭");
				Shutdown(0);
				return;
			}

			//设置调试输出
			Debug.AutoFlush = true;
			Debug.Listeners.Add(new TextWriterTraceListener("DoubanFM.log"));

			Debug.WriteLine(string.Empty);
			Debug.WriteLine("**********************************************************************");
			Debug.WriteLine("豆瓣电台启动时间：" + App.GetPreciseTime(DateTime.Now));
			Debug.WriteLine("**********************************************************************");
			Debug.WriteLine(string.Empty);

			Exit += new ExitEventHandler((sender, e) =>
			{
				if (mutex != null)
				{
					mutex.Close();
					mutex = null;
				}
				Debug.WriteLine(App.GetPreciseTime(DateTime.Now) + " 程序结束，返回代码为" + e.ApplicationExitCode);
			});

            InitializeComponent();

            var player = FindResource("Player") as Player;
            if (player.Settings.CultureInfo != null)
            {
                Thread.CurrentThread.CurrentCulture = player.Settings.CultureInfo;
                Thread.CurrentThread.CurrentUICulture = player.Settings.CultureInfo;
            }

            //System.Windows.Media.RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;

			/* 这句话可以使Global User Interface这个默认的组合字体按当前系统的区域信息选择合适的字形。
			 * 只对FrameworkElement有效。对于FlowDocument，由于是从FrameworkContentElement继承，
			 * 而且FrameworkContentElement.LanguageProperty.OverrideMetadata()无法再次执行，
			 * 目前我知道的最好的办法是在使用了FlowDocument的XAML的根元素上加上xml:lang="zh-CN"，
			 * 这样就能强制Global User Interface在FlowDocument上使用大陆的字形。
			 * */
			FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.Name)));
		}

	    protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
	    {
            SaveSettings();
	        base.OnSessionEnding(e);
	    }

	    /// <summary>
		/// 获取时间的一个精确表示
		/// </summary>
		/// <param name="time">时间</param>
		/// <returns>一个精确表示</returns>
		public static string GetPreciseTime(DateTime time)
		{
			return time.ToString() + " " + time.Millisecond + "ms";
		}

		/// <summary>
		/// 内存映射文件的文件名
		/// </summary>
		public static string _mappedFileName = "{04EFCEB4-F10A-403D-9824-1E685C4B7961}";

		/// <summary>
		/// 将字符串写入内存映射文件
		/// </summary>
		internal static void WriteStringToMappedFile(string content)
		{
			using (MemoryMappedFile mappedFile = MemoryMappedFile.OpenExisting(_mappedFileName))
			{
				using (Stream stream = mappedFile.CreateViewStream())
				{
					using (StreamWriter writer = new StreamWriter(stream))
					{
						writer.WriteLine(content);
					}
				}
			}
		}

		/// <summary>
		/// 从内存映射文件中读入字符串
		/// </summary>
		internal static string ReadStringFromMappedFile()
		{
			using (MemoryMappedFile mappedFile = MemoryMappedFile.OpenExisting(_mappedFileName))
			{
				using (Stream stream = mappedFile.CreateViewStream())
				{
					using (StreamReader reader = new StreamReader(stream))
					{
						return reader.ReadLine();
					}
				}
			}
		}

		/// <summary>
		/// 清除内存映射文件
		/// </summary>
		internal static void ClearMappedFile()
		{
			using (MemoryMappedFile mappedFile = MemoryMappedFile.OpenExisting(_mappedFileName))
			{
				using (Stream stream = mappedFile.CreateViewStream())
				{
					using (StreamWriter writer = new StreamWriter(stream))
					{
						writer.WriteLine("");
					}
				}
			}
		}

	    /// <summary>
		/// 保存设置
		/// </summary>
		/// <param name="mainWindow">软件的主窗口</param>
		public void SaveSettings(DoubanFMWindow mainWindow = null)
		{
			if (mainWindow == null)
			{
				mainWindow = MainWindow as DoubanFMWindow;
			}
			if (mainWindow == null) return;

			var player = FindResource("Player") as Player;
			if (player != null) player.SaveSettings();
			if (mainWindow._lyricsSetting != null) mainWindow._lyricsSetting.Save();
		}

	    public static Version AppVersion { get; private set; }
	}
}
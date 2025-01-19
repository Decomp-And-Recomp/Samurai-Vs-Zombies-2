using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Glu.Diagnostics.Internal;
using UnityEngine;

public static class Logging
{
	public struct LogRecord
	{
		public string Name;

		public int Level;

		public string Format;

		public object[] Args;

		public UnityEngine.Object Context;

		public Dictionary<string, object> Extra;

		public void SetExtraValue(string name, object val)
		{
			if (Extra == null)
			{
				Extra = new Dictionary<string, object>();
			}
			Extra[name] = val;
		}
	}

	public interface ILogger
	{
		bool Propagate { get; set; }

		ILogger Parent { get; set; }

		string Name { get; }

		void Log(int level, string format, params object[] args);

		void Debug(string format, params object[] args);

		void Info(string format, params object[] args);

		void Warn(string format, params object[] args);

		void Error(string format, params object[] args);

		void Critical(string format, params object[] args);

		void Log(UnityEngine.Object context, int level, string format, params object[] args);

		void Debug(UnityEngine.Object context, string format, params object[] args);

		void Info(UnityEngine.Object context, string format, params object[] args);

		void Warn(UnityEngine.Object context, string format, params object[] args);

		void Error(UnityEngine.Object context, string format, params object[] args);

		void Critical(UnityEngine.Object context, string format, params object[] args);

		bool IsEnabledFor(int level);

		void SetLevel(int level);

		int GetLevel();

		int GetEffectiveLevel();

		void AddFilter(IFilter filter);

		void RemoveFilter(IFilter filter);

		bool Filter(ref LogRecord record);

		int GetFilterCount();

		IFilter GetFilter(int i);

		void AddHandler(IHandler handler);

		void RemoveHandler(IHandler handler);

		int GetHandlerCount();

		IHandler GetHandler(int i);

		void CallHandlers(ref LogRecord record);
	}

	public interface IFilter
	{
		bool Filter(ref LogRecord record);
	}

	public interface IHandler
	{
		void Handle(ref LogRecord record);

		void Emit(ref LogRecord record);

		void SetLevel(int level);

		int GetLevel();

		void AddFilter(IFilter filter);

		void RemoveFilter(IFilter filter);

		int GetFilterCount();

		IFilter GetFilter(int i);

		void SetFormatter(IFormatter formatter);

		void Close();
	}

	public interface IFormatter
	{
		string Format(ref LogRecord record);
	}

	public sealed class Logger : ILogger
	{
		private string name;

		private int level;

		private Filterer filters;

		private List<IHandler> handlers;

		private ILogger parent;

		private bool propagate;

		public bool Propagate
		{
			get
			{
				return propagate;
			}
			set
			{
				propagate = value;
			}
		}

		public ILogger Parent
		{
			get
			{
				return parent;
			}
			set
			{
				parent = value;
			}
		}

		public string Name
		{
			get
			{
				return name;
			}
		}

		internal Logger(string name)
		{
			this.name = name ?? string.Empty;
			filters = new Filterer();
			handlers = new List<IHandler>();
			propagate = true;
		}

		public void Log(int level, string format, params object[] args)
		{
			if (IsEnabledFor(level))
			{
				LogImpl(null, level, format, args);
			}
		}

		public void Debug(string format, params object[] args)
		{
			Log(10, format, args);
		}

		public void Info(string format, params object[] args)
		{
			Log(20, format, args);
		}

		public void Warn(string format, params object[] args)
		{
			Log(30, format, args);
		}

		public void Error(string format, params object[] args)
		{
			Log(40, format, args);
		}

		public void Critical(string format, params object[] args)
		{
			Log(50, format, args);
		}

		public void Log(UnityEngine.Object context, int level, string format, params object[] args)
		{
			if (IsEnabledFor(level))
			{
				LogImpl(context, level, format, args);
			}
		}

		public void Debug(UnityEngine.Object context, string format, params object[] args)
		{
			Log(context, 10, format, args);
		}

		public void Info(UnityEngine.Object context, string format, params object[] args)
		{
			Log(context, 20, format, args);
		}

		public void Warn(UnityEngine.Object context, string format, params object[] args)
		{
			Log(context, 30, format, args);
		}

		public void Error(UnityEngine.Object context, string format, params object[] args)
		{
			Log(context, 40, format, args);
		}

		public void Critical(UnityEngine.Object context, string format, params object[] args)
		{
			Log(context, 50, format, args);
		}

		public bool IsEnabledFor(int level)
		{
			if (disable >= level)
			{
				return false;
			}
			return level >= GetEffectiveLevel();
		}

		public void SetLevel(int level)
		{
			this.level = level;
		}

		public int GetLevel()
		{
			return level;
		}

		public int GetEffectiveLevel()
		{
			if (level != 0)
			{
				return level;
			}
			return (parent != null) ? parent.GetEffectiveLevel() : 0;
		}

		public void AddFilter(IFilter filter)
		{
			filters.AddFilter(filter);
		}

		public void RemoveFilter(IFilter filter)
		{
			filters.RemoveFilter(filter);
		}

		public bool Filter(ref LogRecord record)
		{
			return filters.Filter(ref record);
		}

		public int GetFilterCount()
		{
			return filters.Count;
		}

		public IFilter GetFilter(int i)
		{
			return filters.GetFilter(i);
		}

		public void AddHandler(IHandler handler)
		{
			if (handler != null && !handlers.Contains(handler))
			{
				handlers.Add(handler);
			}
		}

		public void RemoveHandler(IHandler handler)
		{
			handlers.Remove(handler);
		}

		public int GetHandlerCount()
		{
			return handlers.Count;
		}

		public IHandler GetHandler(int i)
		{
			return handlers[i];
		}

		public void CallHandlers(ref LogRecord record)
		{
			int count = handlers.Count;
			for (int i = 0; i < count; i++)
			{
				handlers[i].Handle(ref record);
			}
			if (propagate && parent != null)
			{
				parent.CallHandlers(ref record);
			}
		}

		private void LogImpl(UnityEngine.Object context, int level, string format, params object[] args)
		{
			LogRecord logRecord = default(LogRecord);
			logRecord.Name = name;
			logRecord.Level = level;
			logRecord.Format = format;
			logRecord.Args = args;
			logRecord.Context = context;
			LogRecord record = logRecord;
			if (Filter(ref record))
			{
				CallHandlers(ref record);
			}
		}
	}

	public abstract class HandlerBase : IHandler
	{
		private int level;

		private Filterer filters;

		private IFormatter formatter;

		protected HandlerBase()
		{
			filters = new Filterer();
		}

		public void Handle(ref LogRecord record)
		{
			if (record.Level >= level && Filter(ref record))
			{
				Emit(ref record);
			}
		}

		public abstract void Emit(ref LogRecord record);

		public virtual void Close()
		{
		}

		public void SetLevel(int level)
		{
			this.level = level;
		}

		public int GetLevel()
		{
			return level;
		}

		public void SetFormatter(IFormatter formatter)
		{
			this.formatter = formatter;
		}

		public void AddFilter(IFilter filter)
		{
			filters.AddFilter(filter);
		}

		public void RemoveFilter(IFilter filter)
		{
			filters.RemoveFilter(filter);
		}

		public bool Filter(ref LogRecord record)
		{
			return filters.Filter(ref record);
		}

		public int GetFilterCount()
		{
			return filters.Count;
		}

		public IFilter GetFilter(int i)
		{
			return filters.GetFilter(i);
		}

		public string Format(ref LogRecord record)
		{
			IFormatter formatter = this.formatter ?? defaultFormatter;
			return formatter.Format(ref record);
		}
	}

	public sealed class DebugLogHandler : HandlerBase
	{
		public override void Emit(ref LogRecord record)
		{
			string text = Format(ref record);
			RuntimePlatform platform = Application.platform;
			if (platform == RuntimePlatform.IPhonePlayer)
			{
				NativeAPI.Glu_Console_WriteLine(text);
				return;
			}
			try
			{
				UnityLogLogger.Lock();
				UnityEngine.Object context = record.Context;
				if (!object.ReferenceEquals(context, null))
				{
					if (record.Level < 30)
					{
						UnityEngine.Debug.Log(text, context);
					}
					else if (record.Level < 40)
					{
						UnityEngine.Debug.LogWarning(text, context);
					}
					else
					{
						UnityEngine.Debug.LogError(text, context);
					}
				}
				else if (record.Level < 30)
				{
					UnityEngine.Debug.Log(text);
				}
				else if (record.Level < 40)
				{
					UnityEngine.Debug.LogWarning(text);
				}
				else
				{
					UnityEngine.Debug.LogError(text);
				}
			}
			finally
			{
				UnityLogLogger.Unlock();
			}
		}
	}

	public sealed class FileHandler : HandlerBase, IDisposable
	{
		private StreamWriter stream;

		private bool disposed;

		public bool AutoFlush
		{
			get
			{
				CheckDisposed();
				return stream.AutoFlush;
			}
			set
			{
				CheckDisposed();
				stream.AutoFlush = value;
			}
		}

		public FileHandler(string path, FileMode mode)
		{
			FileStream fileStream = null;
			try
			{
				fileStream = File.Open(path, mode, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
				stream = new StreamWriter(fileStream, new UTF8Encoding(false));
				stream.AutoFlush = true;
			}
			catch (Exception)
			{
				if (stream != null)
				{
					stream.Close();
				}
				else if (fileStream != null)
				{
					fileStream.Close();
				}
				throw;
			}
		}

		public override void Emit(ref LogRecord record)
		{
			CheckDisposed();
			string value = Format(ref record);
			stream.WriteLine(value);
		}

		public override void Close()
		{
			Dispose(true);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					stream.Dispose();
					stream = null;
				}
				disposed = true;
			}
		}

		private void CheckDisposed()
		{
			if (disposed)
			{
				throw new ObjectDisposedException("FileHandler");
			}
		}
	}

	public sealed class SimpleFormatter : IFormatter
	{
		private string messageFormat;

		public string MessageFormat
		{
			get
			{
				return messageFormat;
			}
			set
			{
				messageFormat = value;
			}
		}

		public SimpleFormatter()
		{
			messageFormat = "{1}:{2}:{0}";
		}

		public string Format(ref LogRecord record)
		{
			string arg = FormatMessage(record.Format, record.Args);
			string levelName = GetLevelName(record.Level);
			string arg2 = ((!string.IsNullOrEmpty(record.Name)) ? record.Name : "root");
			return string.Format(messageFormat, arg, levelName, arg2);
		}

		public static string FormatMessage(string format, params object[] args)
		{
			if (!args.Any((object obj) => obj is byte[]))
			{
				return (args.Length <= 0) ? format : string.Format(format, args);
			}
			int num = args.Length;
			object[] array = new object[num];
			for (int i = 0; i < num; i++)
			{
				object obj2 = args[i];
				byte[] array2 = obj2 as byte[];
				array[i] = ((array2 == null) ? obj2 : HexPPrinter.ByteArrayToString(array2));
			}
			return string.Format(format, array);
		}
	}

	public sealed class NameFilter : IFilter
	{
		private string[] filters;

		public NameFilter(params string[] filters)
		{
			this.filters = (string[])filters.Clone();
		}

		public bool Filter(ref LogRecord record)
		{
			string name = record.Name;
			int num = filters.Length;
			for (int i = 0; i < num; i++)
			{
				string text = filters[i];
				if (name.StartsWith(text))
				{
					int length = text.Length;
					if (name.Length == length)
					{
						return true;
					}
					if (length == 0 || name[length] == '.')
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	private sealed class Filterer
	{
		private List<IFilter> filters;

		public int Count
		{
			get
			{
				return filters.Count;
			}
		}

		public Filterer()
		{
			filters = new List<IFilter>();
		}

		public void AddFilter(IFilter filter)
		{
			if (filter != null && !filters.Contains(filter))
			{
				filters.Add(filter);
			}
		}

		public void RemoveFilter(IFilter filter)
		{
			filters.Remove(filter);
		}

		public bool Filter(ref LogRecord record)
		{
			int count = filters.Count;
			for (int i = 0; i < count; i++)
			{
				if (!filters[i].Filter(ref record))
				{
					return false;
				}
			}
			return true;
		}

		public IFilter GetFilter(int i)
		{
			return filters[i];
		}
	}

	[Serializable]
	public class SerializationException : Exception
	{
		public SerializationException()
		{
		}

		public SerializationException(string message)
			: base(message)
		{
		}

		public SerializationException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected SerializationException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	public class RuntimeConfig
	{
		public struct Logger
		{
			public string Name;

			public int Level;

			public bool Propagate;
		}

		[XmlRoot("Logging")]
		public struct XmlData
		{
			[XmlAttribute]
			public int Version;

			[XmlArrayItem("Logger")]
			[XmlArray("Loggers")]
			public XmlLogger[] Loggers;

			public int Disabled;

			public bool DebugLogHandlerEnabled;

			public bool FileHandlerEnabled;

			public bool CaptureUnityLog;
		}

		public struct XmlLogger
		{
			public string Name;

			public int Level;

			public bool Propagate;
		}

		private List<Logger> loggers;

		private int disabled;

		private bool debugLogHandlerEnabled;

		private bool fileHandlerEnabled;

		private bool captureUnityLog;

		public int LoggerCount
		{
			get
			{
				return loggers.Count;
			}
		}

		public int Disabled
		{
			get
			{
				return disabled;
			}
			set
			{
				disabled = value;
			}
		}

		public bool CaptureUnityLog
		{
			get
			{
				return captureUnityLog;
			}
			set
			{
				captureUnityLog = value;
			}
		}

		public bool DebugLogHandlerEnabled
		{
			get
			{
				return debugLogHandlerEnabled;
			}
			set
			{
				debugLogHandlerEnabled = value;
			}
		}

		public bool FileHandlerEnabled
		{
			get
			{
				return fileHandlerEnabled;
			}
			set
			{
				fileHandlerEnabled = value;
			}
		}

		public RuntimeConfig()
		{
			loggers = new List<Logger>();
			Reset();
		}

		public void Reset()
		{
			loggers.Clear();
			disabled = 0;
			captureUnityLog = false;
			debugLogHandlerEnabled = true;
			fileHandlerEnabled = true;
		}

		public Logger GetLogger(int loggerIndex)
		{
			return loggers[loggerIndex];
		}

		public void AddLogger(Logger logger)
		{
			loggers.Add(logger);
		}

		public void Remove(int i)
		{
			loggers.RemoveAt(i);
		}

		public void Load(Stream stream)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(XmlData));
			XmlData data = (XmlData)xmlSerializer.Deserialize(stream);
			Validate(ref data);
			List<Logger> list = new List<Logger>();
			XmlLogger[] array = data.Loggers;
			if (array != null)
			{
				int num = array.Length;
				for (int i = 0; i < num; i++)
				{
					XmlLogger xmlLogger = array[i];
					Logger logger = default(Logger);
					logger.Name = xmlLogger.Name;
					logger.Level = xmlLogger.Level;
					logger.Propagate = xmlLogger.Propagate;
					Logger item = logger;
					list.Add(item);
				}
			}
			loggers = list;
			disabled = data.Disabled;
			debugLogHandlerEnabled = data.DebugLogHandlerEnabled;
			fileHandlerEnabled = data.FileHandlerEnabled;
			captureUnityLog = data.CaptureUnityLog;
		}

		public void Save(Stream stream)
		{
			XmlData data = default(XmlData);
			data.Version = 1;
			int loggerCount = LoggerCount;
			data.Loggers = new XmlLogger[loggerCount];
			for (int i = 0; i < loggerCount; i++)
			{
				Logger logger = GetLogger(i);
				data.Loggers[i] = new XmlLogger
				{
					Name = logger.Name,
					Level = logger.Level,
					Propagate = logger.Propagate
				};
			}
			data.Disabled = Disabled;
			data.DebugLogHandlerEnabled = DebugLogHandlerEnabled;
			data.FileHandlerEnabled = FileHandlerEnabled;
			data.CaptureUnityLog = CaptureUnityLog;
			Validate(ref data);
			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
			xmlWriterSettings.Encoding = Encoding.UTF8;
			xmlWriterSettings.Indent = true;
			using (XmlWriter xmlWriter = XmlWriter.Create(stream, xmlWriterSettings))
			{
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(XmlData));
				xmlSerializer.Serialize(xmlWriter, data);
			}
		}

		private void Validate(ref XmlData data)
		{
			if (data.Version != 1)
			{
				throw new SerializationException(string.Format("Unsupported config version {0}", data.Version));
			}
		}
	}

	public sealed class UnityLogLogger : LoggerSingleton<UnityLogLogger>
	{
		private static int lockCount;

		public UnityLogLogger()
		{
			LoggerSingleton<UnityLogLogger>.SetLoggerName("UnityLog");
			Initialize();
		}

		public static void RegisterLogCallback()
		{
			Application.RegisterLogCallback(LogCallback);
		}

		public static void Lock()
		{
			lockCount++;
		}

		public static void Unlock()
		{
			if (lockCount > 0)
			{
				lockCount--;
			}
		}

		public static void LogCallback(string condition, string stackTrace, LogType type)
		{
			if (lockCount <= 0)
			{
				switch (type)
				{
				case LogType.Log:
					break;
				case LogType.Warning:
					break;
				case LogType.Assert:
					break;
				case LogType.Exception:
					break;
				case LogType.Error:
					break;
				}
			}
		}

		private static void Initialize()
		{
			lockCount = 0;
		}
	}

	public const int NOTSET = 0;

	public const int DEBUG = 10;

	public const int INFO = 20;

	public const int WARN = 30;

	public const int ERROR = 40;

	public const int CRITICAL = 50;

	private static bool initialized;

	private static ILogger root;

	private static Dictionary<string, ILogger> loggers;

	private static int disable;

	private static IFormatter defaultFormatter;

	private static List<Action> destructors;

	private static bool registeredUnityLogCallback;

	private static string logFilePath;

	private static RuntimeConfig config;

	public static IFormatter DefaultFormatter
	{
		get
		{
			return defaultFormatter;
		}
		set
		{
			defaultFormatter = value;
		}
	}

	public static string LogFilePath
	{
		get
		{
			return logFilePath;
		}
	}

	private static string DefaultConfigResource
	{
		get
		{
			if (GeneralConfig.IsLive && !UnityEngine.Debug.isDebugBuild)
			{
				return "Conf/logging_live";
			}
			return "Conf/logging";
		}
	}

	public static string DefaultConfigPath
	{
		get
		{
			string text = Path.Combine("Assets/Game/Resources", DefaultConfigResource) + ".xml";
			return text.Replace('\\', '/');
		}
	}

	public static RuntimeConfig Config
	{
		get
		{
			if (config == null)
			{
				LoadConfig();
			}
			return config;
		}
	}

	public static ILogger GetLogger(string name)
	{
		Initialize();
		if (string.IsNullOrEmpty(name))
		{
			return root;
		}
		ILogger value;
		if (loggers.TryGetValue(name, out value))
		{
			return value;
		}
		value = new Logger(name);
		int num = name.LastIndexOf('.');
		if (num > 0)
		{
			string name2 = name.Substring(0, num);
			value.Parent = GetLogger(name2);
		}
		else
		{
			value.Parent = root;
		}
		loggers[name] = value;
		return value;
	}

	public static void Log(int level, string format, params object[] args)
	{
		Initialize();
		root.Log(level, format, args);
	}

	public static void Debug(string format, params object[] args)
	{
		Initialize();
		root.Debug(format, args);
	}

	public static void Info(string format, params object[] args)
	{
		Initialize();
		root.Info(format, args);
	}

	public static void Warn(string format, params object[] args)
	{
		Initialize();
		root.Warn(format, args);
	}

	public static void Error(string format, params object[] args)
	{
		Initialize();
		root.Error(format, args);
	}

	public static void Critical(string format, params object[] args)
	{
		Initialize();
		root.Critical(format, args);
	}

	public static void Log(UnityEngine.Object context, int level, string format, params object[] args)
	{
		Initialize();
		root.Log(context, level, format, args);
	}

	public static void Debug(UnityEngine.Object context, string format, params object[] args)
	{
		Initialize();
		root.Debug(context, format, args);
	}

	public static void Info(UnityEngine.Object context, string format, params object[] args)
	{
		Initialize();
		root.Info(context, format, args);
	}

	public static void Warn(UnityEngine.Object context, string format, params object[] args)
	{
		Initialize();
		root.Warn(context, format, args);
	}

	public static void Error(UnityEngine.Object context, string format, params object[] args)
	{
		Initialize();
		root.Error(context, format, args);
	}

	public static void Critical(UnityEngine.Object context, string format, params object[] args)
	{
		Initialize();
		root.Critical(context, format, args);
	}

	public static int GetDisableLevel()
	{
		Initialize();
		return disable;
	}

	public static void Disable(int level)
	{
		Initialize();
		disable = level;
	}

	public static IEnumerable<ILogger> GetLoggers()
	{
		Initialize();
		return new ILogger[1] { root }.Concat(loggers.Values);
	}

	public static void Initialize()
	{
		if (initialized)
		{
			return;
		}
		root = null;
		loggers = null;
		disable = 0;
		defaultFormatter = null;
		destructors = null;
		registeredUnityLogCallback = false;
		try
		{
			destructors = new List<Action>();
			defaultFormatter = new SimpleFormatter();
			loggers = new Dictionary<string, ILogger>();
			root = new Logger(string.Empty);
			root.SetLevel(30);
			initialized = true;
			ApplyConfig();
		}
		catch (Exception)
		{
			DestroyImpl();
			throw;
		}
	}

	public static void Destroy()
	{
		if (initialized)
		{
			DestroyImpl();
		}
	}

	public static string GetLevelName(int level)
	{
		switch (level)
		{
		case 0:
			return "NOTSET";
		case 10:
			return "DEBUG";
		case 20:
			return "INFO";
		case 30:
			return "WARN";
		case 40:
			return "ERROR";
		case 50:
			return "CRITICAL";
		default:
			return level.ToString();
		}
	}

	public static void RegisterDestructor(Action dtor)
	{
		if (dtor != null)
		{
			destructors.Add(dtor);
		}
	}

	private static void DestroyLogger(ILogger logger)
	{
		logger.Parent = null;
		logger.SetLevel(0);
		int filterCount = logger.GetFilterCount();
		for (int num = filterCount - 1; num >= 0; num--)
		{
			logger.RemoveFilter(logger.GetFilter(num));
		}
		int handlerCount = logger.GetHandlerCount();
		for (int num2 = handlerCount - 1; num2 >= 0; num2--)
		{
			IHandler handler = logger.GetHandler(num2);
			handler.Close();
			logger.RemoveHandler(handler);
		}
	}

	private static void DestroyImpl()
	{
		if (registeredUnityLogCallback)
		{
			Application.RegisterLogCallback(null);
			registeredUnityLogCallback = false;
		}
		if (destructors != null)
		{
			destructors.ForEach(delegate(Action d)
			{
				d();
			});
			destructors = null;
		}
		if (loggers != null)
		{
			foreach (ILogger value in loggers.Values)
			{
				DestroyLogger(value);
			}
			loggers = null;
		}
		if (root != null)
		{
			DestroyLogger(root);
			root = null;
		}
		ReplaceConfig(null);
		initialized = false;
		disable = 0;
		defaultFormatter = null;
	}

	private static void ApplyConfig()
	{
		IHandler handler = null;
		try
		{
			RuntimeConfig runtimeConfig = Config;
			disable = runtimeConfig.Disabled;
			int loggerCount = runtimeConfig.LoggerCount;
			for (int i = 0; i < loggerCount; i++)
			{
				RuntimeConfig.Logger logger = runtimeConfig.GetLogger(i);
				ILogger logger2 = GetLogger(logger.Name);
				logger2.SetLevel(logger.Level);
				logger2.Propagate = logger.Propagate;
			}
			if (runtimeConfig.DebugLogHandlerEnabled)
			{
				handler = new DebugLogHandler();
				root.AddHandler(handler);
				handler = null;
			}
			bool flag = false;
			try
			{
				flag = Application.isPlaying;
			}
			catch (ArgumentException)
			{
				return;
			}
			if (flag && runtimeConfig.FileHandlerEnabled)
			{
				string path = Application.temporaryCachePath;
				if (!UnityEngine.Debug.isDebugBuild)
				{
					path = AJavaTools.GameInfo.GetFilesPath();
				}
				string path2 = string.Format("Log_{0}_{1}.txt", DateTime.Now.ToShortDateString().Replace("/", "."), DateTime.Now.ToLongTimeString().Replace(":", "."));
				logFilePath = Path.Combine(path, path2);
				handler = new FileHandler(logFilePath, FileMode.Create);
				root.AddHandler(handler);
				handler = null;
			}
			if (runtimeConfig.CaptureUnityLog && !Application.isEditor)
			{
				UnityLogLogger.RegisterLogCallback();
				registeredUnityLogCallback = true;
			}
		}
		catch (Exception)
		{
			if (handler != null)
			{
				handler.Close();
			}
			throw;
		}
	}

	public static void ReplaceConfig(RuntimeConfig newConfig)
	{
		config = newConfig;
	}

	public static void LoadConfig()
	{
		config = new RuntimeConfig();
		try
		{
			using (Stream stream = GetDefaultConfigStream())
			{
				if (stream != null)
				{
					config.Load(stream);
				}
			}
		}
		catch (XmlException)
		{
		}
	}

	private static Stream GetDefaultConfigStream()
	{
		TextAsset textAsset = (TextAsset)Resources.Load(DefaultConfigResource, typeof(TextAsset));
		if (textAsset != null)
		{
			return new MemoryStream(textAsset.bytes);
		}
		return null;
	}
}

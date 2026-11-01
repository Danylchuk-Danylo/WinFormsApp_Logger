using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace WinFormsApp_Logger
{
    public class Logger
    {
        private static Logger _instance;
        private static readonly object _instanceLock = new object();
        private static readonly object _fileLock = new object();

        private readonly Queue<string> _logQueue;
        private LoggerConfig _config;
        private bool _isWriting;

        // Приватный конструктор для реализации Singleton
        private Logger()
        {
            _logQueue = new Queue<string>();
            LoadConfiguration();
        }

        public static Logger Instance
        {
            get
            {
                lock (_instanceLock)
                {
                    if (_instance == null)
                    {
                        _instance = new Logger();
                    }
                    return _instance;
                }
            }
        }

        private void LoadConfiguration()
        {
            string configPath = "logger_settings.json";

            if (File.Exists(configPath))
            {
                string json = File.ReadAllText(configPath);
                _config = JsonSerializer.Deserialize<LoggerConfig>(json);
            }
            else
            {
                _config = new LoggerConfig();
                string defaultJson = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configPath, defaultJson);
            }

            // Очищаем файл при запуске, если настройка AppendToFile выключена
            if (!_config.AppendToFile && File.Exists(_config.FileName))
            {
                File.WriteAllText(_config.FileName, string.Empty);
            }
        }

        public void LogData(LogLevel level, string message)
        {
            // Форматируем строку как указано в задании
            string timestamp = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff");
            string logEntry = $"{timestamp} [{level}] : {message}";

            lock (_instanceLock)
            {
                _logQueue.Enqueue(logEntry);

                // Если очередь достигла нужного размера (порции) и мы еще не пишем файл
                if (_logQueue.Count >= _config.QueueNumber && !_isWriting)
                {
                    _isWriting = true;
                    // Запускаем запись в отдельном потоке (чтобы не висла форма)
                    Task.Run(() => FlushQueueToFile());
                }
            }
        }

        private void FlushQueueToFile()
        {
            List<string> linesToWrite = new List<string>();

            lock (_instanceLock)
            {
                while (_logQueue.Count > 0)
                {
                    linesToWrite.Add(_logQueue.Dequeue());
                }
            }

            if (linesToWrite.Count > 0)
            {
                // Блокируем доступ к файлу, чтобы другие потоки его не сломали
                lock (_fileLock)
                {
                    File.AppendAllLines(_config.FileName, linesToWrite);
                }
            }

            lock (_instanceLock)
            {
                _isWriting = false;
                // Проверяем, не накопились ли новые логи, пока мы записывали старые
                if (_logQueue.Count >= _config.QueueNumber)
                {
                    _isWriting = true;
                    Task.Run(() => FlushQueueToFile());
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;

namespace SimpleKeyValueStore
{
    /// Represents a key-value pair in memory
    public class KeyValueEntry
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public KeyValueEntry(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }

    /// Core database that handles in-memory storage and persistence
    public class KeyValueDatabase
    {
        private const string DataFile = "data.db";

        // In-memory index (no Dictionary allowed)
        private List<KeyValueEntry> store;

        public KeyValueDatabase()
        {
            store = new List<KeyValueEntry>();
            LoadFromDisk();
        }

        /// SET command implementation
        /// Appends to disk and updates in-memory store
        public void Set(string key, string value)
        {
            AppendToLog($"SET {key} {value}");
            UpdateInMemory(key, value);
        }

        /// GET command implementation
        /// Returns the latest value for the key
        public string Get(string key)
        {
            // Linear scan from end for "last write wins"
            for (int i = store.Count - 1; i >= 0; i--)
            {
                if (store[i].Key == key)
                {
                    return store[i].Value;
                }
            }

            return null;
        }

        /// Replays data.db on startup to rebuild state
        private void LoadFromDisk()
        {
            if (!File.Exists(DataFile))
                return;

            foreach (string line in File.ReadLines(DataFile))
            {
                string[] parts = line.Split(' ', 3);

                if (parts.Length == 3 && parts[0] == "SET")
                {
                    string key = parts[1];
                    string value = parts[2];
                    UpdateInMemory(key, value);
                }
            }
        }

        /// Writes command to disk immediately (append-only)
        private void AppendToLog(string line)
        {
            using (StreamWriter writer = new StreamWriter(DataFile, append: true))
            {
                writer.WriteLine(line);
            }
        }

        /// Updates in-memory list (does not remove old entries)
        private void UpdateInMemory(string key, string value)
        {
            store.Add(new KeyValueEntry(key, value));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            KeyValueDatabase database = new KeyValueDatabase();

            string input;

            while ((input = Console.ReadLine()) != null)
            {
                string[] parts = input.Split(' ', 3);

                if (parts.Length == 0)
                    continue;

                string command = parts[0].ToUpper();

                if (command == "SET" && parts.Length == 3)
                {
                    database.Set(parts[1], parts[2]);
                }
                else if (command == "GET" && parts.Length == 2)
                {
                    string? result = database.Get(parts[1]);
                    if (result != null)
                    {
                        Console.WriteLine(result);
                    }
                }
                else if (command == "EXIT")
                {
                    break;
                }
            }
        }
    }
}
using System;
using System.IO;
using UnityEngine;

namespace MiniFactory.Persistence
{
    public class SaveService
    {
        private const string FileName = "save.json";

        private readonly string _savePath;

        public SaveService()
        {
            _savePath =
                Path.Combine(
                    Application.persistentDataPath,
                    FileName
                );
        }

        public void Save(GameSaveData data)
        {
            try
            {
                string json =
                    JsonUtility.ToJson(data, true);

                File.WriteAllText(
                    _savePath,
                    json
                );
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"Failed to save game: {exception.Message}"
                );
            }
        }

        public GameSaveData Load()
        {
            if (!File.Exists(_savePath))
                return null;

            try
            {
                string json =
                    File.ReadAllText(_savePath);

                return JsonUtility.FromJson<GameSaveData>(
                    json
                );
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"Failed to load save: {exception.Message}"
                );

                return null;
            }
        }

        public void DeleteSave()
        {
            try
            {
                if (File.Exists(_savePath))
                {
                    File.Delete(_savePath);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"Failed to delete save: {exception.Message}"
                );
            }
        }

        public string GetSavePath()
        {
            return _savePath;
        }
    }
}
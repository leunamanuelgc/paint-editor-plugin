using JetBrains.Annotations;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public abstract class ACommand
    {
        protected List<Layer> backup;

        protected List<Layer> redoBackup;

        public ACommand()
        {
            //De momento lo dejo así, pero el backup tendrá q ser de todas las capas
            backup = new List<Layer>();
            redoBackup = new List<Layer>();
        }

        public void SaveBackup()
        {
            var app = PaintEditorPlugin.Instance;
            backup = new List<Layer>();
            foreach (var layer in app.canvas.layerList)
            {
                Rect r = new Rect(app.canvas.rect.position, app.canvas.realSize);
                Layer newLayer = new Layer(0, r);
                layer.CopyToLayer(newLayer);
                backup.Add(newLayer);
            }
        }

        public void SaveRedoBackup()
        {
            var app = PaintEditorPlugin.Instance;
            redoBackup = new List<Layer>();
            foreach (var layer in app.canvas.layerList)
            {
                Rect r = new Rect(app.canvas.rect.position, app.canvas.realSize);
                Layer newLayer = new Layer(0, r);
                layer.CopyToLayer(newLayer);
                redoBackup.Add(newLayer);
            }
        }

        public void Undo()
        {
            SaveRedoBackup();

            var app = PaintEditorPlugin.Instance;

            if(backup.Count < app.canvas.layerList.Count)
            {
                app.canvas.layerList.RemoveAt(app.canvas.layerList.Count - 1);
            }
            else if (backup.Count > app.canvas.layerList.Count)
            {
                Rect r = new Rect(app.canvas.rect.position, app.canvas.realSize);
                app.canvas.layerList.Add(new Layer(0, r));
            }

            for (int i = 0; i < backup.Count; i++)
            {
                var layer = backup[i];
                layer.CopyToLayer(app.canvas.layerList[i]);
            }
        }

        public void Redo()
        {
            var app = PaintEditorPlugin.Instance;

            if (redoBackup.Count < app.canvas.layerList.Count)
            {
                app.canvas.layerList.RemoveAt(app.canvas.layerList.Count - 1);
            }
            else if (redoBackup.Count > app.canvas.layerList.Count)
            {
                Rect r = new Rect(app.canvas.rect.position, app.canvas.realSize);
                app.canvas.layerList.Add(new Layer(0, r));
            }

            for (int i = 0; i < redoBackup.Count; i++)
            {
                var layer = redoBackup[i];
                layer.CopyToLayer(app.canvas.layerList[i]);
            }
        }

        public abstract bool Execute();

        public string BackupString()
        {
            return backup.ToString();
        }

        public string RedoBackupString()
        {
            return redoBackup.ToString();
        }
    }

    public class CommandHistory
    {
        private static int limit = 100;

        public int historyPointer { get; private set; }

        public List<ACommand> history { get; private set; }

        public CommandHistory() 
        {
            historyPointer = 0;
            history = new List<ACommand>();
        }

        public void Push(ACommand command)
        {
            if(history.Count >= limit)
            {
                history.RemoveAt(0);
            }

            if (historyPointer < history.Count - 1)
            {
                int count = (history.Count - 1) - historyPointer;
                history.RemoveRange(historyPointer + 1, count);
            }

            history.Add(command);
            historyPointer = history.Count - 1;
        }

        public ACommand RetrievePrevious()
        {
            if (historyPointer - 1 < -1) return null;
            var item = history[historyPointer];
            historyPointer -= 1;
            return item;
        }

        public ACommand RetrieveNext()
        {
            if (historyPointer + 1 > history.Count - 1) return null;
            historyPointer += 1;
            var item = history[historyPointer];
            return item;
        }

        public void Clear()
        {
            history.Clear();
            historyPointer = 0;
        }

        public void DebugLog()
        {
            string debugString = $"{historyPointer}: \n";

            for (int i = 0; i < history.Count; i++)
            {
                debugString += history[i].ToString();
                debugString += history[i].BackupString();
                debugString += history[i].RedoBackupString();
                if (i == historyPointer) debugString += " <- ";
                debugString += "\n";
            }

            Debug.Log(debugString);
        }
    }
}


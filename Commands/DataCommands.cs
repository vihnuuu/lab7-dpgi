using System.Windows.Input;
using static System.Data.Entity.Infrastructure.Design.Executor;

namespace WpfApplProject.Commands
{
    public static class DataCommands
    {
        public static RoutedCommand Undo { get; set; }
        public static RoutedCommand New { get; set; }
        public static RoutedCommand Replace { get; set; }
        public static RoutedCommand Save { get; set; }
        public static RoutedCommand Find { get; set; }
        public static RoutedCommand Delete { get; set; }
        public static RoutedCommand Update { get; set; }

        static DataCommands()
        {
            // Undo — Ctrl+Z
            InputGestureCollection undoGestures = new InputGestureCollection();
            undoGestures.Add(new KeyGesture(Key.Z, ModifierKeys.Control, "Ctrl+Z"));
            Undo = new RoutedCommand("Undo", typeof(DataCommands), undoGestures);

            // New — Ctrl+N
            InputGestureCollection newGestures = new InputGestureCollection();
            newGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control, "Ctrl+N"));
            New = new RoutedCommand("New", typeof(DataCommands), newGestures);

            // Replace — Ctrl+R
            InputGestureCollection replaceGestures = new InputGestureCollection();
            replaceGestures.Add(new KeyGesture(Key.R, ModifierKeys.Control, "Ctrl+R"));
            Replace = new RoutedCommand("Replace", typeof(DataCommands), replaceGestures);

            // Save — Ctrl+S
            InputGestureCollection saveGestures = new InputGestureCollection();
            saveGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control, "Ctrl+S"));
            Save = new RoutedCommand("Save", typeof(DataCommands), saveGestures);

            // Find — Ctrl+F
            InputGestureCollection findGestures = new InputGestureCollection();
            findGestures.Add(new KeyGesture(Key.F, ModifierKeys.Control, "Ctrl+F"));
            Find = new RoutedCommand("Find", typeof(DataCommands), findGestures);

            // Delete — Ctrl+D
            InputGestureCollection deleteGestures = new InputGestureCollection();
            deleteGestures.Add(new KeyGesture(Key.D, ModifierKeys.Control, "Ctrl+D"));
            Delete = new RoutedCommand("Delete", typeof(DataCommands), deleteGestures);

            // Update — Ctrl+U
            InputGestureCollection updateGestures = new InputGestureCollection();
            updateGestures.Add(new KeyGesture(Key.U, ModifierKeys.Control, "Ctrl+U")); 
            Update = new RoutedCommand("Update", typeof(DataCommands), updateGestures);
        }
    }
}

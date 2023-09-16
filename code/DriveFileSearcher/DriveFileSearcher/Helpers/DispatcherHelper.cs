using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DriveFileSearcher.Helpers
{
    public static class DispatcherHelper
    {
        public static async Task RunAsync(Func<Task> action)
        {
            await TaskRunAsync(action);
        }

        public static async Task RunAsync(Action action)
        {
            await TaskRunAsync(action);
        }

        public static async Task InvokeAsync(Action action)
        {
            await Application.Current.Dispatcher.InvokeAsync(action);
        }

        private static async Task TaskRunAsync<T>(T action) where T : Delegate
        {
            await InvokeAsync(async () =>
            {
                 await Task.Run(() => action.DynamicInvoke());
            });
        }
    }
}

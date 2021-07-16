using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mckielce_helper {
    static class NotificationHandler {
        static NotificationOverlay overlay;

        public static void Send(string key, string info) {
            if (overlay != null) {
                overlay.instantClose = true;
                overlay.Close();
            }
            overlay = new NotificationOverlay(key, info);
            overlay.Show();
        }
    }
}

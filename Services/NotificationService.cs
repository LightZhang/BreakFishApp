using System;
using System.Media;
using System.Windows.Forms;
using BreakFishApp.Models;

namespace BreakFishApp.Services
{
    public class NotificationService
    {
        private readonly NotifyIcon _icon;
        private readonly Func<AppSettings> _settings;

        public NotificationService(NotifyIcon icon, Func<AppSettings> settings)
        {
            _icon = icon;
            _settings = settings;
        }

        public void ShowReminder(ReminderItem item)
        {
            if (item == null)
            {
                return;
            }

            Show(item.Emoji + " " + item.Title, item.Message);
        }

        public void Show(string title, string message)
        {
            var settings = _settings != null ? _settings() : null;
            if (settings != null && !settings.NotificationEnabled)
            {
                return;
            }

            if (_icon != null)
            {
                _icon.BalloonTipTitle = string.IsNullOrEmpty(title) ? "FishBreak" : title;
                _icon.BalloonTipText = message ?? string.Empty;
                _icon.ShowBalloonTip(8000);
            }

            if (settings != null && settings.SoundEnabled)
            {
                SystemSounds.Asterisk.Play();
            }
        }
    }
}

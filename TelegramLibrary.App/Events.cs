using System;

namespace TelegramLibrary.App
{
    public static class Events
    {
        public delegate string AskUserCode(object sender, PhoneNumberArgs args);

        public delegate string AskUserPassword(object sender, PhoneNumberArgs args);
    }

    public class PhoneNumberArgs : EventArgs
    {
        public string PhoneNumber { get; set; }
    }
}

using System;

namespace ChatSharp.Events
{
    public class ChannelUserEventArgs : EventArgs
    {
        public IrcChannel Channel { get; set; }
        public IrcUser User { get; set; }

        public ChannelUserEventArgs(IrcChannel channel, IrcUser user)
        {
            Channel = channel;
            User = user;
        }
    }
}

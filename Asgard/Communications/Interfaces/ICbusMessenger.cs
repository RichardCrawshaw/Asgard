﻿using System;
using System.Threading.Tasks;
using Asgard.Data;

namespace Asgard.Communications
{
    public interface ICbusMessenger
    {
        event EventHandler<CbusMessageEventArgs> MessageReceived;
        event EventHandler<CbusMessageEventArgs> MessageSent;

        Task<bool> SendMessage(ICbusMessage message);
        Task<bool> SendMessage(ICbusOpCode message) => SendMessage(message.Message);

        void Open();
        
    }
}

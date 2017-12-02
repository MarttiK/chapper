using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Collections.Concurrent;
using StorageClient;

namespace Chapper
{
    public class ChappHub : Hub
    {
        private static ConcurrentDictionary<string, string> _chatClients = new ConcurrentDictionary<string, string>();
        private static IStorageClient _storage;

        public ChappHub(IStorageClient storageClient)
        {
            _storage = storageClient;
        }

        /// <summary>
        /// Send message to all users online
        /// </summary>
        /// <param name="message">message to be sent</param>
        public void Send(string message)
        {
            string user = _chatClients[Context.ConnectionId];
            _storage.StorageMessage(user, message);
            Clients.All.broadcastMessage(user,message);
        }


        /// <summary>
        /// Notify user login to the chat
        /// </summary>
        /// <param name="name">username</param>
        public void Login(string name)
        {
            _storage.StorageLogin(name);
            _chatClients.TryAdd( Context.ConnectionId, name );
            Clients.Others.broadcastNewUser(name);
            var users = _chatClients.Values.ToArray();
            Clients.Caller.usersOnline(users);
        }

        /// <summary>
        /// Notify that user has logged out
        /// </summary>
        /// <param name="name">usernames</param>
        public void Logout(string name)
        {
            _storage.StorageLogout(name);
            _chatClients.TryRemove(Context.ConnectionId, out string value);
            var users = _chatClients.Values.ToArray();
            Clients.Others.usersOnline(users);
        }
    }
}
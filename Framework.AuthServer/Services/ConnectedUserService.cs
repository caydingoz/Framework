namespace Framework.AuthServer.Services
{
    public class ConnectedUserService
    {
        private readonly HashSet<string> _connectedUsers = [];

        public void AddUser(string connectionId)
        {
            lock (_connectedUsers)
            {
                _connectedUsers.Add(connectionId);
            }
        }

        public void RemoveUser(string connectionId)
        {
            lock (_connectedUsers)
            {
                _connectedUsers.Remove(connectionId);
            }
        }

        public IReadOnlyCollection<string> GetConnectedUsers()
        {
            return _connectedUsers.ToList().AsReadOnly();
        }
    }

}

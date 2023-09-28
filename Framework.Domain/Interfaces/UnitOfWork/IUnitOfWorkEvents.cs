namespace Framework.Domain.Interfaces.UnitOfWork
{
    public delegate Task CommittedEventHandler(object sender, EventArgs e);
    public delegate Task RollBackedEventHandler(object sender, EventArgs e);
    public interface IUnitOfWorkEvents
    {
        public event CommittedEventHandler Committed;
        public event RollBackedEventHandler RollBacked;
    }
}
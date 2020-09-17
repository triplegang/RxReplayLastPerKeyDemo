namespace RxReplayLastPerKeyDemo
{
    public class Item
    {
        public int Id { get; private set; }

        public int Value { get; private set; }

        public Item(int id, int value)
        {
            Id = id;
            Value = value;
        }

        public bool HasChanges(Item compareItem)
        {
            return this.Value != compareItem.Value;
        }

        public override string ToString()
        {
            return $"Item: Id={Id}  Value={Value}";
        }
    }
}

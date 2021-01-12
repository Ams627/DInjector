namespace Dinject.Nac
{
    class NaPreExecuteAttribute : System.Attribute
    {
        private readonly string _name;
        public string Name => _name;
        public NaPreExecuteAttribute(string commandName)
        {
            _name = commandName;
        }
    }
}

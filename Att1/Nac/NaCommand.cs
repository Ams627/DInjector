namespace Dinject.Nac
{
    class NaCommandAttribute : System.Attribute
    {
        private readonly string _name;
        public string Name => _name;

        public NaCommandAttribute(string commandName)
        {
            _name = commandName;
            System.Console.WriteLine("Na command");
        }
    }
}

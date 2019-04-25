namespace RiscVSimulator.Controllers
{
    public class UncompleteParseData
    {
        public string optionalLabel { get; set; }
        public string command { get; set; }
        public int lineNumber { get; set; }

        public UncompleteParseData(string _optionalLabel, string _command, int _lineNumber)
        {
            optionalLabel = new string(_optionalLabel);
            command = new string(_command);
            lineNumber = _lineNumber;
        }
    }
}
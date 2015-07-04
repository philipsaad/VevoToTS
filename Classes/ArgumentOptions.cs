using CommandLine;
using CommandLine.Text;

namespace VevoToTS
{
    class Options
    {
        [Option('v', "videoId", Required = true, HelpText = "VEVO Video Id.")]
        public string VideoId { get; set; }

        [Option('o', "outputFile", Required = true, HelpText = "Output filename.")]
        public string OutputFile { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
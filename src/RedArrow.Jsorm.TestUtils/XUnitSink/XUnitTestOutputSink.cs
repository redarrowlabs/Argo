using System;
using System.IO;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Xunit.Abstractions;

namespace RedArrow.Jsorm.TestUtils.XUnitSink
{
    public class XUnitTestOutputSink : ILogEventSink
    {
        readonly ITestOutputHelper _output;
        readonly ITextFormatter _textFormatter;

        public XUnitTestOutputSink(ITestOutputHelper testOutputHelper, ITextFormatter textFormatter)
        {
            if (testOutputHelper == null) throw new ArgumentNullException("testOutputHelper");
            if (textFormatter == null) throw new ArgumentNullException("textFormatter");

            _output = testOutputHelper;
            _textFormatter = textFormatter;
        }

        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null) throw new ArgumentNullException("logEvent");

            var renderSpace = new StringWriter();
            _textFormatter.Format(logEvent, renderSpace);
            _output.WriteLine(renderSpace.ToString());
        }
    }
}

using System;
using System.IO;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Xunit.Abstractions;

namespace RedArrow.Argo.TestUtils.XUnitSink
{
    public class XUnitTestOutputSink : ILogEventSink
    {
        readonly ITestOutputHelper _output;
        readonly ITextFormatter _textFormatter;

        public XUnitTestOutputSink(ITestOutputHelper testOutputHelper, ITextFormatter textFormatter)
        {
            _output = testOutputHelper ?? throw new ArgumentNullException("testOutputHelper");
            _textFormatter = textFormatter ?? throw new ArgumentNullException("textFormatter");
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
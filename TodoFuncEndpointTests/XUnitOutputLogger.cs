// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Abstractions;
namespace TodoFuncEndpointTests;

public class XUnitOutputLogger : ILogger
{
    private readonly ITestOutputHelper _output;
    public XUnitOutputLogger(ITestOutputHelper testOutputHelper, string categoryName)
    {
        ArgumentNullException.ThrowIfNull(testOutputHelper);
        ArgumentException.ThrowIfNullOrEmpty(categoryName);

        Name = categoryName;
        _output = testOutputHelper;
    }

    public string Name { get; }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
        => NullLogger.Instance.BeginScope(state);

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);

        var message = formatter(state, exception);

        if (!string.IsNullOrEmpty(message) || exception != null)
            WriteToOutput(logLevel, Name, eventId.Id, message, exception);
    }

    private void WriteToOutput(LogLevel logLevel, string logName, int eventId, string? message, Exception? exception)
    {
        ArgumentException.ThrowIfNullOrEmpty(logName);
    }
}